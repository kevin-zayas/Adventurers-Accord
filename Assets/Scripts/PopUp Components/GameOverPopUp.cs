using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverPopUp : NetworkBehaviour
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] string rankingText;

    [field: SerializeField]
    public TMP_Text[] GuildRankings { get; private set; }

    [Server]
    public void CalculateRankings()
    {
        List<Player> playerList = new(GameManager.Instance.Players);
        playerList.Sort((a, b) => b.Reputation.CompareTo(a.Reputation));

        print(playerList);

        List<string> rankingTextList = new();
        int prevRanking = 0;
        int prevReputation = 0;

        foreach(Player player in playerList)
        {
            if (player.Reputation != prevReputation)
            {
                prevRanking++;
                prevReputation = player.Reputation;
            }
            rankingTextList.Add(string.Format(rankingText, prevRanking, player.PlayerID, player.Reputation));
        }

        ObserversInitializeGameOverPopUp(rankingTextList);
    }

    [ObserversRpc]
    public void ObserversInitializeGameOverPopUp(List<string> rankings)
    {
        print("initializing game over pop up");
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        for (int i = 0; i < rankings.Count; i++)
        {
            GuildRankings[i].gameObject.SetActive(true);
            GuildRankings[i].text = rankings[i];
        }
    }
}
