using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class QuestCard : Card
{
    [field: SyncVar] public string AbilityName { get; private set; }
    [field: SyncVar] public int GoldReward { get; private set; }
    [field: SyncVar] public int ReputationReward { get; private set; }
    [field: SyncVar] public int LootReward { get; private set; }
    [field: SyncVar] public bool Drain { get; private set; }
    [field: SyncVar] public int PhysicalDrain { get; private set; }
    [field: SyncVar] public int MagicalDrain { get; private set; }

    [field: SyncVar] public bool DisableItems { get; private set; }
    [field: SyncVar] public bool BlockSpells { get; private set; }

    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text goldRewardText;
    [SerializeField] private TMP_Text reputationRewardText;
    [SerializeField] private TMP_Text lootRewardText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text abilityNameText;
    [SerializeField] private GameObject abilityNameObject;
    [SerializeField] private Image cardImage;


    private void Start()
    {
        //physicalPowerText.text = PhysicalPower.ToString();
        //magicalPowerText.text = MagicalPower.ToString();
        //goldRewardText.text = $"{GoldReward} GP";
        //reputationRewardText.text = $"{ReputationReward} Rep.";
        //lootRewardText.text = $"{LootReward} Loot";
    }

    [ServerRpc(RequireOwnership = false)]
    public override void ServerSetCardParent(Transform parent, bool worldPositionStays) { }

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

        //ObserversLoadCardData(cardData);
    }

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

    [TargetRpc]
    public override void TargetCopyCardData(NetworkConnection connection, Card originalCard)
    {
        QuestCard card = originalCard as QuestCard;

        cardImage.sprite = CardDatabase.Instance.SpriteMap[card.Name];

        physicalPowerText.text = card.PhysicalPower.ToString();
        magicalPowerText.text = card.MagicalPower.ToString();
        nameText.text = card.Name;
        abilityNameText.text = card.AbilityName;
        goldRewardText.text = $"{card.GoldReward} GP";
        reputationRewardText.text = $"{card.ReputationReward} Rep.";
        lootRewardText.text = $"{card.LootReward} Loot";

        if (card.Description == "") abilityNameObject.SetActive(false);

    }
}
