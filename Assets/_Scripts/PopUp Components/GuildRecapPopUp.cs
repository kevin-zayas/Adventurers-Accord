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
        foreach (KeyValuePair<string, int> kvp in player.GuildRecapTracker)
        {
            Debug.Log($"{kvp.Key} - {kvp.Value}");
            GameObject newGuildRecap = Instantiate(guildRecap, recapGroup.transform);
            newGuildRecap.GetComponent<TMP_Text>().text = $"{kvp.Key} : {kvp.Value}";
            newGuildRecap.SetActive(true);
        }

        guildIcon.sprite = CardDatabase.Instance.GetGuildSprite(player.GuildType);
        guildIcon.gameObject.SetActive(true);
    }
}
