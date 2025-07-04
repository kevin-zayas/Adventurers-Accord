using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GameKit.Dependencies.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using static CardDatabase;
using static GameManager;

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
    public readonly SyncVar<Hand> ControlledHand = new();
    public int DispatchedAdventurerCount { get; private set; }

    [SerializeField] private Hand handPrefab;
    [SerializeField] private AudioMixer masterMixer;

    public List<AdventurerCard> DiscardPile { get; } = new List<AdventurerCard>();

    public GuildType GuildType { get; private set; }
    public Dictionary<int, Dictionary<string, int>> GuildBonusTracker { get; private set; }
    public readonly SyncDictionary<string, int> GuildRecapTracker = new();

    public bool IsThievesGuild { get; private set; }
    public bool IsMagesGuild { get; private set; }
    public bool IsFightersGuild { get; private set; }
    public bool IsMerchantsGuild { get; private set; }
    public bool IsAssassinsGuild { get; private set; }
    public bool IsAnimating { get; private set; }

    public override void OnStartServer()
    {
        base.OnStartServer();
        IsStartingPlayer.Value = GameManager.Instance.Players.Count == 0;

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

        Hand newHandObject = Instantiate(handPrefab, Vector2.zero, Quaternion.identity);
        Spawn(newHandObject.gameObject, Owner);

        ControlledHand.Value = newHandObject;
        newHandObject.controllingPlayer.Value = this;
        newHandObject.playerID.Value = PlayerID.Value;

        ObserversRenderHand(newHandObject);
        ObserversUpdateGoldText(this.Gold.Value);
        InitializeGuildBonusTracker();

        switch (GuildType)
        {
            case GuildType.ThievesGuild:
                IsThievesGuild = true;
                break;
            case GuildType.MagesGuild:
                IsMagesGuild = true;
                break;
            case GuildType.FightersGuild:
                IsFightersGuild = true;
                break;
            case GuildType.MerchantsGuild:
                IsMerchantsGuild = true;
                break;
            case GuildType.AsassinsGuild:
                IsAssassinsGuild = true;
                break;
            default:
                throw new System.Exception($"GuildType : {GuildType} is not valid");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetGuildType(GuildType guildType)
    {
        GuildType = guildType;
        ObserversSetGuildType(guildType);

    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversSetGuildType(GuildType guildType)
    {
        GuildType = guildType;
    }

    [Server]
    public void InitializeGuildBonusTracker()
    {
        DispatchedAdventurerCount = 0;
        GuildBonusTracker = new()
        {
            { 0, new Dictionary<string, int>() },
            { 1, new Dictionary<string, int>() },
            { 2, new Dictionary<string, int>() }
        };

        for (int i = 0; i < 3; i++)
        {
            switch (GuildType)
            {
                case CardDatabase.GuildType.ThievesGuild:
                    GuildBonusTracker[i].Add("didCompleteQuest", 0);
                    GuildBonusTracker[i].Add("disabledItems", 0);
                    break;
                case CardDatabase.GuildType.MagesGuild:
                    GuildBonusTracker[i].Add("spellsPlayed", 0);
                    break;
                case CardDatabase.GuildType.FightersGuild:
                    GuildBonusTracker[i].Add("physAdventurers", 0);
                    GuildBonusTracker[i].Add("mostPhysPower", 1);
                    break;
                case CardDatabase.GuildType.MerchantsGuild:
                    GuildBonusTracker[i].Add("magicItemsDispatched", 0);
                    break;
                case CardDatabase.GuildType.AsassinsGuild:
                    GuildBonusTracker[i].Add("curseSpellsPlayed", 0);
                    GuildBonusTracker[i].Add("poisonedAdventurers", 0);
                    break;
                default:
                    //throw an exception stating that the Guildtype
                    throw new System.Exception($"GuildType : {GuildType} is not valid");
            }
        }
    }

    [Server]
    public void UpdateGuildRecapTracker(string eventType, int value)
    {
        if (GuildRecapTracker.ContainsKey(eventType))
        {
            GuildRecapTracker[eventType] += value;
        }
        else
        {
            GuildRecapTracker.Add(eventType, value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUpdateGuildRecapTracker(string eventType, int value)
    {
        UpdateGuildRecapTracker(eventType, value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUpdateGuildBonusTracker(int questIndex, string bonusTracker)
    {
        GuildBonusTracker[questIndex][bonusTracker]++;
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
            hand.gameObject.transform.SetPosition(true, new Vector2(960, -300));
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
    public void TogglePlayerIsReady(bool? isReady = null)
    {
        IsReady.Value = isReady ?? !IsReady.Value;

        SyncList<Player> players = GameManager.Instance.Players;
        if (IsReady.Value && players.All(player => player.IsReady.Value))
        {
            GameManager.Instance.SetCanStartGame(true);
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
    private void TargetUpdatePlayerView(NetworkConnection networkConnection, bool isPlayerTurn, Phase currentPhase)
    {
        switch (currentPhase)
        {
            case Phase.Recruit:
            case Phase.Dispatch:

                if (isPlayerTurn) ViewManager.Instance.Show<MainView>();
                else ViewManager.Instance.Show<WaitView>();

                ViewManager.Instance.MainView.UpdateOddJobsBanner(isPlayerTurn && currentPhase == Phase.Dispatch);

                if (currentPhase == Phase.Recruit) ViewManager.Instance.EnableRecruitUI();
                else ViewManager.Instance.EnableQuestUI();
                break;

            case Phase.Ability:
                if (isPlayerTurn) ViewManager.Instance.Show<ResolutionView>();
                else ViewManager.Instance.Show<WaitView>();
                break;

            case Phase.Magic:
                ViewManager.Instance.Show<EndRoundView>();
                break;

            case Phase.GameOver:
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
        Board.Instance.GuildStatusList[PlayerID.Value].SetGoldAmount(gold);

        if (!IsOwner) return;
        Board.Instance.goldText.text = gold.ToString();
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversUpdateReputationText(int reputation)
    {
        Board.Instance.GuildStatusList[PlayerID.Value].SetReputationAmount(reputation);

        if (!IsOwner) return;
        Board.Instance.reputationText.text = reputation.ToString();
    }

    public void SetIsAnimating(bool value)
    {
        IsAnimating = value;
    }

    [Server]
    public void UpdateDispatchedAdventurerCount(int value)
    {
        DispatchedAdventurerCount += value;
        TargetUpdateOddJobbsBanner(Owner, DispatchedAdventurerCount == 0);
    }

    [TargetRpc]
    private void TargetUpdateOddJobbsBanner(NetworkConnection connection, bool value)
    {
        ViewManager.Instance.MainView.UpdateOddJobsBanner(value);
    }
}
