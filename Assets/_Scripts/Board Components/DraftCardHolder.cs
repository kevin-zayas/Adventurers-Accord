using FishNet.Object;
using UnityEngine;

public class DraftCardHolder : CardHolder
{
    [field: SerializeField] public int DraftCardIndex { get; private set; }
    [SerializeField] private GameObject cardSlot;

    protected override void Start()
    {
        base.Start();
        HolderType = CardHolderType.Draft;
    }

    protected override void Update()
    {
        return; //DraftCardHolder will not have swap logic
    }

    [Server]
    public override void AddCard(Card card)
    {
        card.SetCardParent(cardSlot.transform, false, this);
        AddCardHandlerListeners(null, card);
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ServerMoveCard(Card card, CardHolder newCardHolder, Transform originalCardSlot = null)
    {
        RemoveCardHandlerListeners(null, card);
        newCardHolder.AddCard(card);
    }
}
