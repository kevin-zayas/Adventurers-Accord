using UnityEngine;

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

    [field: SerializeField] public string CardDescription { get; set; }
    [field: SerializeField] public string CardSubDescription { get; set; }
    [field: SerializeField] public string CardType { get; set; }

    //Quests
    [field: SerializeField] public int GoldReward { get; set; }
    [field: SerializeField] public int ReputationReward { get; set; }
    [field: SerializeField] public int LootReward { get; set; }
    [field: SerializeField] public bool Drain { get; set; }
    [field: SerializeField] public int PhysicalDrain { get; set; }
    [field: SerializeField] public int MagicalDrain { get; set; }
    [field: SerializeField] public bool DisableItems { get; set; }
    [field: SerializeField] public bool BlockSpells { get; set; }

    //Spells
    [field: SerializeField] public bool IsGreaseSpell { get; set; }

}
