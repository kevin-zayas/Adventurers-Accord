using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : NetworkBehaviour
{
    [field: SerializeField] public PlayerScore[] PlayerScores { get; private set; }

    [SerializeField] private GameObject scoreboardPanel;
    [SerializeField] Image rayCastBlocker;

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
        int scoreboardHeight = 4 + 54 * playerCount;

        RectTransform rectTransform = scoreboardPanel.GetComponent<RectTransform>();
        //rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, scoreboardHeight);

        scoreboardPanel.SetActive(false);
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
