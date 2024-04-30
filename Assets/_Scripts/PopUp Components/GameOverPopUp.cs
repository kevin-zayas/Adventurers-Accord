using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPopUp : NetworkBehaviour
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] string rankingText;
    [SerializeField] Button restartServerButton;

    [field: SerializeField]
    public TMP_Text[] GuildRankings { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public List<string> RankingTextList { get; private set; }

    [field: SerializeField]
    public int MaxReputation { get; private set; }

    [field: SerializeField]
    public int MaxGold { get; private set; }

    private void Start()
    {
        restartServerButton.onClick.AddListener(() =>
        {
            RestartServerPopUp restartServerPopUp = Instantiate(Resources.Load<RestartServerPopUp>("PopUps/RestartServerPopUp"));
            restartServerPopUp.transform.SetParent(GameObject.Find("Canvas").transform);
            restartServerPopUp.transform.localPosition = Vector3.zero;
        });
    }

    [Server]
    public void CalculateRankings()
    {
        List<Player> playerList = new(GameManager.Instance.Players);
        playerList = playerList.OrderByDescending(x => x.Reputation).ThenByDescending(x => x.Gold).ToList();

        RankingTextList = new();
        MaxReputation = playerList[0].Reputation;
        MaxGold = playerList[0].Gold;
        int prevRanking = 0;
        int prevReputation = 0;
        int prevGold = 0;
        

        foreach (Player player in playerList)
        {
            if (player.Reputation != prevReputation || player.Gold != prevGold)
            {
                prevRanking++;
                prevReputation = player.Reputation;
                prevGold = player.Gold;
            }
            RankingTextList.Add(string.Format(rankingText, prevRanking, player.PlayerID + 1, player.Reputation, player.Gold));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerInitializeGameOverPopup(NetworkConnection network, Player player)
    {
        TargetInitializeGameOverPopUp(network, player.Reputation == MaxReputation && player.Gold == MaxGold);
    }

    [TargetRpc]
    public void TargetInitializeGameOverPopUp(NetworkConnection network, bool victory)
    {
        print("initializing game over pop up");
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        for (int i = 0; i < RankingTextList.Count; i++)
        {
            GuildRankings[i].gameObject.SetActive(true);
            GuildRankings[i].text = RankingTextList[i];
        }

        if (victory) titleText.text = "Victory!";
        else titleText.text = "Defeat!";
    }
}
