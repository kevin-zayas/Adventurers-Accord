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
    private int guildBonusPhysicalPower;
    private int guildBonusMagicalPower;

    [AllowMutableSyncTypeAttribute] public SyncVar<int> MaxAdventurerCount = new();
    [AllowMutableSyncTypeAttribute] public SyncVar<int> CurrentAdventurerCount = new();

    public readonly SyncVar<int> EffectiveTotalPower = new();
    [field: SerializeField] public bool IsPartyGreased { get; private set; }

    private readonly Dictionary<string, int> adventurerEffects = new();

    public readonly SyncVar<int> BardBonus = new();

    private bool EnchanterBuff;
    private bool TinkererBuff;
    private const int EnchanterEmpower = 2;
    private const int TinkererEmpower = 2;

    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private Image rewardIndicator;
    [SerializeField] private Sprite _goldReward;
    [SerializeField] private Sprite _silverReward;

    private void Start()
    {
        adventurerEffects.Add("Enchanter", 0);
        adventurerEffects.Add("Tinkerer", 0);
    }

    [Server]
    public void AssignQuestCard(QuestCard questCard)
    {
        QuestCard.Value = questCard;
        MaxAdventurerCount.Value = questCard.PartySizeLimit.Value;
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

            if ((IsPartyGreased || QuestCard.Value.DisableItems.Value) && card.HasItem.Value) card.DisableItem();

            PhysicalPower.Value += card.PhysicalPower.Value;
            MagicalPower.Value += card.MagicalPower.Value;

            if (!IsPartyGreased && card.HasItem.Value)
            {
                PhysicalPower.Value += card.Item.Value.PhysicalPower.Value;
                MagicalPower.Value += card.Item.Value.MagicalPower.Value;
            }
        }

        TotalPhysicalPower.Value = PhysicalPower.Value + SpellPhysicalPower.Value + guildBonusPhysicalPower;
        TotalMagicalPower.Value = MagicalPower.Value + SpellMagicalPower.Value + guildBonusMagicalPower;

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

    [Server]
    public void UpdateGuildBonusPower(int physicalPowerDelta, int magicalPowerDelta)
    {
        guildBonusPhysicalPower += physicalPowerDelta;
        guildBonusMagicalPower += magicalPowerDelta;
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

            if (spellCard.CardName.Value == "Grease") IsPartyGreased = true;
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
        SpellPhysicalPower.Value = 0;
        SpellMagicalPower.Value = 0;

        while (QuestDropZone.transform.childCount > 0)
        {
            Transform cardTransform = QuestDropZone.transform.GetChild(0);
            AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();

            if (card.CardName.Value == "Wolf")
            {
                HandleWolfSummon(true);
                continue;
            }
            DiscardPile.Instance.DiscardCard(card, Player.Value);
        }

        PhysicalPower.Value = 0;
        MagicalPower.Value = 0;
        guildBonusPhysicalPower = 0;
        guildBonusMagicalPower = 0;
        EffectiveTotalPower.Value = 0;
        CurrentAdventurerCount.Value = 0;
        
        ClearSpellEffects();
        ObserversUpdateLaneTotalPower(PhysicalPower.Value, MagicalPower.Value);
    }

    [Server]
    private void ClearSpellEffects()
    {
        IsPartyGreased = false;
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
        if (card.CardName.Value != "Wolf") CurrentAdventurerCount.Value++;

        if (QuestCard.Value.Drain.Value)
        {
            print("Applying drain");
            card.ChangePhysicalPower(-QuestCard.Value.PhysicalDrain.Value);
            card.ChangeMagicalPower(-QuestCard.Value.MagicalDrain.Value);
        }

        if (EnchanterBuff)
        {
            card.ChangePhysicalPower(EnchanterEmpower*adventurerEffects["Enchanter"]);
            card.ChangeMagicalPower(EnchanterEmpower*adventurerEffects["Enchanter"]);
        }

        if (TinkererBuff && card.HasItem.Value)
        {
            card.Item.Value.ChangePhysicalPower(adventurerEffects["Tinkerer"]);
            card.Item.Value.ChangeMagicalPower(adventurerEffects["Tinkerer"]);

            if (card.Item.Value.equippedOnBattlemage) card.Item.Value.ApplyBalancedArsenal();
        }


        if (adventurerEffects.ContainsKey(card.CardName.Value)) adventurerEffects[card.CardName.Value]++;

        switch (card.CardName.Value)
        {
            case "Bard":
                BardBonus.Value++;
                break;
            case "Assassin":
            case "Cleric":
            case "Rogue":
                QuestLocation.Value.CardsToResolvePerLane[Player.Value.PlayerID.Value].Add(card);
                break;
            case "Enchanter":
                EnchanterBuff = adventurerEffects["Enchanter"] > 0;
                UpdateEnchanterBuff(EnchanterEmpower, card);
                break;
            case "Tinkerer":
                TinkererBuff = adventurerEffects["Tinkerer"] > 0;
                UpdateTinkererBuff(TinkererEmpower);     
                break;
            case "Ranger":
                HandleWolfSummon(false, card.ControllingPlayer.Value);
                break;
            
        }

        UpdateQuestLanePower();
    }

    [Server]
    public void RemoveAdventurerFromQuestLane(AdventurerCard card)
    {
        if (card.CardName.Value != "Wolf") CurrentAdventurerCount.Value--;
        if (adventurerEffects.ContainsKey(card.CardName.Value)) adventurerEffects[card.CardName.Value]--;

        switch (card.CardName.Value)
        {
            case "Bard":
                BardBonus.Value--;
                break;
            case "Assassin":
            case "Cleric":
            case "Rogue":
            
                QuestLocation.Value.CardsToResolvePerLane[Player.Value.PlayerID.Value].Remove(card);
                break;
            case "Enchanter":
                EnchanterBuff = adventurerEffects["Enchanter"] > 0;
                UpdateEnchanterBuff(-EnchanterEmpower);
                break;
            case "Tinkerer":
                TinkererBuff = adventurerEffects["Tinkerer"] > 0;
                UpdateTinkererBuff(-TinkererEmpower);
                break;
            case "Ranger":
                HandleWolfSummon(true);
                break;

        }
        if (QuestDropZone.transform.childCount == 0) ObserversUpdateRewardIndicator("blank");
        
        UpdateQuestLanePower();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUpdateDrainEffects(AdventurerCard card)
    {
        if (!QuestCard.Value.Drain.Value) return;
        if (!card.IsBlessed.Value)
        {
            throw new System.Exception($"Divine Blessing not applied to {card.CardName.Value}");
        }

        card.ChangePhysicalPower(QuestCard.Value.PhysicalDrain.Value);      //reverse drain
        card.ChangeMagicalPower(QuestCard.Value.MagicalDrain.Value);

        return;
    }

    [Server]
    private void UpdateEnchanterBuff(int buffDelta, AdventurerCard enchanterCard = null) 
    {
        foreach (Transform cardTransform in QuestDropZone.transform)
        {
            AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();
            if (enchanterCard == card) continue; //prevent the enchanter from buffing itself

            card.ChangePhysicalPower(buffDelta);
            card.ChangeMagicalPower(buffDelta);
        } 
    }

    [Server]
    private void UpdateTinkererBuff(int powerDelta)
    {
        foreach (Transform cardTransform in QuestDropZone.transform)
        {
            AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();
            if (!card.HasItem.Value) continue;

            card.Item.Value.ChangePhysicalPower(powerDelta);
            card.Item.Value.ChangeMagicalPower(powerDelta);

            if (card.Item.Value.equippedOnBattlemage) card.Item.Value.ApplyBalancedArsenal();
        }
    }

    [Server]
    private void HandleWolfSummon(bool despawn, Player controllingPlayer = null)
    {
        if (despawn)
        {
            foreach (Transform cardTransform in QuestDropZone.transform)
            {
                AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();
                if (card.CardName.Value == "Wolf")
                {
                    //card.transform.SetParent(null);
                    card.SetCardParent(null, false);
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
        wolfCard.SetCardOwner(controllingPlayer);
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
        if (color == "gold") rewardIndicator.sprite = _goldReward;
        if (color == "silver") rewardIndicator.sprite = _silverReward;
    }
}
