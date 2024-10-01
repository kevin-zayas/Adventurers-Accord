using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static Player Instance { get; private set; }
    public readonly SyncVar<int> PlayerID = new();
    public readonly SyncVar<bool> IsPlayerTurn = new();
    public readonly SyncVar<bool> IsStartingPlayer = new();
    public readonly SyncVar<int> Gold = new();
    public readonly SyncVar<int> Reputation = new();
    public readonly SyncVar<bool> IsReady = new();

    [SerializeField] private Hand handPrefab;

    public readonly SyncVar<Hand> controlledHand = new();

    public override void OnStartServer()
    {
        base.OnStartServer();
        IsStartingPlayer.Value = GameManager.Instance.Players.Count == 0;

        GameManager.Instance.Players.Add(this);
        Gold.Value = GameManager.Instance.StartingGold;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        GameManager.Instance.Players.Remove(this);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        //controlledHand.OnChange += RenderHand;
        if (!IsOwner) return;

        Instance = this;
        ViewManager.Instance.Initialize();
    }

    [Server]
    public void StartGame()
    {
        PlayerID.Value = GameManager.Instance.Players.IndexOf(this);
        print("Start Game");
        print("Player ID: " + PlayerID.Value);
        print("Client ID: " + Owner.ClientId);

        Hand newHandObject = Instantiate(handPrefab, Vector2.zero, Quaternion.identity);
        Spawn(newHandObject.gameObject, Owner);

        controlledHand.Value = newHandObject;
        newHandObject.controllingPlayer.Value = this;
        newHandObject.playerID.Value = PlayerID.Value;
        ObserversRenderHand(newHandObject);

        ObserversUpdateGoldText(this.Gold.Value);
    }

    [Server]
    public void StopGame()
    {
        //if (controlledPawn != null) controlledPawn.Despawn();
    }

    [ObserversRpc]
    public void ObserversRenderHand(Hand hand)
    {
        if (!IsOwner)
        {
            hand.GetComponent<BoxCollider2D>().enabled = false;
            hand.gameObject.SetActive(false);
            return;
        }

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) print("Canvas not found");

        hand.transform.SetParent(canvas.transform, false);
    }

    [Server]
    public void SetIsPlayerTurn(bool value)
    {
        IsPlayerTurn.Value = value;
    }

    [ServerRpc]
    public void TogglePlayerIsReady()
    {
        IsReady.Value = !IsReady.Value;
    }

    [Server]
    public void UpdatePlayerView()
    {
        TargetUpdatePlayerView(Owner, IsPlayerTurn.Value, GameManager.Instance.CurrentPhase.Value);
    }

    [TargetRpc]
    private void TargetUpdatePlayerView(NetworkConnection networkConnection, bool isPlayerTurn, GameManager.Phase currentPhase)
    {
        switch (currentPhase) 
        {            
            case GameManager.Phase.Recruit:
            case GameManager.Phase.Dispatch:

                if (isPlayerTurn) ViewManager.Instance.Show<MainView>();
                else ViewManager.Instance.Show<WaitView>();

                if (currentPhase == GameManager.Phase.Recruit) ViewManager.Instance.EnableRecruitUI();
                else ViewManager.Instance.EnableQuestUI();

                break;

            case GameManager.Phase.Resolution:
                if (isPlayerTurn) ViewManager.Instance.Show<ResolutionView>();
                else ViewManager.Instance.Show<WaitView>();
                break;

            case GameManager.Phase.Magic:
                ViewManager.Instance.Show<EndRoundView>();
                GameObject.Find("EndRoundView").GetComponent<EndRoundView>().playerID = PlayerID.Value;
                break;

            case GameManager.Phase.GameOver:
                ViewManager.Instance.Show<ResolutionView>();        //blank view with no buttons
                break;
        }
    }

    [Server]
    public void ChangePlayerGold(int value)
    {
        this.Gold.Value += value;
        GameManager.Instance.Scoreboard.UpdatePlayerGold(PlayerID.Value, this.Gold.Value);
        ObserversUpdateGoldText(this.Gold.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerChangePlayerGold(int value)
    {
        ChangePlayerGold(value);
    }

    [Server]
    public void ChangePlayerReputation(int value)
    {
        this.Reputation.Value += value;
        GameManager.Instance.Scoreboard.UpdatePlayerReputation(PlayerID.Value, this.Reputation.Value);
        ObserversUpdateReputationText(this.Reputation.Value);

        if (Reputation.Value >= GameManager.Instance.ReputationGoal) GameManager.Instance.SetPhaseGameOver();
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
