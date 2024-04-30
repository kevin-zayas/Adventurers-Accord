using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public int id;
    public string cardName;
    public int cost;
    public int physPower;
    public int magPower;
    public int slotIndex;
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


    }

    public void LoadCardData(OldCard CardData)
    {
        id = CardData.id;
        cardName = CardData.cardName;
        cost = CardData.cost;
        physPower = CardData.physPower;
        magPower = CardData.magPower;
        cardDescription = CardData.cardDescription;
        spriteImage = CardData.spriteImage;

        // put this in a OnCardDataChanged method and add a UnityEvent whenever those are changed
        nameText.text = cardName;
        costText.text = cost.ToString();
        physPowerText.text = physPower.ToString();
        magPowerText.text = magPower.ToString();
        descriptionText.text = cardDescription;
        artImage.sprite = spriteImage;
    }

    void Update()
    {



    }


}
