using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : NetworkBehaviour
{
    public static Board Instance { get; private set; }

    [field: SerializeField]
    public CardSlot[] CardSlots { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool[] AvailableCardSlots { get; private set; }

    [SerializeField]
    private List<Card> deck;

    private readonly int cardFrequency = 3;

    [SerializeField]
    private Card cardPrefab;

    private void Awake()
    {
        Instance = this;
    }

    [Server]
    public void StartGame()
    {
        ConstructDecks();
        DrawCards();
    }

    [Server]
    private void DrawCards()
    {
        for (int i = 0; i < CardSlots.Length; i++)
        {
            Transform slot = CardSlots[i].transform;
            Card randomCard = deck[Random.Range(0, deck.Count)];
            Card card = Instantiate(randomCard, Vector2.zero, Quaternion.identity);
            card.parent = slot;

            Spawn(card.gameObject);
            AvailableCardSlots[i] = false;
            deck.Remove(randomCard);
        }
    }

    [Server]
    private void ConstructDecks()
    {
        deck = new List<Card>();
        List<Card> cardList = CardDatabase.Instance.tierOneCards;
        for (int i = 0; i < cardList.Count; i++)
        {
            for (int x = 0; x < cardFrequency; x++)
            {
                deck.Add(cardList[i]);
            }
        }

    }
}
