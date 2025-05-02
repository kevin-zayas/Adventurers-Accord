using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CardDatabase;

public class GuildStatus : MonoBehaviour
{
    [SerializeField] private Image guildIcon;
    [SerializeField] private TMP_Text goldAmount;
    [SerializeField] private TMP_Text reputationAmount;

    public void InitializeGuildStatus(Player player)
    {
        SetGuildSprite(player.GuildType);
        SetGoldAmount(player.Gold.Value);
        SetReputationAmount(player.Reputation.Value);
    }

    public void SetGuildSprite(GuildType guildType)
    {
        guildIcon.sprite = CardDatabase.Instance.GetGuildSprite(guildType);
    }

    public void SetGoldAmount(int amount)
    {
        goldAmount.text = amount.ToString();
    }

    public void SetReputationAmount(int amount)
    {
        reputationAmount.text = amount.ToString();
    }
}
