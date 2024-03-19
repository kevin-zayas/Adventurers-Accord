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
        closeButton.onClick.AddListener(() =>
        {
            SetConfirmClosePopupState();
        });

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
            if (ResolutionType == "Rogue") card.Item.ServerDisableItem();
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
            
            card.Parent.parent.GetComponent<QuestLane>().ServerUpdatePower();

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

        message.text = string.Format(PopUpManager.Instance.AssassinConfirmStatText,card.Name);

        leftButton.onClick.AddListener(() =>
        {
            card.ServerChangePhysicalPower(-2);
            card.Parent.parent.GetComponent<QuestLane>().ServerUpdatePower();

            GameManager.Instance.ServerCheckForUnresolvedCards();
            PopUpManager.Instance.ServerDespawnResolutionPopUp(this);

        });

        rightButton.onClick.AddListener(() =>
        {
            card.ServerChangeMagicalPower(-2);
            card.Parent.parent.GetComponent<QuestLane>().ServerUpdatePower();

            GameManager.Instance.ServerCheckForUnresolvedCards();
            PopUpManager.Instance.ServerDespawnResolutionPopUp(this);
        });
    }

    private void SetRogueText()
    {
        titleText = PopUpManager.Instance.RogueTitleText;
        defaultMessageText = PopUpManager.Instance.RogueDefaultMessageText;
        confirmSelectionText = PopUpManager.Instance.RogueConfirmSelectionText;
        confirmCloseText = PopUpManager.Instance.RogueConfirmCloseText;
        buttonText = PopUpManager.Instance.RogueButtonText;
    }

    private void SetAssassinText()
    {
        titleText = PopUpManager.Instance.AssassinTitleText;
        defaultMessageText = PopUpManager.Instance.AssassinDefaultMessageText;
        confirmSelectionText = PopUpManager.Instance.AssassinConfirmSelectionText;
        confirmCloseText = PopUpManager.Instance.AssassinConfirmCloseText;
        buttonText = PopUpManager.Instance.AssassinButtonText;
    }

}
