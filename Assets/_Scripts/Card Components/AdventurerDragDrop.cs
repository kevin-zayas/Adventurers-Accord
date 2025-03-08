using FishNet.Object;
using UnityEngine;

public class AdventurerDragDrop : CardDragDrop
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

        if (!player.IsPlayerTurn.Value) return false; // Allow dragging only during player's turn
        if (card.CardName.Value == "Wolf") return false;

        return true;
    }

    /// <summary>
    /// Begins the drag operation for the card and reverts card scale back to base
    /// </summary>
    public override void BeginDrag()
    {
        if (CanStartDrag())
        {
            base.BeginDrag();
            transform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Handles the specific logic when the drag operation ends.
    /// </summary>
    protected override void HandleEndDrag()
    {
        QuestLane questLane;
        int currentCount = 0;
        int maxCount = 0;

        if (dropZone.CompareTag("Quest"))
        {
            questLane = dropZone.transform.parent.GetComponent<QuestLane>();
            currentCount = questLane.QuestCard.Value.CurrentAdventurerCount.Value;
            maxCount = questLane.QuestCard.Value.MaxAdventurerCount.Value;
        }

        if (dropZone.CompareTag("Quest") && currentCount >= maxCount)
        {
            ResetCardPosition();
        }
        else if (card.IsDraftCard.Value)
        {
            AssignDraftCardToPlayer();
        }
        else
        {
            card.ServerSetCardParent(dropZone.transform, false);
        }
    }

    /// <summary>
    /// Resets the card's position to its original location before dragging.
    /// </summary>
    protected override void ResetCardPosition()
    {
        card.ServerSetCardParent(startParentTransform, true);
        base.ResetCardPosition();
    }
}
