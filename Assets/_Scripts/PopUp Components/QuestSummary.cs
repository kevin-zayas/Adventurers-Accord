using FishNet.Demo.AdditiveScenes;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class QuestSummary : NetworkBehaviour
{
    private string questName;
    private string questStatus;

    private int playerCount = 0;

    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text questStatusText;
    [SerializeField] private TMP_Text physicalPower;
    [SerializeField] private TMP_Text magicalPower;

    [SerializeField] private TMP_Text[] playerSummaries;

    [SerializeField]
    [TextArea]
    private string playerPowerText;

    [SerializeField]
    [TextArea]
    private string rewardText;

    [SerializeField]
    [TextArea]
    private string penaltyText;

    [SerializeField]
    private string bardBonusText;

    [ObserversRpc]
    public void ObserversSetQuestInfo(string name, string status, int totalPhysical, int questPhysical, int totalMagical, int questMagical)
    {
        questName = name;
        questStatus = status;

        questNameText.text = questName;
        questStatusText.text = "Quest: " + questStatus;

        physicalPower.text = $"{totalPhysical} / {questPhysical}";
        magicalPower.text = $"{totalMagical} / {questMagical}";

        switch (questStatus)
        {
            case "Complete!":
                questStatusText.color = Color.green;
                break;
            case "Failed":
                questStatusText.color = Color.red;
                break;
            default:
                questStatusText.color = Color.black;
                break;
        }
    }

    [ObserversRpc]
    public void ObserversSetPlayerSummary(int player, int physPower, int magPower, int gold, int reputation, int loot)
    {
        playerSummaries[playerCount].gameObject.SetActive(true);
        playerSummaries[playerCount].text = Regex.Unescape(string.Format(playerPowerText, player+1, physPower, magPower));
        playerSummaries[playerCount].text += Regex.Unescape(string.Format(rewardText, gold, reputation, loot));
        playerCount++;
    }

    [ObserversRpc]
    public void ObserversSetPlayerSummary(int player, int physPower, int magPower, int reputation)
    {
        playerSummaries[playerCount].gameObject.SetActive(true);
        playerSummaries[playerCount].text = Regex.Unescape(string.Format(playerPowerText, player+1, physPower, magPower));
        playerSummaries[playerCount].text += Regex.Unescape(string.Format(penaltyText, reputation));
        playerCount++;
    }

    [ObserversRpc]
    public void ObserversAddBardBonus(int player, int physPower, int magPower, int bardBonus)
    {
        for (int i = 0; i < playerSummaries.Length; i++)
        {
            if (playerSummaries[i].text.Contains($"Player {player+1}"))
            {
                playerSummaries[i].text += Regex.Unescape(string.Format(bardBonusText, bardBonus));
                return;
            }
        }

        playerSummaries[playerCount].gameObject.SetActive(true);
        playerSummaries[playerCount].text = Regex.Unescape(string.Format(playerPowerText, player+1, physPower, magPower));
        playerSummaries[playerCount].text += Regex.Unescape(string.Format("Rewards:"+bardBonusText, bardBonus));
        playerCount++;
    }
    
}
