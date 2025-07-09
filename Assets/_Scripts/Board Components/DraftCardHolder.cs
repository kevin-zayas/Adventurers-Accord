using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraftCardHolder : CardHolder
{
    [field: SerializeField] public int DraftCardIndex { get; private set; }
    [SerializeField] private GameObject cardSlot;

    protected override void Update()
    {
        return; //DraftCardSlot does not need to update like other card holders
    }

    [Server]
    public override void AddCard(Card card)
    {
        card.SetCardParent(cardSlot.transform, false);
        AddCardHandlerListeners(null, card);
    }

    [ServerRpc(RequireOwnership = false)]
    public override void MoveCard(Card card, CardHolder newCardHolder, Transform originalCardSlot = null)
    {
        RemoveCardHandlerListeners(null, card);
        newCardHolder.AddCard(card);
    }
}
