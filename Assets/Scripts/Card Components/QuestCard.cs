using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class QuestCard : NetworkBehaviour
{
    [field: SerializeField]
    [field: SyncVar]
    public Transform Parent { get; private set; }

    [SyncVar]
    public int questCardIndex;

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



    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text goldRewardText;
    [SerializeField] private TMP_Text reputationRewardText;
    [SerializeField] private TMP_Text lootRewardText;


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

    //[ServerRpc(RequireOwnership = false)]
    //public void ServerSetCardParent(Transform parent, bool worldPositionStays)
    //{
    //    OberserversSetCardParent(parent, worldPositionStays);
    //    this.transform.SetParent(parent, worldPositionStays);
    //    this.Parent = parent;
    //}

    [ObserversRpc(BufferLast = true)]
    private void OberserversSetCardParent(Transform parent, bool worldPositionStays)
    {
        this.transform.SetParent(parent, worldPositionStays);
    }


}
