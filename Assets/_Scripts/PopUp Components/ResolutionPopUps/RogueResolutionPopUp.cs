using FishNet.Object;

public class RogueResolutionPopUp : ResolutionPopUp
{
    public override void SetConfirmSelectionState(AdventurerCard card)
    {
        base.SetConfirmSelectionState(card);
        message.text = string.Format(confirmSelectionText, card.CardName.Value, card.Item.Value.CardName.Value);

        rightButton.onClick.AddListener(() =>
        {
            int questIndex = QuestLocation.QuestLocationIndex;
            
            card.ServerDisableItem("Stolen");
            HandleEndOfResolution(questIndex, card);
        });
    }

    protected override void SetPopUpText()
    {
        titleText = "Sticky Fingers";
        defaultMessageText = "Choose an Adventurer to steal a Magic Item from.";
        confirmSelectionText = "Are you sure you want to steal this {0}'s {1}?";
        buttonText = "Steal";
    }

    protected override void UpdateGuildBonusTracker(int questIndex)
    {
        print($"Guild Type: {Player.Instance.GuildType}");
        if (Player.Instance.GuildType == CardDatabase.GuildType.ThievesGuild)
        {
            Player.Instance.ServerUpdateGuildBonusTracker(questIndex, "stolenItems");
        }
    }
}
