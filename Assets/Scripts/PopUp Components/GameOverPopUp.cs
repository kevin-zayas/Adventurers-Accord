using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        int maxReputation = 0;
        List<Player> playerList = new(GameManager.Instance.Players);
        playerList = playerList.OrderByDescending(x => x.Reputation).ThenByDescending(x => x.Gold).ToList();

        List<string> rankingTextList = new();
        int prevRanking = 0;
        int prevReputation = 0;
        int prevGold = 0;
        maxReputation = playerList[0].Reputation;

        foreach (Player player in playerList)
        {
            if (player.Reputation != prevReputation || player.Gold != prevGold)
            {
                prevRanking++;
                prevReputation = player.Reputation;
                prevGold = player.Gold;
            }
            rankingTextList.Add(string.Format(rankingText, prevRanking, player.PlayerID + 1, player.Reputation, player.Gold));
        }

        foreach (Player player in playerList)
        {

            TargetInitializeGameOverPopUp(player.Owner, rankingTextList, player.Reputation == maxReputation);
        }
    }

    [TargetRpc]
    public void TargetInitializeGameOverPopUp(NetworkConnection network, List<string> rankings, bool victory)
    {
        print("initializing game over pop up");
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        for (int i = 0; i < rankings.Count; i++)
        {
            GuildRankings[i].gameObject.SetActive(true);
            GuildRankings[i].text = rankings[i];
        }

        if (victory) titleText.text = "Victory!";
        else titleText.text = "Defeat!";
    }
}
