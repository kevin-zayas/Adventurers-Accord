using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class Card : NetworkBehaviour
{
    [SyncVar] public Player controllingPlayer;
    [SyncVar] public Hand controllingPlayerHand;
    [SyncVar] public Transform parent;      //[SyncVar(OnChange = nameof(CardParentChanged))]
    
    [SyncVar] public int slotIndex;
    [SyncVar] public int physicalPower;
    [SyncVar] public int magicalPower;

    public TMP_Text physicalPowerText;
    public TMP_Text magicalPowerText;
    public TMP_Text costText;
    public int cost;

    private void Start()
    {
        physicalPowerText.text = physicalPower.ToString();
        magicalPowerText.text = magicalPower.ToString();
        costText.text = cost.ToString();
    }

    [Server]
    public void SetCardParent(Transform parent, bool worldPositionStays)
    {
        OberserversSetCardParent(parent, worldPositionStays);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetCardParent(Transform parent, bool worldPositionStays)
    {
        OberserversSetCardParent(parent, worldPositionStays);
        this.parent = parent;
    }

    [ObserversRpc(BufferLast = true)]
    private void OberserversSetCardParent(Transform parent, bool worldPositionStays)
    {
        this.transform.SetParent(parent, worldPositionStays);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetCardOwner(Player owner)
    {
        //OberserversSetCardOwner(owner);
        controllingPlayer = owner;
        controllingPlayerHand = owner.controlledHand;
    }

    //[ObserversRpc(BufferLast = true)]
    //private void OberserversSetCardOwner(Player owner)
    //{
    //    controllingPlayer = owner;
    //}

    //private void CardParentChanged(Transform oldValue, Transform newValue, bool asServer)
    //{
    //    print($"Old parent is {oldValue}");
    //    print($"New parent is {newValue}");
    //    //print($"Is server? {asServer}");
    //    //if (asServer) return;

    //    if (newValue == null) print("Card parent is null");

    //    this.transform.SetParent(newValue, false);
    //}
}
