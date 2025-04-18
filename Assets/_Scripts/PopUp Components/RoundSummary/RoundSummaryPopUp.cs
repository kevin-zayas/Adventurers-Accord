using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundSummaryPopUp : MonoBehaviour
{
    [SerializeField] Button closeButton;
    [SerializeField] Button viewMoreButton;
    [SerializeField] PlayerRoundSummary playerRoundSummaryPrefab;
    [SerializeField] GameObject playerRoundSummaryGroup;

    [SerializeField] List<QuestSummaryData> questSummaryDataList;

    void Start()
    {

        closeButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });

        viewMoreButton.onClick.AddListener(() =>
        {
            print(questSummaryDataList.Count);
            PopUpManager.Instance.CreateQuestSummaryPopUp(questSummaryDataList);

            //Destroy(GameObject);
        });
    }

    public void InitializeRoundSummaryPopUp(Dictionary<int, PlayerRoundSummaryData> playerSummaries, List<QuestSummaryData> questSummaries)
    {
        questSummaryDataList = questSummaries;

        foreach (Player player in GameManager.Instance.Players)
        {
            PlayerRoundSummaryData playerSummaryData = playerSummaries[player.PlayerID.Value];
            SetPlayerRoundSummary(playerSummaryData);
        }

        SetPopUpToCanvas();
    }

    public void SetPlayerRoundSummary(PlayerRoundSummaryData summaryData)
    {
        PlayerRoundSummary playerRoundSummary = Instantiate(playerRoundSummaryPrefab);
        playerRoundSummary.SetPlayerSummary(summaryData);
        playerRoundSummary.transform.SetParent(playerRoundSummaryGroup.transform);
        
    }    

    private void ObserversSetPlayerRoundSummary(PlayerRoundSummary playerRoundSummary, PlayerRoundSummaryData summaryData)
    {
        playerRoundSummary.transform.SetParent(playerRoundSummaryGroup.transform);
        playerRoundSummary.SetPlayerSummary(summaryData);
    }

    public void SetPopUpToCanvas()
    {
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        RectTransform rt = this.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
    }
}
