using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class Card : NetworkBehaviour
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
    public bool IsDraftCard { get; private set; }
    
    [SyncVar]
    public int draftCardIndex;

    [field: SerializeField]
    [field: SyncVar] 
    public int PhysicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int MagicalPower { get; private set; }

    
    [field: SerializeField] public int OriginalPhysicalPower { get; private set; }
    [field: SerializeField] public int OriginalMagicalPower { get; private set;}

    [field: SerializeField]
    [field: SyncVar]
    public int Cost { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool HasItem { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public Vector3 CurrentScale { get; private set;}

    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text costText;

    private void Start()
    {
        //if (PhysicalPower == 0) PhysicalPower = OriginalPhysicalPower;        // will need to implement when card stat changes are implemented
        //if (MagicalPower == 0) MagicalPower = OriginalMagicalPower;           // this will make sure spotlight card show updated stats
        PhysicalPower = OriginalPhysicalPower;
        MagicalPower = OriginalMagicalPower;

        physicalPowerText.text = PhysicalPower.ToString();
        magicalPowerText.text = MagicalPower.ToString();
        costText.text = Cost.ToString();

        if (IsServer)
        {
            IsDraftCard = true;
        }     
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
        IsDraftCard = false;
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

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetItem(bool hasItem, ItemCard itemCard)
    {
        HasItem = hasItem;

        ItemCardHeader itemCardHeader = gameObject.transform.GetChild(1).GetComponent<ItemCardHeader>();
        Spawn(itemCardHeader.gameObject);

        itemCardHeader.SetItemInfo(itemCard);

        // increase card size to adjust for item header
        ObserversAdjustCardSize(235);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversAdjustCardSize(int height)
    {
        gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }


    [ServerRpc(RequireOwnership = false)]
    public void ServerChangePhysicalPower(int power)
    {
        if (OriginalPhysicalPower > 0 && power != 0)
        {
            PhysicalPower += power;
            ObserversUpdatePowerText(PhysicalPower, MagicalPower);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerChangeMagicalPower(int power)
    {
        if (OriginalMagicalPower > 0 && power != 0)
        {
            MagicalPower += power;
            ObserversUpdatePowerText(PhysicalPower, MagicalPower);
        }
    }

    [ObserversRpc(BufferLast = true)]
    public void ObserversUpdatePowerText(int physicalPower, int magicalPower)
    {
        physicalPowerText.text = physicalPower.ToString();
        magicalPowerText.text = magicalPower.ToString();

        if (physicalPower > OriginalPhysicalPower) physicalPowerText.color = Color.green;
        else if (physicalPower < OriginalPhysicalPower) physicalPowerText.color = Color.red;
        else physicalPowerText.color = Color.white;

        if (magicalPower > OriginalMagicalPower) magicalPowerText.color = Color.green;
        else if (magicalPower < OriginalMagicalPower) magicalPowerText.color = Color.red;
        else magicalPowerText.color = Color.white;
    }

}
