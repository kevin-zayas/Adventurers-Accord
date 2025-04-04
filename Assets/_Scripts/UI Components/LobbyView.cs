using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CardDatabase;

public class LobbyView : View
{
    [SerializeField] Button howToPlayButton;
    [SerializeField] Button readyButton;
    [SerializeField] Button startButton;

    [SerializeField] TMP_Text playerList;
    [SerializeField] TMP_Text readyButtonText;

    [SerializeField] Button fightersGuildButton;
    [SerializeField] Button magesGuildButton;
    [SerializeField] Button thievesGuildButton;
    [SerializeField] Button merchantsGuildButton;
    [SerializeField] Button assassinsGuildButton;
    private Button selectedButton;

    [SerializeField] TMP_Text guildText;


    public override void Initialize()
    {
        howToPlayButton.onClick.AddListener(() => PopUpManager.Instance.CreateHowToPlayPopUp());
        readyButton.onClick.AddListener(() => Player.Instance.TogglePlayerIsReady());
        InitializeGuildButtons();

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

    private void InitializeGuildButtons()
    {
        fightersGuildButton.onClick.AddListener(() =>
        {
            Player.Instance.ServerSetGuildType(GuildType.FightersGuild);
            if (selectedButton != null) selectedButton.interactable = true;
            fightersGuildButton.interactable = false;
            selectedButton = fightersGuildButton;
            guildText.text = "Fighters Guild Selected";
            print(GuildType.FightersGuild);
        });
        magesGuildButton.onClick.AddListener(() =>
        {
            Player.Instance.ServerSetGuildType(GuildType.MagesGuild);
            if (selectedButton != null) selectedButton.interactable = true;
            magesGuildButton.interactable = false;
            selectedButton = magesGuildButton;
            guildText.text = "Mages Guild Selected";
            print(GuildType.MagesGuild);
        });
        thievesGuildButton.onClick.AddListener(() =>
        {
            Player.Instance.ServerSetGuildType(GuildType.ThievesGuild);
            if (selectedButton != null) selectedButton.interactable = true;
            thievesGuildButton.interactable = false;
            selectedButton = thievesGuildButton;
            guildText.text = "Thieves Guild Selected";
            print(GuildType.ThievesGuild);
        });
        merchantsGuildButton.onClick.AddListener(() =>
        {
            Player.Instance.ServerSetGuildType(GuildType.MerchantsGuild);
            if (selectedButton != null) selectedButton.interactable = true;
            merchantsGuildButton.interactable = false;
            selectedButton = merchantsGuildButton;
            guildText.text = "Merchants Guild Selected";
            print(GuildType.MerchantsGuild);
        });
        assassinsGuildButton.onClick.AddListener(() =>
        {
            Player.Instance.ServerSetGuildType(GuildType.AsassinsGuild);
            if (selectedButton != null) selectedButton.interactable = true;
            assassinsGuildButton.interactable = false;
            selectedButton = assassinsGuildButton;
            guildText.text = "Assassins Guild Selected";
            print(GuildType.AsassinsGuild);
        });

    }
}
