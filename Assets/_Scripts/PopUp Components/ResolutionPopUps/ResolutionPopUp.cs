using FishNet.Object;
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
        QuestLocation.ServerSetAllowResolution(true);

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
        QuestLocation.ServerSetAllowResolution(false);

        alertImage.gameObject.SetActive(true);
        alertImage.sprite = yellowAlert;
        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);
        leftButtonText.text = "Cancel";
        rightButtonText.text = buttonText;

        leftButton.onClick.AddListener(() => SetDefaultPopUpSate());
    }

    protected void SetConfirmClosePopupState()
    {
        QuestLocation.ServerSetAllowResolution(false);

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
        Player.Instance.ServerUpdateGuildRecapTracker($"{PopUpManager.Instance.CurrentResolutionType} Resolutions Completed", 1);
        UpdateGuildBonusTracker(questIndex);
        card.ParentTransform.Value.parent.GetComponent<QuestLane>().ServerUpdateQuestLanePower();

        GameManager.Instance.ServerCheckForUnresolvedCards();
        Destroy(this.gameObject);
    }

    protected abstract void SetPopUpText();

    protected virtual void UpdateGuildBonusTracker(int questIndex) { }

}
