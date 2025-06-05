using UnityEngine;
using static PotionCard;

public class PotionDragDrop : CardDragDrop
{
    //protected new PotionCard card;
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
    /// Begins the drag operation for the item card.
    /// </summary>
    public override void BeginDrag()
    {
        if (CanStartDrag())
        {
            base.BeginDrag();
        }
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

        if (adventurerCard.ParentTransform.Value.CompareTag("Quest"))
        {
            if (GameManager.Instance.CurrentPhase.Value == GameManager.Phase.Dispatch && !player.IsPlayerTurn.Value)
            {
                PopUpManager.Instance.CreateToastPopUp("You can only use potions on dispatched Adventurers during your turn");
                ResetCardPosition();
                return;
            }
            HandleQuestEndDrag(adventurerCard);
            return;
        }
        HandleHandEndDrag(adventurerCard);
        return;
        
    }

    protected void HandleHandEndDrag(AdventurerCard adventurerCard)
    {
        if (adventurerCard.IsDraftCard.Value || !adventurerCard.IsOwner)
        {
            string message = "Cannot use Potion: Adventurer does not belong to the player";

            PopUpManager.Instance.CreateToastPopUp(message);
            ResetCardPosition();
            return;
        }

        PotionCard potionCard = card as PotionCard;

        // might need to make a potionCard.CanUseHealingPotion method to prevent duplicated logic in OnPotionResolutionClick
        if (potionCard.PotionType.Value == Potion.Healing && adventurerCard.CurrentRestPeriod.Value == 0)
        {
            PopUpManager.Instance.CreateToastPopUp("Cannot use potion: Adventurer's Rest Period is already 0");
            ResetCardPosition();
            return;
        }
        else if (potionCard.PotionType.Value == Potion.Power && (adventurerCard.OriginalPhysicalPower.Value == 0 && adventurerCard.potionBasePhysicalPower.Value == 0) && 
                                                                 adventurerCard.OriginalMagicalPower.Value == 0 && adventurerCard.potionBaseMagicalPower.Value == 0)
        {
            PopUpManager.Instance.CreateToastPopUp("Cannot use potion: Adventurer cannot be empowered");
            ResetCardPosition();
            return;
        }
        ConfirmationPopUp popUp = PopUpManager.Instance.CreateConfirmationPopUp();
        popUp.InitializeUsePotionPopUp(adventurerCard, potionCard);
    }

    protected void HandleQuestEndDrag(AdventurerCard adventurerCard)
    {
        if (GameManager.Instance.CurrentPhase.Value == GameManager.Phase.Magic)
        {
            GameManager.Instance.ServerResetPlayerEndRoundConfirmation(LocalConnection, player.PlayerID.Value, true);
        }

        QuestLane lane = adventurerCard.transform.parent.parent.GetComponent<QuestLane>();
        QuestLocation questLocation = lane.QuestLocation.Value;

        PopUpManager.Instance.CreatePotionResolutionPopUp(card as PotionCard, questLocation);
        card.gameObject.SetActive(false);
        return;
    }

    /// <summary>
    /// Assigns the draft card to the player, updating the game state accordingly.
    /// </summary>
    protected override void AssignDraftCardToPlayer()
    {
        base.AssignDraftCardToPlayer();
        //card.gameObject.layer = LayerMask.NameToLayer("Magic Items");
        //Player.Instance.ServerUpdateGuildRecapTracker("Magic Items (Purchased)", 1);
    }

    /// <summary>
    /// Resets the item card's position to its original location before dragging.
    /// </summary>
    protected override void ResetCardPosition()
    {
        card.ServerSetCardParent(startParentTransform, true);
        base.ResetCardPosition();
    }
}
