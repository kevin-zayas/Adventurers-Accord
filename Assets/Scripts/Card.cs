using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class Card : ScriptableObject
{
    public int id;
    public string cardName;
    public int cost;
    public int physPower;
    public int magPower;
    public string cardDescription;
    public Sprite spriteImage;


    public Card()
    {

    }

    public Card(int Id, string CardName, int Cost, int PhysPower, int MagPower, string CardDescription, Sprite SpriteImage)
    {
        id = Id;
        cardName = CardName;
        cost = Cost;
        physPower = PhysPower;
        magPower = MagPower;
        cardDescription = CardDescription;
        spriteImage = SpriteImage;
    }
}
