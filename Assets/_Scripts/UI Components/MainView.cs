using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainView : View
{
    public TMP_Text text;

    public override void Initialize()
    {
        endTurnButton.onClick.AddListener(() =>
        {
            if (PlayerPrefs.GetInt("ShowEndTurnConfirmation") == 1)
            {
                ConfirmationPopUp popUp = PopUpManager.Instance.CreateConfirmationPopUp();
                popUp.InitializeEndTurnPopUp();
            }
            else
            {
                GameManager.Instance.EndTurn(true);
            }
            
        });

        base.Initialize();
    }

    public override void Show(object args = null) 
    {
        base.Show(args);
        text.GetComponent<FlashingEffect>().FlashEffect();
    }
}
