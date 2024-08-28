using FishNet.Object;
using UnityEngine;

public class ItemDragDrop : CardDragDrop
{
    #region Serialized Fields
    [SerializeField] private ItemCard itemCard;
    #endregion

    private void Start()
    {
        itemCard = GetComponent<ItemCard>();
    }

    /// <summary>
    /// Determines whether the drag operation can start based on various conditions.
    /// </summary>
    /// <returns>True if the drag can start, otherwise false.</returns>
    protected override bool CanStartDrag()
    {
        if (Input.GetMouseButton(1)) return false; // Prevent dragging on right-click
        if (GameManager.Instance.CurrentPhase == GameManager.Phase.GameOver) return false;

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
        AdventurerCard adventurerCard = dropZone.GetComponent<AdventurerCard>();

        if (adventurerCard.IsDraftCard || adventurerCard.HasItem || !adventurerCard.IsOwner)
        {
            Debug.Log("Cannot equip item: card already has an item, is a draft card, or is not owned by the player.");
            ResetCardPosition();
            return;
        }

        if (adventurerCard.ParentTransform.CompareTag("Quest"))
        {
            Debug.Log("Cannot equip item: card is currently on a quest.");
            ResetCardPosition();
            return;
        }

        if ((itemCard.MagicalPower > 0 && adventurerCard.OriginalMagicalPower == 0) ||
            (itemCard.PhysicalPower > 0 && adventurerCard.OriginalPhysicalPower == 0))
        {
            Debug.Log("Cannot equip item: card does not have the required power type.");
            ResetCardPosition();
            return;
        }

        ConfirmationPopUp popUp = PopUpManager.Instance.CreateConfirmationPopUp(true);
        popUp.InitializeEquipItemPopUp(adventurerCard, this.gameObject);
    }

    /// <summary>
    /// Resets the item card's position to its original location before dragging.
    /// </summary>
    protected override void ResetCardPosition()
    {
        itemCard.ServerSetCardParent(startParentTransform, true);
        base.ResetCardPosition();
    }

    /// <summary>
    /// Not implemented for ItemDragDrop, since item movement is handled via a confirmation pop-up.
    /// </summary>
    protected override void HandleCardMovement()
    {
        throw new System.NotImplementedException();
    }
}
