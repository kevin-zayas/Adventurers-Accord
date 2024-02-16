using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static Player Instance { get; private set; }

    [SyncVar]
    public string username;

    [SyncVar]
    public bool isReady;

    [SyncVar]
    public Pawn ontrolledPawn;

    public override void OnStartServer()
    {
        base.OnStartServer();

        GameManager2.Instance.Players.Add(this);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        GameManager2.Instance.Players.Remove(this);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner) return;

        Instance = this;
    }

}
