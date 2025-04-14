using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerRoundSummary : NetworkBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text goldReward;
    [SerializeField] private TMP_Text reputationReward;
    [SerializeField] private TMP_Text lootReward;

    [SerializeField] private GameObject bonusRewardGroup;
    [SerializeField] private BonusReward bonusRewardPrefab;

    public void SetPlayerRoundSummary(string playerName, int gold, int reputation, int loot, Dictionary<string, BonusRewardData> bonusRewards)
    {
        playerNameText.text = playerName;
        goldReward.text = gold.ToString();
        reputationReward.text = reputation.ToString();
        lootReward.text = loot.ToString();

        foreach (string bonusName in bonusRewards.Keys)
        {
            print($"{bonusName} - {bonusRewards[bonusName].Gold} Gold - {bonusRewards[bonusName].Reputation} Reputation - {bonusRewards[bonusName].Loot} Loot");
            BonusRewardData bonus = bonusRewards[bonusName];
            BonusReward newBonus = Instantiate(bonusRewardPrefab, bonusRewardGroup.transform);
            newBonus.SetBonusReward(bonus.Name, bonus.Gold, bonus.Reputation, bonus.Loot);
        }
    }
}

public class PlayerRoundSummaryData
{
    public string PlayerName;
    public int Gold;
    public int Reputation;
    public int Loot;
    public Dictionary<string,BonusRewardData> BonusRewards;
    public PlayerRoundSummaryData() { }
    public PlayerRoundSummaryData(string playerName)
    {
        PlayerName = playerName;
        Gold = 0;
        Reputation = 0;
        Loot = 0;
        BonusRewards = new();
    }

    public void UpdatePlayerSummary(int gold, int reputation, int loot)
    {
        Gold += gold;
        Reputation += reputation;
        Loot += loot;
    }

    public void AddBonusReward(string bonusName, int gold, int reputation, int loot)
    {
        BonusRewardData bonusRewardData;

        if (BonusRewards.ContainsKey(bonusName))
        {
            bonusRewardData = BonusRewards[bonusName];
            bonusRewardData.UpdateRewardData(gold, reputation, loot);
        }
        else
        {
            bonusRewardData = new BonusRewardData(bonusName, gold, reputation, loot);
            BonusRewards.Add(bonusName, bonusRewardData);
        }
    }
}

