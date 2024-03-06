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

    [field: SerializeField]
    private QuestLocation[] questLocations;

    private readonly int cardFrequency = 3;

    [SerializeField]
    private TMP_Text deckTrackerText;

    [SerializeField]
    private TMP_Text phaseText;

    [SerializeField]
    public TMP_Text goldText;

    [SerializeField]
    public TMP_Text reputationText;

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
        DrawQuestCard();
        UpdateDraftCardOwnwer();
        questLocations[0].StartGame();

        ItemCard item = CardDatabase.Instance.swordPrefab;     //questCards[Random.Range(0, CardDatabase.Instance.questCards.Count)];
        ItemCard itemCard = Instantiate(item, Vector2.zero, Quaternion.identity);

        Spawn(itemCard.gameObject);
        itemCard.SetCardScale(new Vector3(2f, 2f, 1f));
        itemCard.SetCardParent(GameManager.Instance.Players[0].controlledHand.transform, false);


    }

    [Server]
    private void DrawCard(int slotIndex)
    {
        Card randomCard = Deck[Random.Range(0, Deck.Count)];
        Card card = Instantiate(randomCard, Vector2.zero, Quaternion.identity);
        card.draftCardIndex = slotIndex;

        Spawn(card.gameObject);
        card.SetCardParent(CardSlots[slotIndex].transform, false);

        AvailableCardSlots[slotIndex] = false;
        draftCards[slotIndex] = card;
        Deck.Remove(randomCard);
        ObserversUpdateDeckTrackers(Deck.Count);
    }

    [Server]
    private void DrawQuestCard()
    {
        QuestCard randomQuestCard = CardDatabase.Instance.lichPrefab;     //questCards[Random.Range(0, CardDatabase.Instance.questCards.Count)];
        QuestCard questCard = Instantiate(randomQuestCard, Vector2.zero, Quaternion.identity);
        
        //questCard.questCardIndex = 0;

        questLocations[0].AssignQuestCard(questCard);
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
    public void UpdateDraftCardOwnwer()             // should be able to delete this since players can now only drag draft cards if its their turn
    {
        foreach (Card card in draftCards)
        {
            SyncList<Player> players = GameManager.Instance.Players;
            int turn = GameManager.Instance.Turn;

            card.GiveOwnership(players[turn].Owner);
        }
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversUpdateDeckTrackers(int deckSize)
    {
        deckTrackerText.text = deckSize.ToString();
    }

    [ObserversRpc(BufferLast = true)]
    public void ObserversUpdatePhaseText(string phase)
    {
        phaseText.text = $"Phase : {phase}";
    }

    [Server]
    public void CheckQuests()
    {
        questLocations[0].CalculatePowerTotal();
    }

    public void ResetQuests()
    {
        foreach (QuestLocation location in questLocations)
        {
            location.ResetQuestLocation();
        }
    }
}
