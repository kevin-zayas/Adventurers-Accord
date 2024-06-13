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
    [field: SyncVar] public CardData Data { get; protected set; }

    [field: SyncVar] public Player ControllingPlayer { get; protected set; }
    [field: SyncVar] public Hand ControllingPlayerHand { get; protected set; }


    [Server]
    public void SetCardOwner(Player player)
    {
        ControllingPlayer = player;
        ControllingPlayerHand = player.controlledHand;
        GiveOwnership(player.Owner);
    }

    [Server]
    public virtual void SetCardParent(Transform parent, bool worldPositionStays)
    {
        OberserversSetCardParent(parent, worldPositionStays);
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void ServerSetCardParent(Transform parent, bool worldPositionStays)
    {
        OberserversSetCardParent(parent, worldPositionStays);
        this.transform.SetParent(parent, worldPositionStays);
    }

    [ObserversRpc(BufferLast = true)]
    protected virtual void OberserversSetCardParent(Transform parent, bool worldPositionStays)
    {
        this.transform.SetParent(parent, worldPositionStays);
    }

    [Server]
    public virtual void LoadCardData(CardData cardData)
    {
        PhysicalPower = cardData.PhysicalPower;
        MagicalPower = cardData.MagicalPower;
        Name = cardData.CardName;
        Description = cardData.CardDescription;
        Data = cardData;

        ObserversLoadCardData(cardData);
    }

    [ObserversRpc(BufferLast = true)]
    protected virtual void ObserversLoadCardData(CardData cardData) { }
}
