using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Player : NetworkBehaviour
{
    public static Player Instance { get; private set; }

    [SyncVar]
    public string username;

    [field: SyncVar]
    public bool IsReady
    {
        get;

        [ServerRpc(RequireOwnership = false)]   //alternate way to set IsReady
        set;
    }

    [SerializeField]
    private Pawn pawnPrefab;

    [SyncVar]
    public Pawn controlledPawn;

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

        ViewManager.Instance.Initialize();
    }

    //[ServerRpc(RequireOwnership = false)]
    //public void ServerSetIsReady(bool value)
    //{
    //    IsReady = value;
    //}

    [Server]
    public void StartGame()
    {
        //GameObject pawnPrefab = Addressables.LoadAssetAsync<GameObject>("Pawn").WaitForCompletion();
        //GameObject pawnInstance = Instantiate(pawnPrefab);
        //GameObject pawnInstance = Instantiate(pawnPrefab);

        int playerIndex = GameManager2.Instance.Players.IndexOf(this);

        Transform spawnPoint = Board.Instance.Tiles[0].PawnPositions[playerIndex];

        Pawn pawnInstance = Instantiate(pawnPrefab, spawnPoint.position, Quaternion.identity);

        controlledPawn = pawnInstance;

        controlledPawn.controllingPlayer = this;

        Spawn(pawnInstance.gameObject, Owner);

        TargetStartGame(Owner);
    }

    [Server]
    public void StopGame()
    {
        if (controlledPawn != null) controlledPawn.Despawn();
    }

    [TargetRpc]
    private void TargetStartGame(NetworkConnection networkConnection)
    {
        ViewManager.Instance.Show<MainView>();
    }

}
