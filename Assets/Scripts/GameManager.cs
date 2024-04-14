using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [field: SerializeField]
    public ScoreBoard Scoreboard { get; private set; }

    public enum Phase { Draft, Dispatch, Magic, Resolution, GameOver}

    [field: SyncObject]
    [field: SerializeField]
    public SyncList<Player> Players { get; } = new SyncList<Player>();

    [field: SerializeField]
    [field: SyncVar]
    public bool CanStart { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool DidStart { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int Turn { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public Phase CurrentPhase { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int StartingTurn { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int StartingGold { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int ReputationGoal { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool[] PlayerSkipTurnStatus { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool[] PlayerEndRoundStatus { get; private set; }


    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsServer) return;

        CanStart = Players.All(player => player.IsReady);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerStartGame()
    {
        StartGame();
    }

    [Server]
    public void StartGame()
    {
        CurrentPhase = Phase.Draft;
        Board.Instance.ObserversUpdatePhaseText("Draft");

        StartingTurn = 0;

        for (int i = 0; i < Players.Count; i++)
        {
            Players[i].StartGame();
        }

        Board.Instance.StartGame();
        Scoreboard.StartGame(StartingGold);
        DidStart = true;

        PlayerSkipTurnStatus = new bool[Players.Count];

        SetPlayerTurn(Players[Turn]);
    }

    [Server]
    public void StopGame()
    {
        for (int i = 0; i < Players.Count; i++)
        {
            Players[i].StopGame();
        }
        DidStart = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndTurn(bool passTurn)
    {
        if (passTurn) PlayerSkipTurnStatus[Turn] = true;

        Turn = (Turn + 1) % Players.Count;

        if (PlayerSkipTurnStatus.All(status => status))     // if all players have ended turn, move onto next phase
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

    [ServerRpc(RequireOwnership = false)]
    public void ConfirmEndRound(int playerID)
    {
        PlayerEndRoundStatus[playerID] = true;
        Scoreboard.ObserversToggleTurnMarker(playerID,false);

        if (PlayerEndRoundStatus.All(status => status))     // if all players have confirmed end round
        {
            ObserversEnableEndRoundButton();
            EndPhase();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RefreshEndRoundStatus()
    {
        // reset end round status
        PlayerEndRoundStatus = new bool[Players.Count];
        ObserversEnableEndRoundButton();
        Scoreboard.ObserversEnableAllTurnMarkers();
    }

    [ObserversRpc]
    private void ObserversEnableEndRoundButton()
    {
        GameObject.Find("EndRoundView").GetComponent<EndRoundView>().EnableEndRoundButton();
    }

    [Server]
    private void BeginEndRound()
    {
        PlayerEndRoundStatus = new bool[Players.Count];
        foreach (Player player in Players)
        {
            player.UpdatePlayerView();
        }
    }

    [Server]
    public void EndPhase()
    {
        PlayerSkipTurnStatus = new bool[Players.Count];
        switch (CurrentPhase)
        {
            case Phase.Draft:
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

                if (CurrentPhase != Phase.GameOver) CurrentPhase = Phase.Draft;
                Board.Instance.ObserversUpdatePhaseText("Draft", CurrentPhase == Phase.GameOver);

                StartingTurn = (StartingTurn + 1) % Players.Count;
                Turn = StartingTurn;
                SetPlayerTurn(Players[Turn]);
                break;
        }
    }

    [Server]
    public void CheckForUnresolvedCards()
    {
        print("checking for unresolved cards");
        
        for (int i = 0; i < Players.Count; i++)
        {
            int laneIndex = (i + GameManager.Instance.StartingTurn) % Players.Count;
            foreach (QuestLocation questLocation in Board.Instance.QuestLocations)
            {
                if (questLocation.HasUnresolvedCards(laneIndex))
                {
                    return;
                }
            }
        }

        EndPhase();         // if no unresolved cards, end phase
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerCheckForUnresolvedCards()
    {
        CheckForUnresolvedCards();
    }

    [Server]
    public void SetPlayerTurn(Player currentPlayer)
    {
        foreach (Player player in Players)
        {
            player.SetIsPlayerTurn(player == currentPlayer);
            player.UpdatePlayerView();
        }
        Scoreboard.ObserversUpdateTurnMarker(currentPlayer.PlayerID);

    }

    [Server]
    public void EndGame()
    {
        CurrentPhase = Phase.GameOver;
    }

    [Server]
    public void LaunchGameOverPopUp()
    {
        if (CurrentPhase != Phase.GameOver) return;

        GameOverPopUp popUp = PopUpManager.Instance.CreateGameOverPopUp();
        Spawn(popUp.gameObject);

        popUp.CalculateRankings();
    }
}
