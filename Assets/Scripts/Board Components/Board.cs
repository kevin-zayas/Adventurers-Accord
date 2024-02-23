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

    [SerializeField]
    private Card[] draftCards = new Card[4];

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
        for (int i = 0; i < CardSlots.Length; i++)
        {
            DrawCard(i);
        }
        UpdateDraftCardOwnwer();
        
    }

    [Server]
    private void DrawCard(int slotIndex)
    {
        Card randomCard = deck[Random.Range(0, deck.Count)];
        Card card = Instantiate(randomCard, Vector2.zero, Quaternion.identity);
        card.slotIndex = slotIndex;

        Spawn(card.gameObject);
        card.SetCardParent(CardSlots[slotIndex].transform, false);

        AvailableCardSlots[slotIndex] = false;
        draftCards[slotIndex] = card;
        deck.Remove(randomCard);
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

    [ServerRpc(RequireOwnership = false)]
    public void ReplaceCard(int slotIndex)
    {   print("replacing card");
        if (slotIndex >= 0 && deck.Count > 0)
        {
            AvailableCardSlots[slotIndex] = true;
            draftCards[slotIndex] = null;
            DrawCard(slotIndex);
        }
    }

    [Server]
    public void UpdateDraftCardOwnwer()
    {
        foreach (Card card in draftCards)
        {
            SyncList<Player> players = GameManager.Instance.Players;
            int turn = GameManager.Instance.Turn;

            card.GiveOwnership(players[turn].Owner);
        }
    }
}
