using UnityEngine;
using UnityEngine.UI;

public class MainView : View
{
    [SerializeField]
    private Button moveForwardButton;

    [SerializeField]
    private Button moveBackwardButton;

    public override void Initialize()
    {
        moveForwardButton.onClick.AddListener(() => Player.Instance.controlledPawn.ServerMove(-1));
        moveBackwardButton.onClick.AddListener(() => Player.Instance.controlledPawn.ServerMove(-1));

        
        base.Initialize();
    }
}
