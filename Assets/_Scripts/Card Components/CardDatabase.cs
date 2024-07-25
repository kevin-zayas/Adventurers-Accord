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
        CardKeywordMap.Add("Rogue", new List<string> { "Steal" });
        CardKeywordMap.Add("Sorcerer", new List<string> { "Empower" , "Protection"});
        CardKeywordMap.Add("Tinkerer", new List<string> { "Empower", "Protection"});
        //CardKeywordMap.Add("", new List<string> { "" });
    }

    private void InitializeKeywordDefinitionMap()
    {
        KeywordDefinitionMap.Add("Posion", poisonKeywordText);
        KeywordDefinitionMap.Add("Bardsong", bardsongKeywordText);
        KeywordDefinitionMap.Add("Protection", protectionKeywordText);
        KeywordDefinitionMap.Add("Empower", empowerKeywordText);
        KeywordDefinitionMap.Add("Summon", summonKeywordText);
        KeywordDefinitionMap.Add("Steal", stealKeywordText);

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

    private readonly string poisonKeywordText = "Poison explanation";
    private readonly string bardsongKeywordText = "Bardsong explanation";
    private readonly string protectionKeywordText = "Protection explanation";
    private readonly string empowerKeywordText = "Empower explanation";
    private readonly string summonKeywordText = "Summon explanation";
    private readonly string stealKeywordText = "Steal explanation";
    
}
