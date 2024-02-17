using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class Board : NetworkBehaviour
{
    [field: SerializeField]
    public Tile[] Tiles { get; private set; }


}
