using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUp : NetworkBehaviour
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
    //[SerializeField] NetworkConnection networkConnection;

    string titleText;
    string defaultMessageText;
    string confirmSelectionText;
    string confirmCloseText;

    public void InitializePopUp(QuestLocation questLocation)
    {
        print("loading rogue popup info");
        closeButton.onClick.AddListener(() =>
        {
            SetConfirmClosePopupState();
        });

        transform.SetParent(questLocation.transform);
        transform.localPosition = new Vector3(0, -300f, 0);  //bottom center of quest location
        QuestLocation = questLocation;

        SetRogueText();
        SetDefaultPopUpSate();
    }
    private void SetDefaultPopUpSate()
    {
        print("InitializeRoguePopup");
        QuestLocation.ServerSetCanRogueSteal(true);

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
        print("Setting up rogue selection popup");
        QuestLocation.ServerSetCanRogueSteal(false);

        alertImage.gameObject.SetActive(true);
        alertImage.sprite = yellowAlert;
        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);

        leftButtonText.text = "Cancel";
        rightButtonText.text = "'Borrow'";

        print(confirmSelectionText);
        message.text = string.Format(confirmSelectionText, card.Name, card.Item.Name);

        leftButton.onClick.AddListener(() =>
        {
            SetDefaultPopUpSate();

        });

        rightButton.onClick.AddListener(() =>
        {
            card.Item.ServerDisableItem();
            card.Parent.parent.GetComponent<QuestLane>().ServerUpdatePower();

            GameManager.Instance.ServerCheckForUnresolvedCards();
            PopUpManager.Instance.ServerDespawnPopUp(this);

        });
    }

    public void SetConfirmClosePopupState()
    {
        print("Setting close confirmation popup");
        QuestLocation.ServerSetCanRogueSteal(false);

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
            PopUpManager.Instance.ServerDespawnPopUp(this);
        });
    }

    private void SetRogueText()
    {
        titleText = PopUpManager.Instance.RogueTitleText;
        defaultMessageText = PopUpManager.Instance.RogueDefaultMessageText;
        confirmSelectionText = PopUpManager.Instance.RogueConfirmSelectionText;
        confirmCloseText = PopUpManager.Instance.RogueConfirmCloseText;
    }

    private void SetAssassinText()
    {

    }

}
