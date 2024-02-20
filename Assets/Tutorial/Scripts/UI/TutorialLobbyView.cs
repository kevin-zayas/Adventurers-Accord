using FishNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialLobbyView : TutorialView
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
        readyButton.onClick.AddListener(() => TutorialPlayer.Instance.IsReady = !TutorialPlayer.Instance.IsReady);

        if (InstanceFinder.IsServer)
        {
            startButton.onClick.AddListener(() => TutorialGameManager.Instance.StartGame());
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

        int playerCount = TutorialGameManager.Instance.Players.Count;

        for (int i = 0; i < playerCount; i++)
        {
            TutorialPlayer player = TutorialGameManager.Instance.Players[i];

            playerListText += $"Player {player.OwnerId} (Is Ready: {player.IsReady})";

            if (i < playerCount - 1) playerListText += "\r\n";
        }
        playerList.text = playerListText;

        readyButtonText.color = TutorialPlayer.Instance.IsReady ? Color.green : Color.red;

        startButton.interactable = TutorialGameManager.Instance.CanStart;
    }
}

