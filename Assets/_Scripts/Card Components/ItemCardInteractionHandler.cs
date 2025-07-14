using UnityEngine;

public class ItemCardInteractionHandler : CardInteractionHandler
{

    protected override void Start()
    {
        card = GetComponent<ItemCard>();
        base.Start();
    }

    /// <summary>
    /// Handles the specific logic when the drag operation ends, including item equipping validation.
    /// </summary>
    protected override void HandleEndDrag()
    {
        if (card.IsDraftCard.Value)
        {
            OnCardPurchase();
            return;
        }
        AdventurerCard adventurerCard = dropZone.GetComponent<AdventurerCard>();

        if (adventurerCard.IsDraftCard.Value || adventurerCard.HasItem.Value || !adventurerCard.IsOwner)
        {
            string message;
            if (adventurerCard.HasItem.Value) message = "Cannot equip Magic Item: Adventurer already has an item equipped";
            else message = "Cannot equip Magic Item: Adventurer does not belong to the player";

            PopUpManager.Instance.CreateToastPopUp(message);
            EndDragEvent.Invoke(this, true);
            return;
        }

        if (adventurerCard.CurrentCardHolder.Value.IsQuest())
        {
            PopUpManager.Instance.CreateToastPopUp("Cannot equip Magic Item: Adventurer is on a quest");
            EndDragEvent.Invoke(this, true);
            return;
        }

        if ((card.MagicalPower.Value > 0 && adventurerCard.OriginalMagicalPower.Value == 0) ||
            (card.PhysicalPower.Value > 0 && adventurerCard.OriginalPhysicalPower.Value == 0))
        {
            PopUpManager.Instance.CreateToastPopUp("Cannot equip Magic Item: Adventurer does not have the required Power");
            EndDragEvent.Invoke(this, true);
            return;
        }

        ConfirmationPopUp popUp = PopUpManager.Instance.CreateConfirmationPopUp();
        popUp.InitializeEquipItemPopUp(adventurerCard, (ItemCard)card);
    }

    /// <summary>
    /// Assigns the draft card to the player, updating the game state accordingly.
    /// </summary>
    protected override void AssignDraftCardToPlayer()
    {
        base.AssignDraftCardToPlayer();
        card.gameObject.layer = LayerMask.NameToLayer("Magic Items");
        Player.Instance.ServerUpdateGuildRecapTracker("Magic Items (Purchased)", 1);
    }
}
