using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    //public static List<int> cardCountList = new();
    public static Dictionary<int, Card> cardDatabase = new();
    public Card WarriorCardData;
    public Card WizardCardData;
    public Card ClericCardData;
    public Card RogueCardData;
    public Card BardCardData;

    private void Awake()
    {
        cardDatabase.Add(0, WarriorCardData);
        cardDatabase.Add(1, WizardCardData);
        cardDatabase.Add(2, ClericCardData);
        cardDatabase.Add(3, RogueCardData);
        cardDatabase.Add(4, BardCardData);
    }
}
