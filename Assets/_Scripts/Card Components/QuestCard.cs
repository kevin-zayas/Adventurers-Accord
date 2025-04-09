using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestCard : Card
{
    #region SyncVars
    public readonly SyncVar<string> AbilityName = new();
    public readonly SyncVar<bool> BlockSpells = new();
    public readonly SyncVar<bool> DisableItems = new();
    public readonly SyncVar<bool> Drain = new();
    public readonly SyncVar<int> GoldReward = new();
    public readonly SyncVar<int> LootReward = new();
    public readonly SyncVar<int> MagicalDrain = new();
    public readonly SyncVar<int> PhysicalDrain = new();
    public readonly SyncVar<int> ReputationReward = new();
    [AllowMutableSyncTypeAttribute] public SyncVar<int> PartySizeLimit = new();
    #endregion

    #region UI Elements
    [SerializeField] private TMP_Text abilityNameText;
    [SerializeField] private GameObject abilityNameObject;
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text goldRewardText;
    [SerializeField] private TMP_Text lootRewardText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text reputationRewardText;
    [SerializeField] private TMP_Text partySizeLimitText;
    #endregion

    /// <summary>
    /// Loads the card data and updates the relevant SyncVars on the server.
    /// </summary>
    /// <param name="cardData">The card data to load.</param>
    [Server]
    public override void LoadCardData(CardData cardData)
    {
        AbilityName.Value = cardData.AbilityName;
        GoldReward.Value = cardData.GoldReward;
        ReputationReward.Value = cardData.ReputationReward;
        LootReward.Value = cardData.LootReward;
        Drain.Value = cardData.Drain;
        PhysicalDrain.Value = cardData.PhysicalDrain;
        MagicalDrain.Value = cardData.MagicalDrain;
        DisableItems.Value = cardData.DisableItems;
        BlockSpells.Value = cardData.BlockSpells;
        Data.Value = cardData;
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
        goldRewardText.text = cardData.GoldReward.ToString();
        reputationRewardText.text = cardData.ReputationReward.ToString();
        lootRewardText.text = cardData.LootReward.ToString();
        partySizeLimitText.text = cardData.PartySizeLimit.ToString();

        if (cardData.AbilityName == "") abilityNameObject.SetActive(false);

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
        isClone = true;
        QuestCard card = originalCard as QuestCard;

        cardImage.sprite = CardDatabase.Instance.CardSpriteMap[card.CardName.Value];

        physicalPowerText.text = card.PhysicalPower.Value.ToString();
        magicalPowerText.text = card.MagicalPower.Value.ToString();
        nameText.text = card.CardName.Value;
        abilityNameText.text = card.AbilityName.Value;
        goldRewardText.text = card.GoldReward.Value.ToString();
        reputationRewardText.text = card.ReputationReward.Value.ToString();
        lootRewardText.text = card.LootReward.Value.ToString();
        partySizeLimitText.text = card.PartySizeLimit.Value.ToString();

        if (card.CardDescription.Value == "") abilityNameObject.SetActive(false);
    }
}
