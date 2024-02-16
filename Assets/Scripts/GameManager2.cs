using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class GameManager2 : NetworkBehaviour
{
    public static GameManager2 Instance { get; private set; }

    [field: SyncObject]
    public SyncList<Player> Players { get; } = new SyncList<Player>();

    [field: SyncVar]
    public bool CanStart { get; private set; }

    [field: SyncVar]
    public bool DidStart { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsServer) return;

        CanStart = Players.Count > 1;

        print($"There are {Players.Count} players in the game");
    }

    [Server]
    public void StartGame()
    {
        DidStart = true;
    }

    [Server]
    public void StopGame()
    {
        DidStart = false;
    }
}
