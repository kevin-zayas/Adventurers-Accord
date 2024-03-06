using FishNet.Object.Synchronizing;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemCard : NetworkBehaviour
{
    [field: SerializeField]
    [field: SyncVar]
    public Player ControllingPlayer { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public Hand ControllingPlayerHand { get; private set; }

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

    [field: SerializeField]
    [field: SyncVar]
    public string Name { get; private set; }

    [SerializeField] private TMP_Text nameText;

    //[SerializeField] private TMP_Text physicalPowerText;
    //[SerializeField] private TMP_Text magicalPowerText;
    //[SerializeField] private TMP_Text costText;

    private void Awake()
    {
        print("ItemCard Awake");
        print(isActiveAndEnabled);
        gameObject.SetActive(true);
    }
    private void Start()
    {
        //physicalPowerText.text = PhysicalPower.ToString();
        //magicalPowerText.text = MagicalPower.ToString();
        //costText.text = Cost.ToString();
        print("ItemCard Start");
        nameText.text = Name;


    }

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
        ControllingPlayerHand = owner.controlledHand;
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

}
