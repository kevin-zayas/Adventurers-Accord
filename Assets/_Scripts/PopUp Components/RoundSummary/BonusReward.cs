using FishNet.Object;
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

    public void SetBonusReward(string name, int gold, int reputation, int loot)
    {
        bonusName.text = name + " -";
        TMP_Text bonusTextField = bonusAmount1;
        Image bonusIcon = bonusIcon1;

        if (gold != 0)
        {
            bonusTextField.text = "+" + gold.ToString();
            SetBonusIcon("Gold", bonusIcon1);
            bonusTextField = bonusAmount2;          //this only works because reputation and loot are currently mutually exclusive
            bonusIcon = bonusIcon2;
        }
        if (reputation != 0)
        {
            bonusTextField.text = "+" + reputation.ToString();
            SetBonusIcon("Reputation", bonusIcon);
        }
        if (loot != 0)
        {
            bonusTextField.text = "+" + loot.ToString();
            SetBonusIcon("Loot", bonusIcon);
        }
        if (bonusAmount2.text == "")
        {
            bonusAmount2.gameObject.SetActive(false);
            bonusIcon2.gameObject.SetActive(false);
        }
    }

    private void SetBonusIcon(string type, Image icon)
    {
        icon.sprite = type switch
        {
            "Gold" => goldSprite,
            "Reputation" => reputationSprite,
            "Loot" => lootSprite,
            _ => null,
        };
    }
}

public class BonusRewardData
{
    public string Name;
    public int Gold;
    public int Reputation;
    public int Loot;
    public BonusRewardData() { }

    public BonusRewardData(string name, int gold, int reputation, int loot)
    {
        this.Name = name;
        this.Gold = gold;
        this.Reputation = reputation;
        this.Loot = loot;
        //this.Type1 = type1;
        //this.Type2 = type2;
    }

    public void UpdateRewardData(int gold, int reputation, int loot)
    {
        this.Gold += gold;
        this.Reputation += reputation;
        this.Loot += loot;
    }

}
