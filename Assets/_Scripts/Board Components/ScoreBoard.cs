using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

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
            ServerCreateScoreBoardPopUp(LocalConnection);
        }

        if (Input.GetKeyUp(KeyCode.Tab) && scoreBoardPopUp != null)
        {
            ServerCloseScoreBoardPopUp(scoreBoardPopUp);
            scoreBoardPopUp = null;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerCreateScoreBoardPopUp(NetworkConnection connection)
    {
        ScoreBoardPopUp popUp = PopUpManager.Instance.CreateScoreBoardPopUp();
        Spawn(popUp.gameObject);
        popUp.TargetInitializeScoreboard(connection);
        TargetSetScoreBoardPopUp(connection, popUp); 
    }

    [TargetRpc]
    private void TargetSetScoreBoardPopUp(NetworkConnection connection, ScoreBoardPopUp scoreBoardPopUp)
    {
        this.scoreBoardPopUp = scoreBoardPopUp;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerCloseScoreBoardPopUp(ScoreBoardPopUp popUp)
    {
        if (popUp != null) Despawn(popUp.gameObject);
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
