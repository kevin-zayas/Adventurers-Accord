using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionPopUp : NetworkBehaviour
{
    private const string RogueTitleText = "Sticky Fingers";
    private const string RogueDefaultMessageText = "Please choose an Adventurer on this Quest to \"borrow\" an item from.";
    private const string RogueConfirmSelectionText = "Are you sure you want to \"borrow\" this {0}'s {1}?";
    private const string RogueConfirmCloseText = "Are you sure you don't want to \"borrow\" an item this round?";
    private const string RogueButtonText = "\"Borrow\"";

    private const string AssassinTitleText = "Poisonous Blade";
    private const string AssassinDefaultMessageText = "Please choose an Adventurer on this Quest to poison.";
    private const string AssassinConfirmSelectionText = "Are you sure you want to poison this {0}?";
    private const string AssassinConfirmCloseText = "Are you sure you don't want to poison an Adventurer this round?";
    private const string AssassinButtonText = "Poison";
    private const string AssassinConfirmStatText = "Would you like to target this {0}'s Physical or Magical Power?";

    [SerializeField] Button leftButton;
    [SerializeField] Button rightButton;
    [SerializeField] TMP_Text leftButtonText;
    [SerializeField] TMP_Text rightButtonText;

    [SerializeField] Button closeButton;
    [SerializeField] TMP_Text title;
    [SerializeField] TMP_Text message;
    [SerializeField] Image alertImage;
    [SerializeField] Sprite yellowAlert;
    [SerializeField] Sprite redAlert;
    //[SerializeField] Image noticeImage;

    [SerializeField] QuestLocation QuestLocation;
    
    [field:SerializeField]
    public string ResolutionType { get; private set; }

    string titleText;
    string defaultMessageText;
    string confirmSelectionText;
    string confirmCloseText;
    string buttonText;

    public void InitializePopUp(QuestLocation questLocation, string cardName)
    {
        print("initializing pop up");

        transform.SetParent(questLocation.transform);
        transform.localPosition = new Vector3(0, -300f, 0);  //bottom center of quest location
        QuestLocation = questLocation;
        ResolutionType = cardName;

        if (ResolutionType == "Rogue") SetRogueText();
        else if (ResolutionType == "Assassin") SetAssassinText();

        SetDefaultPopUpSate();
    }
    private void SetDefaultPopUpSate()
    {
        print("Setting default state");
        QuestLocation.ServerSetAllowResolution(true);

        alertImage.gameObject.SetActive(false);

        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);

        leftButton.onClick.RemoveAllListeners();
        rightButton.onClick.RemoveAllListeners();

        title.text = titleText;
        message.text = defaultMessageText;
    }

    public void SetConfirmSelectionState(Card card)
    {
        print("setting confirm selection state");
        QuestLocation.ServerSetAllowResolution(false);

        alertImage.gameObject.SetActive(true);
        alertImage.sprite = yellowAlert;
        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);

        leftButtonText.text = "Cancel";
        rightButtonText.text = buttonText;

        if (ResolutionType == "Rogue") message.text = string.Format(confirmSelectionText, card.Name, card.Item.Name);
        else if (ResolutionType == "Assassin") message.text = string.Format(confirmSelectionText, card.Name);


        leftButton.onClick.AddListener(() =>
        {
            SetDefaultPopUpSate();

        });

        rightButton.onClick.AddListener(() =>
        {
            if (ResolutionType == "Rogue") card.ServerDisableItem("Stolen");
            else if (ResolutionType == "Assassin")
            {
                if (card.PhysicalPower > 0 && card.MagicalPower == 0) card.ServerChangePhysicalPower(-2);
                else if (card.PhysicalPower == 0 && card.MagicalPower > 0) card.ServerChangeMagicalPower(-2);
                else if (card.PhysicalPower > 0 && card.MagicalPower > 0)
                {
                    SetAssassinConfirmStatPopupState(card);
                    return;
                }
            }
            
            card.Parent.parent.GetComponent<QuestLane>().ServerUpdateQuestLanePower();

            GameManager.Instance.ServerCheckForUnresolvedCards();
            PopUpManager.Instance.ServerDespawnResolutionPopUp(this);

        });
    }

    public void SetConfirmClosePopupState()
    {
        print("Setting close confirmation state");
        QuestLocation.ServerSetAllowResolution(false);

        alertImage.gameObject.SetActive(true);
        alertImage.sprite = redAlert;
        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);

        leftButton.onClick.RemoveAllListeners();
        rightButton.onClick.RemoveAllListeners();

        leftButtonText.text = "Cancel";
        rightButtonText.text = "Yes";

        message.text = confirmCloseText;

        leftButton.onClick.AddListener(() =>
        {
            SetDefaultPopUpSate();

        });

        rightButton.onClick.AddListener(() =>
        {
            GameManager.Instance.ServerCheckForUnresolvedCards();
            PopUpManager.Instance.ServerDespawnResolutionPopUp(this);
        });
    }

    public void SetAssassinConfirmStatPopupState(Card card)
    {
        print("Setting assassin confirm stat popup state");

        leftButton.onClick.RemoveAllListeners();
        rightButton.onClick.RemoveAllListeners();

        leftButtonText.text = "Physical";
        rightButtonText.text = "Magical";

        message.text = string.Format(AssassinConfirmStatText,card.Name);

        leftButton.onClick.AddListener(() =>
        {
            card.ServerChangePhysicalPower(-2);
            card.Parent.parent.GetComponent<QuestLane>().ServerUpdateQuestLanePower();

            GameManager.Instance.ServerCheckForUnresolvedCards();
            PopUpManager.Instance.ServerDespawnResolutionPopUp(this);

        });

        rightButton.onClick.AddListener(() =>
        {
            card.ServerChangeMagicalPower(-2);
            card.Parent.parent.GetComponent<QuestLane>().ServerUpdateQuestLanePower();

            GameManager.Instance.ServerCheckForUnresolvedCards();
            PopUpManager.Instance.ServerDespawnResolutionPopUp(this);
        });
    }

    private void SetRogueText()
    {
        titleText = RogueTitleText;
        defaultMessageText = RogueDefaultMessageText;
        confirmSelectionText = RogueConfirmSelectionText;
        confirmCloseText = RogueConfirmCloseText;
        buttonText = RogueButtonText;
    }

    private void SetAssassinText()
    {
        titleText = AssassinTitleText;
        defaultMessageText = AssassinDefaultMessageText;
        confirmSelectionText = AssassinConfirmSelectionText;
        confirmCloseText = AssassinConfirmCloseText;
        buttonText = AssassinButtonText;
    }

}
