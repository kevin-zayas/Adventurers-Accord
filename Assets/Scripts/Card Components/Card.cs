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
    
    [SyncVar] public int draftCardIndex;

    [SyncVar] private string Name;
    [SyncVar] private string Description;

    [field: SerializeField] public int OriginalPhysicalPower { get; private set; }
    [field: SerializeField] public int OriginalMagicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar] 
    public int PhysicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int MagicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int ItemPhysicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int ItemMagicalPower { get; private set; }


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
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    
    private void Start()
    {
        nameText.text = Name;
        descriptionText.text = Description;
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
    public void ServerSetCardOwner(Player player)
    {
        ControllingPlayer = player;
        ControllingPlayerHand = player.controlledHand;
        GiveOwnership(player.Owner);
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
        UpdateItemPower(itemCard.PhysicalPower, itemCard.MagicalPower);
        
        ObserversAdjustCardSize(233);    // increase card size to adjust for item header
    }

    [Server]
    private void UpdateItemPower(int physicalPower, int magicalPower)
    {
        ItemPhysicalPower = physicalPower;
        ItemMagicalPower = magicalPower;
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

    [Server]
    public void LoadCardData(CardData cardData)
    {
        PhysicalPower = cardData.physicalPower;
        MagicalPower = cardData.magicalPower;
        OriginalPhysicalPower = cardData.originalPhysicalPower;
        OriginalMagicalPower = cardData.originalMagicalPower;
        Name = cardData.cardName;
        Description = cardData.cardDescription;
        Cost = cardData.cost;

        ObserversLoadCardData(cardData);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversLoadCardData(CardData cardData)
    {
        physicalPowerText.text = cardData.physicalPower.ToString();
        magicalPowerText.text = cardData.magicalPower.ToString();
        nameText.text = cardData.cardName;
        descriptionText.text = cardData.cardDescription;
        costText.text = cardData.cost.ToString();
    }

}
