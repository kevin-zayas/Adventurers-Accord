using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GuildRecapPopUp : PopUp
{
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject recapGroup;
    [SerializeField] private GameObject guildRecap;
    [SerializeField] private Image guildIcon;

    protected override void Start()
    {
        base.Start();
        closeButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });
    }
    public void InitializeGuildRecapPopUp(Player player)
    {
        var sortedKeys = new List<string>(player.GuildRecapTracker.Keys);
        sortedKeys.Sort();
        foreach (string key in sortedKeys)
        {
            int value = player.GuildRecapTracker[key];
            GameObject newGuildRecap = Instantiate(guildRecap, recapGroup.transform);
            newGuildRecap.GetComponent<TMP_Text>().text = $"{key} : {value}";
            newGuildRecap.SetActive(true);
        }

        guildIcon.sprite = CardDatabase.Instance.GetGuildSprite(player.GuildType);
        guildIcon.gameObject.SetActive(true);
    }
}
