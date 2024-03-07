using FishNet.Object.Synchronizing;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemCardHeader : NetworkBehaviour
{
    [field: SerializeField]
    [field: SyncVar]
    public Player ControllingPlayer { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public Card AttachedCard { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public Transform Parent { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int PhysicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int MagicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool IsEquipped { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public Vector3 CurrentScale { get; private set; }

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;

    [Server]
    public void SetCardParent(Transform parent, bool worldPositionStays)
    {
        OberserversSetCardParent(parent, worldPositionStays, CurrentScale);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetCardParent(Transform parent, bool worldPositionStays)
    {
        OberserversSetCardParent(parent, worldPositionStays, CurrentScale);
        this.transform.SetParent(parent, worldPositionStays);
        this.Parent = parent;
    }

    [ObserversRpc(BufferLast = true)]
    private void OberserversSetCardParent(Transform parent, bool worldPositionStays, Vector3 scale)
    {
        this.transform.localScale = scale;
        this.transform.SetParent(parent, worldPositionStays);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetCardOwner(Player owner)
    {
        ControllingPlayer = owner;
        //ControllingPlayerHand = owner.controlledHand;
    }

    [Server]
    public void SetCardScale(Vector3 scale)
    {
        this.CurrentScale = scale;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetCardScale(Vector3 scale)
    {
        this.CurrentScale = scale;
    }

    [Server]
    public void SetItemInfo(ItemCard itemCard)
    {
        PhysicalPower = itemCard.PhysicalPower;
        MagicalPower = itemCard.MagicalPower;

        ObserversSetItemInfo(PhysicalPower,MagicalPower,itemCard.Name);
    }

    [ObserversRpc(BufferLast = true)]
    public void ObserversSetItemInfo(int physicalPower, int magicalPower, string name)
    {
        physicalPowerText.text = physicalPower.ToString();
        magicalPowerText.text = magicalPower.ToString();
        nameText.text = name;
    }

    [Server]
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        ObserversSetActive(active);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversSetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
