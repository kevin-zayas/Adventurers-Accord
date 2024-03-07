using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : NetworkBehaviour
{
    public static CardDatabase Instance { get; private set; }

    public List<CardData> tierOneCards = new();
    public List<Card> tierTwoCards = new();

    public List<QuestCard> questCards = new();

    public List<ItemCard> lootCards = new();


    public Card adventurerCardPrefab;

    //QUEST CARDS

    [SerializeField] private QuestCard slimePrefab;
    [SerializeField] public QuestCard lichPrefab;
    [SerializeField] private QuestCard beholderPrefab;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
