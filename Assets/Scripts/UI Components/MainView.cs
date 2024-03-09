using UnityEngine;
using UnityEngine.UI;

public class MainView : View
{
    [SerializeField]
    private Button endTurnButton;

    public override void Initialize()
    {
        endTurnButton.onClick.AddListener(() =>
        {
            GameManager.Instance.EndTurn(true);
        });

        base.Initialize();
    }
}
