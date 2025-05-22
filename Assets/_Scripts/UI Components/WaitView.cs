using TMPro;
using UnityEngine;

public class WaitView : View
{
    [SerializeField] private TMP_Text waitingText;
    public override void Show(object args = null)
    {
        base.Show(args);
        waitingText.text = $"Waiting for Player {GameManager.Instance.ClientTurn + 1}";
    }
}
