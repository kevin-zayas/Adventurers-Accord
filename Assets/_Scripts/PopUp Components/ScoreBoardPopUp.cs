using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBoardPopUp : NetworkBehaviour
{
    private readonly List<PlayerScore> PlayerScores = new();
    [SerializeField] private PlayerScore playerScorePrefab;
    [SerializeField] private GameObject playerScoreGroup;
    [SerializeField] private GameObject rosterGroup;

    [TargetRpc]
    public void TargetInitializeScoreboard(NetworkConnection connection)
    {
        InitializeScoreBoard();
        ServerPopulateGuildRoster(connection);

        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        RectTransform rt = this.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
    }

    protected void InitializeScoreBoard()
    {
        int playerCount = GameManager.Instance.Players.Count;
        //int scoreboardHeight = 4 + 54 * playerCount;

        //RectTransform rectTransform = this.GetComponent<RectTransform>();
        //rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, scoreboardHeight);

        for (int i = 0; i < playerCount; i++)
        {
            Player player = GameManager.Instance.Players[i];
            int gold = player.Gold.Value;
            int reputation = player.Reputation.Value;

            PlayerScore playerScore = Instantiate(playerScorePrefab, Vector2.zero, Quaternion.identity);
            playerScore.InitializeScore(i, gold, reputation);
            PlayerScores.Add(playerScore);
            playerScore.transform.SetParent(playerScoreGroup.transform, false);

            UpdateTurnMarkers(i, player);
        }
    }

    protected void UpdateTurnMarkers(int playerIndex, Player player)
    {
        if (GameManager.Instance.CurrentPhase.Value == GameManager.Phase.Magic)
        {
            PlayerScores[playerIndex].TurnMarker.SetActive(!GameManager.Instance.PlayerEndRoundStatus[playerIndex]);
        }
        else if (player.IsPlayerTurn.Value)
        {
            PlayerScores[playerIndex].TurnMarker.SetActive(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected void ServerPopulateGuildRoster(NetworkConnection connection)
    {
        Player player = GameManager.Instance.Players[connection.ClientId];

        foreach (Transform handCard in player.controlledHand.Value.transform)
        {
            if (handCard.GetComponent<AdventurerCard>() != null)
            {
                print(handCard.GetComponent<AdventurerCard>().CardName.Value);
                GameObject newCardObject = Instantiate(handCard.gameObject, Vector2.zero, Quaternion.identity);
                Spawn(newCardObject);

                Card newCard = newCardObject.GetComponent<Card>();
                newCard.CopyCardData(connection, newCardObject, handCard.gameObject);

                TargetSetCardParent(connection, newCardObject);
            }
        }
    }

    [TargetRpc]
    private void TargetSetCardParent(NetworkConnection connection, GameObject card)
    {
        card.transform.SetParent(rosterGroup.transform, false);
    }

    public void UpdatePlayerGold(int playerIndex, int gold)
    {
        PlayerScores[playerIndex].UpdateGold(gold);
    }

    public void UpdatePlayerReputation(int playerIndex, int reputation)
    {
        PlayerScores[playerIndex].UpdateReputation(reputation);
    }

    public void UpdatePlayerTurnMarker(int playerIndex)
    {
        for (int i = 0; i < PlayerScores.Count; i++)
        {
            PlayerScores[i].TurnMarker.SetActive(i == playerIndex);
        }
    }

    public void EnableAllTurnMarkers()
    {
        for (int i = 0; i < PlayerScores.Count; i++)
        {
            PlayerScores[i].TurnMarker.SetActive(true);
        }
    }

    public void ToggleTurnMarker(int playerIndex, bool value)
    {
        PlayerScores[playerIndex].TurnMarker.SetActive(value);
    }
}
