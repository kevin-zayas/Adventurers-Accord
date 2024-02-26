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
}
