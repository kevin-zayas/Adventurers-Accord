using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class ResolutionPopUp : MonoBehaviour
{
    protected string titleText;
    protected string defaultMessageText;
    protected string confirmSelectionText;
    protected string confirmCloseText = "You won't be able to use this ability this round. Are you sure?";
    protected string buttonText;
    protected virtual string ResolutionType => null;
    protected List<AdventurerCardInteractionHandler> clickListeners = new();

    [SerializeField] protected Button leftButton;
    [SerializeField] protected Button rightButton;
    [SerializeField] protected TMP_Text leftButtonText;
    [SerializeField] protected TMP_Text rightButtonText;

    [SerializeField] protected Button closeButton;
    [SerializeField] protected TMP_Text title;
    [SerializeField] protected TMP_Text message;
    [SerializeField] protected Image alertImage;
    [SerializeField] protected Sprite yellowAlert;
    [SerializeField] protected Sprite redAlert;

    [SerializeField] protected QuestLocation QuestLocation;

    public virtual void InitializePopUp(QuestLocation questLocation, string cardName)
    {
        Player.Instance.ServerUpdateGuildRecapTracker($"{cardName} Resolutions Prompted", 1);
        QuestLocation = questLocation;
        transform.SetParent(questLocation.transform);
        transform.localPosition = new Vector3(0, -175f, 0);  //bottom center of quest location
        transform.SetParent(GameObject.Find("Canvas").transform);

        SetPopUpText();
        SetDefaultPopUpSate();
        closeButton.onClick.AddListener(() => SetConfirmClosePopupState());
    }

    protected virtual void SetDefaultPopUpSate()
    {
        AddClickEventListeners(QuestLocation);

        alertImage.gameObject.SetActive(false);
        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);
        leftButton.onClick.RemoveAllListeners();
        rightButton.onClick.RemoveAllListeners();

        title.text = titleText;
        message.text = defaultMessageText;
    }

    public virtual void SetConfirmSelectionState(AdventurerCard card)
    {
        RemoveClickEventListeners();

        alertImage.gameObject.SetActive(true);
        alertImage.sprite = yellowAlert;
        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);
        leftButtonText.text = "Cancel";
        rightButtonText.text = buttonText;

        leftButton.onClick.AddListener(() => SetDefaultPopUpSate());
    }

    protected virtual void SetConfirmClosePopupState()
    {
        alertImage.gameObject.SetActive(true);
        alertImage.sprite = redAlert;
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
            GameManager.Instance.ServerCheckForUnresolvedCards();
            Destroy(this.gameObject);
        });
    }

    protected void HandleEndOfResolution(int questIndex, AdventurerCard card)
    {
        Player.Instance.ServerUpdateGuildRecapTracker($"{ResolutionType} Resolutions Completed", 1);
        UpdateGuildBonusTracker(questIndex);
        card.CurrentCardHolder.Value.QuestLane.ServerUpdateQuestLanePower();

        GameManager.Instance.ServerCheckForUnresolvedCards();
        Destroy(this.gameObject);
    }

    protected abstract void SetPopUpText();

    protected virtual void UpdateGuildBonusTracker(int questIndex) { }
    protected virtual void IsResolutionClickValid(AdventurerCard card) { }

    protected void AddClickEventListeners(QuestLocation questLocation)
    {
        foreach (QuestLane lane in questLocation.QuestLanes)
        {
            foreach (Transform cardSlotTransform in lane.QuestDropZone.transform)
            {
                AdventurerCardInteractionHandler cardHandler = cardSlotTransform.GetChild(0).GetComponent<AdventurerCardInteractionHandler>();
                cardHandler.PointerClickEvent.AddListener(IsResolutionClickValid);
                clickListeners.Add(cardHandler);
            }
        }
    }

    protected void RemoveClickEventListeners()
    {
        foreach (var handler in clickListeners)
        {
            handler.PointerClickEvent.RemoveListener(IsResolutionClickValid);
        }
        clickListeners.Clear();
    }

    public virtual void SetEndTurnButtonActive(bool value)
    {
        return;
    }
}
