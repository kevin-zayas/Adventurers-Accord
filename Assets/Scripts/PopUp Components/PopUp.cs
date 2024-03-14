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

    public void CreateRoguePopUp(QuestLocation questLocation)
    {
        print("loading rogue popup info");
        closeButton.onClick.AddListener(() =>
        {
            ConfirmClosePopUp();
        });

        transform.SetParent(questLocation.transform);
        transform.localPosition = new Vector3(0, -300f, 0);  //bottom center of quest location

        QuestLocation = questLocation;
        InitializeRoguePopup();
    }
    private void InitializeRoguePopup()
    {
        print("InitializeRoguePopup");
        QuestLocation.ServerSetCanRogueSteal(true);

        alertImage.gameObject.SetActive(false);

        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);

        leftButton.onClick.RemoveAllListeners();
        rightButton.onClick.RemoveAllListeners();

        title.text = "Sticky Fingers";
        message.text = "Please choose an Adventurer on this Quest to 'borrow' an item from.";
    }

    public void ConfirmRogueSelectionPopUp(Card card)
    {
        print("Setting up rogue selection popup");
        QuestLocation.ServerSetCanRogueSteal(false);

        alertImage.gameObject.SetActive(true);
        alertImage.sprite = yellowAlert;
        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);

        leftButtonText.text = "Cancel";
        rightButtonText.text = "'Borrow'";

        message.text = $"Are you sure you want to 'borrow' this {card.Name}'s {card.Item.Name}?";

        leftButton.onClick.AddListener(() =>
        {
            InitializeRoguePopup();

        });

        rightButton.onClick.AddListener(() =>
        {
            card.Item.ServerDisableItem();
            card.Parent.parent.GetComponent<QuestLane>().ServerUpdatePower();

            GameManager.Instance.ServerCheckForUnresolvedCards();
            PopUpManager.Instance.ServerDespawnPopUp(this);

        });
    }

    public void ConfirmClosePopUp()
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

        message.text = $"Are you sure you don't want to 'borrow' an item this round?";

        leftButton.onClick.AddListener(() =>
        {
            InitializeRoguePopup();

        });

        rightButton.onClick.AddListener(() =>
        {
            GameManager.Instance.ServerCheckForUnresolvedCards();
            PopUpManager.Instance.ServerDespawnPopUp(this);
        });
    }

}
