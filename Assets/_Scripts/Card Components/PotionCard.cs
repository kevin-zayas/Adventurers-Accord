using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class PotionCard : Card
{
    #region SyncVars
    public readonly SyncVar<Potion> PotionType = new();
    #endregion
    public enum Potion
    {
        None,
        Healing,
        Power,
        Strength,
        Intelligence
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
        cardTypeText.text = cardData.CardType.ToString();
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
            case Potion.Power:
                ApplyPotionOfPower(adventurerCard);
                break;
            case Potion.Strength:
                ApplyPotionOfStrength(adventurerCard);
                break;
            case Potion.Intelligence:
                ApplyPotionOfIntelligence(adventurerCard);
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

    [Server]
    private void ApplyPotionOfPower(AdventurerCard adventurerCard)
    {
        adventurerCard.ApplyPotionPhysicalPower(1);
        adventurerCard.ApplyPotionMagicalPower(1);
    }

    [Server]
    private void ApplyPotionOfStrength(AdventurerCard adventurerCard)
    {
        bool increasedFromZero = adventurerCard.OriginalPhysicalPower.Value == 0 && adventurerCard.potionBasePhysicalPower.Value == 0;
        adventurerCard.ApplyPotionPhysicalPower(4, true);

        QuestLane questLane = adventurerCard.CurrentCardHolder.Value.QuestLane;
        questLane.ApplyEnchanterBuff(adventurerCard, increasedFromZero, false);
    }

    [Server]
    private void ApplyPotionOfIntelligence(AdventurerCard adventurerCard)
    {
        bool increasedFromZero = adventurerCard.OriginalMagicalPower.Value == 0 && adventurerCard.potionBaseMagicalPower.Value == 0;
        adventurerCard.ApplyPotionMagicalPower(4, true);

        QuestLane questLane = adventurerCard.CurrentCardHolder.Value.QuestLane;
        questLane.ApplyEnchanterBuff(adventurerCard, false, increasedFromZero);
    }

}
