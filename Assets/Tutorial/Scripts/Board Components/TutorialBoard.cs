using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class TutorialBoard : NetworkBehaviour
{
    public static TutorialBoard Instance { get; private set; }

    [field: SerializeField]
    public TutorialTile[] Tiles { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public int Wrap(int index)
    {
        return index < 0 ? Math.Abs((Tiles.Length - Math.Abs(index)) % Tiles.Length) : index % Tiles.Length;
    }

    public TutorialTile[] Slice(int start, int end)
    {
        if (Tiles.Length == 0) return Array.Empty<TutorialTile>();

        List<TutorialTile> slice = new();

        int steps = Math.Abs(end - start);

        if (end > start)
        {
            for (int i = start; i <= start + steps; i++)
            {
                slice.Add(Tiles[Wrap(i)]);
            }
        }
        else
        {
            for (int i = start; i >= start - steps; i--)
            {
                slice.Add(Tiles[Wrap(i)]);
            }
        }
        return slice.ToArray();
    }
 
    [ServerRpc(RequireOwnership = false)]
    public void ServerSetTileOwner(int tileindex, TutorialPlayer value)
    {
        ObserversSetTileOwner(tileindex, value);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversSetTileOwner(int tileIndex,TutorialPlayer value)
    {
        Tiles[tileIndex].owningPlayer = value;
    }
}
