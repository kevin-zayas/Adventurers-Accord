using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestLane : NetworkBehaviour
{
    //allowmutablesynctypeattribute?
    [AllowMutableSyncTypeAttribute] public SyncVar<QuestLocation> QuestLocation = new();

    public readonly SyncVar<QuestCard> QuestCard = new();
    public readonly SyncVar<Player> Player = new();
    [field: SerializeField] public GameObject QuestDropZone { get; private set; }
    [field: SerializeField] public GameObject SpellDropZone { get; private set; }

    public readonly SyncVar<int> PhysicalPower = new();
    public readonly SyncVar<int> MagicalPower = new();
    public readonly SyncVar<int> SpellPhysicalPower = new();
    public readonly SyncVar<int> SpellMagicalPower = new();
    public readonly SyncVar<int> TotalPhysicalPower = new();
    public readonly SyncVar<int> TotalMagicalPower = new();
    [AllowMutableSyncTypeAttribute] public SyncVar<int> MaxAdventurerCount = new();
    [AllowMutableSyncTypeAttribute] public SyncVar<int> CurrentAdventurerCount = new();

    public readonly SyncVar<int> EffectiveTotalPower = new();
    [field: SerializeField] public bool IsGreased { get; private set; }

    private readonly Dictionary<string, int> adventurerEffects = new();

    public readonly SyncVar<int> BardBonus = new();
    public readonly SyncVar<bool> ClericProtection = new();

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
        QuestCard.Value = questCard;
        MaxAdventurerCount.Value = questCard.MaxAdventurerCount.Value;
    }

    [Server]
    public void UpdateQuestLanePower()
    {
        PhysicalPower.Value = 0;
        MagicalPower.Value = 0;
        TotalPhysicalPower.Value = 0;
        TotalMagicalPower.Value = 0;
        EffectiveTotalPower.Value = 0;

        for (int i = 0; i < QuestDropZone.transform.childCount; i++)
        {
            Transform cardTransform = QuestDropZone.transform.GetChild(i);
            AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();

            if (IsGreased && card.HasItem.Value) card.DisableItem("Greased");
            if (QuestCard.Value.DisableItems.Value && card.HasItem.Value) card.DisableItem("Nullified");

            PhysicalPower.Value += card.PhysicalPower.Value;
            MagicalPower.Value += card.MagicalPower.Value;

            if (!IsGreased && card.HasItem.Value)
            {
                PhysicalPower.Value += card.Item.Value.PhysicalPower.Value;
                MagicalPower.Value += card.Item.Value.MagicalPower.Value;
            }
        }

        TotalPhysicalPower.Value = PhysicalPower.Value + SpellPhysicalPower.Value;
        TotalMagicalPower.Value = MagicalPower.Value + SpellMagicalPower.Value;

        if (QuestCard.Value.PhysicalPower.Value > 0) EffectiveTotalPower.Value += TotalPhysicalPower.Value;
        if (QuestCard.Value.MagicalPower.Value > 0) EffectiveTotalPower.Value += TotalMagicalPower.Value;

        ObserversUpdateLaneTotalPower(TotalPhysicalPower.Value, TotalMagicalPower.Value);
        QuestLocation.Value.UpdateTotalPower();

        QuestLocation.Value.CalculateQuestContributions(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUpdateQuestLanePower()
    {
        UpdateQuestLanePower();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUpdateSpellEffects()
    {
        SpellPhysicalPower.Value = 0;
        SpellMagicalPower.Value = 0;

        for (int i = 0; i < SpellDropZone.transform.childCount; i++)
        {
            Transform spellCardTransform = SpellDropZone.transform.GetChild(i);
            SpellCard spellCard = spellCardTransform.GetComponent<SpellCard>();

            SpellPhysicalPower.Value += spellCard.PhysicalPower.Value;
            SpellMagicalPower.Value += spellCard.MagicalPower.Value;

            if (spellCard.CardName.Value == "Grease") IsGreased = true;
        }

        UpdateQuestLanePower();
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversUpdateLaneTotalPower(int physicalPower, int magicalPower)
    {
        physicalPowerText.text = physicalPower.ToString();
        magicalPowerText.text = magicalPower.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetQuestLanePlayer(Player player)
    {
        Player.Value = player;
    }

    [Server]
    public void ResetQuestLane()
    {
        PhysicalPower.Value = 0;
        MagicalPower.Value = 0;
        SpellPhysicalPower.Value = 0;
        SpellMagicalPower.Value = 0;
        EffectiveTotalPower.Value = 0;
        CurrentAdventurerCount.Value = 0;

        while (QuestDropZone.transform.childCount > 0)
        {
            Transform cardTransform = QuestDropZone.transform.GetChild(0);
            AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();

            if (card.CardName.Value == "Wolf")
            {
                SummonWolfCompanion(true);
                continue;
            }
            DiscardPile.Instance.DiscardCard(card, Player.Value);
        }
        ClearSpellEffects();
        ObserversUpdateLaneTotalPower(PhysicalPower.Value, MagicalPower.Value);
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
        CurrentAdventurerCount.Value++;

        if (QuestCard.Value.Drain.Value && !ClericProtection.Value)
        {
            print("Applying drain");
            card.ChangePhysicalPower(-QuestCard.Value.PhysicalDrain.Value);
            card.ChangeMagicalPower(-QuestCard.Value.MagicalDrain.Value);
        }

        if (EnchanterBuff)
        {
            card.ChangePhysicalPower(adventurerEffects["Enchanter"]);
            card.ChangeMagicalPower(adventurerEffects["Enchanter"]);
        }

        if (TinkererBuff && card.HasItem.Value)
        {
            card.Item.Value.ChangePhysicalPower(adventurerEffects["Tinkerer"]);
            card.Item.Value.ChangeMagicalPower(adventurerEffects["Tinkerer"]);
        }


        if (adventurerEffects.ContainsKey(card.CardName.Value)) adventurerEffects[card.CardName.Value]++;

        switch (card.CardName.Value)
        {
            case "Bard":
                BardBonus.Value++;
                break;
            case "Cleric":
                ClericProtection.Value = true;
                if (adventurerEffects["Cleric"] == 1) UpdateDrainEffects();
                break;
            case "Rogue":
            case "Assassin":
                QuestLocation.Value.CardsToResolvePerLane[Player.Value.PlayerID.Value].Add(card);
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
        CurrentAdventurerCount.Value--;
        if (adventurerEffects.ContainsKey(card.CardName.Value)) adventurerEffects[card.CardName.Value]--;

        switch (card.CardName.Value)
        {
            case "Bard":
                BardBonus.Value--;
                break;
            case "Cleric":
                if (adventurerEffects["Cleric"] == 0)
                {
                    ClericProtection.Value = false;
                    UpdateDrainEffects();
                }
                break;
            case "Rogue":
            case "Assassin":
                QuestLocation.Value.CardsToResolvePerLane[Player.Value.PlayerID.Value].Remove(card);
                break;
            case "Enchanter":
                if (adventurerEffects["Enchanter"] == 0) EnchanterBuff = false;
                UpdateEnchanterBuff(-1);
                break;
            case "Tinkerer":
                if (adventurerEffects["Tinkerer"] == 0) TinkererBuff = false;
                UpdateTinkererBuff(-1);
                break;
            case "Ranger":
                SummonWolfCompanion(true);
                break;

        }
        if (QuestDropZone.transform.childCount == 0) ObserversUpdateRewardIndicator("blank");
        
        UpdateQuestLanePower();
    }

    [Server]
    private void UpdateDrainEffects()
    {
        if (!QuestCard.Value.Drain.Value) return;

        foreach (Transform cardTransform in QuestDropZone.transform)
        {
            AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();
            //if (card.Name == "Cleric") continue;          // only need this if applying drain after updating ClericProtection bool in AddAdventurerToQuestLane

            if (ClericProtection.Value)
            {
                card.ChangePhysicalPower(QuestCard.Value.PhysicalDrain.Value);      //reverse drain
                card.ChangeMagicalPower(QuestCard.Value.MagicalDrain.Value);    
            }
            else
            {
                print("Applying Drain");
                card.ChangePhysicalPower(-QuestCard.Value.PhysicalDrain.Value);
                card.ChangeMagicalPower(-QuestCard.Value.MagicalDrain.Value);
            }
        }
    }

    [Server]
    private void UpdateEnchanterBuff(int buffDelta) 
    {
        foreach (Transform cardTransform in QuestDropZone.transform)
        {
            AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();         // Enchanter is 0,0 so wont be buffed anyway, but we may want to change in the future
            if (card.CardName.Value == "Enchanter") continue;                 // If we change, will need to figure out how to differentiate Enchanter buffs so multiple enchanters can buff each other 

            card.ChangePhysicalPower(buffDelta);
            card.ChangeMagicalPower(buffDelta);
        } 
    }

    [Server]
    private void UpdateTinkererBuff(int buffDelta)
    {
        foreach (Transform cardTransform in QuestDropZone.transform)
        {
            AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();
            if (!card.HasItem.Value) continue;
            print($"{card.CardName} changing item power by {buffDelta}");
            card.Item.Value.ChangePhysicalPower(buffDelta);
            card.Item.Value.ChangeMagicalPower(buffDelta);
        }
    }

    [Server]
    private void SummonWolfCompanion(bool despawn)
    {
        if (despawn)
        {
            foreach (Transform cardTransform in QuestDropZone.transform)
            {
                AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();
                if (card.CardName.Value == "Wolf")
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
        //Destroy(wolfCard.GetComponent<AdventurerDragDrop>());

        Spawn(wolfCard.gameObject);
        wolfCard.LoadCardData(wolfCardData);
        wolfCard.SetCardParent(Player.Value.controlledHand.Value.transform,false);
        wolfCard.SetCardParent(QuestDropZone.transform, false);
    }

    [ObserversRpc]
    public void ObserversUpdateRewardIndicator(string color)
    {
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
