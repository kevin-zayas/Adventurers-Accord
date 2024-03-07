using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Adventurer Card")]
public class CardData : ScriptableObject
{
    public string cardName;
    public int physicalPower;
    public int magicalPower;
    public int originalPhysicalPower;
    public int originalMagicalPower;
    public int cost;
    public string cardDescription;
    //public Sprite spriteImage;

}
