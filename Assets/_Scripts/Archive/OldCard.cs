using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[System.Serializable]

//[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class OldCard : ScriptableObject
{
    public int id;
    public string cardName;
    public int cost;
    public int physPower;
    public int magPower;
    public string cardDescription;
    public Sprite spriteImage;

    public int slotIndex;

}
