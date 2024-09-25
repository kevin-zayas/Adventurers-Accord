using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundSummaryPopUp : NetworkBehaviour
{
    [SerializeField] Button closeButton;

    [field: SerializeField] public SyncVar<QuestSummary>[] QuestSummaries { get; private set; }

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
        print("initializing round summary pop up");
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
