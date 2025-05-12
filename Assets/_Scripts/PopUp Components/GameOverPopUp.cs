using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPopUp : MonoBehaviour
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] Button restartServerButton;

    [field: SerializeField] public GuildRanking[] GuildRankings { get; private set; }

    private readonly List<GuildRankingData> RankingDataList = new();
    private int MaxReputation;
    private int MaxGold;

    private void Start()
    {
        restartServerButton.onClick.AddListener(() =>
        {
            ConfirmationPopUp confirmationPopUp = PopUpManager.Instance.CreateConfirmationPopUp();
            confirmationPopUp.InitializeRestartServerPopUp();
        });
    }

    public void CalculateRankings()
    {
        List<Player> playerList = new(GameManager.Instance.Players);
        playerList = playerList.OrderByDescending(x => x.Reputation.Value).ThenByDescending(x => x.Gold.Value).ToList();

        MaxReputation = playerList[0].Reputation.Value;
        MaxGold = playerList[0].Gold.Value;
        int currentRank = 0;
        int prevReputation = 0;
        int prevGold = 0;
        
        foreach (Player player in playerList)
        {
            if (player.Reputation.Value != prevReputation || player.Gold.Value != prevGold)
            {
                currentRank++;
                prevReputation = player.Reputation.Value;
                prevGold = player.Gold.Value;
            }
            GuildRankingData rankingData = new(currentRank, player);
            RankingDataList.Add(rankingData);
        }
    }

    public void InitializeGameOverPopUp()
    {
        CalculateRankings();
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        RectTransform rt = this.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;

        for (int i = 0; i < RankingDataList.Count; i++)
        {
            GuildRankings[i].gameObject.SetActive(true);
            GuildRankings[i].InitializeRanking(RankingDataList[i]);
        }

        foreach (KeyValuePair<string, int> kvp in Player.Instance.GuildRecapTracker)
        {
            Debug.Log($"{kvp.Key} - {kvp.Value}");
        }
            
    }
}
