using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class ItemCard : Card
{
    #region SyncVars
    private readonly SyncVar<string> SubDescription = new();
    #endregion

    #region UI Elements
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text subDescriptionText;
    #endregion

    /// <summary>
    /// Despawns the item on the server.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ServerDespawnItem()
    {
        this.Despawn();
    }

    /// <summary>
    /// Loads the card data and updates the relevant SyncVars on the server.
    /// </summary>
    /// <param name="cardData">The card data to load.</param>
    [Server]
    public override void LoadCardData(CardData cardData)
    {
        SubDescription.Value = cardData.CardSubDescription;

        base.LoadCardData(cardData);
    }

    /// <summary>
    /// Updates the card's visual representation on all clients based on the provided card data.
    /// </summary>
    /// <param name="cardData">The card data to load into the visual elements.</param>
    [ObserversRpc(BufferLast = true)]
    protected override void ObserversLoadCardData(CardData cardData)
    {
        physicalPowerText.text = cardData.PhysicalPower.ToString();
        magicalPowerText.text = cardData.MagicalPower.ToString();
        nameText.text = cardData.CardName;
        descriptionText.text = cardData.CardDescription;
        subDescriptionText.text = cardData.CardSubDescription;
        cardTypeText.text = cardData.CardType;
        costText.text = cardData.Cost.ToString();

        cardImage.sprite = CardDatabase.Instance.CardSpriteMap[cardData.CardName];
    }

    /// <summary>
    /// Copies the card data to the target client from the original card.
    /// </summary>
    /// <param name="connection">The network connection of the target client.</param>
    /// <param name="originalCard">The original card to copy data from.</param>
    [TargetRpc]
    public override void TargetCopyCardData(NetworkConnection connection, Card originalCard)
    {
        IsClone = true;
        ItemCard card = originalCard as ItemCard;

        cardImage.sprite = CardDatabase.Instance.CardSpriteMap[card.CardName.Value];

        physicalPowerText.text = card.PhysicalPower.Value.ToString();
        magicalPowerText.text = card.MagicalPower.Value.ToString();
        nameText.text = card.CardName.Value;
        descriptionText.text = card.CardDescription.Value;
        subDescriptionText.text = card.SubDescription.Value;
        costText.text = card.Cost.Value.ToString();
    }

    /// <summary>
    /// Copies the item header data to the target client.
    /// </summary>
    /// <param name="connection">The network connection of the target client.</param>
    /// <param name="itemHeader">The item header to copy data from.</param>
    [TargetRpc]
    public void TargetCopyItemHeaderData(NetworkConnection connection, ItemCardHeader itemHeader)
    {
        IsClone = true;
        cardImage.sprite = CardDatabase.Instance.CardSpriteMap[itemHeader.CardName.Value];

        physicalPowerText.text = itemHeader.PhysicalPower.Value.ToString();
        magicalPowerText.text = itemHeader.MagicalPower.Value.ToString();
        nameText.text = itemHeader.CardName.Value;
        descriptionText.text = itemHeader.CardDescription.Value;
        subDescriptionText.text = itemHeader.Data.Value.CardSubDescription;

        UpdatePowerTextColor(itemHeader.PhysicalPower.Value, itemHeader.MagicalPower.Value, itemHeader.Data.Value.OriginalPhysicalPower, itemHeader.Data.Value.MagicalPower);
    }
}
