using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class QuestLane : NetworkBehaviour
{
    [field: SerializeField]
    [field: SyncVar]
    public QuestLocation QuestLocation { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public Player Player { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public GameObject DropZone { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public GameObject SpellDropZone { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int PhysicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int MagicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int SpellPhysicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int SpellMagicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int EffectiveTotalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool IsGreased { get; private set; }

    private readonly Dictionary<string, int> adventurerEffects = new();

    [field: SerializeField]
    [field: SyncVar]
    public int BardBonus { get; private set; }

    private bool ClericProtection;
    private bool EnchanterBuff;
    private bool TinkererBuff;

    //[field: SerializeField]
    //[field: SyncVar]
    //public QuestCard QuestCard { get; private set; }

    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;

    private void Start()
    {
        adventurerEffects.Add("Cleric", 0);
        adventurerEffects.Add("Enchanter", 0);
        adventurerEffects.Add("Tinkerer", 0);
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
            Card card = cardTransform.GetComponent<Card>();

            if (IsGreased && card.HasItem) card.DisableItem("Greased");
            if (QuestLocation.QuestCard.DisableItems && card.HasItem) card.DisableItem("Nullified");

            PhysicalPower += card.PhysicalPower;
            MagicalPower += card.MagicalPower;

            if (!IsGreased && card.HasItem)
            {
                PhysicalPower += card.Item.PhysicalPower;
                MagicalPower += card.Item.MagicalPower;
            }
        }

        if (QuestLocation.QuestCard.MagicalPower > 0) EffectiveTotalPower += MagicalPower + SpellMagicalPower;
        if (QuestLocation.QuestCard.PhysicalPower > 0) EffectiveTotalPower += PhysicalPower + SpellPhysicalPower;

        ObserversUpdatePower(PhysicalPower + SpellPhysicalPower, MagicalPower + SpellMagicalPower);
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
        EffectiveTotalPower = 0;

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
            Card card = cardTransform.GetComponent<Card>();

            if (card.Name == "Wolf Companion")
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
    public void AddAdventurerToQuestLane(Card card)
    {
        if (QuestLocation.QuestCard.Drain && !ClericProtection)
        {
            print("Applying drain");
            card.ChangePhysicalPower(-QuestLocation.QuestCard.PhysicalDrain);
            card.ChangeMagicalPower(-QuestLocation.QuestCard.MagicalDrain);
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
    public void RemoveAdventurerFromQuestLane(Card card)
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

        UpdateQuestLanePower();
    }

    [Server]
    private void UpdateDrainEffects()
    {
        if (!QuestLocation.QuestCard.Drain) return;

        foreach (Transform cardTransform in DropZone.transform)
        {
            Card card = cardTransform.GetComponent<Card>();
            //if (card.Name == "Cleric") continue;          // only need this if applying drain after updating ClericProtection bool in AddAdventurerToQuestLane

            if (ClericProtection)
            {
                card.ChangePhysicalPower(QuestLocation.QuestCard.PhysicalDrain);      //reverse drain
                card.ChangeMagicalPower(QuestLocation.QuestCard.MagicalDrain);    
            }
            else
            {
                print("Applying Drain");
                card.ChangePhysicalPower(-QuestLocation.QuestCard.PhysicalDrain);
                card.ChangeMagicalPower(-QuestLocation.QuestCard.MagicalDrain);
            }
        }
    }

    [Server]
    private void UpdateEnchanterBuff(int buffDelta) 
    {
        foreach (Transform cardTransform in DropZone.transform)
        {
            Card card = cardTransform.GetComponent<Card>();         // Enchanter is 0,0 so wont be buffed anyway, but we may want to change in the future
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
            Card card = cardTransform.GetComponent<Card>();
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
                Card card = cardTransform.GetComponent<Card>();
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
        Card wolfCard = Instantiate(CardDatabase.Instance.adventurerCardPrefab, Vector2.zero, Quaternion.identity);

        Spawn(wolfCard.gameObject);
        wolfCard.LoadCardData(wolfCardData);
        wolfCard.SetCardParent(Player.controlledHand.transform,false);
        wolfCard.SetCardParent(DropZone.transform, false);
    }

    //[Server]
    //public void AssignQuestCard(QuestCard questCard)
    //{
    //    QuestCard = questCard;
    //}

}
