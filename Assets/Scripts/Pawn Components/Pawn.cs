using System.Linq;
using DG.Tweening;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Pawn : NetworkBehaviour
{
    [SyncVar]
    public Player controllingPlayer;

    public int currentPosition;

    private bool _isMoving;

    [ServerRpc(RequireOwnership = false)]
    public void ServerMove(int steps)   // moving from server side, not client side so Pawn is not client authoratative (Client Authoratative = false)
    {
        if (_isMoving) return;

        _isMoving = true;

        Tile[] tiles = Board.Instance.Slice(currentPosition, currentPosition + steps);

        int controllingPlayerIndex = GameManager2.Instance.Players.IndexOf(controllingPlayer);

        Vector3[] path = tiles.Select(tile => tile.PawnPositions[controllingPlayerIndex].position).ToArray();

        Tween tween = transform.DOPath(path, 1.25f);

        tween.OnComplete(() =>
        {
            _isMoving = false;

            ObserversSetCurrentPosition(currentPosition + steps);

            GameManager2.Instance.EndTurn();
        });

        tween.Play();
    }

    [ObserversRpc(BufferLast = true)]
    public void ObserversSetCurrentPosition(int value)
    {
        currentPosition = value;
    }
}
