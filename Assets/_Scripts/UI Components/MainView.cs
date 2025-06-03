using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class MainView : View
{
    [SerializeField] private TMP_Text yourTurnText;
    [SerializeField] private TMP_Text oddJobsGold;
    [SerializeField] private GameObject oddJobsBanner;

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
        yourTurnText.GetComponent<FlashingEffect>().FlashEffect();
    }

    public void UpdateOddJobsBanner(bool isEnabled)
    {
        oddJobsBanner.SetActive(isEnabled);

        int oddJobGold = 1 + (GameManager.Instance.RoundNumber.Value + 1) / 2;
        oddJobsGold.text = oddJobGold.ToString();
    }
}
