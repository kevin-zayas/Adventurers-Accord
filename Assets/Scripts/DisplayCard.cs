using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DisplayCard : MonoBehaviour
{
    public List<Card> displayCards = new List<Card>();
    public int displayId;

    public int id;
    public string cardName;
    public int cost;
    public int physPower;
    public int magPower;
    public string cardDescription;
    public Sprite spriteImage;


    public TMP_Text nameText;
    public TMP_Text physPowerText;
    public TMP_Text magPowerText;
    public TMP_Text descriptionText;
    public TMP_Text costText;
    public Image artImage;
    

    void Start()
    {
        displayCards[0] = CardDatabase.cardList[displayId];

    }

    void Update()
    {
        id = displayCards[0].id;
        cardName = displayCards[0].cardName;
        cost = displayCards[0].cost;
        physPower = displayCards[0].physPower;
        magPower = displayCards[0].magPower;
        cardDescription = displayCards[0].cardDescription;
        spriteImage = displayCards[0].spriteImage;

        nameText.text = cardName;
        costText.text = "" + cost;
        physPowerText.text = "" + physPower;
        magPowerText.text = "" + magPower;
        descriptionText.text = cardDescription;
        artImage.sprite = spriteImage;
    }

}
