using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
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

    [AllowMutableSyncTypeAttribute] public SyncVar<bool> CanStartGame = new();
    [field: SerializeField] public bool DidStartGame { get; private set; }
    [field: SerializeField] public int Turn { get; private set; }

    [AllowMutableSyncTypeAttribute] public SyncVar<Phase> CurrentPhase = new(new SyncTypeSettings(WritePermission.ServerOnly));
    [field: SerializeField] public int StartingTurn { get; private set; }
    [field: SerializeField] public int StartingGold { get; private set; }
    [field: SerializeField] public int ReputationGoal { get; private set; }
    [field: SerializeField] public bool[] PlayerSkipTurnStatus { get; private set; }
    [field: SerializeField] public SyncList<bool> PlayerEndRoundStatus { get; } = new SyncList<bool>();
    [field: SerializeField] public int RoundNumber { get; private set; }
    #endregion

    #region Game Phases
    public enum Phase { Recruit, Dispatch, Magic, Resolution, Recovery, GameOver }
    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsServerInitialized) return;

        if (DidStartGame && Players.Count == 0)
        {
            DeploymentManager.Instance.InitiateServerRestart();
            DidStartGame = false;
            print("restarting server");
        }
    }

    //[ObserversRpc]
    //public void ObserversSetCanStartGame(bool value)
    //{
    //    CanStartGame.Value = value;
    //}

    [Server]
    public void SetCanStartGame(bool value)
    {
        CanStartGame.Value = value;
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
        RoundNumber = 1;
        CurrentPhase.Value = Phase.Recruit;
        Board.Instance.ObserversUpdatePhaseText("Recruit");

        StartingTurn = 0;

        foreach (Player player in Players)
        {
            player.StartGame();
        }

        Board.Instance.StartGame();
        CheckMerchantsGuildGold();
        DidStartGame = true;

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
        DidStartGame = false;
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
        PlayerEndRoundStatus.Clear();
        for (int i = 0; i < Players.Count; i++)
        {
            PlayerEndRoundStatus.Add(false);
        }
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
        Board.Instance.ObserversUpdateTurnMarker(playerID, false);

        if (PlayerEndRoundStatus.All(status => status))
        {
            ObserversEnableEndRoundButton();
            //EndPhase();
            Board.Instance.CheckQuestsForCompletion();
        }
    }

    /// <summary>
    /// Refreshes the end round status, resetting all players' statuses and enabling the end round button.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ServerRefreshEndRoundStatus()
    {
        PlayerEndRoundStatus.Clear();
        for (int i = 0; i < Players.Count; i++)
        {
            PlayerEndRoundStatus.Add(false);
        }
        ObserversEnableEndRoundButton();
        Board.Instance.ObserversUpdateTurnMarker();
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
        switch (CurrentPhase.Value)
        {
            case Phase.Recruit:
                CurrentPhase.Value = Phase.Dispatch;
                Board.Instance.ObserversUpdatePhaseText("Dispatch");
                Turn = StartingTurn;
                SetPlayerTurn(Players[Turn]);
                break;

            case Phase.Dispatch:
                CurrentPhase.Value = Phase.Resolution;
                Board.Instance.ObserversUpdatePhaseText("Resolution");
                CheckForUnresolvedCards();
                break;

            case Phase.Resolution:
                CurrentPhase.Value = Phase.Magic;
                Board.Instance.ObserversUpdatePhaseText("Magic");
                BeginEndRound();
                Board.Instance.ObserversUpdateTurnMarker();
                break;

            case Phase.Magic:
                Board.Instance.ResetQuests();
                Board.Instance.CheckAllQuestsComplete();

                if (CurrentPhase.Value == Phase.GameOver)
                {
                    EndGame();
                    break;
                }

                CurrentPhase.Value = Phase.Recruit;
                Board.Instance.ObserversUpdatePhaseText("Recruit");

                StartingTurn = (StartingTurn + 1) % Players.Count;
                Turn = StartingTurn;
                SetPlayerTurn(Players[Turn]);
                RoundNumber++;

                DiscardPile.Instance.RecoverAdventurers();
                ResetGuildBonusTrackers();
                CheckMerchantsGuildGold();
                break;

            case Phase.GameOver:
                EndGame();
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
        Board.Instance.ObserversUpdateTurnMarker(currentPlayer.PlayerID.Value);
    }

    /// <summary>
    /// Sets the game phase to Game Over.
    /// </summary>
    [Server]
    public void SetPhaseGameOver()
    {
        CurrentPhase.Value = Phase.GameOver;
    }

    /// <summary>
    /// Ends the game, updating the board and launching the Game Over pop-up.
    /// </summary>
    [Server]
    public void EndGame()
    {
        Board.Instance.ObserversUpdatePhaseText("", true);
    }


    [Server]
    public void ResetGuildBonusTrackers()
    {
        foreach (Player player in Players)
        {
            player.InitializeGuildBonusTracker();
        }
    }

    [Server]
    private void CheckMerchantsGuildGold()
    {
        foreach (Player player in Players)
        {
            if (player.isMerchantsGuild)
            {
                player.ChangePlayerGold(1);
                print($"Merchant Guild Bonus - Player {player.PlayerID.Value} +1 GP - new round");
            }
        }
    }
}
