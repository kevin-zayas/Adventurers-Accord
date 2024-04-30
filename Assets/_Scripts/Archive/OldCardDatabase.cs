using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldCardDatabase : MonoBehaviour
{
    //public static List<int> cardCountList = new();
    public static List<OldCard> tierOneCardList = new();
    public static List<OldCard> tierTwoCardList = new();
    //public Card WarriorCardData;
    //public Card WizardCardData;
    //public Card ClericCardData;
    //public Card RogueCardData;
    //public Card BardCardData;

    private void Awake()
    {
        tierOneCardList.Add(Resources.Load("SOCards/Adventurers/T1/Warrior") as OldCard);
        tierOneCardList.Add(Resources.Load("SOCards/Adventurers/T1/Wizard") as OldCard);
        tierOneCardList.Add(Resources.Load("SOCards/Adventurers/T1/Cleric") as OldCard);
        tierOneCardList.Add(Resources.Load("SOCards/Adventurers/T1/Rogue") as OldCard);
        tierOneCardList.Add(Resources.Load("SOCards/Adventurers/T1/Bard") as OldCard);

        tierTwoCardList.Add(Resources.Load("SOCards/Adventurers/T2/Spellblade") as OldCard);
        tierTwoCardList.Add(Resources.Load("SOCards/Adventurers/T2/Enchanter") as OldCard);
        tierTwoCardList.Add(Resources.Load("SOCards/Adventurers/T2/Ranger") as OldCard);
        tierTwoCardList.Add(Resources.Load("SOCards/Adventurers/T2/Assassin") as OldCard);
        tierTwoCardList.Add(Resources.Load("SOCards/Adventurers/T2/Tinkerer") as OldCard);
        tierTwoCardList.Add(Resources.Load("SOCards/Adventurers/T2/Sorcerer") as OldCard);
    }
}
