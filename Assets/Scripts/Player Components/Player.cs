using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.XR;

public class Player : NetworkBehaviour
{
    public static Player Instance { get; private set; }

    [SyncVar]
    public int playerID;

    [field: SyncVar]
    public int Gold { get; private set; }

    [SyncVar]
    public int reputation;

    [field: SyncVar]
    public bool IsReady
    {
        get;

        [ServerRpc(RequireOwnership = false)]   //alternate way to set IsReady
        set;
    }

    [SerializeField]
    private Hand handPrefab;

    [SyncVar(OnChange =nameof(RenderHand))]
    public Hand controlledHand;

    public override void OnStartServer()
    {
        base.OnStartServer();

        GameManager.Instance.Players.Add(this);
        Gold = 10;
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
        playerID = GameManager.Instance.Players.IndexOf(this);
        print("Start Game");
        print("Player ID: " + playerID);
        print("Client ID: " + Owner.ClientId);

        Hand handInstance = Instantiate(handPrefab, Vector3.zero, Quaternion.identity);

        controlledHand = handInstance;
        handInstance.controllingPlayer = this;
        handInstance.playerID = playerID;
        Spawn(handInstance.gameObject, Owner);

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

    [ServerRpc]
    public void ServerChangeGold(int value)
    {
        this.Gold += value;
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

    public void RenderHand(Hand prevHand, Hand newHand, bool asSever)
    {
        if (asSever) return;

        if (!IsOwner)
        {
            newHand.GetComponent<BoxCollider2D>().enabled = false;
            return;
        }

        if (controlledHand == null) print("hand is null");

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) print("Canvas not found");

        controlledHand.transform.SetParent(canvas.transform, false);
    }
}
