using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndRoundView : View
{
    [SerializeField]
    private Button endRoundButton;

    public int playerID;
    public TMP_Text text;

    public override void Initialize()
    {
        endRoundButton.onClick.AddListener(() =>
        {
            GameManager.Instance.ConfirmEndRound(playerID);
            endRoundButton.interactable = false;
            text.gameObject.SetActive(false);
        });

        base.Initialize();
    }

    public void EnableEndRoundUI()
    {
        if (!endRoundButton.interactable)
        {
            endRoundButton.interactable = true;
            text.gameObject.SetActive(true);
            text.GetComponent<FlashingEffect>().FlashEffect();
        }
    }
    public override void Show(object args = null)
    {
        base.Show(args);
        text.GetComponent<FlashingEffect>().FlashEffect();
    }
}
