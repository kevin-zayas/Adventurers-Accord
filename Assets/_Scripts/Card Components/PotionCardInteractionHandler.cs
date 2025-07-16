using static PotionCard;

public class PotionCardInteractionHandler : CardInteractionHandler
{
    protected override void Start()
    {
        card = GetComponent<PotionCard>();
        base.Start();
    }

    /// <summary>
    /// Determines whether the drag operation can start, based on various conditions.
    /// </summary>
    /// <returns>True if the drag can start, otherwise false.</returns>
    protected override bool CanStartDrag()
    {
        if (!base.CanStartDrag()) return false;
        if (card.IsDraftCard.Value) return true;
        if (GameManager.Instance.CurrentPhase.Value == GameManager.Phase.Ability)
        {
            PopUpManager.Instance.CreateToastPopUp("You cannot use potions during Ability Phase");
            return false;
        }
        return true;
    }

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

        AdventurerCard adventurerCard = dropZone.GetComponent<AdventurerCard>();

        if (adventurerCard.CurrentCardHolder.Value.IsQuest())
        {
            if (GameManager.Instance.CurrentPhase.Value == GameManager.Phase.Dispatch && !player.IsPlayerTurn.Value)
            {
                PopUpManager.Instance.CreateToastPopUp("You can only use potions on dispatched Adventurers during your turn");
                EndDragEvent.Invoke(this, true);
                return;
            }
            HandleQuestEndDrag(adventurerCard);     //redundant check
            return;
        }
        HandleHandEndDrag(adventurerCard);
        return;
    }

    protected void HandleHandEndDrag(AdventurerCard adventurerCard)
    {
        if (adventurerCard.IsDraftCard.Value || !adventurerCard.IsOwner)    //check cards currentcardHolder type instead?
        {
            string message = "Cannot use Potion: Adventurer does not belong to the player";

            PopUpManager.Instance.CreateToastPopUp(message);
            EndDragEvent.Invoke(this, true);
            return;
        }

        PotionCard potionCard = card as PotionCard;

        // might need to make a potionCard.CanUseHealingPotion method to prevent duplicated logic in OnPotionResolutionClick
        if (potionCard.PotionType.Value == Potion.Healing && adventurerCard.CurrentRestPeriod.Value == 0)
        {
            PopUpManager.Instance.CreateToastPopUp("Cannot use potion: Adventurer's Rest Period is already 0");
            EndDragEvent.Invoke(this, true);
            return;
        }
        else if (potionCard.PotionType.Value == Potion.Power && (adventurerCard.OriginalPhysicalPower.Value == 0 && adventurerCard.potionBasePhysicalPower.Value == 0) &&
                                                                 adventurerCard.OriginalMagicalPower.Value == 0 && adventurerCard.potionBaseMagicalPower.Value == 0)
        {
            PopUpManager.Instance.CreateToastPopUp("Cannot use potion: Adventurer cannot be empowered");
            EndDragEvent.Invoke(this, true);
            return;
        }
        ConfirmationPopUp popUp = PopUpManager.Instance.CreateConfirmationPopUp();
        popUp.InitializeUsePotionPopUp(adventurerCard, potionCard);
    }

    protected void HandleQuestEndDrag(AdventurerCard adventurerCard)
    {
        GameManager.Instance.ServerResetPlayerEndRoundConfirmation(LocalConnection, player.PlayerID.Value, true);

        QuestLane questLane = adventurerCard.CurrentCardHolder.Value.QuestLane;
        QuestLocation questLocation = questLane.QuestLocation.Value;

        cardCanvas.overrideSorting = false;
        PopUpManager.Instance.CreatePotionResolutionPopUp(card as PotionCard, questLocation);
        card.gameObject.SetActive(false);
        return;
    }
}
