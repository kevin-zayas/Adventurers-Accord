using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerScore : NetworkBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text reputationText;

    [field: SerializeField] 
    public GameObject TurnMarker { get; private set; }

    [Server]
    public void InitializeScore(int playerID, int startingGold)
    {
        ObserversInitializeScore(playerID, startingGold, 0);
    }

    [Server]
    public void UpdatePlayerGold(int gold)
    {
        ObserversUpdatePlayerGold(gold);
    }

    [Server]
    public void UpdatePlayerReputation(int reputation)
    {
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

    [ObserversRpc]
    public void ObserversInitializeScore(int playerID, int gold, int reputation)
    {
        playerNameText.text = $"Player {playerID+1} -";
        goldText.text = $"{gold} GP";
        reputationText.text = $"{reputation} Rep";
    }
}
