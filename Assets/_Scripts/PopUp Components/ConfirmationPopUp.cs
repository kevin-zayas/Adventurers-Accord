using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationPopUp : MonoBehaviour
{
    [SerializeField] Button cancelButton;
    [SerializeField] Button confirmButton;
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text messageText;

    // End Turn Confirmation
    const string endTurnTitle = "End Turn?";
    const string recruitEndTurnMessage = "You will not be able to recruit an Adventurer until next round.";
    const string dispatchEndTurnMessage = "You will not be able to dispatch an Adventurer until next round.";

    // Equip Item Confirmation
    const string equipItemTitle = "Are you sure you want to equip this {0} on this {1}?";
    const string equipItemMessage = "You will not be able unequip this Item.";
    private AdventurerCard adventurerCard;
    private GameObject itemCardObject;

    // Restart Server Confirmation
    const string restartServerTitle = "Restart Server?";
    const string restartServerMessage = "This will reset the game for all players and cannot be undone.";

    void Start()
    {
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        RectTransform rt = this.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;

    }

    public void InitializeEndTurnPopUp()
    {
        cancelButton.onClick.AddListener(() =>
        {
            GameObject.Find("MainView").GetComponent<MainView>().SetEndTurnButtonActive(true);
            Destroy(gameObject);
        });

        confirmButton.onClick.AddListener(() =>
        {
            GameManager.Instance.EndTurn(true);
            GameObject.Find("MainView").GetComponent<MainView>().SetEndTurnButtonActive(true);
            Destroy(gameObject);
        });

        titleText.text = endTurnTitle;
        if (GameManager.Instance.CurrentPhase.Value == GameManager.Phase.Recruit) messageText.text = recruitEndTurnMessage;
        else messageText.text = dispatchEndTurnMessage;
    }

    public void InitializeEquipItemPopUp(AdventurerCard card, GameObject item)
    {
        adventurerCard = card;
        itemCardObject = item;

        cancelButton.onClick.AddListener(() =>
        {
            ItemCard itemCard = itemCardObject.GetComponent<ItemCard>();
            itemCard.ServerSetCardParent(itemCard.ControllingPlayerHand.Value.transform, true);
            Destroy(gameObject);
        });

        confirmButton.onClick.AddListener(() =>
        {
            adventurerCard.ServerEquipItem(true, itemCardObject.GetComponent<ItemCard>().Data.Value);
            itemCardObject.GetComponent<ItemCard>().ServerDespawnItem();
            Destroy(gameObject);
        });

        titleText.text = string.Format(equipItemTitle, item.GetComponent<ItemCard>().CardName.Value, card.CardName.Value);
        messageText.text = equipItemMessage;

        //RectTransform titleRect = titleText.GetComponent<RectTransform>();      // modify transform to avoid message cutoff
        //titleRect.position = new Vector2(0f, 15f);

        //RectTransform messageRect = messageText.GetComponent<RectTransform>();      // modify transform to avoid message cutoff
        //messageRect.position = new Vector2(0f, -25f);


    }

    public void InitializeRestartServerPopUp()
    {
        cancelButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });

        confirmButton.onClick.AddListener(() =>
        {
            ApiManager.Instance.RestartGameServer();
            //Destroy(gameObject);
        });

        titleText.text = restartServerTitle;
        messageText.text = restartServerMessage;
    }
}
