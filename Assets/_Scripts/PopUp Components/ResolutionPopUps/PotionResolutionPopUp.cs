using UnityEngine;
using static PotionCard;

public class PotionResolutionPopUp : ResolutionPopUp
{
    protected override string ResolutionType => "Potion";
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
            GameManager.Instance.ServerResetEndRoundConfirmations();
            
            QuestLane questLane = card.CurrentCardHolder.Value.QuestLane;

            potionCard.UsePotion(card);
            questLane.ServerUpdateQuestLanePower();
            potionCard.CurrentCardHolder.Value.ServerMoveCard(potionCard, null, potionCard.transform.parent);
            PopUpManager.Instance.ClearResolutionType();
            SetEndTurnButtonActive(true);
            Destroy(gameObject);
        });
    }

    protected override void SetConfirmClosePopupState()
    {
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
            potionCard.CardHandler.InvokeEndDrag();
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

    protected override void IsResolutionClickValid(AdventurerCard card)
    {
        if (card.ControllingPlayer.Value != Player.Instance)
        {
            PopUpManager.Instance.CreateToastPopUp("Cannot use Potion: Adventurer does not belong to the player");
            return;
        }
        if (card.CardName.Value == "Wolf")      // TODO: Replace with isSummon check
        {
            PopUpManager.Instance.CreateToastPopUp("Cannot use Potion: Summons cannot use potions");
            return;
        }

        Potion potionType = potionCard.PotionType.Value;
        bool hasPower = (card.OriginalPhysicalPower.Value > 0 || card.potionBasePhysicalPower.Value > 0) || // TODO: Remove this when relaxing empowerment restrictions
                        (card.OriginalMagicalPower.Value > 0 || card.potionBaseMagicalPower.Value > 0);

        bool validTarget = potionType switch
        {
            Potion.Healing => card.CurrentRestPeriod.Value > 0,
            Potion.Power => hasPower,
            Potion.Strength => true,
            Potion.Intelligence => true,
            _ => false
        };

        string errorMessage = potionType switch
        {
            Potion.Healing => "Cannot use Potion: Adventurer's Rest Period is already 0",
            Potion.Power => "Cannot use Potion: Target must have Physical or Magical Power",
            Potion.Strength => "",
            Potion.Intelligence => "",
            _ => "Unknown potion type"
        };

        if (validTarget) 
            PopUpManager.Instance.CurrentResolutionPopUp.SetConfirmSelectionState(card);
        else
            PopUpManager.Instance.CreateToastPopUp(errorMessage);
    }

    public override void SetEndTurnButtonActive(bool value)
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
