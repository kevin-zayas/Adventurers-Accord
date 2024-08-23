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

    [field: SerializeField, SyncObject] public SyncList<Player> Players { get; } = new SyncList<Player>();

    [field: SerializeField, SyncVar] public bool CanStart { get; private set; }
    [field: SerializeField, SyncVar] public bool DidStart { get; private set; }
    [field: SerializeField, SyncVar] public int Turn { get; private set; }
    [field: SerializeField, SyncVar] public Phase CurrentPhase { get; private set; }
    [field: SerializeField, SyncVar] public int StartingTurn { get; private set; }
    [field: SerializeField, SyncVar] public int StartingGold { get; private set; }
    [field: SerializeField, SyncVar] public int StartingLoot { get; private set; }
    [field: SerializeField, SyncVar] public int ReputationGoal { get; private set; }
    [field: SerializeField, SyncVar] public bool[] PlayerSkipTurnStatus { get; private set; }
    [field: SerializeField, SyncVar] public bool[] PlayerEndRoundStatus { get; private set; }
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
        if (!IsServer) return;

        CanStart = Players.All(player => player.IsReady);
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

        foreach (var player in Players)
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
        foreach (var player in Players)
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
    /// Confirms that a player has ended their round, and checks if all players have done so.
    /// </summary>
    /// <param name="playerID">The ID of the player confirming the end of their round.</param>
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
    /// Resets the end round status for all players, preparing for the next round.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void RefreshEndRoundStatus()
    {
        PlayerEndRoundStatus = new bool[Players.Count];
        ObserversEnableEndRoundButton();
        Scoreboard.ObserversEnableAllTurnMarkers();
    }

    /// <summary>
    /// Enables the "End Round" button on all clients.
    /// </summary>
    [ObserversRpc(BufferLast = true)]
    private void ObserversEnableEndRoundButton()
    {
        GameObject.Find("EndRoundView").GetComponent<EndRoundView>().EnableEndRoundUI();
    }

    /// <summary>
    /// Prepares the end-of-round process for all players.
    /// </summary>
    [Server]
    private void BeginEndRound()
    {
        PlayerEndRoundStatus = new bool[Players.Count];
        foreach (var player in Players)
        {
            player.UpdatePlayerView();
        }
    }

    /// <summary>
    /// Ends the current phase and advances to the next phase based on game logic.
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
    /// Checks for any unresolved cards in the current phase before advancing.
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

        EndPhase(); // If no unresolved cards, end phase
    }

    /// <summary>
    /// Server-side method to trigger a check for unresolved cards.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ServerCheckForUnresolvedCards()
    {
        CheckForUnresolvedCards();
    }

    /// <summary>
    /// Sets the current player's turn and updates all clients.
    /// </summary>
    /// <param name="currentPlayer">The player whose turn it currently is.</param>
    [Server]
    public void SetPlayerTurn(Player currentPlayer)
    {
        foreach (var player in Players)
        {
            player.SetIsPlayerTurn(player == currentPlayer);
            player.UpdatePlayerView();
        }
        Scoreboard.ObserversUpdateTurnMarker(currentPlayer.PlayerID);
    }

    /// <summary>
    /// Sets the game phase to "Game Over".
    /// </summary>
    [Server]
    public void SetPhaseGameOver()
    {
        CurrentPhase = Phase.GameOver;
    }

    /// <summary>
    /// Ends the game and triggers the game over logic.
    /// </summary>
    [Server]
    public void EndGame()
    {
        Board.Instance.ObserversUpdatePhaseText("", true);
        LaunchGameOverPopUp();
    }

    /// <summary>
    /// Launches the game over popup and calculates final rankings.
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
