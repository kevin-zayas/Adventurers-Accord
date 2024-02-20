using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TutorialPlayer : NetworkBehaviour
{
    public static TutorialPlayer Instance { get; private set; }

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
    private TutorialPawn pawnPrefab;

    [SyncVar]
    public TutorialPawn controlledPawn;

    public override void OnStartServer()
    {
        base.OnStartServer();

        TutorialGameManager.Instance.Players.Add(this);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        TutorialGameManager.Instance.Players.Remove(this);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner) return;

        Instance = this;

        TutorialViewManager.Instance.Initialize();
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

        int playerIndex = TutorialGameManager.Instance.Players.IndexOf(this);

        Transform spawnPoint = TutorialBoard.Instance.Tiles[0].PawnPositions[playerIndex];

        TutorialPawn pawnInstance = Instantiate(pawnPrefab, spawnPoint.position, Quaternion.identity);

        controlledPawn = pawnInstance;

        controlledPawn.controllingPlayer = this;

        Spawn(pawnInstance.gameObject, Owner);
    }

    [Server]
    public void StopGame()
    {
        if (controlledPawn != null) controlledPawn.Despawn();
    }

    [Server]
    public void BeginTurn()
    {
        TargetBeginTurn(Owner,TutorialGameManager.Instance.Turn == TutorialGameManager.Instance.Players.IndexOf(this));
    }

    [TargetRpc]
    private void TargetBeginTurn(NetworkConnection networkConnection, bool canPlay)
    {
        if (canPlay)
        {
            TutorialViewManager.Instance.Show<TutorialMainView>();
        }
        else
        {
            TutorialViewManager.Instance.Show<TutorialWaitView>();
        }
    }
}
