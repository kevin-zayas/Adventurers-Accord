using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionResolutionPopUp : ResolutionPopUp
{
    PotionCard potionCard;

    public void InitializePopUp(QuestLocation questLocation, PotionCard potionCard)
    {
        this.potionCard = potionCard;
        confirmCloseText = "Do you want to cancel the use of this potion?";
        base.InitializePopUp(questLocation, potionCard.CardName.Value);
    }

    public override void SetConfirmSelectionState(AdventurerCard card)
    {
        base.SetConfirmSelectionState(card);
        message.text = string.Format(confirmSelectionText, card.CardName.Value);

        rightButton.onClick.AddListener(() =>
        {
            potionCard.UsePotion(card);
            card.ParentTransform.Value.parent.GetComponent<QuestLane>().ServerUpdateQuestLanePower();
            potionCard.ServerDespawnCard();
            Destroy(gameObject);
        });
    }

    protected override void SetConfirmClosePopupState()
    {
        QuestLocation.ServerSetAllowResolution(false);

        alertImage.gameObject.SetActive(true);
        alertImage.sprite = yellowAlert;
        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);
        leftButton.onClick.RemoveAllListeners();
        rightButton.onClick.RemoveAllListeners();

        leftButtonText.text = "Go Back";
        rightButtonText.text = "Yes";
        message.text = confirmCloseText;

        leftButton.onClick.AddListener(() => SetDefaultPopUpSate());
        rightButton.onClick.AddListener(() =>
        {
            potionCard.transform.SetParent(potionCard.ControllingPlayerHand.Value.transform, false);
            potionCard.gameObject.SetActive(true);
            Destroy(this.gameObject);
        });
    }

    protected override void SetPopUpText()
    {
        titleText = potionCard.CardName.Value;
        defaultMessageText = "Please choose an Adventurer to give this potion.";
        confirmSelectionText = "Are you sure you want use this potion on this {0}?";
        buttonText = "Use";
    }

    protected override void UpdateGuildBonusTracker(int questIndex)
    {
        return;
    }
}
