using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [field: SerializeField] public ScoreBoard Scoreboard { get; private set; }

    [field: SerializeField] public SyncList<Player> Players { get; } = new SyncList<Player>();

    [field: SerializeField] public bool CanStart { get; private set; }
    [field: SerializeField] public bool DidStart { get; private set; }
    [field: SerializeField] public int Turn { get; private set; }
    [field: SerializeField] public Phase CurrentPhase { get; private set; }
    [field: SerializeField] public int StartingTurn { get; private set; }
    [field: SerializeField] public int StartingGold { get; private set; }
    [field: SerializeField] public int StartingLoot { get; private set; }
    [field: SerializeField] public int ReputationGoal { get; private set; }
    [field: SerializeField] public bool[] PlayerSkipTurnStatus { get; private set; }
    [field: SerializeField] public bool[] PlayerEndRoundStatus { get; private set; }
    #endregion

    #region Game Phases
    public enum Phase { Recruit, Dispatch, Magic, Resolution, GameOver }
    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsServerInitialized) return;

        CanStart = Players.All(player => player.IsReady.Value);

        if (DidStart && Players.Count == 0)
        {
            ApiManager.Instance.RestartGameServer();
            DidStart = false;
        }
    }

    /// <summary>
    /// Starts the game by initializing all necessary game states and starting the first phase.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ServerStartGame()
    {
        StartGame();
    }

    /// <summary>
    /// Initializes the game, sets the initial phase, and prepares players and board.
    /// </summary>
    [Server]
    public void StartGame()
    {
        CurrentPhase = Phase.Recruit;
        Board.Instance.ObserversUpdatePhaseText("Recruit");

        StartingTurn = 0;

        foreach (Player player in Players)
        {
            player.StartGame();
        }

        Board.Instance.StartGame();
        Scoreboard.StartGame(StartingGold);
        DidStart = true;

        PlayerSkipTurnStatus = new bool[Players.Count];
        SetPlayerTurn(Players[Turn]);
    }

    /// <summary>
    /// Stops the game and resets the game state.
    /// </summary>
    [Server]
    public void StopGame()
    {
        foreach (Player player in Players)
        {
            player.StopGame();
        }
        DidStart = false;
    }

    /// <summary>
    /// Ends the current player's turn and advances to the next player or phase.
    /// </summary>
    /// <param name="passTurn">Indicates whether the current player wants to pass their turn.</param>
    [ServerRpc(RequireOwnership = false)]
    public void EndTurn(bool passTurn)
    {
        if (passTurn) PlayerSkipTurnStatus[Turn] = true;

        Turn = (Turn + 1) % Players.Count;

        if (PlayerSkipTurnStatus.All(status => status))
        {
            EndPhase();
            return;
        }

        while (PlayerSkipTurnStatus[Turn])
        {
            Turn = (Turn + 1) % Players.Count;
        }

        SetPlayerTurn(Players[Turn]);
    }

    /// <summary>
    /// Prepares for the end of the round by resetting player statuses and updating player views.
    /// </summary>
    [Server]
    private void BeginEndRound()
    {
        PlayerEndRoundStatus = new bool[Players.Count];
        foreach (Player player in Players)
        {
            player.SetIsPlayerTurn(false);
            player.UpdatePlayerView();
        }
    }

    /// <summary>
    /// Confirms that a player has ended their round and checks if all players have confirmed.
    /// Advances the game state if all players have confirmed.
    /// </summary>
    /// <param name="playerID">The ID of the player confirming the end of the round.</param>
    [ServerRpc(RequireOwnership = false)]
    public void ConfirmEndRound(int playerID)
    {
        PlayerEndRoundStatus[playerID] = true;
        Scoreboard.ObserversToggleTurnMarker(playerID, false);

        if (PlayerEndRoundStatus.All(status => status))
        {
            ObserversEnableEndRoundButton();
            EndPhase();
        }
    }

    /// <summary>
    /// Refreshes the end round status, resetting all players' statuses and enabling the end round button.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void RefreshEndRoundStatus()
    {
        PlayerEndRoundStatus = new bool[Players.Count];
        ObserversEnableEndRoundButton();
        Scoreboard.ObserversEnableAllTurnMarkers();
    }

    /// <summary>
    /// Enables the end round button on all clients.
    /// </summary>
    [ObserversRpc(BufferLast = true)]
    private void ObserversEnableEndRoundButton()
    {
        GameObject.Find("EndRoundView").GetComponent<EndRoundView>().EnableEndRoundUI();
    }

    /// <summary>
    /// Ends the current game phase and advances to the next phase.
    /// </summary>
    [Server]
    public void EndPhase()
    {
        PlayerSkipTurnStatus = new bool[Players.Count];
        switch (CurrentPhase)
        {
            case Phase.Recruit:
                CurrentPhase = Phase.Dispatch;
                Board.Instance.ObserversUpdatePhaseText("Dispatch");
                Turn = StartingTurn;
                SetPlayerTurn(Players[Turn]);
                break;

            case Phase.Dispatch:
                CurrentPhase = Phase.Resolution;
                Board.Instance.ObserversUpdatePhaseText("Resolution");
                CheckForUnresolvedCards();
                break;

            case Phase.Resolution:
                CurrentPhase = Phase.Magic;
                Board.Instance.ObserversUpdatePhaseText("Magic");
                BeginEndRound();
                Scoreboard.ObserversEnableAllTurnMarkers();
                break;

            case Phase.Magic:
                Board.Instance.CheckQuestsForCompletion();
                Board.Instance.ResetQuests();

                if (CurrentPhase == Phase.GameOver)
                {
                    EndGame();
                    break;
                }

                CurrentPhase = Phase.Recruit;
                Board.Instance.ObserversUpdatePhaseText("Recruit");

                StartingTurn = (StartingTurn + 1) % Players.Count;
                Turn = StartingTurn;
                SetPlayerTurn(Players[Turn]);
                break;
        }
    }

    /// <summary>
    /// Checks all quests for unresolved cards and ends the phase if no unresolved cards are found.
    /// </summary>
    [Server]
    public void CheckForUnresolvedCards()
    {
        for (int i = 0; i < Players.Count; i++)
        {
            int laneIndex = (i + StartingTurn) % Players.Count;
            foreach (QuestLocation questLocation in Board.Instance.QuestLocations)
            {
                if (questLocation.HasUnresolvedCards(laneIndex))
                {
                    return;
                }
            }
        }

        EndPhase();
    }

    /// <summary>
    /// Server-side method to check for unresolved cards.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ServerCheckForUnresolvedCards()
    {
        CheckForUnresolvedCards();
    }

    /// <summary>
    /// Sets the turn for the specified player, updating the turn indicators on the scoreboard.
    /// </summary>
    /// <param name="currentPlayer">The player whose turn it is.</param>
    [Server]
    public void SetPlayerTurn(Player currentPlayer)
    {
        foreach (Player player in Players)
        {
            player.SetIsPlayerTurn(player == currentPlayer);
            player.UpdatePlayerView();
        }

        Scoreboard.ObserversUpdateTurnMarker(currentPlayer.PlayerID.Value);
    }

    /// <summary>
    /// Sets the game phase to Game Over.
    /// </summary>
    [Server]
    public void SetPhaseGameOver()
    {
        CurrentPhase = Phase.GameOver;
    }

    /// <summary>
    /// Ends the game, updating the board and launching the Game Over pop-up.
    /// </summary>
    [Server]
    public void EndGame()
    {
        Board.Instance.ObserversUpdatePhaseText("", true);
        LaunchGameOverPopUp();
    }

    /// <summary>
    /// Launches the Game Over pop-up, calculating the final rankings.
    /// </summary>
    [Server]
    public void LaunchGameOverPopUp()
    {
        if (CurrentPhase != Phase.GameOver) return;

        GameOverPopUp popUp = PopUpManager.Instance.CreateGameOverPopUp();
        Spawn(popUp.gameObject);

        popUp.CalculateRankings();
    }
}
