using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GameKit.Dependencies.Utilities;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class Player : NetworkBehaviour
{
    public static Player Instance { get; private set; }
    public readonly SyncVar<int> PlayerID = new();
    public readonly SyncVar<bool> IsPlayerTurn = new();
    public readonly SyncVar<bool> IsStartingPlayer = new();
    public readonly SyncVar<int> Gold = new();
    public readonly SyncVar<int> Reputation = new();
    [AllowMutableSyncTypeAttribute] public SyncVar<bool> IsReady = new();
    public readonly SyncVar<bool> CanStartGame = new();
    public readonly SyncVar<Hand> controlledHand = new();

    [SerializeField] private Hand handPrefab;
    [SerializeField] private AudioMixer masterMixer;

    [field: SerializeField] public SyncList<AdventurerCard> DiscardPile { get; } = new SyncList<AdventurerCard>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        IsStartingPlayer.Value = GameManager.Instance.Players.Count == 0;
        //print("new build uploaded");

        GameManager.Instance.Players.Add(this);
        GameManager.Instance.CanStartGame.Value = false;
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
        if (!IsOwner) return;

        Instance = this;
        ViewManager.Instance.Initialize();

        float volume = PlayerPrefs.GetFloat("SavedMasterVolume", 50);
        masterMixer.SetFloat("MasterVolume", Mathf.Log10(volume / 100) * 20f);
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
            hand.gameObject.transform.SetPosition(true, new Vector2(960,-300));
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

        if (IsReady.Value)
        {
            print("Player Is Ready");
            if (GameManager.Instance.Players.All(player => player.IsReady.Value))
            {
                print("All Players Ready");
                GameManager.Instance.SetCanStartGame(true);
            }
        }
        else
        {
            GameManager.Instance.SetCanStartGame(false);
        }
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
        ScoreBoard.Instance.ObserversUpdatePlayerGold(PlayerID.Value, this.Gold.Value);
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
        ScoreBoard.Instance.ObserversUpdatePlayerReputation(PlayerID.Value, this.Reputation.Value);
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
