using FishNet.Object;
using UnityEngine;

public class AdventurerDragDrop : CardDragDrop
{
    #region Serialized Fields
    [SerializeField] private AdventurerCard card;
    [SerializeField] private Player player;
    #endregion

    private void Start()
    {
        card = GetComponent<AdventurerCard>();

        if (!IsClient) return;

        if (player == null)
        {
            player = GameManager.Instance.Players[LocalConnection.ClientId];
        }
    }

    /// <summary>
    /// Handles the collision enter event, with additional logic to filter invalid drop zones.
    /// </summary>
    /// <param name="collision">The collision data associated with this event.</param>
    protected override void OnCollisionEnter2D(Collision2D collision)
    {   
        //revisit after tuning dragging logic. this may not even be possible if you can only drag cards during dispatch.
        if (collision.gameObject.CompareTag("Quest") && GameManager.Instance.CurrentPhase != GameManager.Phase.Dispatch) return;
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
        if (!card.IsDraftCard && !IsOwner) return false; // Prevent dragging non-draft cards if not owner
        if (GameManager.Instance.CurrentPhase == GameManager.Phase.Resolution) return false;
        if (!player.IsPlayerTurn) return false; // Allow dragging only during player's turn
        if (card.IsDraftCard && GameManager.Instance.CurrentPhase != GameManager.Phase.Recruit) return false;

        return !card.IsDraftCard || player.Gold >= card.Cost; // Check player gold if dragging a DraftCard
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
        if (card.IsDraftCard)
        {
            AssignDraftCardToPlayer();
        }
        else
        {
            HandleCardMovement();
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

    /// <summary>
    /// Assigns the draft card to the player, updating the game state accordingly.
    /// </summary>
    private void AssignDraftCardToPlayer()
    {
        CardSlot cardSlot = startParentTransform.GetComponent<CardSlot>();

        card.ServerSetCardParent(dropZone.transform, false);
        card.ServerSetCardOwner(player);

        player.ServerChangePlayerGold(-card.Cost);
        Board.Instance.ReplaceDraftCard(cardSlot.SlotIndex);
        GameManager.Instance.EndTurn(false);
    }

    /// <summary>
    /// Handles the movement of the card to a new drop zone.
    /// </summary>
    protected override void HandleCardMovement()
    {
        card.ServerSetCardParent(dropZone.transform, false);
    }
}
