using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndRoundView : View
{
    [SerializeField] private Button endRoundButton;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private GameObject turnTracker;
    [SerializeField] private FlashingEffect flash;
    

    public override void Initialize()
    {
        endRoundButton.onClick.AddListener(() =>
        {
            GameManager.Instance.ConfirmEndRound(Player.Instance.PlayerID.Value);
            endRoundButton.interactable = false;
            turnTracker.SetActive(false);
        });

        base.Initialize();
    }

    public void EnableEndRoundUI()
    {
        if (!endRoundButton.interactable)
        {
            endRoundButton.interactable = true;
            turnTracker.SetActive(true);
            flash.FlashEffect();
        }
    }
    public override void Show(object args = null)
    {
        base.Show(args);
        flash.FlashEffect();
    }
}
