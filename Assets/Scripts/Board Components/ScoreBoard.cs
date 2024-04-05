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

    //[SerializeField] GameObject scoreboardPanelPrefab;
    [SerializeField] GameObject turnMarkerPrefab;

    [SerializeField] private GameObject scoreboardPanel;

    [SerializeField]
    [SyncVar]
    private GameObject turnMarker;

    [SyncVar]
    private int playerCount;

    [Server]
    public void StartGame(int startingGold)
    {
        playerCount = GameManager.Instance.Players.Count;

        turnMarker = Instantiate(turnMarkerPrefab, new Vector2(-45f, 0f), Quaternion.identity);
        Spawn(turnMarker);

        ObserversInitializeScoreboard(playerCount, turnMarker);

        for (int i = 0; i < playerCount; i++)
        {
            Spawn(PlayerScores[i].gameObject);
            PlayerScores[i].InitializeScore(i, startingGold);
        }
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversInitializeScoreboard(int playerCount, GameObject marker)
    {
        scoreboardPanel.GetComponent<Image>().enabled = true;

        int scoreboardHeight = 25 + (115 * playerCount);

        RectTransform rectTransform = scoreboardPanel.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, scoreboardHeight);

        turnMarker = marker;
        ObserversUpdateTurnMarker(0);
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

    [ObserversRpc]
    public void ObserversUpdateTurnMarker(int playerID)
    {
        print(PlayerScores[playerID]);
        turnMarker.transform.SetParent(PlayerScores[playerID].transform, false);
        turnMarker.transform.localPosition = new Vector3(-387.5f, 0, 0);
    }
}
