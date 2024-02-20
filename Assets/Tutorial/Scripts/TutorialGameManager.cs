using System.Linq;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class TutorialGameManager : NetworkBehaviour
{
    public static TutorialGameManager Instance { get; private set; }

    [field: SyncObject]
    public SyncList<TutorialPlayer> Players { get; } = new SyncList<TutorialPlayer>();

    [field: SerializeField]
    [field: SyncVar]
    public bool CanStart { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool DidStart { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int Turn { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsServer) return;

        CanStart = Players.All(player => player.IsReady);

        print($"There are {Players.Count} players in the game");
    }

    [Server]
    public void StartGame()
    {
        for (int i = 0; i < Players.Count; i++)
        {
            Players[i].StartGame();
        }

        DidStart = true;

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
    }

    [Server]
    public void EndTurn()
    {
        Turn = (Turn + 1) % Players.Count;

        BeginTurn();
    }
}
