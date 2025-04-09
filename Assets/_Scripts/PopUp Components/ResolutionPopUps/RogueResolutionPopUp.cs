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
        defaultMessageText = "Please choose an Adventurer on this Quest to \"borrow\" an item from.";
        confirmSelectionText = "Are you sure you want to \"borrow\" this {0}'s {1}?";
        confirmCloseText = "Are you sure you don't want to \"borrow\" an item this round?";
        buttonText = "\"Borrow\"";
    }

    [ServerRpc(RequireOwnership = false)]
    protected override void ServerUpdateGuildBonusTracker(int playerID, int questIndex)
    {
        Player player = GameManager.Instance.Players[playerID];
        if (player.isThievesGuild)
        {
            player.GuildBonusTracker[questIndex]["stolenItems"]++;
        }
    }
}
