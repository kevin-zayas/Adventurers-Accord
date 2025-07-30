using static PotionCard;

public class PotionCardInteractionHandler : CardInteractionHandler
{
    /// <summary>
    /// Handles the specific logic when the drag operation ends, including item equipping validation.
    /// </summary>
    protected override void HandleEndDrag()
    {
        if (card.IsDraftCard.Value)
        {
            OnCardPurchase();
            return;
        }

        if (!IsEndDragValid()) return;

        AdventurerCard adventurerCard = dropZone.GetComponent<AdventurerCard>();

        if (adventurerCard.CurrentCardHolder.Value.IsQuest())
        {
            HandleQuestPotionUse(adventurerCard);
        }
        else
        {
            HandleHandPotionUse(adventurerCard);
        }
    }

    protected void HandleHandPotionUse(AdventurerCard adventurerCard)
    {
        PotionCard potionCard = card as PotionCard;
        ConfirmationPopUp popUp = PopUpManager.Instance.CreateConfirmationPopUp();
        popUp.InitializeUsePotionPopUp(adventurerCard, potionCard);
    }

    protected void HandleQuestPotionUse(AdventurerCard adventurerCard)
    {
        GameManager.Instance.ServerResetPlayerEndRoundConfirmation(LocalConnection, player.PlayerID.Value, true);

        QuestLane questLane = adventurerCard.CurrentCardHolder.Value.QuestLane;
        QuestLocation questLocation = questLane.QuestLocation.Value;

        cardCanvas.overrideSorting = false;
        PopUpManager.Instance.CreatePotionResolutionPopUp(card as PotionCard, questLocation);
        card.gameObject.SetActive(false);
        return;
    }

    protected override bool IsEndDragValid()
    {
        AdventurerCard adventurerCard = dropZone.GetComponent<AdventurerCard>();

        if (!adventurerCard.IsOwner)
        {
            EndDragEvent.Invoke(this, true);
            return BlockDragWithMessage("Cannot use Potion: Adventurer does not belong to you");
        }

        if (adventurerCard.CurrentCardHolder.Value.IsQuest())       // Using potion on Quest
        {
            var currentPhase = GameManager.Instance.CurrentPhase.Value;
            bool isMagicPhase = currentPhase == GameManager.Phase.Magic;
            bool isDispatchPhase = currentPhase == GameManager.Phase.Dispatch;
            bool isMyTurn = player.IsPlayerTurn.Value;

            if (!isMagicPhase && !(isDispatchPhase && isMyTurn))
            {
                EndDragEvent.Invoke(this, true);
                return BlockDragWithMessage("You can only use Potions during Magic Phase, or while dispatching Adventurers");
            }
        }
        else        // Using potion in Hand
        {
            PotionCard potionCard = card as PotionCard;

            // might need to make a potionCard.CanUseHealingPotion method to prevent duplicated logic in OnPotionResolutionClick
            // 7/25/25 - TODO: remove logic for empowering reqs, and dont prevent healing potion use if rest period is 0
            if (potionCard.PotionType.Value == Potion.Healing && adventurerCard.CurrentRestPeriod.Value == 0)
            {
                EndDragEvent.Invoke(this, true);
                return BlockDragWithMessage("Cannot use potion: Adventurer's Rest Period is already 0");
            }
            else if (potionCard.PotionType.Value == Potion.Power && (adventurerCard.OriginalPhysicalPower.Value == 0 && adventurerCard.potionBasePhysicalPower.Value == 0) &&
                                                                     adventurerCard.OriginalMagicalPower.Value == 0 && adventurerCard.potionBaseMagicalPower.Value == 0)
            {
                EndDragEvent.Invoke(this, true);
                return BlockDragWithMessage("Cannot use potion: Adventurer cannot be empowered");
            }
        }
        return true;
    }
}
