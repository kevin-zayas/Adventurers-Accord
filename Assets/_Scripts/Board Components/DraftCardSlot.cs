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
    public override void AddCard(Card card, Transform cardHolderTransform)
    {
        card.SetCardParent(cardSlot.transform, false);
        AddCardHandlerListeners(null, card);
    }

    [ServerRpc(RequireOwnership = false)]
    public override void MoveCard(Card card, Transform newTransform)
    {
        RemoveCardHandlerListeners(null, card);
        CardInteractionHandler cardHandler = card.GetComponent<CardInteractionHandler>();

        TargetEndDrag(card.Owner, cardHandler, false);        //needs to happen on the target
        CardHolder cardHolder = newTransform.GetComponent<CardHolder>();
        cardHolder.AddCard(card, newTransform);
        //card.SetCardParent(newTransform, false);
    }
}
