using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class PopUpManager : NetworkBehaviour
{
    public static PopUpManager Instance { get; private set; }

    [SerializeField] private EndTurnPopUp EndTurnPopUpPrefab;
    [SerializeField] private EquipItemPopUp EquipItemPopUpPrefab;
    [SerializeField] private GameOverPopUp GameOverPopUpPrefab;
    [SerializeField] private ResolutionPopUp ResolutionPopUpPrefab;
    [SerializeField] private RestartServerPopUp RestartServerPopUpPrefab;
    [SerializeField] private RoundSummaryPopUp RoundSummaryPopUpPrefab;
    
    //public PopUp Prefab { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public ResolutionPopUp CurrentResolutionPopUp { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public GameOverPopUp GameOverPopUpInstance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    [Server]
    public ResolutionPopUp CreateResolutionPopUp()
    {
        print("Creating PopUp");
        ResolutionPopUp popUp = Instantiate(ResolutionPopUpPrefab);
        CurrentResolutionPopUp = popUp;
        return popUp;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerDespawnResolutionPopUp(ResolutionPopUp popUp)
    {
        print("Despawning PopUp");
        Despawn(popUp.gameObject);
    }

    [Server]
    public RoundSummaryPopUp CreateRoundSummaryPopUp()
    {
        print("Creating Round Summary PopUp");
        RoundSummaryPopUp popUp = Instantiate(RoundSummaryPopUpPrefab);
        return popUp;
    }

    [Server]
    public void CloseRoundSummaryPopUp(NetworkConnection networkConnection, GameObject popUp, bool despawn)
    {
        TargetCloseRoundSummaryPopUp(networkConnection, popUp);

        if (despawn)
        {
            Despawn(popUp);
        }
    }

    [TargetRpc]
    public void TargetCloseRoundSummaryPopUp(NetworkConnection networkConnection, GameObject popUp)
    {
        //if (IsServer) return;
        print("closing round summary pop up");
        popUp.SetActive(false);

        if (GameManager.Instance.CurrentPhase == GameManager.Phase.GameOver)
        {
            Player player = GameManager.Instance.Players[LocalConnection.ClientId];
            this.GameOverPopUpInstance.ServerInitializeGameOverPopup(networkConnection, player);
        }

    }

    [Server]
    public GameOverPopUp CreateGameOverPopUp()
    {
        print("Creating GameOver PopUp");
        GameOverPopUp popUp = Instantiate(GameOverPopUpPrefab);
        GameOverPopUpInstance = popUp;
        return popUp;
    }

    public  EndTurnPopUp CreateEndTurnPopUp()
    {
        EndTurnPopUp popUp = Instantiate(EndTurnPopUpPrefab);
        return popUp;
    }

    public EquipItemPopUp CreateEquipItemPopUp()
    {
        EquipItemPopUp popUp = Instantiate(EquipItemPopUpPrefab);
        return popUp;
    }

    public RestartServerPopUp CreateRestartServerPopUp()
    {
        RestartServerPopUp popUp = Instantiate(RestartServerPopUpPrefab);
        return popUp;
    }
}
