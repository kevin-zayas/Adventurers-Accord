using FishNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyView : View
{
    [SerializeField]
    private TMP_Text playerList;

    [SerializeField]
    private Button readyButton;

    [SerializeField]
    private TMP_Text readyButtonText;

    [SerializeField]
    private Button startButton;

    public override void Initialize()
    {
        readyButton.onClick.AddListener(() => Player.Instance.IsReady = !Player.Instance.IsReady);

        if (InstanceFinder.IsServer)
        {
            startButton.onClick.AddListener(() => GameManager2.Instance.StartGame());
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

        int playerCount = GameManager2.Instance.Players.Count;

        for (int i = 0; i < playerCount; i++)
        {
            Player player = GameManager2.Instance.Players[i];

            playerListText += $"Player {player.OwnerId} (Is Ready: {player.IsReady})";

            if (i < playerCount - 1) playerListText += "\r\n";
        }
        playerList.text = playerListText;

        readyButtonText.color = Player.Instance.IsReady ? Color.green : Color.red;

        startButton.interactable = GameManager2.Instance.CanStart;
    }
}

