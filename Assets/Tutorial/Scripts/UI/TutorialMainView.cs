using UnityEngine;
using UnityEngine.UI;

public class TutorialMainView : TutorialView
{
    [SerializeField]
    private Button purchaseTileButton;

    [SerializeField]
    private Button moveForwardButton;

    [SerializeField]
    private Button moveBackwardButton;

    public override void Initialize()
    {
        purchaseTileButton.onClick.AddListener(() =>
        {
            int pawnPosition = TutorialPlayer.Instance.controlledPawn.currentPosition;

            if (TutorialBoard.Instance.Tiles[pawnPosition].owningPlayer == null)
            {
                TutorialBoard.Instance.ServerSetTileOwner(pawnPosition, TutorialPlayer.Instance);
            }
        });
        moveForwardButton.onClick.AddListener(() => TutorialPlayer.Instance.controlledPawn.ServerMove(1));
        moveBackwardButton.onClick.AddListener(() => TutorialPlayer.Instance.controlledPawn.ServerMove(-1));

        
        base.Initialize();
    }
}
