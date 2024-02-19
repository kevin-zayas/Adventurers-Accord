using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class Board : NetworkBehaviour
{
    public static Board Instance { get; private set; }

    [field: SerializeField]
    public Tile[] Tiles { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
