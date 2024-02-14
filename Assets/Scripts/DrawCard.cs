using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class DrawCard : NetworkBehaviour
{
    public PlayerManager playerManager;


    public void StartGame()
    {
        print("button press");

        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        NetworkIdentity local = NetworkClient.localPlayer;
        print($"local ID: {local}");
        print($"networkIdentity: {networkIdentity}");

        playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.CmdStart();

        //playerManager.CmdDrawCard(0);
    }

}
