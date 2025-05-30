using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static CardData;

public class PotionCard : Card
{
    #region SyncVars
    public readonly SyncVar<Potion> PotionType = new();
    #endregion
    public enum Potion
    {
        Healing,
        Strength,
        Intelligence,
        Agility,
        Stamina
    }
    #region UI Elements
    [SerializeField] private TMP_Text descriptionText;
    #endregion

    /// <summary>
    /// Loads the card data and updates the relevant SyncVars on the server.
    /// </summary>
    /// <param name="cardData">The card data to load.</param>
    [Server]
    public override void LoadCardData(CardData cardData)
    {
        PotionType.Value = cardData.PotionType;
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
        cardTypeText.text = cardData.CardType;
        costText.text = cardData.Cost.ToString();

        cardImage.sprite = CardDatabase.Instance.CardSpriteMap[cardData.CardName];
    }

    /// <summary>
    /// Updates the target client with the copied card data from the original card.
    /// </summary>
    /// <param name="connection">The network connection of the target client.</param>
    /// <param name="originalCard">The original card to copy data from.</param>
    [TargetRpc]
    public override void TargetCopyCardData(NetworkConnection connection, Card originalCard)
    {
        IsClone = true;
        PotionCard card = originalCard as PotionCard;

        cardImage.sprite = CardDatabase.Instance.CardSpriteMap[card.CardName.Value];

        physicalPowerText.text = card.PhysicalPower.Value.ToString();
        magicalPowerText.text = card.MagicalPower.Value.ToString();
        nameText.text = card.CardName.Value;
        descriptionText.text = card.CardDescription.Value;
        costText.text = card.Cost.Value.ToString();

        //UpdatePowerTextColor(card.PhysicalPower.Value, card.MagicalPower.Value, card.OriginalPhysicalPower.Value, card.OriginalMagicalPower.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UsePotion(AdventurerCard adventurerCard)
    {
        switch (PotionType.Value)
        {
            case Potion.Healing:
                ApplyHealingPotion(adventurerCard);
                break;
            default:
                Debug.LogError($"Unknown potion type: {PotionType.Value}");
                break;
        }
    }

    [Server]
    private void ApplyHealingPotion(AdventurerCard adventurerCard)
    {
        adventurerCard.ChangeCurrentRestPeriod(-1, true);
    }
}
