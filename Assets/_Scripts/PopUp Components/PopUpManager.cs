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

    public ResolutionPopUp CurrentResolutionPopUp;
    public string CurrentResolutionType;

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
