using FishNet.Object;
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
    [SerializeField] TMP_Text readyButtonText;

    [SerializeField] Button fightersGuildButton;
    [SerializeField] Button magesGuildButton;
    [SerializeField] Button thievesGuildButton;
    [SerializeField] Button merchantsGuildButton;
    [SerializeField] Button assassinsGuildButton;
    private Button selectedButton;

    [SerializeField] TMP_Text guildText;
    [SerializeField] GameObject[] playerEntries;

    [SerializeField] Sprite FightersGuildSprite;
    [SerializeField] Sprite DefaultGuildSprite;

    [field: SerializeField, TextArea(3, 6)] private string fightersGuildText;
    [field: SerializeField, TextArea(3, 6)] private string magesGuildText;
    [field: SerializeField, TextArea(3, 6)] private string thievesGuildText;
    [field: SerializeField, TextArea(3, 6)] private string merchantsGuildText;
    [field: SerializeField, TextArea(3, 6)] private string assassinsGuildText;

    private readonly Dictionary<GuildType, Sprite> SpriteMap = new();
    private readonly Dictionary<GuildType, string> DescriptionMap = new();

    public override void Initialize()
    {
        howToPlayButton.onClick.AddListener(() => PopUpManager.Instance.CreateHowToPlayPopUp());
        readyButton.onClick.AddListener(() => Player.Instance.TogglePlayerIsReady());
        InitializeGuildButtons();
        InitializeGuildMaps();

        //if (InstanceFinder.IsServer)
        if (Player.Instance.IsStartingPlayer.Value) // could just check is player 1
        {
            startButton.onClick.AddListener(() => GameManager.Instance.ServerStartGame());
        }
        else
        {
            startButton.gameObject.SetActive(false);
        }
        //ServerUpdateLobby();
        base.Initialize();
    }

    private void Update()
    {
        if (!IsInitialized) return;

        string playerText;

        for (int i = 0; i < GameManager.Instance.Players.Count; i++)
        {
            Player player = GameManager.Instance.Players[i];
            playerEntries[i].SetActive(true);
            playerText = $"Player {player.OwnerId + 1} - ";

            if (player.GuildType == GuildType.None) playerText += "Selecting Guild";
            else if (player.IsReady.Value) playerText += "Ready";
            else playerText += "Not Ready";
            

            playerEntries[i].GetComponentInChildren<TMP_Text>().text = playerText;
            playerEntries[i].GetComponentInChildren<Image>().sprite = SpriteMap[player.GuildType];
        }

        readyButton.interactable = Player.Instance.GuildType != GuildType.None;
        readyButtonText.color = Player.Instance.IsReady.Value ? Color.green : Color.red;
        startButton.interactable = GameManager.Instance.CanStartGame.Value;
    }
    private void SetGuildType(GuildType guildType, Button guildButton)
    {
        Player.Instance.ServerSetGuildType(guildType);
        if (selectedButton != null) selectedButton.interactable = true;
        guildButton.interactable = false;
        selectedButton = guildButton;
        guildText.text = DescriptionMap[guildType];
        print(guildType);
    }

    private void InitializeGuildButtons()
    {
        fightersGuildButton.onClick.AddListener(() => SetGuildType(GuildType.FightersGuild, fightersGuildButton));
        magesGuildButton.onClick.AddListener(() => SetGuildType(GuildType.MagesGuild, magesGuildButton));
        thievesGuildButton.onClick.AddListener(() => SetGuildType(GuildType.ThievesGuild, thievesGuildButton));
        merchantsGuildButton.onClick.AddListener(() => SetGuildType(GuildType.MerchantsGuild, merchantsGuildButton));
        assassinsGuildButton.onClick.AddListener(() => SetGuildType(GuildType.AsassinsGuild, assassinsGuildButton));
    }

    private void InitializeGuildMaps()
    {
        SpriteMap.Add(GuildType.FightersGuild, FightersGuildSprite);
        SpriteMap.Add(GuildType.MagesGuild, DefaultGuildSprite);
        SpriteMap.Add(GuildType.ThievesGuild, DefaultGuildSprite);
        SpriteMap.Add(GuildType.MerchantsGuild, DefaultGuildSprite);
        SpriteMap.Add(GuildType.AsassinsGuild, DefaultGuildSprite);
        SpriteMap.Add(GuildType.None, DefaultGuildSprite);

        DescriptionMap.Add(GuildType.FightersGuild, fightersGuildText);
        DescriptionMap.Add(GuildType.MagesGuild, magesGuildText);
        DescriptionMap.Add(GuildType.ThievesGuild, thievesGuildText);
        DescriptionMap.Add(GuildType.MerchantsGuild, merchantsGuildText);
        DescriptionMap.Add(GuildType.AsassinsGuild, assassinsGuildText);
        DescriptionMap.Add(GuildType.None, "No Guild Selected");
    }
}
