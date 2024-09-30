using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : NetworkBehaviour
{
    public readonly SyncVar<Player> controllingPlayer = new();

    public readonly SyncVar<int> playerID = new();
}
