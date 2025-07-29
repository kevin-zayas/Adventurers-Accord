public class AssassinResolutionPopUp : ResolutionPopUp
{
    private string assassinConfirmStatText;
    protected override string ResolutionType => "Assassin";

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
            card.ServerApplyPoison(true, false);
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

    protected override void IsResolutionClickValid(AdventurerCard card)
    {
        bool isPlayerCard = card.ControllingPlayer.Value == Player.Instance;
        bool isBlessed = card.IsBlessed.Value;
        bool hasNoPower = card.MagicalPower.Value <= 0 && card.PhysicalPower.Value <= 0;

        if (isPlayerCard)
        {
            PopUpManager.Instance.CreateToastPopUp("Invalid target: This Adventurer belongs to you.");
        }
        else if (isBlessed)
        {
            PopUpManager.Instance.CreateToastPopUp("Invalid target: This Adventurer is protected by Divine Blessing.");
        }
        else if (hasNoPower)
        {
            PopUpManager.Instance.CreateToastPopUp("Invalid target: This Adventurer has no remaining Power.");
        }
        else
        {
            PopUpManager.Instance.CurrentResolutionPopUp.SetConfirmSelectionState(card);
        }
    }
}
