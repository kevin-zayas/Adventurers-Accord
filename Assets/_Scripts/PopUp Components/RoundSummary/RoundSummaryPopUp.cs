using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundSummaryPopUp : NetworkBehaviour
{
    [SerializeField] Button closeButton;
    [SerializeField] PlayerRoundSummary playerRoundSummaryPrefab;
    [SerializeField] GameObject playerRoundSummaryGroup;

    //[AllowMutableSyncTypeAttribute] public SyncList<QuestSummary> QuestSummaries = new();

    private int playerCount;
    private int totalPlayers;

    // Start is called before the first frame update
    void Start()
    {
        if (IsServerInitialized) totalPlayers = GameManager.Instance.Players.Count;

        closeButton.onClick.AddListener(() =>
        {
            ServerClosePopUp(LocalConnection);
        });
    }

    [Server]
    public void SetPlayerRoundSummary(PlayerRoundSummaryData summaryData)
    {
        PlayerRoundSummary newPlayerRoundSummary = Instantiate(playerRoundSummaryPrefab);
        Spawn(newPlayerRoundSummary.gameObject);
        ObserversSetPlayerRoundSummary(newPlayerRoundSummary, summaryData);
    }

    [ObserversRpc]
    private void ObserversSetPlayerRoundSummary(PlayerRoundSummary playerRoundSummary, PlayerRoundSummaryData summaryData)
    {
        playerRoundSummary.transform.SetParent(playerRoundSummaryGroup.transform);
        playerRoundSummary.SetPlayerRoundSummary(summaryData.PlayerName, summaryData.Gold, summaryData.Reputation, summaryData.Loot, summaryData.BonusRewards);
    }

    [ObserversRpc]
    public void ObserversInitializeRoundSummaryPopUp()
    {
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        RectTransform rt = this.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerClosePopUp(NetworkConnection connection)
    {
        playerCount++;
        PopUpManager.Instance.CloseRoundSummaryPopUp(connection, this.gameObject, playerCount == totalPlayers);
    }

}
