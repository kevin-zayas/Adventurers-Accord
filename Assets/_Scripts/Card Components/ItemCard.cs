using FishNet.Object.Synchronizing;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FishNet.Connection;

public class ItemCard : Card
{
    [SyncVar] private string SubDescription;

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text subDescriptionText;
    [SerializeField] private Image cardImage;

    private void Start()
    {
        //nameText.text = Name;
        //descriptionText.text = Description;
        //subDescriptionText.text = SubDescription;
        //physicalPowerText.text = PhysicalPower.ToString();
        //magicalPowerText.text = MagicalPower.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerDespawnItem()
    {
        this.Despawn();
    }

    [Server]
    public override void LoadCardData(CardData cardData)
    {
        SubDescription = cardData.CardSubDescription;
        Data = cardData;

        base.LoadCardData(cardData);
        //ObserversLoadCardData(cardData);
    }

    [ObserversRpc(BufferLast = true)]
    protected override void ObserversLoadCardData(CardData cardData)
    {
        physicalPowerText.text = cardData.PhysicalPower.ToString();
        magicalPowerText.text = cardData.MagicalPower.ToString();
        nameText.text = cardData.CardName;
        descriptionText.text = cardData.CardDescription;
        subDescriptionText.text = cardData.CardSubDescription;

        cardImage.sprite = Resources.Load<Sprite>("ItemSpell_Sprites/" + cardData.CardName);
    }

    [TargetRpc]
    public override void TargetCopyCardData(NetworkConnection connection, Card originalCard)
    {
        ItemCard card = originalCard as ItemCard;

        cardImage.sprite = CardDatabase.Instance.SpriteMap[card.Name];

        physicalPowerText.text = card.PhysicalPower.ToString();
        magicalPowerText.text = card.MagicalPower.ToString();
        nameText.text = card.Name;
        descriptionText.text = card.Description;
        subDescriptionText.text = card.SubDescription;

    }

}
