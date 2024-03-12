using FishNet.Object.Synchronizing;
using FishNet.Object;
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
    public string Name { get; private set; }

    [SyncVar] private string Description;
    [SyncVar] private string SubDescription;

    [field: SerializeField]
    [field: SyncVar]
    public int PhysicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int MagicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public CardData CardData { get; private set; }

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text subDescriptionText;

    private void Start()
    {
        nameText.text = Name;
        descriptionText.text = Description;
        subDescriptionText.text = SubDescription;
        physicalPowerText.text = PhysicalPower.ToString();
        magicalPowerText.text = MagicalPower.ToString();
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
        this.transform.SetParent(parent, worldPositionStays);
    }

    [ObserversRpc(BufferLast = true)]
    private void OberserversSetCardParent(Transform parent, bool worldPositionStays)
    {
        Vector3 scale;

        if (parent.CompareTag("Hand")) scale = new Vector3(2f, 2f, 1f);
        else scale = new Vector3(1f, 1f, 1f);

        this.transform.localScale = scale;
        this.transform.SetParent(parent, worldPositionStays);
    }

    [Server]
    public void SetCardOwner(Player player)
    {
        ControllingPlayer = player;
        ControllingPlayerHand = player.controlledHand;
        GiveOwnership(player.Owner);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetCardOwner(Player player)
    {
        ControllingPlayer = player;
        ControllingPlayerHand = player.controlledHand;
        GiveOwnership(player.Owner);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerDespawnItem()
    {
        this.Despawn();
    }

    [Server]
    public void LoadCardData(CardData cardData)
    {
        PhysicalPower = cardData.physicalPower;
        MagicalPower = cardData.magicalPower;
        Name = cardData.cardName;
        Description = cardData.cardDescription;
        SubDescription = cardData.cardSubDescription;
        CardData = cardData;

        ObserversLoadCardData(cardData);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversLoadCardData(CardData cardData)
    {
        physicalPowerText.text = cardData.physicalPower.ToString();
        magicalPowerText.text = cardData.magicalPower.ToString();
        nameText.text = cardData.cardName;
        descriptionText.text = cardData.cardDescription;
        subDescriptionText.text = cardData.cardSubDescription;
    }

}
