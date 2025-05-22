using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CardDatabase;

public class PlayerScore : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text reputationText;
    [SerializeField] private Button rosterButton;
    [SerializeField] private Image guildIcon;

    public void InitializeScore(int playerID, int gold, int reputation, GuildType guildType)
    {
        playerNameText.text = $"Player {playerID + 1}";
        goldText.text = gold.ToString();
        reputationText.text = reputation.ToString();
        guildIcon.sprite = CardDatabase.Instance.GetGuildSprite(guildType);
    }

    public void UpdateGold(int gold)
    {
        goldText.text = gold.ToString();
    }

    public void UpdateReputation(int reputation)
    {
        reputationText.text = reputation.ToString();
    }

    public Button GetRosterButton()
    {
        return rosterButton;
    }
}
