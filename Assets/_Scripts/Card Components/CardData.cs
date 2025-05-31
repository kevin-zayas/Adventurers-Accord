using UnityEngine;
using static Card;
using static PotionCard;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardData : ScriptableObject
{
    //TODO: make all public fields with private setters

    [field: SerializeField] public string CardName { get; set; }
    [field: SerializeField] public int PhysicalPower { get; set; }
    [field: SerializeField] public int MagicalPower { get; set; }
    [field: SerializeField] public int OriginalPhysicalPower { get; set; }
    [field: SerializeField] public int OriginalMagicalPower { get; set; }
    [field: SerializeField] public int Cost { get; set; }
    [field: SerializeField, TextArea(3, 10)] public string CardDescription { get; set; }
    [field: SerializeField, TextArea(2, 10)] public string CardSubDescription { get; set; }
    [field: SerializeField] public CardType CardType { get; set; }
    [field: SerializeField] public string AbilityName { get; set; }

    //Adventurers
    [field: SerializeField] public int RestPeriod { get; set; }

    //Quests
    [field: SerializeField] public int GoldReward { get; set; }
    [field: SerializeField] public int ReputationReward { get; set; }
    [field: SerializeField] public int LootReward { get; set; }
    [field: SerializeField] public int ReputationPenalty { get; set; }
    [field: SerializeField] public int GoldPenalty { get; set; }
    [field: SerializeField] public int RestPeriodPenalty { get; set; }
    [field: SerializeField] public int PartySizeLimit { get; set; }
    [field: SerializeField] public bool Drain { get; set; }
    [field: SerializeField] public int PhysicalDrain { get; set; }
    [field: SerializeField] public int MagicalDrain { get; set; }
    [field: SerializeField] public bool DisableItems { get; set; }
    [field: SerializeField] public bool BlockSpells { get; set; }

    //Spells
    [field: SerializeField] public bool IsNegativeEffect { get; set; }
    [field: SerializeField] public bool IsNumerical { get; set; }

    //Potions
    [field: SerializeField] public Potion PotionType { get; set; }
}
