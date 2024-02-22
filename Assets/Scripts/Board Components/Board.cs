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
        for (int i = 0; i < CardSlots.Length; i++)
        {
            DrawCard(i);
        }
        
    }

    [Server]
    private void DrawCard(int slotIndex)
    {
        print("Drawing card");
        Card randomCard = deck[Random.Range(0, deck.Count)];
        Card card = Instantiate(randomCard, Vector2.zero, Quaternion.identity);

        card.parent = CardSlots[slotIndex].transform;
        card.tag = "DraftCard";
        card.slotIndex = slotIndex;

        Spawn(card.gameObject);
        //card.ServerSetCardParent(CardSlots[slotIndex].transform);
        AvailableCardSlots[slotIndex] = false;
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
            DrawCard(slotIndex);
        }
    }
}
