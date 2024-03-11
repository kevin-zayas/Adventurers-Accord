using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardData : ScriptableObject
{
    public string cardName;
    public int physicalPower;
    public int magicalPower;
    public int originalPhysicalPower;
    public int originalMagicalPower;
    public int cost;

    public string cardDescription;
    public string cardSubDescription;
    public string cardType;
    //public Sprite spriteImage;

    public int goldReward;
    public int reputationReward;
    public int lootReward;
    public bool drain;
    public int physicalDrain;
    public int magicalDrain;

}
