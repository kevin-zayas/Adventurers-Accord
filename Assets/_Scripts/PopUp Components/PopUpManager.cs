using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PopUpManager : NetworkBehaviour
{
    public static PopUpManager Instance { get; private set; }
    
    [SerializeField] CreditsPopUp CreditsPopUpPrefab;
    [SerializeField] GameOverPopUp GameOverPopUpPrefab;
    [SerializeField] HowToPlayPopUp HowToPlayPopUpPrefab;
    [SerializeField] ResolutionPopUp AssassinResolutionPopUpPrefab;
    [SerializeField] ResolutionPopUp ClericResolutionPopUpPrefab;
    [SerializeField] ResolutionPopUp RogueResolutionPopUpPrefab;
    [SerializeField] RoundSummaryPopUp RoundSummaryPopUpPrefab;
    [SerializeField] ConfirmationPopUp ConfirmationPopUpPrefab;
    [SerializeField] ConfirmationPopUp EquipConfirmationPopUpPrefab;
    [SerializeField] SettingsPopUp SettingsPopUpPrefab;
    [SerializeField] ScoreBoardPopUp ScoreBoardPopUpPrefab;
    [SerializeField] GuildRosterPopUp GuildRosterPopUpPrefab;
    [SerializeField] GuildRosterPopUp RivalGuildRosterPopUpPrefab;

    public readonly SyncVar<ResolutionPopUp> CurrentResolutionPopUp = new();
    public readonly SyncVar<string> CurrentResolutionType = new();
    public readonly SyncVar<GameOverPopUp> GameOverPopUpInstance = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    print("Q pressed");
        //}
    }

    [Server]
    public ResolutionPopUp CreateResolutionPopUp(string cardName)
    {

        ResolutionPopUp popUp;
        if (cardName == "Assassin") popUp = Instantiate(AssassinResolutionPopUpPrefab);
        else if (cardName == "Rogue") popUp = Instantiate(RogueResolutionPopUpPrefab);
        else if (cardName == "Cleric") popUp = Instantiate(ClericResolutionPopUpPrefab);
        else
        {
            Debug.LogError($"No Resolution PopUp prefab found for card name: {cardName}");
            return null;
        }
        CurrentResolutionPopUp.Value = popUp;
        CurrentResolutionType.Value = cardName;
        return popUp;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerDespawnResolutionPopUp(ResolutionPopUp popUp)
    {
        Despawn(popUp.gameObject);
    }

    [Server]
    public RoundSummaryPopUp CreateRoundSummaryPopUp()
    {
        RoundSummaryPopUp popUp = Instantiate(RoundSummaryPopUpPrefab);
        return popUp;
    }

    [Server]
    public void CloseRoundSummaryPopUp(NetworkConnection networkConnection, GameObject popUp, bool despawn)
    {
        TargetCloseRoundSummaryPopUp(networkConnection, popUp);

        if (despawn)
        {
            Despawn(popUp);
        }
    }

    [TargetRpc]
    public void TargetCloseRoundSummaryPopUp(NetworkConnection networkConnection, GameObject popUp)
    {
        //if (IsServer) return;
        popUp.SetActive(false);

        if (GameManager.Instance.CurrentPhase.Value == GameManager.Phase.GameOver)
        {
            Player player = GameManager.Instance.Players[LocalConnection.ClientId];
            this.GameOverPopUpInstance.Value.ServerInitializeGameOverPopup(networkConnection, player);
        }
    }

    [Server]
    public GameOverPopUp CreateGameOverPopUp()
    {
        GameOverPopUp popUp = Instantiate(GameOverPopUpPrefab);
        GameOverPopUpInstance.Value = popUp;
        return popUp;
    }

    public ConfirmationPopUp CreateConfirmationPopUp()
    {
        ConfirmationPopUp popUp = Instantiate(ConfirmationPopUpPrefab);
        return popUp;
    }

    public HowToPlayPopUp CreateHowToPlayPopUp()
    {
        HowToPlayPopUp howToPlayPopUp = Instantiate(HowToPlayPopUpPrefab);
        return howToPlayPopUp;
    }

    public CreditsPopUp CreateCreditsPopUp()
    {
        CreditsPopUp creditsPopUp = Instantiate(CreditsPopUpPrefab);
        return creditsPopUp;
    }

    public SettingsPopUp CreateSettingsPopUp()
    {
        SettingsPopUp settingsPopUp = Instantiate(SettingsPopUpPrefab);
        return settingsPopUp;
    }

    [Server]
    public ScoreBoardPopUp CreateScoreBoardPopUp()
    {
        ScoreBoardPopUp scoreBoardPopUp = Instantiate(ScoreBoardPopUpPrefab);
        return scoreBoardPopUp;
    }

    [Server]
    public GuildRosterPopUp CreateGuildRosterPopUp(bool isViewingRival)
    {
        GuildRosterPopUp guildRosterPopUp;

        if (isViewingRival) guildRosterPopUp = Instantiate(RivalGuildRosterPopUpPrefab);
        else guildRosterPopUp = Instantiate(GuildRosterPopUpPrefab);

        return guildRosterPopUp;
    }
}
