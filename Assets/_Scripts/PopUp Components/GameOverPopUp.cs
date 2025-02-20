using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
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

    [field: SerializeField] public TMP_Text[] GuildRankings { get; private set; }

    public readonly SyncList<string> RankingTextList = new();
    public int MaxReputation { get; private set; }
    public int MaxGold { get; private set; }

    private void Start()
    {
        restartServerButton.onClick.AddListener(() =>
        {
            ConfirmationPopUp confirmationPopUp = PopUpManager.Instance.CreateConfirmationPopUp();
            confirmationPopUp.InitializeRestartServerPopUp();
        });
    }

    [Server]
    public void CalculateRankings()
    {
        List<Player> playerList = new(GameManager.Instance.Players);
        playerList = playerList.OrderByDescending(x => x.Reputation.Value).ThenByDescending(x => x.Gold.Value).ToList();

        RankingTextList.Clear();
        MaxReputation = playerList[0].Reputation.Value;
        MaxGold = playerList[0].Gold.Value;
        int prevRanking = 0;
        int prevReputation = 0;
        int prevGold = 0;
        

        foreach (Player player in playerList)
        {
            if (player.Reputation.Value != prevReputation || player.Gold.Value != prevGold)
            {
                prevRanking++;
                prevReputation = player.Reputation.Value;
                prevGold = player.Gold.Value;
            }
            RankingTextList.Add(string.Format(rankingText, prevRanking, player.PlayerID.Value + 1, player.Reputation.Value, player.Gold.Value));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerInitializeGameOverPopup(NetworkConnection network, Player player)
    {
        TargetInitializeGameOverPopUp(network, player.Reputation.Value == MaxReputation && player.Gold.Value == MaxGold);
    }

    [TargetRpc]
    public void TargetInitializeGameOverPopUp(NetworkConnection network, bool victory)
    {
        print("initializing game over pop up");
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        RectTransform rt = this.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;

        for (int i = 0; i < RankingTextList.Count; i++)
        {
            GuildRankings[i].gameObject.SetActive(true);
            GuildRankings[i].text = RankingTextList[i];
        }

        if (victory) titleText.text = "Victory!";
        else titleText.text = "Defeat!";
    }
}
