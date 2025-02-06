using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.UI;

public class RoundSummaryPopUp : NetworkBehaviour
{
    [SerializeField] Button closeButton;

    [AllowMutableSyncTypeAttribute] public SyncList<QuestSummary> QuestSummaries = new();

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

    [ObserversRpc]
    public void ObserversInitializeRoundSummaryPopUp()
    {
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerClosePopUp(NetworkConnection connection)
    {
        playerCount++;
        PopUpManager.Instance.CloseRoundSummaryPopUp(connection, this.gameObject, playerCount == totalPlayers);
    }

}
