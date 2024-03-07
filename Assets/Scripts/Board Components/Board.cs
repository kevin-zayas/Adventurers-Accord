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
    private List<CardData> Deck { get; } = new List<CardData>();

    [field: SerializeField]
    private List<ItemCard> LootDeck { get; } = new List<ItemCard>();

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

        questLocations[0].StartGame();

        foreach (Player player in GameManager.Instance.Players)         //for testing item/spells
        {
            RewardLoot(player, 2);
        }
    }

    [Server]
    private void DrawCard(int slotIndex)
    {
        CardData randomCardData = Deck[Random.Range(0, Deck.Count)];
        Card card = Instantiate(CardDatabase.Instance.adventurerCardPrefab, Vector2.zero, Quaternion.identity);
        card.draftCardIndex = slotIndex;

        Spawn(card.gameObject);
        card.LoadCardData(randomCardData);
        card.SetCardParent(CardSlots[slotIndex].transform, false);

        AvailableCardSlots[slotIndex] = false;
        draftCards[slotIndex] = card;
        Deck.Remove(randomCardData);
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
        for (int i = 0; i < cardFrequency; i++)
        {
            Deck.AddRange(CardDatabase.Instance.tierOneCards);
            LootDeck.AddRange(CardDatabase.Instance.lootCards);
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

    [Server]
    public void ResetQuests()
    {
        foreach (QuestLocation location in questLocations)
        {
            location.ResetQuestLocation();
        }
    }

    [Server]
    public void RewardLoot(Player player, int lootAmount)
    {
        if (LootDeck.Count == 0) return;
        if (LootDeck.Count < lootAmount) lootAmount = LootDeck.Count;

        for (int i=0; i<lootAmount; i++)
        {
            ItemCard randomLoot = LootDeck[Random.Range(0, LootDeck.Count)];
            ItemCard itemCard = Instantiate(randomLoot, Vector2.zero, Quaternion.identity);

            Spawn(itemCard.gameObject);
            itemCard.SetCardScale(new Vector3(2f, 2f, 1f));
            itemCard.SetCardParent(player.controlledHand.transform, false);

            LootDeck.Remove(randomLoot);
        }
    }
}
