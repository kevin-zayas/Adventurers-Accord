using DG.Tweening;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestSpellCardHolder : CardHolder
{
    [SerializeField] private QuestLane questLane;
    public override QuestLane QuestLane => questLane;

    protected override void Start()
    {
        base.Start();
        HolderType = CardHolderType.Spell;
    }

    [Server]
    public override void AddCard(Card card)
    {
        base.AddCard(card);
        questLane.UpdateSpellEffects();
    }

    [Server]
    public override void MoveCard(Card card, CardHolder newCardHolder, Transform originalCardSlot = null)
    {
        RemoveCardHandlerListeners(card.Owner, card);
        Despawn(card.gameObject);
        originalCardSlot.SetParent(null);
        Despawn(originalCardSlot.gameObject);
    }

    protected override void SetCardScale(GameObject card)
    {
        card.transform.DOScale(new Vector3(0.6f, 0.6f, 1f), 0.2f).SetEase(Ease.OutBack);
    }
}
