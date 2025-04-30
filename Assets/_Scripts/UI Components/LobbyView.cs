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

    private Button selectedButton;
    [SerializeField] GameObject[] playerEntries;

    [SerializeField] private List<Button> guildButtons = new();
    [SerializeField] private List<GameObject> guildPages = new();
    [SerializeField] private List<GuildType> guildTypes = new();

    [SerializeField] private GameObject currentGuildPage;
    [SerializeField] private GameObject content;

    public override void Initialize()
    {
        howToPlayButton.onClick.AddListener(() => PopUpManager.Instance.CreateHowToPlayPopUp());
        readyButton.onClick.AddListener(() => Player.Instance.TogglePlayerIsReady());
        InitializeGuildButtons();

        if (Player.Instance.IsStartingPlayer.Value)
        {
            startButton.onClick.AddListener(() => GameManager.Instance.ServerStartGame());
        }
        else
        {
            startButton.gameObject.SetActive(false);
        }
        base.Initialize();
    }

    private void InitializeGuildButtons()
    {
        foreach (Button button in guildButtons)
        {
            button.onClick.AddListener(() =>
            {
                SetGuildType(button);
            });
        }
    }
    private void ChangeGuildDescription(int newPageIndex)
    {
        Destroy(currentGuildPage);

        currentGuildPage = Instantiate(guildPages[newPageIndex], new Vector2(0f,-50f), Quaternion.identity);
        currentGuildPage.transform.localScale = new Vector2(.9f, .9f);
        currentGuildPage.transform.SetParent(content.transform, false);
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

            if (player.GuildType == GuildType.None) continue;

            playerEntries[i].GetComponentInChildren<Image>().sprite = CardDatabase.Instance.GetGuildSprite(player.GuildType);
            playerEntries[i].GetComponentInChildren<Image>().enabled = true;

        }
        readyButton.interactable = Player.Instance.GuildType != GuildType.None;
        readyButtonText.color = Player.Instance.IsReady.Value ? Color.green : Color.red;
        startButton.interactable = GameManager.Instance.CanStartGame.Value;
    }
    private void SetGuildType(Button guildButton)
    {
        int guildIndex = guildButtons.IndexOf(guildButton);
        Player.Instance.ServerSetGuildType(guildTypes[guildIndex]);
        if (selectedButton != null) selectedButton.interactable = true;
        guildButton.interactable = false;
        selectedButton = guildButton;
        ChangeGuildDescription(guildIndex);

        Player.Instance.TogglePlayerIsReady(false);
    }
}
