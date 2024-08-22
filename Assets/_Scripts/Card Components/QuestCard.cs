using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestCard : Card
{
    #region SyncVars
    [field: SyncVar] public bool BlockSpells { get; private set; }
    [field: SyncVar] public bool DisableItems { get; private set; }
    [field: SyncVar] public bool Drain { get; private set; }
    [field: SyncVar] public int GoldReward { get; private set; }
    [field: SyncVar] public int LootReward { get; private set; }
    [field: SyncVar] public int MagicalDrain { get; private set; }
    [field: SyncVar] public int PhysicalDrain { get; private set; }
    [field: SyncVar] public int ReputationReward { get; private set; }
    #endregion

    #region General Variables
    public string AbilityName { get; private set; }
    #endregion

    #region UI Elements
    [SerializeField] private TMP_Text abilityNameText;
    [SerializeField] private GameObject abilityNameObject;
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text goldRewardText;
    [SerializeField] private TMP_Text lootRewardText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text reputationRewardText;
    #endregion

    /// <summary>
    /// Loads the card data and updates the relevant SyncVars on the server.
    /// </summary>
    /// <param name="cardData">The card data to load.</param>
    [Server]
    public override void LoadCardData(CardData cardData)
    {
        AbilityName = cardData.AbilityName;
        GoldReward = cardData.GoldReward;
        ReputationReward = cardData.ReputationReward;
        LootReward = cardData.LootReward;
        Drain = cardData.Drain;
        PhysicalDrain = cardData.PhysicalDrain;
        MagicalDrain = cardData.MagicalDrain;
        DisableItems = cardData.DisableItems;
        BlockSpells = cardData.BlockSpells;
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
        abilityNameText.text = cardData.AbilityName;
        goldRewardText.text = $"{cardData.GoldReward} GP";
        reputationRewardText.text = $"{cardData.ReputationReward} Rep";
        lootRewardText.text = $"{cardData.LootReward} Loot";

        if (cardData.AbilityName == "") abilityNameObject.SetActive(false);

        cardImage.sprite = CardDatabase.Instance.SpriteMap[cardData.CardName];
    }

    /// <summary>
    /// Updates the target client with the copied card data from the original card.
    /// </summary>
    /// <param name="connection">The network connection of the target client.</param>
    /// <param name="originalCard">The original card to copy data from.</param>
    [TargetRpc]
    public override void TargetCopyCardData(NetworkConnection connection, Card originalCard)
    {
        QuestCard card = originalCard as QuestCard;

        cardImage.sprite = CardDatabase.Instance.SpriteMap[card.CardName];

        physicalPowerText.text = card.PhysicalPower.ToString();
        magicalPowerText.text = card.MagicalPower.ToString();
        nameText.text = card.CardName;
        abilityNameText.text = card.AbilityName;
        goldRewardText.text = $"{card.GoldReward} GP";
        reputationRewardText.text = $"{card.ReputationReward} Rep.";
        lootRewardText.text = $"{card.LootReward} Loot";

        if (card.CardDescription == "") abilityNameObject.SetActive(false);
    }
}
