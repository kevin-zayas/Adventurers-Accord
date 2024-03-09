using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndRoundView : View
{
    [SerializeField]
    private Button endRoundButton;

    public int playerID;

    public override void Initialize()
    {
        endRoundButton.onClick.AddListener(() =>
        {
            GameManager.Instance.ConfirmEndRound(playerID);
            endRoundButton.interactable = false;
        });

        base.Initialize();
    }

    public void EnableEndRoundButton()
    {
        endRoundButton.interactable = true;
    }
}
