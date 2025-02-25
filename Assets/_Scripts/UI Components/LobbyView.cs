using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyView : View
{
    [SerializeField] Button howToPlayButton;
    [SerializeField] Button readyButton;
    [SerializeField] Button startButton;

    [SerializeField] TMP_Text playerList;
    [SerializeField] TMP_Text readyButtonText;    

    public override void Initialize()
    {
        howToPlayButton.onClick.AddListener(() => PopUpManager.Instance.CreateHowToPlayPopUp());

        readyButton.onClick.AddListener(() => Player.Instance.TogglePlayerIsReady());

        //if (InstanceFinder.IsServer)
        if (Player.Instance.IsStartingPlayer.Value) // could just check is player 1
        {
            startButton.onClick.AddListener(() => GameManager.Instance.ServerStartGame());
        }
        else
        {
            startButton.gameObject.SetActive(false);
        }

        base.Initialize();
    }

    private void Update()
    {
        if (!IsInitialized) return;

        string playerListText = "";

        int playerCount = GameManager.Instance.Players.Count;

        for (int i = 0; i < playerCount; i++)
        {
            Player player = GameManager.Instance.Players[i];

            if (player.IsReady.Value) playerListText += $"Player {player.OwnerId+1} - Ready";
            else playerListText += $"Player {player.OwnerId + 1} - Not Ready";

            if (i < playerCount - 1) playerListText += "\r\n";
        }
        playerList.text = playerListText;

        readyButtonText.color = Player.Instance.IsReady.Value ? Color.green : Color.red;

        startButton.interactable = GameManager.Instance.CanStartGame.Value;
    }
}
