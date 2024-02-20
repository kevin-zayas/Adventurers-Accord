using System.Linq;
using DG.Tweening;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class TutorialPawn : NetworkBehaviour
{
    [SyncVar]
    public TutorialPlayer controllingPlayer;

    public int currentPosition;

    private bool _isMoving;

    [ServerRpc(RequireOwnership = false)]
    public void ServerMove(int steps)   // moving from server side, not client side so Pawn is not client authoratative (Client Authoratative = false)
    {
        if (_isMoving) return;

        _isMoving = true;

        TutorialTile[] tiles = TutorialBoard.Instance.Slice(currentPosition, currentPosition + steps);

        int controllingPlayerIndex = TutorialGameManager.Instance.Players.IndexOf(controllingPlayer);

        Vector3[] path = tiles.Select(tile => tile.PawnPositions[controllingPlayerIndex].position).ToArray();

        Tween tween = transform.DOPath(path, 1.25f);

        tween.OnComplete(() =>
        {
            _isMoving = false;

            ObserversSetCurrentPosition(currentPosition + steps);

            TutorialGameManager.Instance.EndTurn();
        });

        tween.Play();
    }

    [ObserversRpc(BufferLast = true)]
    public void ObserversSetCurrentPosition(int value)
    {
        currentPosition = value;
    }
}
