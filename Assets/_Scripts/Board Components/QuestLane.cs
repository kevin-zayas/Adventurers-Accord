using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QuestLane : NetworkBehaviour
{
    [field: SerializeField, SyncVar]
    public QuestLocation QuestLocation { get; private set; }

    [field: SerializeField, SyncVar]
    public QuestCard QuestCard { get; private set; }

    [field: SerializeField]
    public Player Player { get; private set; }

    [field: SerializeField]
    public GameObject DropZone { get; private set; }

    [field: SerializeField]
    public GameObject SpellDropZone { get; private set; }

    [field: SerializeField, SyncVar]
    public int PhysicalPower { get; private set; }

    [field: SerializeField, SyncVar]
    public int MagicalPower { get; private set; }

    [field: SerializeField, SyncVar]
    public int SpellPhysicalPower { get; private set; }

    [field: SerializeField, SyncVar]
    public int SpellMagicalPower { get; private set; }

    [field: SerializeField, SyncVar]
    public int EffectiveTotalPower { get; private set; }

    [field: SerializeField]
    public bool IsGreased { get; private set; }

    private readonly Dictionary<string, int> adventurerEffects = new();

    [field: SerializeField, SyncVar]
    public int BardBonus { get; private set; }

    [field: SerializeField, SyncVar]
    public bool ClericProtection { get; private set; }

    private bool EnchanterBuff;
    private bool TinkererBuff;

    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private Image rewardIndicator;

    private void Start()
    {
        adventurerEffects.Add("Cleric", 0);
        adventurerEffects.Add("Enchanter", 0);
        adventurerEffects.Add("Tinkerer", 0);
    }

    [Server]
    public void AssignQuestCard(QuestCard questCard)
    {
        QuestCard = questCard;
    }

    [Server]
    public void UpdateQuestLanePower()
    {
        PhysicalPower = 0;
        MagicalPower = 0;
        EffectiveTotalPower = 0;

        for (int i = 0; i < DropZone.transform.childCount; i++)
        {
            Transform cardTransform = DropZone.transform.GetChild(i);
            AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();

            if (IsGreased && card.HasItem) card.DisableItem("Greased");
            if (QuestCard.DisableItems && card.HasItem) card.DisableItem("Nullified");

            PhysicalPower += card.PhysicalPower;
            MagicalPower += card.MagicalPower;

            if (!IsGreased && card.HasItem)
            {
                PhysicalPower += card.Item.PhysicalPower;
                MagicalPower += card.Item.MagicalPower;
            }
        }

        if (QuestCard.PhysicalPower > 0) EffectiveTotalPower += PhysicalPower + SpellPhysicalPower;
        if (QuestCard.MagicalPower > 0) EffectiveTotalPower += MagicalPower + SpellMagicalPower;

        ObserversUpdatePower(PhysicalPower + SpellPhysicalPower, MagicalPower + SpellMagicalPower);
        QuestLocation.UpdateTotalPower();

        QuestLocation.CalculateQuestContributions(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUpdateQuestLanePower()
    {
        UpdateQuestLanePower();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUpdateSpellEffects()
    {
        SpellPhysicalPower = 0;
        SpellMagicalPower = 0;

        for (int i = 0; i < SpellDropZone.transform.childCount; i++)
        {
            Transform spellCardTransform = SpellDropZone.transform.GetChild(i);
            SpellCard spellCard = spellCardTransform.GetComponent<SpellCard>();

            SpellPhysicalPower += spellCard.PhysicalPower;
            SpellMagicalPower += spellCard.MagicalPower;

            if (spellCard.IsGreaseSpell) IsGreased = true;
        }

        UpdateQuestLanePower();
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversUpdatePower(int physicalPower, int magicalPower)
    {
        physicalPowerText.text = physicalPower.ToString();
        magicalPowerText.text = magicalPower.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetQuestLanePlayer(Player player)
    {
        Player = player;
    }

    [Server]
    public void ResetQuestLane()
    {
        PhysicalPower = 0;
        MagicalPower = 0;
        SpellPhysicalPower = 0;
        SpellMagicalPower = 0;
        EffectiveTotalPower = 0;

        while (DropZone.transform.childCount > 0)
        {
            Transform cardTransform = DropZone.transform.GetChild(0);
            AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();

            if (card.Name == "Wolf")
            {
                SummonWolfCompanion(true);
                continue;
            }
            card.SetCardParent(card.ControllingPlayerHand.transform, false);
        }
        ClearSpellEffects();
        ObserversUpdatePower(PhysicalPower, MagicalPower);
    }

    [Server]
    private void ClearSpellEffects()
    {
        IsGreased = false;
        for (int i = 0; i < SpellDropZone.transform.childCount; i++)
        {
            Transform spellCardTransform = SpellDropZone.transform.GetChild(i);
            SpellCard spellCard = spellCardTransform.GetComponent<SpellCard>();

            spellCard.Despawn();
        }
    }

    [Server]
    public void AddAdventurerToQuestLane(AdventurerCard card)
    {
        if (QuestCard.Drain && !ClericProtection)
        {
            print("Applying drain");
            card.ChangePhysicalPower(-QuestCard.PhysicalDrain);
            card.ChangeMagicalPower(-QuestCard.MagicalDrain);
        }

        if (EnchanterBuff)
        {
            card.ChangePhysicalPower(adventurerEffects["Enchanter"]);
            card.ChangeMagicalPower(adventurerEffects["Enchanter"]);
        }

        if (TinkererBuff && card.HasItem)
        {
            card.Item.ServerChangePhysicalPower(adventurerEffects["Tinkerer"]);
            card.Item.ServerChangeMagicalPower(adventurerEffects["Tinkerer"]);
        }


        if (adventurerEffects.ContainsKey(card.Name)) adventurerEffects[card.Name]++;

        switch (card.Name)
        {
            case "Bard":
                BardBonus++;
                break;
            case "Cleric":
                ClericProtection = true;
                if (adventurerEffects["Cleric"] == 1) UpdateDrainEffects();
                break;
            case "Rogue":
            case "Assassin":
                QuestLocation.CardsToResolvePerLane[Player.PlayerID].Add(card);
                break;
            case "Enchanter":
                if (adventurerEffects["Enchanter"] == 1) EnchanterBuff = true;
                UpdateEnchanterBuff(1);     //check for card ID here so enchanter cant buff itself if changing Enchanter stats
                break;
            case "Tinkerer":
                if (adventurerEffects["Tinkerer"] == 1) TinkererBuff = true;
                UpdateTinkererBuff(1);     
                break;
            case "Ranger":
                SummonWolfCompanion(false);
                break;
            
        }

        UpdateQuestLanePower();
    }

    [Server]
    public void RemoveAdventurerFromQuestLane(AdventurerCard card)
    {
        if (adventurerEffects.ContainsKey(card.Name)) adventurerEffects[card.Name]--;

        switch (card.Name)
        {
            case "Bard":
                BardBonus--;
                break;
            case "Cleric":
                if (adventurerEffects["Cleric"] == 0)
                {
                    ClericProtection = false;
                    UpdateDrainEffects();
                }
                break;
            case "Rogue":
            case "Assassin":
                QuestLocation.CardsToResolvePerLane[Player.PlayerID].Remove(card);
                break;
            case "Enchanter":
                if (adventurerEffects["Enchanter"] == 0) EnchanterBuff = false;
                UpdateEnchanterBuff(-1);
                break;
            case "Tinkerer":
                if (adventurerEffects["Tinkerer"] == 0) TinkererBuff = true;
                UpdateTinkererBuff(-1);
                break;
            case "Ranger":
                SummonWolfCompanion(true);
                break;

        }
        print(DropZone.transform.childCount);
        if (DropZone.transform.childCount == 0) ObserversUpdateRewardIndicator("blank");
        
        UpdateQuestLanePower();
    }

    [Server]
    private void UpdateDrainEffects()
    {
        if (!QuestCard.Drain) return;

        foreach (Transform cardTransform in DropZone.transform)
        {
            AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();
            //if (card.Name == "Cleric") continue;          // only need this if applying drain after updating ClericProtection bool in AddAdventurerToQuestLane

            if (ClericProtection)
            {
                card.ChangePhysicalPower(QuestCard.PhysicalDrain);      //reverse drain
                card.ChangeMagicalPower(QuestCard.MagicalDrain);    
            }
            else
            {
                print("Applying Drain");
                card.ChangePhysicalPower(-QuestCard.PhysicalDrain);
                card.ChangeMagicalPower(-QuestCard.MagicalDrain);
            }
        }
    }

    [Server]
    private void UpdateEnchanterBuff(int buffDelta) 
    {
        foreach (Transform cardTransform in DropZone.transform)
        {
            AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();         // Enchanter is 0,0 so wont be buffed anyway, but we may want to change in the future
            if (card.Name == "Enchanter") continue;                 // If we change, will need to figure out how to differentiate Enchanter buffs so multiple enchanters can buff each other 

            card.ChangePhysicalPower(buffDelta);
            card.ChangeMagicalPower(buffDelta);
        } 
    }

    [Server]
    private void UpdateTinkererBuff(int buffDelta)
    {
        foreach (Transform cardTransform in DropZone.transform)
        {
            AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();
            if (!card.HasItem) continue;
            print($"{card.Name} changing item power by {buffDelta}");
            card.Item.ServerChangePhysicalPower(buffDelta);
            card.Item.ServerChangeMagicalPower(buffDelta);
        }
    }

    [Server]
    private void SummonWolfCompanion(bool despawn)
    {
        if (despawn)
        {
            foreach (Transform cardTransform in DropZone.transform)
            {
                AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();
                if (card.Name == "Wolf")
                {
                    card.transform.SetParent(null);
                    card.Despawn();
                    break;
                }
            }
            return;
        }

        CardData wolfCardData = CardDatabase.Instance.wolfCardData;
        AdventurerCard wolfCard = Instantiate(CardDatabase.Instance.adventurerCardPrefab, Vector2.zero, Quaternion.identity);

        Spawn(wolfCard.gameObject);
        wolfCard.LoadCardData(wolfCardData);
        wolfCard.SetCardParent(Player.controlledHand.transform,false);
        wolfCard.SetCardParent(DropZone.transform, false);
    }

    [ObserversRpc]
    public void ObserversUpdateRewardIndicator(string color)
    {
        print(color);
        if (color == "blank")
        {
            rewardIndicator.enabled = false;
            return;
        }
        rewardIndicator.enabled = true;
        if (color == "gold") rewardIndicator.color = Color.yellow;
        if (color == "silver") rewardIndicator.color = Color.gray;
    }
}
