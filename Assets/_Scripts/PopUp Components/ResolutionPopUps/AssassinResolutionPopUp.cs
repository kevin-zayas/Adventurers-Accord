using FishNet.Object;

public class AssassinResolutionPopUp : ResolutionPopUp
{
    private string assassinConfirmStatText;

    public override void SetConfirmSelectionState(AdventurerCard card)
    {
        base.SetConfirmSelectionState(card);
        message.text = string.Format(confirmSelectionText, card.CardName.Value);

        rightButton.onClick.AddListener(() =>
        {
            int questIndex = QuestLocation.QuestLocationIndex;
            
            if (card.PhysicalPower.Value > 0 && card.MagicalPower.Value == 0) card.ServerApplyPoison(true, false);
            else if (card.PhysicalPower.Value == 0 && card.MagicalPower.Value > 0) card.ServerApplyPoison(false, true);
            else if (card.PhysicalPower.Value > 0 && card.MagicalPower.Value > 0)
            {
                SetAssassinConfirmStatPopupState(card);
                return;
            }

            HandleEndOfResolution(questIndex, card);
        });
    }

    private void SetAssassinConfirmStatPopupState(AdventurerCard card)
    {
        leftButton.onClick.RemoveAllListeners();
        rightButton.onClick.RemoveAllListeners();

        leftButtonText.text = "Physical";
        rightButtonText.text = "Magical";

        message.text = string.Format(assassinConfirmStatText, card.CardName.Value);
        int questIndex = QuestLocation.QuestLocationIndex;

        leftButton.onClick.AddListener(() =>
        {
            card.ServerApplyPoison(true,false);
            HandleEndOfResolution(questIndex, card);
        });

        rightButton.onClick.AddListener(() =>
        {
            card.ServerApplyPoison(false, true);
            HandleEndOfResolution(questIndex, card);
        });
    }

    protected override void SetPopUpText()
    {
        titleText = "Poison Expert";
        defaultMessageText = "Choose an Adventurer on this Quest to poison.";
        confirmSelectionText = "Are you sure you want to poison this {0}?";
        buttonText = "Poison";
        assassinConfirmStatText = "Would you like to target this {0}'s Physical or Magical Power?";
    }

    protected override void UpdateGuildBonusTracker(int questIndex)
    {
        print($"Guild Type: {Player.Instance.GuildType}");
        if (Player.Instance.GuildType == CardDatabase.GuildType.AsassinsGuild)
        {
            Player.Instance.ServerUpdateGuildBonusTracker(questIndex, "poisonedAdventurers");
        }
    }
}
