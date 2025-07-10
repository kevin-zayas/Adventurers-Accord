using FishNet.Object;

public class ClericResolutionPopUp : ResolutionPopUp
{
    public override void SetConfirmSelectionState(AdventurerCard card)
    {
        base.SetConfirmSelectionState(card);
        message.text = string.Format(confirmSelectionText, card.CardName.Value);

        rightButton.onClick.AddListener(() =>
        {
            int questIndex = QuestLocation.QuestLocationIndex;

            card.ServerGrantDivineBlessing();
            card.CurrentCardHolder.Value.QuestLane.ServerUpdateDrainEffects(card);
            HandleEndOfResolution(questIndex, card);
        });
    }

    protected override void SetPopUpText()
    {
        titleText = "Divine Blessing";
        defaultMessageText = "Please choose an Adventurer to bless.";
        confirmSelectionText = "Are you sure you want to bless this {0}?";
        buttonText = "Bless";
    }

    protected override void UpdateGuildBonusTracker(int questIndex)
    {
        return;
    }
}
