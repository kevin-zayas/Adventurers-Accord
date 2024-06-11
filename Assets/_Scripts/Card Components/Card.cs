using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card : NetworkBehaviour
{
    [field: SyncVar] public string Name { get; protected set; }
    [field: SyncVar] public string Description { get; protected set; }
    [field: SyncVar] public int PhysicalPower { get; protected set; }
    [field: SyncVar] public int MagicalPower { get; protected set; }

    [field: SyncVar] public Player ControllingPlayer { get; protected set; }
    [field: SyncVar] public Hand ControllingPlayerHand { get; protected set; }


    [Server]
    public void SetCardOwner(Player player)
    {
        ControllingPlayer = player;
        ControllingPlayerHand = player.controlledHand;
        GiveOwnership(player.Owner);
    }
}
