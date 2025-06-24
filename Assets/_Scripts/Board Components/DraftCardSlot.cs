using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraftCardSlot : CardHolder
{
    [field: SerializeField] public int SlotIndex { get; private set; }
    [SerializeField] private GameObject cardSlot;
    // Start is called before the first frame update

    [Server]
    public override void AddCard(Card card)
    {
        card.SetCardParent(cardSlot.transform, false);
        AddCardHandlerListeners(card);
    }

    [ServerRpc(RequireOwnership = false)]
    public override void MoveCard(Card card, Transform newTransform)
    {
        RemoveCardHandlerListeners(card);
        CardInteractionHandler cardHandler = card.GetComponent<CardInteractionHandler>();

        EndDrag(cardHandler, false);
        CardHolder cardHolder = newTransform.GetComponent<CardHolder>();
        cardHolder.AddCard(card);
        //card.SetCardParent(newTransform, false);
    }
}
