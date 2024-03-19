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

    [field: SerializeField]
    [field: SyncVar]
    public QuestSummary[] QuestSummaries { get; private set; }

    [SyncVar] private int playerCount;
    [SyncVar] private int totalPlayers;



    // Start is called before the first frame update
    void Start()
    {
        if (IsServer) totalPlayers = GameManager.Instance.Players.Count;

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
        PopUpManager.Instance.ServerCloseRoundSummaryPopUp(connection, this.gameObject, playerCount == totalPlayers);
    }

}
