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

    public static PopUpManager Instance { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public PopUp CurrentPopUp { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    [Server]
    public PopUp CreatePopUp()
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
}
