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
    const string recruitEndTurnMessage = "You will be unable to recruit Adventurers until next round.";
    const string dispatchEndTurnMessage = "You will be unable to dispatch Adventurers until next round.";

    // Equip Item Confirmation
    const string equipItemTitle = "Equip this {0} on your {1}?";
    const string equipItemMessage = "You will not be able unequip this Item.";

    // Use Spell Confirmation
    const string castSpellOtherTitle = "Cast {0} {1} Player {2}'s party?";
    const string castSpellSelfTitle = "Cast {0} {1} your party?";
    const string castSpellMessage = "You will not be able to undo this action";

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

    public void InitializeEquipItemPopUp(AdventurerCard adventurerCard, ItemCard itemCard)
    {
        cancelButton.onClick.AddListener(() =>
        {
            itemCard.ServerSetCardParent(itemCard.ControllingPlayerHand.Value.transform, true);
            Destroy(gameObject);
        });

        confirmButton.onClick.AddListener(() =>
        {
            adventurerCard.ServerEquipItem(true, itemCard.Data.Value);
            itemCard.ServerDespawnItem();
            Destroy(gameObject);
        });

        titleText.text = string.Format(equipItemTitle, itemCard.CardName.Value, adventurerCard.CardName.Value);
        messageText.text = equipItemMessage;

        FormatTextTransforms();
    }

    public void InitializeCastSpellPopUp(GameObject dropZone, SpellCard spellCard)
    {
        QuestLane questLane = dropZone.transform.parent.GetComponent<QuestLane>();
        string preposition;

        cancelButton.onClick.AddListener(() =>
        {
            spellCard.ServerSetCardParent(spellCard.ControllingPlayerHand.Value.transform, true);
            Destroy(gameObject);
        });

        confirmButton.onClick.AddListener(() =>
        {
            spellCard.ServerSetCardParent(dropZone.transform, false);
            questLane.ServerUpdateSpellEffects();

            if (GameManager.Instance.CurrentPhase.Value == GameManager.Phase.Magic)
            {
                GameManager.Instance.ServerRefreshEndRoundStatus();
            }

            Destroy(gameObject);
        });

        preposition = spellCard.IsNegativeEffect.Value ? "on" : "for";
        
        if (spellCard.ControllingPlayer.Value == questLane.Player.Value)
        {
            titleText.text = string.Format(castSpellSelfTitle, spellCard.CardName.Value, preposition);
        }
        else
        {
            titleText.text = string.Format(castSpellOtherTitle, spellCard.CardName.Value, preposition, questLane.Player.Value.PlayerID.Value + 1);
        }
                
        messageText.text = castSpellMessage;
        FormatTextTransforms();
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

    private void FormatTextTransforms()
    {
        RectTransform titleRect = titleText.GetComponent<RectTransform>();      // modify transform position for better formatting
        titleRect.anchoredPosition = new Vector2(0f, 15f);

        RectTransform messageRect = messageText.GetComponent<RectTransform>();
        messageRect.anchoredPosition = new Vector2(0f, -25f);
    }
}
