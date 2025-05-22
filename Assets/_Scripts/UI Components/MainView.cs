using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainView : View
{
    [SerializeField] private Button endTurnButton;

    public TMP_Text text;

    public override void Initialize()
    {
        GameManager gm = GameManager.Instance;

        endTurnButton.onClick.AddListener(() =>
        {
            ConfirmationPopUp popUp = PopUpManager.Instance.CreateConfirmationPopUp();
            popUp.InitializeEndTurnPopUp();
        });

        base.Initialize();
    }

    public override void Show(object args = null) 
    {
        base.Show(args);
        text.GetComponent<FlashingEffect>().FlashEffect();
    }


    public void SetEndTurnButtonActive(bool value)
    {
        endTurnButton.interactable = value;
    }
}
