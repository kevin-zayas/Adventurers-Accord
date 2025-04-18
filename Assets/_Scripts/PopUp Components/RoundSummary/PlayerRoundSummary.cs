using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerRoundSummary : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text goldReward;
    [SerializeField] private TMP_Text reputationReward;
    [SerializeField] private TMP_Text lootReward;
    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;

    [SerializeField] private GameObject bonusRewardGroup;
    [SerializeField] private BonusReward bonusRewardPrefab;

    public PlayerRoundSummary() { }
    public void SetPlayerSummary(PlayerRoundSummaryData summaryData)
    {
        playerNameText.text = summaryData.PlayerName;
        goldReward.text = summaryData.Gold.ToString();
        reputationReward.text = summaryData.Reputation.ToString();
        lootReward.text = summaryData.Loot.ToString();
        Dictionary<string, BonusRewardData> bonusRewards = summaryData.BonusRewards;

        foreach (string bonusName in bonusRewards.Keys)
        {
            print($"{bonusName} - {bonusRewards[bonusName].Gold} Gold - {bonusRewards[bonusName].Reputation} Reputation - {bonusRewards[bonusName].Loot} Loot");
            BonusRewardData bonus = bonusRewards[bonusName];
            BonusReward newBonus = Instantiate(bonusRewardPrefab, bonusRewardGroup.transform);
            newBonus.SetBonusReward(bonus.Name, bonus.Gold, bonus.Reputation, bonus.Loot);
        }

        //playerQuestSummaryData
        if (physicalPowerText != null)
        {
            physicalPowerText.text = summaryData.PhysicalPower.ToString();
            magicalPowerText.text = summaryData.MagicalPower.ToString();
        }
    }
}

public class PlayerRoundSummaryData
{
    public string PlayerName;
    public int Gold;
    public int Reputation;
    public int Loot;
    public int PhysicalPower;
    public int MagicalPower;

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

    public void UpdatePlayerSummary(int gold, int reputation, int loot, int physPower = 0, int magPower = 0)
    {
        Gold += gold;
        Reputation += reputation;
        Loot += loot;
        PhysicalPower = physPower;
        MagicalPower = magPower;
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

