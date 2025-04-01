using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : NetworkBehaviour
{
    public static CardDatabase Instance { get; private set; }

    public enum GuildType 
    {
        FightersGuild,
        MagesGuild,
        ThievesGuild,
        MerchantsGuild,
        AsassinsGuild
    }

    public List<CardData> tierOneCards = new();
    public List<CardData> tierTwoCards = new();

    public List<CardData> levelOneQuestCards = new();
    public List<CardData> levelTwoQuestCards = new();
    public List<CardData> levelThreeQuestCards = new();

    public List<CardData> spellCards = new();
    public List<CardData> itemCards = new();

    public List<CardData> rareItemCards = new();

    public List<CardData> fightersGuildRoster = new();
    public List<CardData> magesGuildRoster = new();
    public List<CardData> thievesGuildRoster = new();
    public List<CardData> merchantsGuildRoster = new();
    public List<CardData> assassinGuildRoster = new();

    //make private set
    public AdventurerCard adventurerCardPrefab;
    public ItemCard itemCardPrefab;
    public SpellCard spellCardPrefab;
    public QuestCard questCardPrefab;

    public CardData wolfCardData;

    public Dictionary<string, Sprite> SpriteMap { get; private set; } = new();   //make private and use a function to access
    private readonly Dictionary<string, List<string>> CardKeywordMap = new();
    public Dictionary<string, string> KeywordDefinitionMap { get; private set; } = new();
    public Dictionary<GuildType, List<CardData>> GuildRosterMap { get; private set; } = new();



    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeCardKeywordMap();
        InitializeKeywordDefinitionMap();
        InitializeSpriteMap();
        InitializeGuildRosters();
    }

    private void InitializeCardKeywordMap()
    {
        //ADVENTURERERS
        CardKeywordMap.Add("Assassin", new List<string> { "Poison" });//, "Resolution Phase"});
        CardKeywordMap.Add("Bard", new List<string> { "Bardsong" });
        CardKeywordMap.Add("Cleric", new List<string> { "Protection" });
        CardKeywordMap.Add("Enchanter", new List<string> { "Empower" });
        CardKeywordMap.Add("Ranger", new List<string> { "Summon" });
        CardKeywordMap.Add("Rogue", new List<string> { "Disable" });//, "Resolution Phase"});
        CardKeywordMap.Add("Sorcerer", new List<string> { "Empower" });
        CardKeywordMap.Add("Tinkerer", new List<string> { "Empower"  });
        //CardKeywordMap.Add("", new List<string> { "" });

        //ITEMS
        CardKeywordMap.Add("+1 Staff", new List<string> { "Magic Items", "Equip"});
        CardKeywordMap.Add("+2 Staff", new List<string> { "Magic Items", "Equip" });
        CardKeywordMap.Add("+1 Sword", new List<string> { "Magic Items", "Equip" });
        CardKeywordMap.Add("+2 Sword", new List<string> { "Magic Items", "Equip" });

        //SPELLS
        CardKeywordMap.Add("Cloud of Daggers", new List<string> {"Boost", "Magic Spells", "Magic Phase" });
        CardKeywordMap.Add("Fireball", new List<string> { "Boost", "Magic Spells", "Magic Phase" });
        CardKeywordMap.Add("Grease", new List<string> { "Disable", "Magic Spells", "Magic Phase" });
        CardKeywordMap.Add("Hex", new List<string> { "Afflict", "Magic Spells", "Magic Phase" });
        CardKeywordMap.Add("Mana Drain", new List<string> { "Afflict", "Magic Spells", "Magic Phase" });

        //QUESTS
        CardKeywordMap.Add("Giant Scorpion", new List<string> { "Weaken" });
        CardKeywordMap.Add("Lich", new List<string> { "Weaken" });
        CardKeywordMap.Add("Beholder", new List<string> { "Disable" });

    }

    private void InitializeKeywordDefinitionMap()
    {
        KeywordDefinitionMap.Add("Poison", poisonKeywordText);
        KeywordDefinitionMap.Add("Bardsong", bardsongKeywordText);
        KeywordDefinitionMap.Add("Protection", protectionKeywordText);
        KeywordDefinitionMap.Add("Empower", empowerKeywordText);
        KeywordDefinitionMap.Add("Summon", summonKeywordText);
        KeywordDefinitionMap.Add("Disable", disableKeywordText);

        KeywordDefinitionMap.Add("Magic Items", magicItemKeywordText);
        KeywordDefinitionMap.Add("Equip", equipKeywordText);

        KeywordDefinitionMap.Add("Magic Spells", magicSpellKeywordText);
        KeywordDefinitionMap.Add("Afflict", afflictKeywordText);
        KeywordDefinitionMap.Add("Boost", boostKeywordText);

        KeywordDefinitionMap.Add("Magic Phase", magicPhaseKeywordText);
        KeywordDefinitionMap.Add("Resolution Phase", resolutionPhaseKeywordText);

        KeywordDefinitionMap.Add("Weaken", weakenKeywordText);

    }

    public List<string> GetCardKeywords(string cardName)
    {
        return CardKeywordMap.GetValueOrDefault(cardName);
    }

    public string GetKeywordDefinition(string keyword)
    {
        return KeywordDefinitionMap.GetValueOrDefault(keyword);
    }

    private void InitializeSpriteMap()
    {
        SaveSprites(tierOneCards, "Card_Sprites/");
        SaveSprites(tierTwoCards, "Card_Sprites/");
        SpriteMap.Add("Wolf", Resources.Load<Sprite>("Card_Sprites/Wolf"));

        SaveSprites(itemCards, "ItemSpell_Sprites/");
        SaveSprites(spellCards, "ItemSpell_Sprites/");
        SaveSprites(rareItemCards, "ItemSpell_Sprites/");

        SaveSprites(levelOneQuestCards, "Quest_Sprites/");
        SaveSprites(levelTwoQuestCards, "Quest_Sprites/");
        SaveSprites(levelThreeQuestCards, "Quest_Sprites/");
    }

    private void SaveSprites(List<CardData> cardList, string path)
    {
        foreach (CardData card in cardList)
        {
            SpriteMap.Add(card.CardName, Resources.Load<Sprite>(path + card.CardName));
        }
    }

    private void InitializeGuildRosters() 
    {
        GuildRosterMap[GuildType.FightersGuild] = fightersGuildRoster;
        GuildRosterMap[GuildType.MagesGuild] = magesGuildRoster;
        GuildRosterMap[GuildType.ThievesGuild] = thievesGuildRoster;
        GuildRosterMap[GuildType.MerchantsGuild] = merchantsGuildRoster;
        GuildRosterMap[GuildType.AsassinsGuild] = assassinGuildRoster;
    }

    private readonly string poisonKeywordText = "Decreases the <color=#6C00D7>Power</color> of a targeted Adventurer by a specified amount until the Quest is resolved. This value cannot be decreased below 0.";
    private readonly string bardsongKeywordText = "<align=left>Gain rewards based on the number of Bards on this Quest:\n<line-indent=15%>1 Bard:   +1 Gold\n2 Bards: +2 Gold, +1 Reputation\n3 Bards: +4 Gold, +2 Reputation</line-indent></align>";
    private readonly string protectionKeywordText = "Prevents affected Adventurers from having their <color=#6C00D7>Power</color> reduced until the Quest is resolved.";
    private readonly string empowerKeywordText = "Increases the <color=#6C00D7>Power</color> of a card by a specified amount. A card can gain <color=#6C00D7>Power</color> in each stat (<color=#EB000E>Physical</color> and <color=#000EEB>Magical</color>) that is non-zero.";
    private readonly string summonKeywordText = "Spawns a card at the specified Quest location. The summoned card counts as an Adventurer.";
    private readonly string disableKeywordText = "Temporarily nullifies the effects of a specified Magic Item. The item is restored to normal after the Quest ends.";

    private readonly string magicItemKeywordText = "Grants a permanent boost to the <color=#6C00D7>Power</color> of an Adventurer, or provides a special ability. Magic Items cannot be removed once Equipped.";
    private readonly string equipKeywordText = "To equip a card, drag a Magic Item card onto an Adventurer in your hand. The Adventurer's corresponding <color=#6C00D7>Power</color> must be greater than 0 to equip the item.";

    private readonly string magicSpellKeywordText = "Provides a one-time effect on a Quest. Play during the Magic Phase by dragging the card onto a Quest Lane.";
    private readonly string afflictKeywordText = "Decreases Total <color=#6C00D7>Power</color> on a Quest Lane by a specified amount until the Quest is resolved. This value cannot be decreased below 0.";
    private readonly string boostKeywordText = "Increases Total <color=#6C00D7>Power</color> on a Quest Lane by a specified amount until the Quest is resolved.";

    private readonly string resolutionPhaseKeywordText = "The phase where abilities requiring player decisions are executed, and their effects are applied.";
    private readonly string magicPhaseKeywordText = "The phase in which players can play Magic Spells to affect the outcome of a Quest.";

    private readonly string weakenKeywordText = "Reduces the <color=#6C00D7>Power</color> of all Adventurers on this Quest by a specified amount until the Quest is resolved. This value cannot be decreased below 0.";
}
