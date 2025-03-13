using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : NetworkBehaviour
{
    public static ScoreBoard Instance;
    private ScoreBoardPopUp scoreBoardPopUp;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && scoreBoardPopUp == null)
        {
            scoreBoardPopUp = PopUpManager.Instance.CreateScoreBoardPopUp();
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            Destroy(scoreBoardPopUp.gameObject);
        }
    }

    [ObserversRpc]
    public void ObserversUpdatePlayerGold(int playerID, int gold)
    {
        if (scoreBoardPopUp != null)
        {
            scoreBoardPopUp.UpdatePlayerGold(playerID, gold);
        }
    }

    [ObserversRpc]
    public void ObserversUpdatePlayerReputation(int playerID, int reputation)
    {
        if (scoreBoardPopUp != null)
        {
            scoreBoardPopUp.UpdatePlayerReputation(playerID, reputation);
        }
    }

    [ObserversRpc]
    public void ObserversUpdateTurnMarker(int playerID)
    {
        if (scoreBoardPopUp != null)
        {
            scoreBoardPopUp.UpdatePlayerTurnMarker(playerID);
        }
    }

    [ObserversRpc]
    public void ObserversEnableAllTurnMarkers()
    {
        if (scoreBoardPopUp != null)
        {
            scoreBoardPopUp.EnableAllTurnMarkers();
        }
    }

    [ObserversRpc]
    public void ObserversToggleTurnMarker(int playerID, bool value)
    {
        if (scoreBoardPopUp != null)
        {
            scoreBoardPopUp.ToggleTurnMarker(playerID, value);
        }
    }
}
