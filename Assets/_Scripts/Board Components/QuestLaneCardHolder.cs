using DG.Tweening;
using FishNet.Object;
using UnityEngine;

public class QuestLaneCardHolder : CardHolder
{
    [SerializeField] private QuestLane questLane;
    public override Vector3 Scale => new(0.6f, 0.6f, 1f);
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

    [Server]
    public override void MoveCard(Card card, CardHolder newCardHolder, Transform originalCardSlot = null)
    {
        AdventurerCard adventurerCard = (AdventurerCard)card;
        bool isWolf = card.CardName.Value == "Wolf";        //can replace with isSummon, during Summon rework

        if (isWolf) adventurerCard.RemoveAdventurer(questLane);

        base.MoveCard(card, newCardHolder, originalCardSlot);

        if (!isWolf) adventurerCard.RemoveAdventurer(questLane);
    }
}

