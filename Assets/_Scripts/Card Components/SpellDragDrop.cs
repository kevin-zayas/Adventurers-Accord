using FishNet.Object;
using UnityEngine;

public class SpellDragDrop : CardDragDrop
{
    #region Serialized Fields
    //[SerializeField] private SpellCard card;
    #endregion

    protected override void Start()
    {
        card = GetComponent<SpellCard>();
        base.Start();
    }

    /// <summary>
    /// Determines whether the drag operation can start based on various conditions.
    /// </summary>
    /// <returns>True if the drag can start, otherwise false.</returns>
    protected override bool CanStartDrag()
    {
        if (!base.CanStartDrag()) return false;
        if (card.IsDraftCard.Value) return true;

        if (transform.parent.CompareTag("Quest")) return false; // Prevent dragging if the card is already in a quest lane

        

        return true;
    }

    /// <summary>
    /// Begins the drag operation for the spell card.
    /// </summary>
    public override void BeginDrag()
    {
        if (CanStartDrag())
        {
            base.BeginDrag();
        }
    }

    /// <summary>
    /// Handles the specific logic when the drag operation ends, including spell usage validation.
    /// </summary>
    protected override void HandleEndDrag()
    {
        if (card.IsDraftCard.Value)
        {
            if (!dropZone.CompareTag("Hand"))
            {
                ResetCardPosition();
                return;
            }
            AssignDraftCardToPlayer();
            return;
        }

        QuestLane questLane = dropZone.transform.parent.GetComponent<QuestLane>();

        if (questLane.QuestCard.Value.BlockSpells.Value)
        {
            Debug.Log("Spells can't be used on this Quest");
            ResetCardPosition();
            return;
        }

        if (questLane.QuestDropZone.transform.childCount == 0)
        {
            Debug.Log("Spells can't be used on a lane with no Adventurers");
            ResetCardPosition();
            return;
        }

        HandleCardMovement();
    }

    /// <summary>
    /// Assigns the draft card to the player, updating the game state accordingly.
    /// </summary>
    protected override void AssignDraftCardToPlayer()
    {
        base.AssignDraftCardToPlayer();
        card.gameObject.layer = LayerMask.NameToLayer("Magic Spells");
    }

    /// <summary>
    /// Resets the spell card's position to its original location before dragging.
    /// </summary>
    protected override void ResetCardPosition()
    {
        card.ServerSetCardParent(startParentTransform, true);
        base.ResetCardPosition();
    }

    /// <summary>
    /// Handles the movement of the spell card to a new drop zone and updates the quest lane.
    /// </summary>
    protected override void HandleCardMovement()
    {
        QuestLane questLane = dropZone.transform.parent.GetComponent<QuestLane>();
        card.ServerSetCardParent(dropZone.transform, false);
        questLane.ServerUpdateSpellEffects();

        if (GameManager.Instance.CurrentPhase.Value == GameManager.Phase.Magic)
        {
            GameManager.Instance.RefreshEndRoundStatus();
        }
    }
}
