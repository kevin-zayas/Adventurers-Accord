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
        closeButton.onClick.AddListener(() =>
        {
            ServerClosePopUp(LocalConnection);
        });
        if (IsServer) totalPlayers = GameManager.Instance.Players.Count;
    }

    [ObserversRpc]
    public void ObserversInitializeRoundSummaryPopUp()
    {
        print("initializing round summary pop up");
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = new Vector3(0, 0, 0);  //center of screen
    }

    [TargetRpc]
    public void TargetClosePopUp(NetworkConnection networkConnection)
    {
        if (IsServer) return;
        gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerClosePopUp(NetworkConnection networkConnection)
    {
        TargetClosePopUp(networkConnection);
        playerCount++;
        if (playerCount != totalPlayers) return;

        Despawn(this.gameObject);
    }

}
