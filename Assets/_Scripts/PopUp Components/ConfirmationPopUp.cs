using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationPopUp : PopUp
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

        RectTransform titleRect = titleText.GetComponent<RectTransform>();      // modify transform position for better formatting
        titleRect.anchoredPosition = new Vector2(0f, 15f);

        RectTransform messageRect = messageText.GetComponent<RectTransform>();
        messageRect.anchoredPosition = new Vector2(0f, -25f);
    }

    public void InitializeRestartServerPopUp()
    {
        cancelButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });

        confirmButton.onClick.AddListener(() =>
        {
            DeploymentManager.Instance.InitiateServerRestart();
        });

        titleText.text = restartServerTitle;
        messageText.text = restartServerMessage;
    }
}
