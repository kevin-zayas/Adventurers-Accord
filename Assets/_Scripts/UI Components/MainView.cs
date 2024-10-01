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
            if (gm.CurrentPhase.Value == GameManager.Phase.Recruit && gm.Players[gm.Turn].Gold.Value < 5)
            {
                gm.EndTurn(true);
                return;
            }

            //could keep a tracker of Adventurer card count on each player and increment/decrement whenever dispatching or returning.
            //then could check if Adventurer count is > 0 during dispatch phase

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
