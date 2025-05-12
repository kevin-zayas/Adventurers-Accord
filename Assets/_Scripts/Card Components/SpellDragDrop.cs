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
        if (GameManager.Instance.CurrentPhase.Value != GameManager.Phase.Magic) return false; // Prevent dragging during all phases except Magic
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
            OnCardPurchase();
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

        ConfirmationPopUp popUp = PopUpManager.Instance.CreateConfirmationPopUp();
        popUp.InitializeCastSpellPopUp(dropZone, (SpellCard)card);
    }

    /// <summary>
    /// Assigns the draft card to the player, updating the game state accordingly.
    /// </summary>
    protected override void AssignDraftCardToPlayer()
    {
        base.AssignDraftCardToPlayer();
        card.gameObject.layer = LayerMask.NameToLayer("Magic Spells");
        player.ServerUpdateGuildRecapTracker("Magic Spells (Purchased)", 1);

        if (((SpellCard)card).IsNegativeEffect.Value)
        {
            player.ServerUpdateGuildRecapTracker("Magic Spell Curses (Purchased)", 1);
        }
    }

    /// <summary>
    /// Resets the spell card's position to its original location before dragging.
    /// </summary>
    protected override void ResetCardPosition()
    {
        card.ServerSetCardParent(startParentTransform, true);
        base.ResetCardPosition();
    }
}
