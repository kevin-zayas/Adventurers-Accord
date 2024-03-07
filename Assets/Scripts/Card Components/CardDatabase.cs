using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : NetworkBehaviour
{
    public static CardDatabase Instance { get; private set; }

    public List<Card> tierOneCards = new();
    public List<Card> tierTwoCards = new();

    public List<QuestCard> questCards = new();

    public List<ItemCard> lootCards = new();

    //TIER 1 CARDS
    [SerializeField] private Card warriorPrefab;

    [SerializeField] private Card wizardPrefab;

    [SerializeField] private Card clericPrefab;

    [SerializeField] private Card roguePrefab;

    [SerializeField] private Card bardPrefab;

    //QUEST CARDS

    [SerializeField] private QuestCard slimePrefab;

    [SerializeField] public QuestCard lichPrefab;

    [SerializeField] private QuestCard beholderPrefab;

    // MAGIC ITEM CARDS

    [SerializeField] public ItemCard swordPrefab;
    [SerializeField] public ItemCard staffPrefab;





    private void Awake()
    {
        Instance = this;
        tierOneCards.Add(warriorPrefab);
        tierOneCards.Add(wizardPrefab);
        tierOneCards.Add(clericPrefab);
        tierOneCards.Add(roguePrefab);
        tierOneCards.Add(bardPrefab);

        lootCards.Add(swordPrefab);
        lootCards.Add(staffPrefab);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
