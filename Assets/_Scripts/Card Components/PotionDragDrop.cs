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

        if (adventurerCard.IsDraftCard.Value || !adventurerCard.IsOwner)
        {
            string message = "Cannot use Potion: Adventurer does not belong to the player";

            PopUpManager.Instance.CreateToastPopUp(message);
            ResetCardPosition();
            return;
        }

        if (adventurerCard.ParentTransform.Value.CompareTag("Quest"))
        {
            PopUpManager.Instance.CreateToastPopUp("Cannot use potion: Adventurer is on a quest");
            ResetCardPosition();
            return;
        }

        PotionCard potionCard = card as PotionCard;

        if (potionCard.PotionType.Value == Potion.Healing && adventurerCard.CurrentRestPeriod.Value == 0)
        {
            PopUpManager.Instance.CreateToastPopUp("Cannot use potion: Adventurer's Rest Period is already 0");
            ResetCardPosition();
            return;
        }
        ConfirmationPopUp popUp = PopUpManager.Instance.CreateConfirmationPopUp();
        popUp.InitializeUsePotionPopUp(adventurerCard, potionCard);
    }

    /// <summary>
    /// Assigns the draft card to the player, updating the game state accordingly.
    /// </summary>
    protected override void AssignDraftCardToPlayer()
    {
        base.AssignDraftCardToPlayer();
        card.gameObject.layer = LayerMask.NameToLayer("Magic Items");
        Player.Instance.ServerUpdateGuildRecapTracker("Magic Items (Purchased)", 1);
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
