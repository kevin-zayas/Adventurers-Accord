using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static Player Instance { get; private set; }

    [SyncVar]
    public int playerID;

    [field: SyncVar]
    public bool IsReady
    {
        get;

        [ServerRpc(RequireOwnership = false)]   //alternate way to set IsReady
        set;
    }

    [SerializeField]
    private Hand handPrefab;

    [SyncVar]
    public Hand controlledHand;

    public override void OnStartServer()
    {
        base.OnStartServer();

        GameManager.Instance.Players.Add(this);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        GameManager.Instance.Players.Remove(this);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner) return;

        Instance = this;

        ViewManager.Instance.Initialize();
    }

    [Server]
    public void StartGame()
    {
        //GameObject pawnPrefab = Addressables.LoadAssetAsync<GameObject>("Pawn").WaitForCompletion();
        //GameObject pawnInstance = Instantiate(pawnPrefab);
        //GameObject pawnInstance = Instantiate(pawnPrefab);

        playerID = GameManager.Instance.Players.IndexOf(this);
        print("Start Game");
        print("Player ID: " + playerID);
        print("Client ID: " + Owner.ClientId);

        Hand handInstance = Instantiate(handPrefab, Vector3.zero, Quaternion.identity);

        controlledHand = handInstance;
        handInstance.controllingPlayer = this;
        handInstance.playerID = playerID;
        Spawn(handInstance.gameObject, Owner);

        TargetRenderHand(Owner,handInstance, playerID);

    }

    [Server]
    public void StopGame()
    {
        //if (controlledPawn != null) controlledPawn.Despawn();
    }

    [Server]
    public void BeginTurn()
    {
        TargetBeginTurn(Owner, GameManager.Instance.Turn == GameManager.Instance.Players.IndexOf(this));
    }

    [TargetRpc]
    private void TargetBeginTurn(NetworkConnection networkConnection, bool canPlay)
    {
        if (canPlay)
        {
            ViewManager.Instance.Show<MainView>();
        }
        else
        {
            ViewManager.Instance.Show<WaitView>();
        }
    }

    [TargetRpc]
    private void TargetRenderHand(NetworkConnection networkConnection, Hand hand, int id)
    {
        print("Render Hand");
        print($"Owner ID: {Owner.ClientId}");
        print($"Client ID: {networkConnection.ClientId}");
        print($"Player ID: {id}");

        print(controlledHand.playerID);

        if (!IsOwner) print("not owner");
        if (hand == null) print("hand is null");

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) print("Canvas not found");

        hand.transform.SetParent(canvas.transform, false);

    }

    public void CreateHand()
    {
        print(IsOwner);
        print("CreateHand");
        TargetCreateHand(Owner);
    }

    [TargetRpc]
    private void TargetCreateHand(NetworkConnection owner)
    {
        print(IsOwner);
        print("ServerCreateHand");
        GameObject canvas = GameObject.Find("Canvas");

        Hand handInstance = Instantiate(handPrefab, Vector3.zero, Quaternion.identity);
        handInstance.transform.SetParent(canvas.transform, false);

        controlledHand = handInstance;

        Spawn(handInstance.gameObject, owner);
    }
}
