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

    public Dictionary<string,Sprite> SpriteMap { get; private set; }
    public Dictionary<string, List<string>> DescriptionKeyMap { get; private set; }
    public Dictionary<string, string> DescriptionMap { get; private set; }



    private void Awake()
    {
        Instance = this;
        SpriteMap = new();
        DescriptionKeyMap = new();
        DescriptionMap = new();
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeDescriptionKeyMap();
        InitializeSpriteMap();
    }

    private void InitializeDescriptionKeyMap()
    {
        DescriptionKeyMap.Add("Assassin", new List<string> { "test1", "test2" });
    }

    public string GetCardDescription(string keyword)
    {
        return DescriptionMap[keyword];
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
}
