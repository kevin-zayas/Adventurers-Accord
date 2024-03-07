using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Linq;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum Phase { Draft, Dispatch, Magic, Resolve }

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
    public Player CurrentTurnPlayer { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int StartingTurn { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool[] PlayerSkipTurnStatus { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsServer) return;

        CanStart = Players.All(player => player.IsReady);
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
        DidStart = true;

        PlayerSkipTurnStatus = new bool[Players.Count];

        BeginTurn();
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

    [Server]
    public void BeginTurn()
    {
        for (int i = 0; i < Players.Count; i++)
        {
            Players[i].BeginTurn();
        }
        CurrentTurnPlayer = Players[Turn];
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndTurn(bool passTurn)
    {
        if (passTurn) PlayerSkipTurnStatus[Turn] = true;

        Turn = (Turn + 1) % Players.Count;
        int attemptCount = 0;

        while (PlayerSkipTurnStatus[Turn] && attemptCount < Players.Count)
        {
            Turn = (Turn + 1) % Players.Count;
            attemptCount++;
        }

        if (attemptCount == Players.Count)
        {
            EndPhase();
            return;
        }

        BeginTurn();
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
                break;
            case Phase.Dispatch:
                Board.Instance.CheckQuests();
                Board.Instance.ResetQuests();
                CurrentPhase = Phase.Draft;
                Board.Instance.ObserversUpdatePhaseText("Draft");
                StartingTurn = (StartingTurn + 1) % Players.Count;
                Turn = StartingTurn;
                break;
            //case Phase.Magic:
            //    CurrentPhase = Phase.Resolve;
            //    break;
            //case Phase.Resolve:
            //    CurrentPhase = Phase.Draft;
            //    break;
        }
        BeginTurn();
    }
}
