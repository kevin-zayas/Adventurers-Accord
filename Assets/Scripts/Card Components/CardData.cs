using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardData : ScriptableObject
{
    //TODO: make all public fields with private setters

    public string cardName;
    public int physicalPower;
    public int magicalPower;
    public int originalPhysicalPower;
    public int originalMagicalPower;
    public int cost;

    public string cardDescription;
    public string cardSubDescription;
    public string cardType;

    //Quests
    public int goldReward;
    public int reputationReward;
    public int lootReward;
    public bool drain;
    public int physicalDrain;
    public int magicalDrain;
    public bool disableItems;
    public bool blockSpells;
    
    //Spells
    public bool isGreaseSpell;

}
