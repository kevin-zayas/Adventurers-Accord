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
    [SerializeField] Image noticeImage;

    [SerializeField] QuestLocation QuestLocation;
    //[SerializeField] NetworkConnection networkConnection;

    public void CreateRoguePopUp(QuestLocation questLocation)
    {
        print("loading rogue popup info");
        closeButton.onClick.AddListener(() =>
        {
            //create popup to confirm not stealing this turn
            Destroy(gameObject);
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

        noticeImage.gameObject.SetActive(false);

        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);

        leftButton.onClick.RemoveAllListeners();
        rightButton.onClick.RemoveAllListeners();

        title.text = "Sticky Fingers";
        message.text = "Please choose an Adventurer to 'borrow' an item from.";
    }

    public void ConfirmRogueSelectionPopUp(Card card)
    {
        print("Setting up rogue selection popup");
        QuestLocation.ServerSetCanRogueSteal(false);

        noticeImage.gameObject.SetActive(true);
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
            card.Item.ServerChangeMagicalPower(-1);         //make a Item.Disable() method to handle this that adds a gray cover
            card.Item.ServerChangePhysicalPower(-1);
            card.Parent.parent.GetComponent<QuestLane>().ServerUpdatePower();

            GameManager.Instance.ServerCheckForUnresolvedCards();
            PopUpManager.Instance.ServerDespawnPopUp(this);

        });
    }

    //[ServerRpc(RequireOwnership = false)]
    //private void ServerDespawnPopUp()
    //{
    //    Despawn(this.gameObject);
    //}


}
