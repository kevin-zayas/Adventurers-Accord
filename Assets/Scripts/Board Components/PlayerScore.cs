using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerScore : NetworkBehaviour
{
    [field: SerializeField]
    [field: SyncVar]
    public int PlayerID { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int Gold { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int Reputation { get; private set; }

    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text reputationText;

    [Server]
    public void InitializeScore(int playerID, int startingGold)
    {
        //PlayerID = playerID;
        //Gold = startingGold;
        //Reputation = 0;

        ObserversInitializeScore(playerID, startingGold, 0);
    }

    [Server]
    public void UpdatePlayerGold(int gold)
    {
        //Gold = gold;

        ObserversUpdatePlayerGold(gold);
    }

    [Server]
    public void UpdatePlayerReputation(int reputation)
    {
        //Reputation = reputation;

        ObserversUpdatePlayerReputation(reputation);
    }

    [ObserversRpc]
    private void ObserversUpdatePlayerGold(int gold)
    {
        goldText.text = $"{gold} GP";
    }

    [ObserversRpc]
    private void ObserversUpdatePlayerReputation(int reputation)
    {
        reputationText.text = $"{reputation} Rep";
    }

    //[Server]
    //public void UpdatePlayerScore(int gold, int reputation)
    //{
    //    Gold = gold;
    //    Reputation = reputation;

    //    ObserversInitializeScore(PlayerID, Gold, Reputation);
    //}

    [ObserversRpc]
    public void ObserversInitializeScore(int playerID, int gold, int reputation)
    {
        //gameObject.SetActive(true);
        playerNameText.text = $"Player {playerID+1} -";
        goldText.text = $"{gold} GP";
        reputationText.text = $"{reputation} Rep";
    }
}
