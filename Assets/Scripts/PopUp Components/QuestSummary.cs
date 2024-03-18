using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestSummary : MonoBehaviour
{
    private string questName;
    private string questStatus;
    private int playerCount = 0;

    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text questStatusText;

    [SerializeField] private TMP_Text[] playerSummaries;

    [SerializeField] private string completedText;
    [SerializeField] private string failedText;

    public void SetQuestInfo(string name, string status, int playerCount)
    {
        questName = name;
        questStatus = status;
        //this.playerCount = playerCount;

        questNameText.text = questName;
        questStatusText.text = questStatus;

        //for (int i = 0; i < playerSummaries.Length; i++)
        //{
        //    if (i < playerCount)
        //    {
        //        playerSummaries[i].gameObject.SetActive(true);
        //    }
        //    else
        //    {
        //        playerSummaries[i].gameObject.SetActive(false);
        //    }
        //}
    }

    public void SetPlayerSummary(int player, int physPower, int magPower, int gold, int reputation, int loot)
    {
        playerSummaries[playerCount].text = string.Format(completedText, player, physPower, magPower, gold, reputation, loot);
        playerCount++;
    }
}
