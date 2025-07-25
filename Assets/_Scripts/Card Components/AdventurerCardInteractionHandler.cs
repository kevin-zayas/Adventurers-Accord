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

        bool isCardOnQuest = card.CurrentCardHolder.Value.IsQuest();
        bool notDispatchOrNotTurn = GameManager.Instance.CurrentPhase.Value != GameManager.Phase.Dispatch || !player.IsPlayerTurn.Value;

        if (isCardOnQuest && notDispatchOrNotTurn)
            return BlockDragWithMessage("You can only move dispatched Adventurers during Dispatch Phase and on your turn");

        if (card.CardName.Value == "Wolf") // TODO: Replace with isSummon check
            return BlockDragWithMessage("You cannot move Summons");

        return true;
    }

    /// <summary>
    /// Handles the specific logic when the drag operation ends.
    /// </summary>
    protected override void HandleEndDrag()
    {
        if (card.IsDraftCard.Value)
        {
            OnCardPurchase();
            return;
        }

        if (!IsEndDragValid()) return;
        
        //EndDragEvent.Invoke(this, false);
        originalCardHolder.ServerMoveCard(card, dropZone.GetComponent<CardHolder>(), originalCardSlot);  // Move to Hand/Quest
        
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

    protected override bool IsEndDragValid()
    {
        if (!dropZone.CompareTag("Quest"))
            return true;

        QuestLane questLane = dropZone.transform.parent.GetComponent<QuestLane>();
        bool isDispatchPhase = GameManager.Instance.CurrentPhase.Value == GameManager.Phase.Dispatch;
        bool isMyTurn = player.IsPlayerTurn.Value;

        if (!isDispatchPhase || !isMyTurn)
        {
            EndDragEvent.Invoke(this, true);
            return BlockDragWithMessage("You can only dispatch Adventurers Dispatch Phase and on your turn");
        }

        if (IsQuestLaneFull(questLane))
        {
            EndDragEvent.Invoke(this, true);
            return BlockDragWithMessage("This Quest's party size limit has been reached");
        }
        
        return true;
    }

    protected bool IsQuestLaneFull(QuestLane questLane)
    {
        return questLane.CurrentAdventurerCount.Value >= questLane.MaxAdventurerCount.Value;
    }
}
