using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class QuestCard : Card
{
    #region SyncVars
    public readonly SyncVar<string> AbilityName = new();
    public readonly SyncVar<bool> BlockSpells = new();
    public readonly SyncVar<bool> DisableItems = new();
    public readonly SyncVar<bool> Drain = new();
    public readonly SyncVar<int> GoldReward = new();
    public readonly SyncVar<int> ReputationReward = new();
    public readonly SyncVar<int> LootReward = new();
    public readonly SyncVar<int> GoldPenalty = new();
    public readonly SyncVar<int> ReputationPenalty = new();
    public readonly SyncVar<int> RestPeriodPenalty = new();
    public readonly SyncVar<int> MagicalDrain = new();
    public readonly SyncVar<int> PhysicalDrain = new();

    [AllowMutableSyncTypeAttribute] public SyncVar<int> PartySizeLimit = new();
    #endregion

    #region UI Elements
    [SerializeField] private TMP_Text abilityNameText;
    [SerializeField] private TMP_Text abilityDescriptionText;
    [SerializeField] private GameObject abilityBox;
    [SerializeField] private TMP_Text goldRewardText;
    [SerializeField] private TMP_Text lootRewardText;
    [SerializeField] private TMP_Text reputationRewardText;
    [SerializeField] private TMP_Text partySizeLimitText;
    [SerializeField] private TMP_Text reputationPenaltyText;
    [SerializeField] private TMP_Text restPeriodPenaltyText;
    #endregion

    /// <summary>
    /// Loads the card data and updates the relevant SyncVars on the server.
    /// </summary>
    /// <param name="cardData">The card data to load.</param>
    [Server]
    public override void LoadCardData(CardData cardData)
    {
        AbilityName.Value = cardData.AbilityName;
        CardDescription.Value = cardData.CardDescription;
        GoldReward.Value = cardData.GoldReward;
        ReputationReward.Value = cardData.ReputationReward;
        LootReward.Value = cardData.LootReward;
        GoldPenalty.Value = cardData.GoldPenalty;
        ReputationPenalty.Value = cardData.ReputationPenalty;
        RestPeriodPenalty.Value = cardData.RestPeriodPenalty;
        Drain.Value = cardData.Drain;
        PhysicalDrain.Value = cardData.PhysicalDrain;
        MagicalDrain.Value = cardData.MagicalDrain;
        DisableItems.Value = cardData.DisableItems;
        BlockSpells.Value = cardData.BlockSpells;
        PartySizeLimit.Value = cardData.PartySizeLimit;

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
        abilityNameText.text = cardData.AbilityName;
        abilityDescriptionText.text = cardData.CardDescription;
        goldRewardText.text = cardData.GoldReward.ToString();
        reputationRewardText.text = cardData.ReputationReward.ToString();
        lootRewardText.text = cardData.LootReward.ToString();
        partySizeLimitText.text = cardData.PartySizeLimit.ToString();
        reputationPenaltyText.text = cardData.ReputationPenalty.ToString();
        restPeriodPenaltyText.text = cardData.RestPeriodPenalty.ToString();

        if (cardData.AbilityName == "") abilityBox.SetActive(false);

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
        QuestCard card = originalCard as QuestCard;

        cardImage.sprite = CardDatabase.Instance.CardSpriteMap[card.CardName.Value];

        physicalPowerText.text = card.PhysicalPower.Value.ToString();
        magicalPowerText.text = card.MagicalPower.Value.ToString();
        nameText.text = card.CardName.Value;
        abilityNameText.text = card.AbilityName.Value;
        abilityDescriptionText.text = card.CardDescription.Value;
        goldRewardText.text = card.GoldReward.Value.ToString();
        reputationRewardText.text = card.ReputationReward.Value.ToString();
        lootRewardText.text = card.LootReward.Value.ToString();
        partySizeLimitText.text = card.PartySizeLimit.Value.ToString();
        reputationPenaltyText.text = card.ReputationPenalty.Value.ToString();
        restPeriodPenaltyText.text = card.RestPeriodPenalty.Value.ToString();

        if (card.CardDescription.Value == "") abilityBox.SetActive(false);
    }
}
