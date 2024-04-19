using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestCard : NetworkBehaviour
{
    [field: SerializeField]
    [field: SyncVar]
    public Transform Parent { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public string Name { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int PhysicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int MagicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int GoldReward { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int ReputationReward { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int LootReward { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool Drain { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int PhysicalDrain { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int MagicalDrain { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool DisableItems { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool BlockSpells { get; private set; }



    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text goldRewardText;
    [SerializeField] private TMP_Text reputationRewardText;
    [SerializeField] private TMP_Text lootRewardText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image cardImage;


    private void Start()
    {
        physicalPowerText.text = PhysicalPower.ToString();
        magicalPowerText.text = MagicalPower.ToString();
        goldRewardText.text = $"{GoldReward} GP";
        reputationRewardText.text = $"{ReputationReward} Rep.";
        lootRewardText.text = $"{LootReward} Loot";
    }

    [Server]
    public void SetCardParent(Transform parent, bool worldPositionStays)
    {
        OberserversSetCardParent(parent, worldPositionStays);
    }

    [ObserversRpc(BufferLast = true)]
    private void OberserversSetCardParent(Transform parent, bool worldPositionStays)
    {
        this.transform.SetParent(parent, worldPositionStays);
    }

    [Server]
    public void LoadCardData(CardData cardData)
    {
        PhysicalPower = cardData.physicalPower;
        MagicalPower = cardData.magicalPower;
        Name = cardData.cardName;
        GoldReward = cardData.goldReward;
        ReputationReward = cardData.reputationReward;
        LootReward = cardData.lootReward;
        Drain = cardData.drain;
        PhysicalDrain = cardData.physicalDrain;
        MagicalDrain = cardData.magicalDrain;
        DisableItems = cardData.disableItems;
        BlockSpells = cardData.blockSpells;

        ObserversLoadCardData(cardData);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversLoadCardData(CardData cardData)
    {
        physicalPowerText.text = cardData.physicalPower.ToString();
        magicalPowerText.text = cardData.magicalPower.ToString();
        nameText.text = cardData.cardName;
        descriptionText.text = cardData.cardDescription;
        goldRewardText.text = $"{cardData.goldReward} GP";
        reputationRewardText.text = $"{cardData.reputationReward} Rep.";
        lootRewardText.text = $"{cardData.lootReward} Loot";

        cardImage.sprite = Resources.Load<Sprite>("Quest_Sprites/" + cardData.cardName);
    }


}
