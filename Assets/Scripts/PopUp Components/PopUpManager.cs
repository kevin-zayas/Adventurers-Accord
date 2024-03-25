using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class PopUpManager : NetworkBehaviour
{
    [field: SerializeField] public string RogueTitleText { get; private set; }
    [field: SerializeField] public string RogueDefaultMessageText { get; private set; }
    [field: SerializeField] public string RogueConfirmSelectionText { get; private set; }
    [field: SerializeField] public string RogueConfirmCloseText { get; private set; }
    [field: SerializeField] public string RogueButtonText { get; private set; }



    [field: SerializeField] public string AssassinTitleText { get; private set; }
    [field: SerializeField] public string AssassinDefaultMessageText { get; private set; }
    [field: SerializeField] public string AssassinConfirmSelectionText { get; private set; }
    [field: SerializeField] public string AssassinConfirmCloseText { get; private set; }
    [field: SerializeField] public string AssassinButtonText { get; private set; }
    [field: SerializeField] public string AssassinConfirmStatText { get; private set; }

    public static PopUpManager Instance { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public ResolutionPopUp CurrentPopUp { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    [Server]
    public ResolutionPopUp CreateResolutionPopUp()
    {
        print("Creating PopUp");
        ResolutionPopUp popUp = Instantiate(Resources.Load<ResolutionPopUp>("PopUps/ResolutionPopUp"));
        CurrentPopUp = popUp;
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
        RoundSummaryPopUp popUp = Instantiate(Resources.Load<RoundSummaryPopUp>("PopUps/RoundSummaryPopUp"));
        return popUp;
    }

    [Server]
    public void CloseRoundSummaryPopUp(NetworkConnection networkConnection, GameObject popUp, bool despawn)
    {
        TargetCloseRoundSummaryPopUp(networkConnection, popUp);

        if (despawn)
        {
            Despawn(popUp);
            GameManager.Instance.LaunchGameOverPopUp();
        }
    }

    [TargetRpc]
    public void TargetCloseRoundSummaryPopUp(NetworkConnection networkConnection, GameObject popUp)
    {
        //if (IsServer) return;
        print("closing round summary pop up");
        popUp.SetActive(false);
    }

    [Server]
    public GameOverPopUp CreateGameOverPopUp()
    {
        print("Creating GameOver PopUp");
        GameOverPopUp popUp = Instantiate(Resources.Load<GameOverPopUp>("PopUps/GameOverPopUp"));
        return popUp;
    }
}
