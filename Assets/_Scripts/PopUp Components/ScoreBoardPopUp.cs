using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoardPopUp : NetworkBehaviour
{
    private readonly List<PlayerScore> PlayerScores = new();
    [SerializeField] private PlayerScore playerScorePrefab;
    [SerializeField] private GameObject playerScoreGroup;
    [SerializeField] private GameObject rosterGroup;
    [SerializeField] private Button closeButton;

    void Start()
    {
        closeButton.onClick.AddListener(() =>
        {
            ServerClosePopUp();
        });
    }

    [TargetRpc]
    public void TargetInitializeScoreboard(NetworkConnection connection)
    {
        InitializeScoreBoard();

        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        RectTransform rt = this.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
    }

    protected void InitializeScoreBoard()
    {
        int playerCount = GameManager.Instance.Players.Count;

        for (int i = 0; i < playerCount; i++)
        {
            Player player = GameManager.Instance.Players[i];
            int gold = player.Gold.Value;
            int reputation = player.Reputation.Value;

            PlayerScore playerScore = Instantiate(playerScorePrefab, Vector2.zero, Quaternion.identity);
            playerScore.InitializeScore(i, gold, reputation, player.GuildType);
            PlayerScores.Add(playerScore);
            playerScore.transform.SetParent(playerScoreGroup.transform, false);

            Button rosterButton = playerScore.GetRosterButton();
            bool isViewingRival = LocalConnection.ClientId != i;

            rosterButton.onClick.AddListener(() =>
            {
                ServerCreateGuildRosterPopUp(LocalConnection, player, isViewingRival);
            });
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerCreateGuildRosterPopUp(NetworkConnection connection, Player player, bool isViewingRival)
    {
        PopUpManager.Instance.CreateGuildRosterPopUp(connection, player, isViewingRival);
        Despawn(gameObject);
    }

    public void UpdatePlayerGold(int playerIndex, int gold)
    {
        PlayerScores[playerIndex].UpdateGold(gold);
    }

    public void UpdatePlayerReputation(int playerIndex, int reputation)
    {
        PlayerScores[playerIndex].UpdateReputation(reputation);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerClosePopUp()
    {
        Despawn(gameObject);
    }
}
