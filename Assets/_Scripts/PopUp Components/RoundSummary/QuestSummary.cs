using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static QuestLocation;

public class QuestSummary : MonoBehaviour
{
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text questStatusText;
    [SerializeField] private TMP_Text physicalPower;
    [SerializeField] private TMP_Text magicalPower;

    [SerializeField] private TMP_Text[] playerSummaries;
    [SerializeField] private PlayerRoundSummary playerQuestSummaryPrefab;
    [SerializeField] private GameObject playerQuestSummaryGroup;

    public void SetQuestInfo(QuestSummaryData questSummary)
    {
        questNameText.text = questSummary.QuestName;
        questStatusText.text = "Quest: " + questSummary.Status.ToString();

        physicalPower.text = $"{questSummary.TotalPhysicalPower} / {questSummary.PhysicalPower}";
        magicalPower.text = $"{questSummary.TotalMagicalPower}  /  {questSummary.MagicalPower}";

        questStatusText.color = questSummary.Status switch
        {
            QuestStatus.Complete => Color.green,
            QuestStatus.Failed => Color.red,
            _ => Color.black,
        };

        for (int i = 0; i < questSummary.QuestPlayerSummaries.Count; i++)
        {
            PlayerRoundSummaryData playerQuestSummaryData = questSummary.QuestPlayerSummaries[i];
            PlayerRoundSummary newPlayerQuestSummary = Instantiate(playerQuestSummaryPrefab);
            newPlayerQuestSummary.SetPlayerSummary(playerQuestSummaryData);
            newPlayerQuestSummary.transform.SetParent(playerQuestSummaryGroup.transform);
        }
        Canvas.ForceUpdateCanvases();
        playerQuestSummaryGroup.GetComponent<VerticalLayoutGroup>().enabled = false;
        playerQuestSummaryGroup.GetComponent<VerticalLayoutGroup>().enabled = true;
    }
}

public class QuestSummaryData
{
    public string QuestName;
    public int PhysicalPower;
    public int MagicalPower;
    public int TotalPhysicalPower;
    public int TotalMagicalPower;
    public QuestStatus Status;
    public Dictionary<int,PlayerRoundSummaryData> QuestPlayerSummaries;

    public QuestSummaryData() { }
    public QuestSummaryData(QuestLocation questLocation)
    {
        QuestName = questLocation.QuestCard.Value.CardName.Value;
        PhysicalPower = questLocation.QuestCard.Value.PhysicalPower.Value;
        MagicalPower = questLocation.QuestCard.Value.MagicalPower.Value;
        TotalPhysicalPower = questLocation.TotalPhysicalPower.Value;
        TotalMagicalPower = questLocation.TotalMagicalPower.Value;
        Status = questLocation.Status;
        QuestPlayerSummaries = new();
    }
}
