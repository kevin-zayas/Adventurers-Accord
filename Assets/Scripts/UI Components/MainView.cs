using UnityEngine;
using UnityEngine.UI;

public class MainView : View
{
    [SerializeField]
    private Button endTurnButton;

    public override void Initialize()
    {
        GameManager gm = GameManager.Instance;

        endTurnButton.onClick.AddListener(() =>
        {
            if (gm.CurrentPhase == GameManager.Phase.Draft && gm.Players[gm.Turn].Gold < 5)
            {
                gm.EndTurn(true);
                return;
            }

            //could keep a tracker of Adventurer card count on each player and increment/decrement whenever dispatching or returning.
            //then could check if Adventurer count is > 0 during dispatch phase

            EndTurnPopUp popUp = PopUpManager.Instance.CreateEndTurnPopUp();
            popUp.InitializeEndTurnPopUp();
            GameObject.Find("MainView").GetComponent<MainView>().SetEndTurnButtonActive(false);
        });

        base.Initialize();
    }

    public void SetEndTurnButtonActive(bool value)
    {
        endTurnButton.interactable = value;
    }
}
