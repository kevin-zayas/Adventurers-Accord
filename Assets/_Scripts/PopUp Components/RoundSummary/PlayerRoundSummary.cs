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

    [SerializeField] private GameObject bonusRewardGroup;
    [SerializeField] private BonusReward bonusRewardPrefab;

    public void SetPlayerRoundSummary(string playerName, int gold, int reputation, int loot, List<BonusRewardData> bonusRewards)
    {
        playerNameText.text = playerName;
        goldReward.text = gold.ToString();
        reputationReward.text = reputation.ToString();
        lootReward.text = loot.ToString();

        foreach (BonusRewardData bonus in bonusRewards)
        {
            BonusReward newBonus = Instantiate(bonusRewardPrefab, bonusRewardGroup.transform);
            newBonus.SetBonusReward(bonus.Name, bonus.Amount1, bonus.Type1, bonus.Amount2, bonus.Type2);
        }
    }
}

