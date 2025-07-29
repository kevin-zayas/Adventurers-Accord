public class ClericResolutionPopUp : ResolutionPopUp
{
    protected override string ResolutionType => "Cleric";

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

    protected override void IsResolutionClickValid(AdventurerCard card)
    {
        bool isBlessed = card.IsBlessed.Value;

        if (isBlessed)
        {
            PopUpManager.Instance.CreateToastPopUp("Invalid target: This Adventurer is already protected by Divine Blessing.");
        }
        else
        {
            PopUpManager.Instance.CurrentResolutionPopUp.SetConfirmSelectionState(card);
        }
    }
}
