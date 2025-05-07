using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CardDatabase;

public class GuildStatus : MonoBehaviour
{
    [SerializeField] private Image guildIcon;
    [SerializeField] private TMP_Text goldAmount;
    [SerializeField] private TMP_Text reputationAmount;
    [SerializeField] private GameObject ownerBorder;
    [SerializeField] private GameObject turnIndicator;

    public void InitializeGuildStatus(Player player)
    {
        SetGuildSprite(player.GuildType);
        SetGoldAmount(player.Gold.Value);
        SetReputationAmount(player.Reputation.Value);
        gameObject.SetActive(true);

        if (player.IsOwner)
        {
            ownerBorder.SetActive(true);
        }
        else
        {
            ownerBorder.SetActive(false);
        }
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

    public void SetTurnIndicator(bool isActive)
    {
        turnIndicator.SetActive(isActive);
    }
}
