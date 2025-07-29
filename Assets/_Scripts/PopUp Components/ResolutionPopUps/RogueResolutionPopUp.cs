public class RogueResolutionPopUp : ResolutionPopUp
{
    protected override string ResolutionType => "Rogue";

    public override void SetConfirmSelectionState(AdventurerCard card)
    {
        base.SetConfirmSelectionState(card);
        message.text = string.Format(confirmSelectionText, card.CardName.Value, card.Item.Value.CardName.Value);

        rightButton.onClick.AddListener(() =>
        {
            int questIndex = QuestLocation.QuestLocationIndex;

            card.ServerDisableItem();
            HandleEndOfResolution(questIndex, card);
        });
    }

    protected override void SetPopUpText()
    {
        titleText = "Saboteur";
        defaultMessageText = "Choose an Adventurer with a Magic Item to disable.";
        confirmSelectionText = "Are you sure you want to disable this {0}'s {1}?";
        buttonText = "Disable";
    }

    protected override void UpdateGuildBonusTracker(int questIndex)
    {
        print($"Guild Type: {Player.Instance.GuildType}");
        if (Player.Instance.GuildType == CardDatabase.GuildType.ThievesGuild)
        {
            Player.Instance.ServerUpdateGuildBonusTracker(questIndex, "disabledItems");
        }
    }

    protected override void IsResolutionClickValid(AdventurerCard card)
    {
        bool isPlayerCard = card.ControllingPlayer.Value == Player.Instance;
        bool hasNoItem = !card.HasItem.Value;
        bool itemIsDisabled = card.HasItem.Value && card.Item.Value.IsDisabled.Value;

        if (isPlayerCard)
        {
            PopUpManager.Instance.CreateToastPopUp("Invalid target: This Adventurer belongs to you.");
        }
        else if (hasNoItem)
        {
            PopUpManager.Instance.CreateToastPopUp("Invalid target: This Adventurer has no Magic Item equipped.");
        }
        else if (itemIsDisabled)
        {
            PopUpManager.Instance.CreateToastPopUp("Invalid target: This Adventurer's Magic Item is already disabled.");
        }
        else
        {
            PopUpManager.Instance.CurrentResolutionPopUp.SetConfirmSelectionState(card);
        }
    }
}
