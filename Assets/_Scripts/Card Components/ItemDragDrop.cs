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

        if (adventurerCard.IsDraftCard.Value || adventurerCard.HasItem.Value || !adventurerCard.IsOwner)
        {
            Debug.Log("Cannot equip item: card already has an item, is a draft card, or is not owned by the player.");
            ResetCardPosition();
            return;
        }

        if (adventurerCard.ParentTransform.Value.CompareTag("Quest"))
        {
            Debug.Log("Cannot equip item: card is currently on a quest.");
            ResetCardPosition();
            return;
        }

        if ((itemCard.MagicalPower.Value > 0 && adventurerCard.OriginalMagicalPower.Value == 0) ||
            (itemCard.PhysicalPower.Value > 0 && adventurerCard.OriginalPhysicalPower.Value == 0))
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
