using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
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
    [SerializeField] PotionResolutionPopUp PotionResolutionPopUpPrefab;
    [SerializeField] RoundSummaryPopUp RoundSummaryPopUpPrefab;
    [SerializeField] QuestSummaryPopUp QuestSummaryPopUpPrefab;
    [SerializeField] ConfirmationPopUp ConfirmationPopUpPrefab;
    [SerializeField] ConfirmationPopUp EquipConfirmationPopUpPrefab;
    [SerializeField] SettingsPopUp SettingsPopUpPrefab;
    [SerializeField] ScoreBoardPopUp ScoreBoardPopUpPrefab;
    [SerializeField] GuildRosterPopUp GuildRosterPopUpPrefab;
    [SerializeField] GuildRosterPopUp RivalGuildRosterPopUpPrefab;
    [SerializeField] GuildRecapPopUp GuildRecapPopUpPrefab;
    [SerializeField] ToastPopUp ToastPopUpPrefab;
    [SerializeField] AdventurerRegistryPopUp AdventurerRegistryPopUpPrefab;

    public ResolutionPopUp CurrentResolutionPopUp { get; private set; }
    public string CurrentResolutionType { get; private set; }
    public Player ResolvingPlayer { get; private set; }
    private PotionCard currentResolutionPotion;

    private void Awake()
    {
        Instance = this;
    }

    [TargetRpc]
    public void CreateResolutionPopUp(NetworkConnection connection, string cardName, QuestLocation questLocation)
    {
        ResolutionPopUp popUp;
        if (cardName == "Assassin") popUp = Instantiate(AssassinResolutionPopUpPrefab);
        else if (cardName == "Rogue") popUp = Instantiate(RogueResolutionPopUpPrefab);
        else if (cardName == "Cleric") popUp = Instantiate(ClericResolutionPopUpPrefab);
        else
        {
            Debug.LogError($"No Resolution PopUp prefab found for card name: {cardName}");
            return;
        }
        popUp.InitializePopUp(questLocation, cardName);
        CurrentResolutionPopUp = popUp;
        CurrentResolutionType = cardName;
    }

    public void CreatePotionResolutionPopUp(PotionCard potionCard, QuestLocation questLocation)
    {
        DestroyCurrentPotionResolutionPopUp();

        PotionResolutionPopUp popUp = Instantiate(PotionResolutionPopUpPrefab);
        popUp.InitializePopUp(questLocation, potionCard);
        currentResolutionPotion = potionCard;
        CurrentResolutionPopUp = popUp;
        CurrentResolutionType = potionCard.CardName.Value;
        ResolvingPlayer = potionCard.ControllingPlayer.Value;
    }

    public void DestroyCurrentPotionResolutionPopUp()
    {
        if (currentResolutionPotion != null)
        {
            currentResolutionPotion.transform.SetParent(Player.Instance.ControlledHand.Value.transform, false);
            currentResolutionPotion.gameObject.SetActive(true);
            Destroy(CurrentResolutionPopUp.gameObject);
            ClearResolutionType();
        }
    }

    public void ClearResolutionType()
    {
        currentResolutionPotion = null;
        CurrentResolutionPopUp = null;
        CurrentResolutionType = null;
        ResolvingPlayer = null;
    }

    [ObserversRpc]
    public void CreateRoundSummaryPopUp(Dictionary<int, PlayerRoundSummaryData> playerSummaries, List<QuestSummaryData> questSummaries)
    {
        RoundSummaryPopUp popUp = Instantiate(RoundSummaryPopUpPrefab);
        popUp.InitializeRoundSummaryPopUp(playerSummaries, questSummaries);
    }

    public void CreateQuestSummaryPopUp(List<QuestSummaryData> questSummaries)
    {
        QuestSummaryPopUp popUp = Instantiate(QuestSummaryPopUpPrefab);
        popUp.InitializeQuestSummaryPopUp(questSummaries);
    }

    public void CreateGameOverPopUp()
    {
        GameOverPopUp popUp = Instantiate(GameOverPopUpPrefab);
        popUp.InitializeGameOverPopUp();
    }

    public void CreateGuildRecapPopUp(Player player)
    {
        GuildRecapPopUp popUp = Instantiate(GuildRecapPopUpPrefab);
        popUp.InitializeGuildRecapPopUp(player);
    }

    public void CreateToastPopUp(string message)
    {
        ToastPopUp popUp = Instantiate(ToastPopUpPrefab);
        popUp.InitializeToastPopUp(message);
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
    public void CreateScoreBoardPopUp(NetworkConnection connection)
    {
        ScoreBoardPopUp scoreBoardPopUp = Instantiate(ScoreBoardPopUpPrefab);
        Spawn(scoreBoardPopUp.gameObject);
        scoreBoardPopUp.TargetInitializeScoreboard(connection);
        ScoreBoard.Instance.TargetSetScoreBoardPopUp(connection, scoreBoardPopUp);
    }

    [Server]
    public void CreateGuildRosterPopUp(NetworkConnection connection, Player player, bool isViewingRival)
    {
        GuildRosterPopUp guildRosterPopUp;
        if (isViewingRival) guildRosterPopUp = Instantiate(RivalGuildRosterPopUpPrefab);
        else guildRosterPopUp = Instantiate(GuildRosterPopUpPrefab);

        Spawn(guildRosterPopUp.gameObject);
        guildRosterPopUp.TargetInitializeGuildRoster(connection, player, isViewingRival);
        ScoreBoard.Instance.TargetSetGuildRosterPopUp(connection, guildRosterPopUp);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerCreateGuildRosterPopUp(NetworkConnection connection, Player player, bool isViewingRival)
    {
        CreateGuildRosterPopUp(connection, player, isViewingRival);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerCreateAdventurerRegistryPopUp(NetworkConnection connection)
    {
        AdventurerRegistryPopUp popUp = Instantiate(AdventurerRegistryPopUpPrefab);
        Spawn(popUp.gameObject);
        popUp.TargetInitializeAdventurerRegistry(connection);
    }
}
