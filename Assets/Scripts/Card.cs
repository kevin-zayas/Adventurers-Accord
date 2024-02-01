using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public class Card : MonoBehaviour
{
    public int id;
    public string cardName;
    public int cost;
    public int physPower;
    public int magPower;
    public string cardDescription;


    public Card()
    {

    }

    public Card(int Id, string CardName, int Cost, int PhysPower, int MagPower, string CardDescription)
    {
        id = Id;
        cardName = CardName;
        cost = Cost;
        physPower = PhysPower;
        magPower = MagPower;
        cardDescription = CardDescription;
    }
}
