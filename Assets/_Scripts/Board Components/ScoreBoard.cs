using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : NetworkBehaviour
{
    private List<PlayerScore> PlayerScores = new();

    [SerializeField] private PlayerScore playerScorePrefab;
    [SerializeField] private GameObject scoreboardPanel;
    [SerializeField] private GameObject playerScoreGroup;
    [SerializeField] Image rayCastBlocker;

    private int playerCount;

    [Server]
    public void StartGame(int startingGold)
    {
        playerCount = GameManager.Instance.Players.Count;
        ObserversInitializeScoreboard(playerCount,startingGold);
        ObserversUpdateTurnMarker(0);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversInitializeScoreboard(int playerCount, int startingGold)
    {
        int scoreboardHeight = 4 + 54 * playerCount;

        RectTransform rectTransform = scoreboardPanel.GetComponent<RectTransform>();
        //rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, scoreboardHeight);
        scoreboardPanel.SetActive(false);

        for (int i = 0; i < playerCount; i++)
        {
            PlayerScore playerScore = Instantiate(playerScorePrefab, Vector2.zero, Quaternion.identity);
            playerScore.InitializeScore(i, startingGold);
            PlayerScores.Add(playerScore);
            playerScore.transform.SetParent(playerScoreGroup.transform, false);

        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            scoreboardPanel.SetActive(true);
            rayCastBlocker.enabled = true;

            this.gameObject.transform.SetAsLastSibling();
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            scoreboardPanel.SetActive(false);
            rayCastBlocker.enabled = false;

        }
    }

    [ObserversRpc]
    public void ObserversUpdatePlayerGold(int playerID, int gold)
    {
        PlayerScores[playerID].UpdatePlayerGold(gold);
    }

    [ObserversRpc]
    public void UpdateUpdatePlayerReputation(int playerID, int reputation)
    {
        PlayerScores[playerID].UpdatePlayerReputation(reputation);
    }

    [ObserversRpc]
    public void ObserversUpdateTurnMarker(int playerID)
    {
        for (int i = 0; i < PlayerScores.Count; i++)
        {
            PlayerScores[i].TurnMarker.SetActive(i == playerID);
        }
    }

    [ObserversRpc]
    public void ObserversEnableAllTurnMarkers()
    {
        for (int i = 0; i < PlayerScores.Count; i++)
        {
            PlayerScores[i].TurnMarker.SetActive(true);
        }
    }

    [ObserversRpc]
    public void ObserversToggleTurnMarker(int playerID, bool value)
    {
        PlayerScores[playerID].TurnMarker.SetActive(value);
    }
}
