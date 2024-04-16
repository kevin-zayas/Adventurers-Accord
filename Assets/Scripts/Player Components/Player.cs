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

    [field: SerializeField]
    [field: SyncVar]
    public bool IsStartingPlayer { get; private set; }

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
        IsStartingPlayer = GameManager.Instance.Players.Count == 0;

        GameManager.Instance.Players.Add(this);
        Gold = GameManager.Instance.StartingGold;
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
    public void SetIsPlayerTurn(bool value)
    {
        IsPlayerTurn = value;
    }

    [Server]
    public void UpdatePlayerView()
    {
        print("UpdatePlayerView");
        print(GameManager.Instance.CurrentPhase);
        TargetUpdatePlayerView(Owner, IsPlayerTurn, GameManager.Instance.CurrentPhase);
    }

    [TargetRpc]
    private void TargetUpdatePlayerView(NetworkConnection networkConnection, bool isPlayerTurn, GameManager.Phase currentPhase)
    {
        switch (currentPhase) 
        {            
            case GameManager.Phase.Draft:
            case GameManager.Phase.Dispatch:

                if (isPlayerTurn) ViewManager.Instance.Show<MainView>();
                else ViewManager.Instance.Show<WaitView>();

                break;

            case GameManager.Phase.Resolution:
                if (isPlayerTurn) ViewManager.Instance.Show<ResolutionView>();
                else ViewManager.Instance.Show<WaitView>();
                break;

            case GameManager.Phase.Magic:
                ViewManager.Instance.Show<EndRoundView>();
                GameObject.Find("EndRoundView").GetComponent<EndRoundView>().playerID = PlayerID;
                break;

            case GameManager.Phase.GameOver:
                ViewManager.Instance.Show<ResolutionView>();        //blank view with no buttons
                break;
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

    [Server]
    public void ChangePlayerGold(int value)
    {
        this.Gold += value;
        GameManager.Instance.Scoreboard.UpdatePlayerGold(PlayerID, this.Gold);
        ObserversUpdateGoldText(this.Gold);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerChangePlayerGold(int value)
    {
        ChangePlayerGold(value);
    }

    [Server]
    public void ChangePlayerReputation(int value)
    {
        this.Reputation += value;
        GameManager.Instance.Scoreboard.UpdatePlayerReputation(PlayerID, this.Reputation);
        ObserversUpdateReputationText(this.Reputation);

        if (Reputation >= GameManager.Instance.ReputationGoal) GameManager.Instance.SetPhaseGameOver();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerChangePlayerReputation(int value)
    {
        ChangePlayerReputation(value);
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
}
