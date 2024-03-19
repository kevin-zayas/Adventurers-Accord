using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestSummary : NetworkBehaviour
{
    private string questName;
    private string questStatus;
    private int playerCount = 0;

    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text questStatusText;

    [SerializeField] private TMP_Text[] playerSummaries;

    [SerializeField]
    [TextArea]
    private string completedText;
    [SerializeField] private string failedText;

    [ObserversRpc]
    public void ObserversSetQuestInfo(string name, string status)
    {
        questName = name;
        questStatus = status;

        questNameText.text = questName;
        questStatusText.text = questStatus;
    }

    [ObserversRpc]
    public void ObserversSetPlayerSummary(int player, int physPower, int magPower, int gold, int reputation, int loot)
    {
        playerSummaries[playerCount].gameObject.SetActive(true);
        playerSummaries[playerCount].text = string.Format(completedText, player, physPower, magPower, gold, reputation, loot);
        playerCount++;
    }
}
