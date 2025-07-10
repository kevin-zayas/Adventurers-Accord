using UnityEngine;

public class PotionResolutionPopUp : ResolutionPopUp
{
    PotionCard potionCard;

    public void InitializePopUp(QuestLocation questLocation, PotionCard potionCard)
    {
        this.potionCard = potionCard;
        confirmCloseText = "Do you want to cancel the use of this potion?";
        base.InitializePopUp(questLocation, potionCard.CardName.Value);
        SetEndTurnButtonActive(false);
    }

    public override void SetConfirmSelectionState(AdventurerCard card)
    {
        base.SetConfirmSelectionState(card);
        message.text = string.Format(confirmSelectionText, card.CardName.Value);

        rightButton.onClick.AddListener(() =>
        {
            if (GameManager.Instance.CurrentPhase.Value == GameManager.Phase.Magic)
            {
                GameManager.Instance.ServerResetEndRoundConfirmations();
            }
            QuestLane questLane = card.CurrentCardHolder.Value.QuestLane;

            potionCard.UsePotion(card);
            questLane.ServerUpdateQuestLanePower();
            potionCard.ServerDespawnCard();
            PopUpManager.Instance.ClearResolutionType();
            SetEndTurnButtonActive(true);
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
            PopUpManager.Instance.ClearResolutionType();
            SetEndTurnButtonActive(true);
            Destroy(gameObject);
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

    private void SetEndTurnButtonActive(bool value)
    {
        GameManager.Phase phase = GameManager.Instance.CurrentPhase.Value;
        View view;

        switch (phase)
        {
            case GameManager.Phase.Dispatch:
                view = GameObject.Find("MainView").GetComponent<MainView>();
                break;
            case GameManager.Phase.Magic:
                view = GameObject.Find("EndRoundView").GetComponent<EndRoundView>();
                break;
            default:
                Debug.LogError("Error - Potion quest drag logic reached during invalid phase");
                return;
        }

        view.SetButtonInteractable(value);
    }
}
