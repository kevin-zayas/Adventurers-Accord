using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

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

    //[SerializeField]
    //private TutorialPawn pawnPrefab;

    //[SyncVar]
    //public TutorialPawn controlledPawn;

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

        int playerIndex = GameManager.Instance.Players.IndexOf(this);

        //Transform spawnPoint = TutorialBoard.Instance.Tiles[0].PawnPositions[playerIndex];

        //TutorialPawn pawnInstance = Instantiate(pawnPrefab, spawnPoint.position, Quaternion.identity);

        //controlledPawn = pawnInstance;

        //controlledPawn.controllingPlayer = this;

        //Spawn(pawnInstance.gameObject, Owner);
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
}
