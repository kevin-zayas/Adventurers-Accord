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
    public PopUp CurrentPopUp { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    [Server]
    public PopUp CreateResolutionPopUp()
    {
        print("Creating PopUp");
        PopUp popUp = Instantiate(Resources.Load<PopUp>("UI/PopUp"));
        CurrentPopUp = popUp;
        return popUp;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerDespawnPopUp(PopUp popUp)
    {
        print("Despawning PopUp");
        Despawn(popUp.gameObject);
    }

    [Server]
    public RoundSummaryPopUp CreateRoundSummaryPopUp()
    {
        print("Creating Round Summary PopUp");
        RoundSummaryPopUp popUp = Instantiate(Resources.Load<RoundSummaryPopUp>("UI/RoundSummaryPopUp"));
        return popUp;
    }
}
