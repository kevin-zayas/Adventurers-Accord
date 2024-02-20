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
            GameManager.Instance.EndTurn();
        });

        //may be useful for buying card logic

        //purchaseTileButton.onClick.AddListener(() =>
        //{
        //    int pawnPosition = TutorialPlayer.Instance.controlledPawn.currentPosition;

        //    if (TutorialBoard.Instance.Tiles[pawnPosition].owningPlayer == null)
        //    {
        //        TutorialBoard.Instance.ServerSetTileOwner(pawnPosition, TutorialPlayer.Instance);
        //    }
        //});

        base.Initialize();
    }
}
