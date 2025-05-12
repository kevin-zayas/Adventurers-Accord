using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CardDatabase;

public class GuildRanking : MonoBehaviour
{
    [SerializeField] TMP_Text rank;
    [SerializeField] TMP_Text playerName;
    [SerializeField] TMP_Text playerReputation;
    [SerializeField] TMP_Text playerGold;
    [SerializeField] Image guildIcon;
    [SerializeField] Button recapButton;

    public void InitializeRanking(GuildRankingData guildRankingData)
    {
        this.rank.text = guildRankingData.Rank.ToString();
        this.playerName.text = guildRankingData.PlayerName;
        this.playerReputation.text = guildRankingData.PlayerReputation.ToString();
        this.playerGold.text = guildRankingData.PlayerGold.ToString();
        guildIcon.sprite = CardDatabase.Instance.GetGuildSprite(guildRankingData.GuildType);

        recapButton.onClick.AddListener(() =>
        { 
            print("Recap Button Clicked");
            PopUpManager.Instance.CreateGuildRecapPopUp(guildRankingData.Player);
        });
    }
}

public class GuildRankingData
{
    public int Rank;
    public string PlayerName;
    public int PlayerReputation;
    public int PlayerGold;
    public GuildType GuildType;
    public Player Player;

    public GuildRankingData(int rank, Player player)
    {
        this.Rank = rank;
        this.PlayerName = $"Player {player.PlayerID.Value + 1} -";
        this.PlayerReputation = player.Reputation.Value;
        this.PlayerGold = player.Gold.Value;
        this.GuildType = player.GuildType;
        this.Player = player;
    }
}
