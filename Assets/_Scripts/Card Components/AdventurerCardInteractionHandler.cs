using UnityEngine;

public class AdventurerCardInteractionHandler : CardInteractionHandler
{
    #region Serialized Fields
    //[SerializeField] private AdventurerCard card;
    #endregion

    protected override void Start()
    {
        card = GetComponent<AdventurerCard>();
        base.Start();
    }

    /// <summary>
    /// Handles the collision enter event, with additional logic to filter invalid drop zones.
    /// </summary>
    /// <param name="collision">The collision data associated with this event.</param>
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        //revisit after tuning dragging logic. this may not even be possible if you can only drag cards during dispatch.
        if (collision.gameObject.CompareTag("Quest") && GameManager.Instance.CurrentPhase.Value != GameManager.Phase.Dispatch) return;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Magic Items")) return; // Prevent dragging onto Magic Item

        base.OnCollisionEnter2D(collision);
    }

    /// <summary>
    /// Determines whether the drag operation can start, based on various conditions.
    /// </summary>
    /// <returns>True if the drag can start, otherwise false.</returns>
    protected override bool CanStartDrag()
    {
        if (!base.CanStartDrag()) return false;
        if (card.IsDraftCard.Value) return true;
        if (GameManager.Instance.CurrentPhase.Value != GameManager.Phase.Dispatch) // Prevent dragging during all phases except Dispatch
        {
            PopUpManager.Instance.CreateToastPopUp("You can only move Adventurers during Dispatch Phase");
            return false;
        }
        if (!player.IsPlayerTurn.Value)
        {
            PopUpManager.Instance.CreateToastPopUp("You can only dispatch Adventurers during your turn");
            return false;
        }
        if (card.CardName.Value == "Wolf") return false;

        return true;
    }

    /// <summary>
    /// Begins the drag operation for the card and reverts card scale back to base
    /// </summary>
    //public override void BeginDrag()
    //{
    //    if (CanStartDrag())
    //    {
    //        base.BeginDrag();
    //        transform.localScale = Vector3.one;
    //    }
    //}

    /// <summary>
    /// Handles the specific logic when the drag operation ends.
    /// </summary>
    protected override void HandleEndDrag()
    {
        QuestLane questLane = dropZone.transform.parent.GetComponent<QuestLane>();

        if (dropZone.CompareTag("Quest"))
        {
            if (IsQuestLaneFull(questLane))
            {
                PopUpManager.Instance.CreateToastPopUp("This Quest's party size limit has been reached");
                //ResetCardPosition();
            }
            else
            {
                player.ControlledHand.Value.MoveCard(card, dropZone.transform); // Move from Hand to Quest
            }
        }
        else if (card.IsDraftCard.Value)
        {
            OnCardPurchase();
        }
        else
        {
            CardHolder cardHolder = startParentTransform.GetComponent<CardHolder>();
            cardHolder.MoveCard(card, dropZone.transform);  // Move from Quest to Hand
            //card.ServerSetCardParent(dropZone.transform, false);
        }

        //EndDragEvent.Invoke(this, returningToSlot);
    }

    /// <summary>
    /// Assigns the draft card to the player, updating the game state accordingly.
    /// </summary>
    protected override void AssignDraftCardToPlayer()
    {
        base.AssignDraftCardToPlayer();
        player.ServerUpdateGuildRecapTracker("Adventurers Purchased", 1);
        if (card.Cost.Value == 5) player.ServerUpdateGuildRecapTracker("Adventurers Purchased (T1)", 1);
        else player.ServerUpdateGuildRecapTracker("Adventurers Purchased (T2)", 1);
    }

    /// <summary>
    /// Resets the card's position to its original location before dragging.
    /// </summary>
    protected override void ResetCardPosition()
    {
        card.ServerSetCardParent(startParentTransform, true);
        base.ResetCardPosition();
    }

    protected bool IsQuestLaneFull(QuestLane questLane)
    {
        if (questLane.CurrentAdventurerCount.Value >= questLane.MaxAdventurerCount.Value) return true;

        return false;
    }
}
