using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Pawn : NetworkBehaviour
{
    [SyncVar]
    public Player controllingPlayer;


}
