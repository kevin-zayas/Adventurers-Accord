using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    //public static List<int> cardCountList = new();
    public static List<Card> tierOneCardList = new();
    public static List<Card> tierTwoCardList = new();
    //public Card WarriorCardData;
    //public Card WizardCardData;
    //public Card ClericCardData;
    //public Card RogueCardData;
    //public Card BardCardData;

    private void Awake()
    {
        tierOneCardList.Add(Resources.Load("SOCards/Adventurers/T1/Warrior") as Card);
        tierOneCardList.Add(Resources.Load("SOCards/Adventurers/T1/Wizard") as Card);
        tierOneCardList.Add(Resources.Load("SOCards/Adventurers/T1/Cleric") as Card);
        tierOneCardList.Add(Resources.Load("SOCards/Adventurers/T1/Rogue") as Card);
        tierOneCardList.Add(Resources.Load("SOCards/Adventurers/T1/Bard") as Card);

        tierTwoCardList.Add(Resources.Load("SOCards/Adventurers/T2/Spellblade") as Card);
        tierTwoCardList.Add(Resources.Load("SOCards/Adventurers/T2/Enchanter") as Card);
        tierTwoCardList.Add(Resources.Load("SOCards/Adventurers/T2/Ranger") as Card);
        tierTwoCardList.Add(Resources.Load("SOCards/Adventurers/T2/Assassin") as Card);
        tierTwoCardList.Add(Resources.Load("SOCards/Adventurers/T2/Tinkerer") as Card);
        tierTwoCardList.Add(Resources.Load("SOCards/Adventurers/T2/Sorcerer") as Card);
    }
}
