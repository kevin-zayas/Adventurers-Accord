using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BonusReward : MonoBehaviour
{
    [SerializeField] private TMP_Text bonusName;
    [SerializeField] private TMP_Text bonusAmount1;
    [SerializeField] private TMP_Text bonusAmount2;
    [SerializeField] private Image bonusIcon1;
    [SerializeField] private Image bonusIcon2;

    [SerializeField] private Sprite goldSprite;
    [SerializeField] private Sprite reputationSprite;
    [SerializeField] private Sprite lootSprite;

    public void SetBonusReward(string name, int amount1, string type1, int amount2=0, string type2="")
    {
        bonusName.text = name;
        bonusAmount1.text = amount1.ToString();
        SetBonusIcon(type1, bonusIcon1);

        if (amount2 > 0 && !string.IsNullOrEmpty(type2))
        {
            bonusAmount2.text = amount2.ToString();
            SetBonusIcon(type2, bonusIcon2);
        }
        else
        {
            bonusAmount2.gameObject.SetActive(false);
            bonusIcon2.gameObject.SetActive(false);
        }
    }

    private void SetBonusIcon(string type, Image icon)
    {
        switch (type)
        {
            case "Gold":
                icon.sprite = goldSprite;
                break;
            case "Reputation":
                icon.sprite = reputationSprite;
                break;
            case "Loot":
                icon.sprite = lootSprite;
                break;
            default:
                icon.sprite = null;
                break;
        }
    }
}

public class BonusRewardData
{
    public string Name;
    public int Amount1;
    public string Type1;
    public int Amount2;
    public string Type2;

    public BonusRewardData(string name, int amount1, string type1, int amount2 = 0, string type2 = "")
    {
        this.Name = name;
        this.Amount1 = amount1;
        this.Type1 = type1;
        this.Amount2 = amount2;
        this.Type2 = type2;
    }
}
