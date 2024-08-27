using FishNet.Object;
using UnityEngine;

public class SpellDragDrop : CardDragDrop
{
    #region Serialized Fields
    [SerializeField] private SpellCard spellCard;
    #endregion

    private void Start()
    {
        spellCard = GetComponent<SpellCard>();
    }

    protected override bool CanStartDrag()
    {
        if (Input.GetMouseButton(1)) return false; // Prevent dragging on right-click
        if (!IsOwner) return false; // Ensure the player owns the card before dragging
        if (transform.parent.CompareTag("Quest")) return false; // Prevent dragging if the card is already in a quest lane

        // Only allow dragging during the Dispatch or Magic phase
        if (GameManager.Instance.CurrentPhase != GameManager.Phase.Dispatch &&
            GameManager.Instance.CurrentPhase != GameManager.Phase.Magic)
        {
            Debug.Log("Can't move spells during this phase");
            return false;
        }

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
    /// Ends the drag operation for the spell card, handling card placement and validation.
    /// </summary>
    public override void EndDrag()
    {
        base.EndDrag();
    }

    protected override void HandleEndDrag()
    {
        QuestLane questLane = dropZone.transform.parent.GetComponent<QuestLane>();

        if (questLane.QuestCard.BlockSpells)
        {
            Debug.Log("Spells can't be used on this Quest");
            ResetCardPosition();
            return;
        }

        if (questLane.DropZone.transform.childCount == 0)
        {
            Debug.Log("Spells can't be used on a lane with no Adventurers");
            ResetCardPosition();
            return;
        }

        HandleCardMovement();
    }

    /// <summary>
    /// Resets the spell card's position to its original location before dragging.
    /// </summary>
    protected override void ResetCardPosition()
    {
        spellCard.ServerSetCardParent(startParentTransform, true);
        base.ResetCardPosition();
    }

    /// <summary>
    /// Handles the movement of the spell card to a new drop zone and updates the quest lane.
    /// </summary>
    protected override void HandleCardMovement()
    {
        QuestLane questLane = dropZone.transform.parent.GetComponent<QuestLane>();
        spellCard.ServerSetCardParent(dropZone.transform, false);
        questLane.ServerUpdateSpellEffects();

        if (GameManager.Instance.CurrentPhase == GameManager.Phase.Magic)
        {
            GameManager.Instance.RefreshEndRoundStatus();
        }
    }
}
