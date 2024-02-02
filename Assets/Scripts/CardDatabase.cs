using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    public static List<Card> cardList = new List<Card>();

    private void Awake()
    {
        cardList.Add(new Card(0, "None", 0, 0, 0, "None", Resources.Load<Sprite>("DefaultSprite")));
        cardList.Add(new Card(1, "Warrior", 5, 3, 0, "Argh", Resources.Load<Sprite>("DefaultSprite")));
        cardList.Add(new Card(2, "Wizard", 5, 0, 3, "Drzzt", Resources.Load<Sprite>("DefaultSprite")));
        cardList.Add(new Card(3, "Cleric", 5, 0, 2, "Cleric's Protection", Resources.Load<Sprite>("DefaultSprite")));
        cardList.Add(new Card(4, "Rogue", 5, 2, 0, "Sticky Fingers", Resources.Load<Sprite>("DefaultSprite")));
    }
}
