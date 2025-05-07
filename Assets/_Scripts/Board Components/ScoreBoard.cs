using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class ScoreBoard : NetworkBehaviour
{
    public static ScoreBoard Instance;
    private ScoreBoardPopUp scoreBoardPopUp;
    private GuildRosterPopUp guildRosterPopUp;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (scoreBoardPopUp == null && guildRosterPopUp == null)
            {
                ServerCreateScoreBoardPopUp(LocalConnection);
            }
            else
            {
                ServerClosePopUp(scoreBoardPopUp);
                scoreBoardPopUp = null;

                ServerClosePopUp(guildRosterPopUp);
                guildRosterPopUp = null;
            }
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

    [TargetRpc]
    public void TargetSetGuildRosterPopUp(NetworkConnection connection, GuildRosterPopUp guildRosterPopUp)
    {
        this.guildRosterPopUp = guildRosterPopUp;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerClosePopUp(ScoreBoardPopUp popUp)
    {
        if (popUp != null) Despawn(popUp.gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerClosePopUp(GuildRosterPopUp popUp)
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
}
