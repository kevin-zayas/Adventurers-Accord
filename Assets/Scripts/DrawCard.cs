using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DrawCard : NetworkBehaviour
{
    public PlayerManager playerManager;


    public void StartGame()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        print("button press");
        print(networkIdentity);
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.CmdDrawCard(0);
    }
}
