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
            
            if (card.PhysicalPower.Value > 0 && card.MagicalPower.Value == 0) card.ServerChangePhysicalPower(-2);
            else if (card.PhysicalPower.Value == 0 && card.MagicalPower.Value > 0) card.ServerChangeMagicalPower(-2);
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
            card.ServerChangePhysicalPower(-2);
            HandleEndOfResolution(questIndex, card);
        });

        rightButton.onClick.AddListener(() =>
        {
            card.ServerChangeMagicalPower(-2);
            HandleEndOfResolution(questIndex, card);
        });
    }

    protected override void SetPopUpText()
    {
        titleText = "Poisonous Blade";
        defaultMessageText = "Please choose an Adventurer on this Quest to poison.";
        confirmSelectionText = "Are you sure you want to poison this {0}?";
        confirmCloseText = "Are you sure you don't want to poison an Adventurer this round?";
        buttonText = "Poison";
        assassinConfirmStatText = "Would you like to target this {0}'s Physical or Magical Power?";
    }

    [ServerRpc(RequireOwnership = false)]
    protected override void ServerUpdateGuildBonusTracker(int playerID, int questIndex)
    {
        Player player = GameManager.Instance.Players[playerID];
        if (player.isAssassinsGuild)
        {
            player.GuildBonusTracker[questIndex]["poisonedAdventurers"]++;
        }
    }
}
