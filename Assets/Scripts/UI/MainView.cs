using UnityEngine;
using UnityEngine.UI;

public class MainView : View
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
            int pawnPosition = Player.Instance.controlledPawn.currentPosition;

            if (Board.Instance.Tiles[pawnPosition].owningPlayer == null)
            {
                Board.Instance.ServerSetTileOwner(pawnPosition, Player.Instance);
            }
        });
        moveForwardButton.onClick.AddListener(() => Player.Instance.controlledPawn.ServerMove(1));
        moveBackwardButton.onClick.AddListener(() => Player.Instance.controlledPawn.ServerMove(-1));

        
        base.Initialize();
    }
}
