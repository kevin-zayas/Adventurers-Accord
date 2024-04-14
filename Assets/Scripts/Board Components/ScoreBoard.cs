using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class ScoreBoard : NetworkBehaviour
{
    [field: SerializeField]
    public PlayerScore[] PlayerScores { get; private set; }

    //[SerializeField] GameObject scoreboardPanelPrefab;
    [SerializeField] GameObject turnMarkerPrefab;

    [SerializeField] private GameObject scoreboardPanel;

    [SerializeField]
    private GameObject turnMarker;

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

        ObserversUpdateTurnMarker(0);
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

    [ObserversRpc]
    public void ObserversUpdateTurnMarker(int playerID)
    {
        for (int i = 0; i < PlayerScores.Length; i++)
        {
            PlayerScores[i].TurnMarker.SetActive(i == playerID);
        }
    }

    [ObserversRpc]
    public void ObserversEnableAllTurnMarkers()
    {
        for (int i = 0; i < PlayerScores.Length; i++)
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
