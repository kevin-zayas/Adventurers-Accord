using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : NetworkBehaviour
{
    public static CardDatabase Instance { get; private set; }

    public List<CardData> tierOneCards = new();
    public List<CardData> tierTwoCards = new();

    public List<CardData> levelOneQuestCards = new();
    public List<CardData> levelTwoQuestCards = new();
    public List<CardData> levelThreeQuestCards = new();

    public List<CardData> spellCards = new();
    public List<CardData> itemCards = new();

    public List<CardData> rareItemCards = new();

    //make private set
    public AdventurerCard adventurerCardPrefab;
    public ItemCard itemCardPrefab;
    public SpellCard spellCardPrefab;
    public QuestCard questCardPrefab;

    public CardData wolfCardData;

    public Dictionary<string,Sprite> SpriteMap { get; private set; }    //make private and use a function to access
    private Dictionary<string, List<string>> CardKeywordMap;
    public Dictionary<string, string> KeywordDefinitionMap { get; private set; }



    private void Awake()
    {
        Instance = this;
        SpriteMap = new();
        CardKeywordMap = new();
        KeywordDefinitionMap = new();
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeCardKeywordMap();
        InitializeKeywordDefinitionMap();
        InitializeSpriteMap();
    }

    private void InitializeCardKeywordMap()
    {
        CardKeywordMap.Add("Assassin", new List<string> { "Poison" });
        CardKeywordMap.Add("Bard", new List<string> { "Bardsong" });
        CardKeywordMap.Add("Cleric", new List<string> { "Protection" });
        CardKeywordMap.Add("Enchanter", new List<string> { "Empower" });
        CardKeywordMap.Add("Ranger", new List<string> { "Summon" });
        CardKeywordMap.Add("Rogue", new List<string> { "Disable" });
        CardKeywordMap.Add("Sorcerer", new List<string> { "Empower" });
        CardKeywordMap.Add("Tinkerer", new List<string> { "Empower" });
        //CardKeywordMap.Add("", new List<string> { "" });
    }

    private void InitializeKeywordDefinitionMap()
    {
        KeywordDefinitionMap.Add("Posion", poisonKeywordText);
        KeywordDefinitionMap.Add("Bardsong", bardsongKeywordText);
        KeywordDefinitionMap.Add("Protection", protectionKeywordText);
        KeywordDefinitionMap.Add("Empower", empowerKeywordText);
        KeywordDefinitionMap.Add("Summon", summonKeywordText);
        KeywordDefinitionMap.Add("Disable", disableKeywordText);

    }

    public List<string> GetCardKeywords(string cardName)
    {
        print(cardName);
        print(CardKeywordMap.GetValueOrDefault(cardName));
        return CardKeywordMap.GetValueOrDefault(cardName);
        //return new List<string> { "Poison" };
    }

    public string GetKeywordDefinition(string keyword)
    {
        //return KeywordDefinitionMap[keyword];
        print(keyword);
        print(KeywordDefinitionMap.GetValueOrDefault(keyword));
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

    private readonly string poisonKeywordText = "Decreases a specified stat of a targeted Adventurer by a specified amount until the Quest is resolved. This stat cannot be decreased lower than 0.";
    private readonly string bardsongKeywordText = "<align=left>Gain rewards based on the number of Bards on this Quest:\n<line-indent=15%>1 Bard:   +1 Gold\n2 Bards: +2 Gold, +1 Reputation\n3 Bards: +4 Gold, +2 Reputation</line-indent></align>";
    private readonly string protectionKeywordText = "Prevents targeted Adventurers from having their Power reduced until the Quest is resolved.";
    private readonly string empowerKeywordText = "Increases the Power of a card by a specified amount. A card can gain Power in each stat (Physical and Magical) that is non-zero.";
    private readonly string summonKeywordText = "Spawns a card at the specified Quest location. The summoned card counts as an Adventurer.";
    private readonly string disableKeywordText = "Temporarily nullifies the effects of a specified Magic Item. The item is restored to normal after the Quest ends.";
    
}
