using DG.Tweening;
using FishNet.Object;
using UnityEngine;

public class QuestSpellCardHolder : CardHolder
{
    [SerializeField] private QuestLane questLane;
    public override QuestLane QuestLane => questLane;
    public override Vector3 Scale => new(0.6f, 0.6f, 1f);

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
}
