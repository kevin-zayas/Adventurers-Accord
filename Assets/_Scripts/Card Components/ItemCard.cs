using FishNet.Object.Synchronizing;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCard : Card
{
    [SyncVar] private string SubDescription;

    [field: SerializeField]
    [field: SyncVar]
    public CardData CardData { get; private set; }

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text subDescriptionText;
    [SerializeField] private Image cardImage;

    private void Start()
    {
        nameText.text = Name;
        descriptionText.text = Description;
        subDescriptionText.text = SubDescription;
        physicalPowerText.text = PhysicalPower.ToString();
        magicalPowerText.text = MagicalPower.ToString();
    }

    //[Server]
    //public void SetCardParent(Transform parent, bool worldPositionStays)
    //{
    //    OberserversSetCardParent(parent, worldPositionStays);
    //}

    //[ServerRpc(RequireOwnership = false)]
    //public void ServerSetCardParent(Transform parent, bool worldPositionStays)
    //{
    //    OberserversSetCardParent(parent, worldPositionStays);
    //    this.transform.SetParent(parent, worldPositionStays);
    //}

    //[ObserversRpc(BufferLast = true)]
    //private void OberserversSetCardParent(Transform parent, bool worldPositionStays)
    //{
    //    Vector3 scale;

    //    //if (parent.CompareTag("Hand")) scale = new Vector3(2f, 2f, 1f);
    //    scale = new Vector3(1f, 1f, 1f);

    //    this.transform.localScale = scale;
    //    this.transform.SetParent(parent, worldPositionStays);
    //}

    [ServerRpc(RequireOwnership = false)]
    public void ServerDespawnItem()
    {
        this.Despawn();
    }

    [Server]
    public void LoadCardData(CardData cardData)
    {
        PhysicalPower = cardData.PhysicalPower;
        MagicalPower = cardData.MagicalPower;
        Name = cardData.CardName;
        Description = cardData.CardDescription;
        SubDescription = cardData.CardSubDescription;
        CardData = cardData;

        ObserversLoadCardData(cardData);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversLoadCardData(CardData cardData)
    {
        physicalPowerText.text = cardData.PhysicalPower.ToString();
        magicalPowerText.text = cardData.MagicalPower.ToString();
        nameText.text = cardData.CardName;
        descriptionText.text = cardData.CardDescription;
        subDescriptionText.text = cardData.CardSubDescription;

        cardImage.sprite = Resources.Load<Sprite>("ItemSpell_Sprites/" + cardData.CardName);
    }

}
