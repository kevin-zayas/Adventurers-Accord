using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [field: SerializeField]
    private List<Card> Deck { get; } = new List<Card>();

    private readonly int cardFrequency = 3;

    [SerializeField]
    private TMP_Text deckTrackerText;

    [SerializeField]
    [SyncVar(OnChange = nameof(UpdateDeckTrackers))]
    private int deckSize;

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
        Card randomCard = Deck[Random.Range(0, Deck.Count)];
        Card card = Instantiate(randomCard, Vector2.zero, Quaternion.identity);
        card.slotIndex = slotIndex;

        Spawn(card.gameObject);
        card.SetCardParent(CardSlots[slotIndex].transform, false);

        AvailableCardSlots[slotIndex] = false;
        draftCards[slotIndex] = card;
        Deck.Remove(randomCard);
        deckSize = Deck.Count;
    }

    [Server]
    private void ConstructDecks()
    {
        List<Card> cardList = CardDatabase.Instance.tierOneCards;
        for (int i = 0; i < cardList.Count; i++)
        {
            for (int x = 0; x < cardFrequency; x++)
            {
                Deck.Add(cardList[i]);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReplaceCard(int slotIndex)
    {   print("replacing card");
        if (slotIndex >= 0 && Deck.Count > 0)
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

    private void UpdateDeckTrackers(int oldSize, int newSize, bool asServer)
    {
        deckTrackerText.text = deckSize.ToString();
    }
}
