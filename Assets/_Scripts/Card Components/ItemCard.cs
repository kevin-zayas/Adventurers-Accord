using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCard : Card
{
    #region SyncVars
    [SyncVar] private string SubDescription;
    #endregion

    #region UI Elements
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text physicalPowerText;
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
        SubDescription = cardData.CardSubDescription;
        Data = cardData;

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

        cardImage.sprite = Resources.Load<Sprite>("ItemSpell_Sprites/" + cardData.CardName);
    }

    /// <summary>
    /// Copies the card data to the target client from the original card.
    /// </summary>
    /// <param name="connection">The network connection of the target client.</param>
    /// <param name="originalCard">The original card to copy data from.</param>
    [TargetRpc]
    public override void TargetCopyCardData(NetworkConnection connection, Card originalCard)
    {
        ItemCard card = originalCard as ItemCard;

        cardImage.sprite = CardDatabase.Instance.SpriteMap[card.CardName];

        physicalPowerText.text = card.PhysicalPower.ToString();
        magicalPowerText.text = card.MagicalPower.ToString();
        nameText.text = card.CardName;
        descriptionText.text = card.CardDescription;
        subDescriptionText.text = card.SubDescription;
    }

    /// <summary>
    /// Copies the item header data to the target client.
    /// </summary>
    /// <param name="connection">The network connection of the target client.</param>
    /// <param name="itemHeader">The item header to copy data from.</param>
    [TargetRpc]
    public void TargetCopyItemHeaderData(NetworkConnection connection, ItemCardHeader itemHeader)
    {
        cardImage.sprite = CardDatabase.Instance.SpriteMap[itemHeader.CardName];

        physicalPowerText.text = itemHeader.PhysicalPower.ToString();
        magicalPowerText.text = itemHeader.MagicalPower.ToString();
        nameText.text = itemHeader.CardName;
        descriptionText.text = itemHeader.CardDescription;
        subDescriptionText.text = itemHeader.Data.CardSubDescription;

        UpdatePowerTextColor(itemHeader.PhysicalPower, itemHeader.MagicalPower, itemHeader.Data.OriginalPhysicalPower, itemHeader.Data.MagicalPower);
    }

    /// <summary>
    /// Updates the power text color based on comparison with the original power values.
    /// </summary>
    /// <param name="physicalPower">The current physical power.</param>
    /// <param name="magicalPower">The current magical power.</param>
    /// <param name="originalPhysicalPower">The original physical power.</param>
    /// <param name="originalMagicalPower">The original magical power.</param>
    private void UpdatePowerTextColor(int physicalPower, int magicalPower, int originalPhysicalPower, int originalMagicalPower)
    {
        physicalPowerText.color = physicalPower > originalPhysicalPower ? Color.green :
                                  physicalPower < originalPhysicalPower ? Color.red : Color.white;

        magicalPowerText.color = magicalPower > originalMagicalPower ? Color.green :
                                 magicalPower < originalMagicalPower ? Color.red : Color.white;
    }
}
