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
        //scoreboardPanel = Instantiate(scoreboardPanelPrefab, Vector2.zero, Quaternion.identity);
        //Spawn(scoreboardPanel);
        playerCount = GameManager.Instance.Players.Count;
        ObserversInitializeScoreboard(playerCount, startingGold);

        for (int i = 0; i < playerCount; i++)
        {
            //PlayerScores[i] = scoreboardPanel.transform.GetChild(i).GetComponent<PlayerScore>();
            Spawn(PlayerScores[i].gameObject);
            PlayerScores[i].InitializeScore(i, startingGold);
            //PlayerScores[i].ObserversInitializeScore(i + 1, startingGold, 0);
        }

        

        //for (int i = 0; i < playerCount; i++)
        //{
        //    PlayerScores[i].ObserversInitializeScore(i, startingGold, 0);
        //}
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversInitializeScoreboard(int playerCount, int startingGold)
    {
        scoreboardPanel.GetComponent<Image>().enabled = true;
        GameObject canvas = GameObject.Find("Scoreboard");
        scoreboardPanel.transform.SetParent(canvas.transform, false);

        int scoreboardHeight = 25 + (125 * playerCount);

        RectTransform rectTransform = transform.GetChild(0).GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, scoreboardHeight);

        //for (int i = 0; i < playerCount; i++)
        //{
            //PlayerScores[i].gameObject.SetActive(true);
            //PlayerScores[i].ObserversInitializeScore(i + 1, startingGold, 0);
        //}
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
