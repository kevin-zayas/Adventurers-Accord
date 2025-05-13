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
        if (GameManager.Instance.CurrentPhase.Value != GameManager.Phase.Magic) // Prevent dragging during all phases except Magic
        {
            PopUpManager.Instance.CreateToastPopUp("You can only use Magic Spells during Magic Phase");
            return false; 
        }

        if (transform.parent.CompareTag("Quest")) // Prevent dragging if the card is already in a quest lane
        {
            PopUpManager.Instance.CreateToastPopUp("You cannot move Magic Spells that have already been cast");
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
    /// Handles the specific logic when the drag operation ends, including spell usage validation.
    /// </summary>
    protected override void HandleEndDrag()
    {
        if (card.IsDraftCard.Value)
        {
            OnCardPurchase();
            return;
        }

        QuestLane questLane = dropZone.transform.parent.GetComponent<QuestLane>();

        if (questLane.QuestCard.Value.BlockSpells.Value)
        {
            PopUpManager.Instance.CreateToastPopUp("Magic Spells cannot be used on this Quest");
            ResetCardPosition();
            return;
        }

        if (questLane.QuestDropZone.transform.childCount == 0)
        {
            PopUpManager.Instance.CreateToastPopUp("Magic Spells cannot be used on a lane with no Adventurers");
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
