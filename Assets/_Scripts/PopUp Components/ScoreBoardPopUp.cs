using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBoardPopUp : PopUp
{
    private List<PlayerScore> PlayerScores = new();
    [SerializeField] private PlayerScore playerScorePrefab;
    [SerializeField] private GameObject playerScoreGroup;

    protected override void Start()
    {
        InitializeScoreBoard();
        base.Start();
    }

    protected void InitializeScoreBoard()
    {
        int playerCount = GameManager.Instance.Players.Count;
        //int scoreboardHeight = 4 + 54 * playerCount;

        //RectTransform rectTransform = this.GetComponent<RectTransform>();
        //rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, scoreboardHeight);
        //scoreboardPanel.SetActive(false);

        for (int i = 0; i < playerCount; i++)
        {
            Player player = GameManager.Instance.Players[i];
            int gold = player.Gold.Value;
            int reputation = player.Reputation.Value;

            PlayerScore playerScore = Instantiate(playerScorePrefab, Vector2.zero, Quaternion.identity);
            playerScore.InitializeScore(i, gold, reputation);
            PlayerScores.Add(playerScore);
            playerScore.transform.SetParent(playerScoreGroup.transform, false);

            InitializeTurnMarkers(i, playerScore, player);
        }
    }

    protected void InitializeTurnMarkers(int playerIndex, PlayerScore playerScore, Player player)
    {
        if (GameManager.Instance.CurrentPhase.Value == GameManager.Phase.Magic)
        {
            playerScore.TurnMarker.SetActive(!GameManager.Instance.PlayerEndRoundStatus[playerIndex]);
        }
        else if (player.IsPlayerTurn.Value)
        {
            playerScore.TurnMarker.SetActive(true);
        }
    }
}
