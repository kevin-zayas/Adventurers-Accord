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

    private void Awake()
    {
        cardDatabase.Add(0, WarriorCardData);
        cardDatabase.Add(1, WizardCardData);
        cardDatabase.Add(2, ClericCardData);
        cardDatabase.Add(3, RogueCardData);

        //maintain a list/dictionary if each card will have a different frequency
        //cardCountList.Add(2);
        //cardCountList.Add(2);
        //cardCountList.Add(2);
        //cardCountList.Add(2);
    }
}
