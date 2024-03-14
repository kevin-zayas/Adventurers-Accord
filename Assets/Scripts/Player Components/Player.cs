using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class Player : NetworkBehaviour
{
    public static Player Instance { get; private set; }

    [field: SyncVar]
    public int PlayerID { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool IsPlayerTurn { get; private set; }

    [field: SyncVar]
    public int Gold { get; private set; }

    [field: SyncVar]
    public int Reputation { get; private set; }

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
        Gold = 25;
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
        PlayerID = GameManager.Instance.Players.IndexOf(this);
        print("Start Game");
        print("Player ID: " + PlayerID);
        print("Client ID: " + Owner.ClientId);

        Hand handInstance = Instantiate(handPrefab, Vector3.zero, Quaternion.identity);

        controlledHand = handInstance;
        handInstance.controllingPlayer = this;
        handInstance.playerID = PlayerID;
        Spawn(handInstance.gameObject, Owner);

        ObserversUpdateGoldText(this.Gold);
    }

    [Server]
    public void StopGame()
    {
        //if (controlledPawn != null) controlledPawn.Despawn();
    }

    [Server]
    public void BeginTurn()
    {   
        //if (GameManager.Instance.CurrentPhase == GameManager.Phase.Magic) TargetBeginTurn(Owner, true);
        //else TargetBeginTurn(Owner, GameManager.Instance.Turn == GameManager.Instance.Players.IndexOf(this));

        TargetBeginTurn(Owner, GameManager.Instance.Turn == GameManager.Instance.Players.IndexOf(this));
    }

    [Server]
    public void BeginEndRound()
    {
        TargetBeginEndRound(Owner);
    }

    [TargetRpc]
    public void TargetBeginEndRound(NetworkConnection networkConnection)
    {

        ViewManager.Instance.Show<EndRoundView>();
        GameObject.Find("EndRoundView").GetComponent<EndRoundView>().playerID = LocalConnection.ClientId;
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

    [ServerRpc(RequireOwnership = false)]
    public void ServerChangeGold(int value)
    {
        this.Gold += value;
        ObserversUpdateGoldText(this.Gold);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerChangeReputation(int value)
    {
        this.Reputation += value;
        ObserversUpdateReputationText(this.Reputation);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversUpdateGoldText(int gold)
    {
        if (!IsOwner) return;
        Board.Instance.goldText.text = $"{gold} GP";
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversUpdateReputationText(int reputation)
    {
        if (!IsOwner) return;
        Board.Instance.reputationText.text = $"{reputation} Rep.";
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetIsPlayerTurn(bool value)
    {
        SetIsPlayerTurn(value);
    }

    [Server]
    public void SetIsPlayerTurn(bool value)
    {
        IsPlayerTurn = value;
    }
}
