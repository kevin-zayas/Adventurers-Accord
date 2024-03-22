using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : NetworkBehaviour
{
    [field: SerializeField]
    public PlayerScore[] PlayerScores { get; private set; }

    [SerializeField] GameObject scoreboardPanelPrefab;

    [SerializeField] private GameObject scoreboardPanel;

    [SyncVar]
    private int playerCount;

    [Server]
    public void StartGame(int startingGold)
    {
        playerCount = GameManager.Instance.Players.Count;
        ObserversInitializeScoreboard(playerCount);

        for (int i = 0; i < playerCount; i++)
        {
            Spawn(PlayerScores[i].gameObject);
            PlayerScores[i].InitializeScore(i, startingGold);
        }
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversInitializeScoreboard(int playerCount)
    {
        scoreboardPanel.GetComponent<Image>().enabled = true;

        int scoreboardHeight = 25 + (115 * playerCount);

        RectTransform rectTransform = scoreboardPanel.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, scoreboardHeight);
    }

    [Server]
    public void UpdatePlayerGold(int playerID, int gold)
    {
        PlayerScores[playerID].UpdatePlayerGold(gold);
    }

    [Server]
    public void UpdatePlayerReputation(int playerID, int reputation)
    {
        PlayerScores[playerID].UpdatePlayerReputation(reputation);
    }
}
