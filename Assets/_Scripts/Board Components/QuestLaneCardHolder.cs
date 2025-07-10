using DG.Tweening;
using FishNet.Object;
using UnityEngine;

public class QuestLaneCardHolder : CardHolder
{
    [SerializeField] private QuestLane questLane;
    public override QuestLane QuestLane => questLane;
    protected override void Start()
    {
        base.Start();
        HolderType = CardHolderType.Quest;
    }

    [Server]
    public override void AddCard(Card card)
    {
        base.AddCard(card);

        if (card is AdventurerCard adventurerCard)
        {
            adventurerCard.DispatchAdventurer(questLane);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ServerMoveCard(Card card, CardHolder newCardHolder, Transform originalCardSlot = null)
    {
        RemoveCardHandlerListeners(card.Owner, card);
        newCardHolder.AddCard(card);
        originalCardSlot.SetParent(null);
        Despawn(originalCardSlot.gameObject);

        if (card is AdventurerCard adventurerCard)
        {
            adventurerCard.RemoveAdventurer(questLane);
        }
    }

    protected override void SetCardScale(GameObject card)
    {
        card.transform.DOScale(new Vector3(0.6f, 0.6f, 1f), 0.2f).SetEase(Ease.OutBack);      //dotween?
    }
}

