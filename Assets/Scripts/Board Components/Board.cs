using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Board : NetworkBehaviour
{
    public static Board Instance { get; private set; }

    [field: SerializeField]
    public CardSlot[] CardSlots { get; private set; }

    [field: SerializeField]
    private List<CardData> T1Deck { get; } = new List<CardData>();

    [field: SerializeField]
    private List<CardData> T2Deck { get; } = new List<CardData>();

    [field: SerializeField]
    private List<CardData> LootDeck { get; } = new List<CardData>();

    [field: SerializeField]
    private List<CardData> QuestCardsDeck { get; } = new List<CardData>();

    [field: SerializeField]
    private QuestLocation[] questLocations;

    private readonly int cardFrequency = 3;

    [SerializeField]
    private TMP_Text t1DeckTrackerText;

    [SerializeField]
    private TMP_Text t2DeckTrackerText;

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
        List<CardData> deck; 

        if (slotIndex < 4) deck = T1Deck;
        else deck = T2Deck;

        CardData randomCardData= deck[Random.Range(0, deck.Count)];

        Card card = Instantiate(CardDatabase.Instance.adventurerCardPrefab, Vector2.zero, Quaternion.identity);

        Spawn(card.gameObject);
        card.LoadCardData(randomCardData);
        card.SetCardParent(CardSlots[slotIndex].transform, false);

        deck.Remove(randomCardData);
        ObserversUpdateDeckTrackers(T1Deck.Count, T2Deck.Count);
    }

    [Server]
    private void DrawQuestCard()
    {
        CardData randomQuestData = QuestCardsDeck[Random.Range(0, QuestCardsDeck.Count)];
        QuestCard questCard = Instantiate(CardDatabase.Instance.questCardPrefab, Vector2.zero, Quaternion.identity);
        //questCard.questCardIndex = 0;

        Spawn(questCard.gameObject);
        questCard.LoadCardData(randomQuestData);
        questLocations[0].AssignQuestCard(questCard);  //set quest index here
        QuestCardsDeck.Remove(randomQuestData);
    }

    [Server]
    private void ConstructDecks()
    {
        for (int i = 0; i < cardFrequency; i++)
        {
            T1Deck.AddRange(CardDatabase.Instance.tierOneCards);
            T2Deck.AddRange(CardDatabase.Instance.tierTwoCards);
            LootDeck.AddRange(CardDatabase.Instance.lootCards);
        }
        QuestCardsDeck.AddRange(CardDatabase.Instance.questCards);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReplaceCard(int slotIndex)
    {   print("replacing card");
        List<CardData> deck;

        if (slotIndex < 4) deck = T1Deck;
        else deck = T2Deck;

        if (deck.Count > 0)
        {
            DrawCard(slotIndex);
        }
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversUpdateDeckTrackers(int t1DeckSize, int t2DeckSize)
    {
        t1DeckTrackerText.text = t1DeckSize.ToString();
        t2DeckTrackerText.text = t2DeckSize.ToString();
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
            CardData randomLootData = LootDeck[Random.Range(0, LootDeck.Count)];

            if (randomLootData.cardType == "Item")
            {
                ItemCard itemCard = Instantiate(CardDatabase.Instance.itemCardPrefab, Vector2.zero, Quaternion.identity);

                Spawn(itemCard.gameObject);
                itemCard.LoadCardData(randomLootData);
                itemCard.SetCardOwner(player);
                itemCard.SetCardParent(player.controlledHand.transform, false);
            }
            else
            {
                SpellCard spellCard = Instantiate(CardDatabase.Instance.spellCardPrefab, Vector2.zero, Quaternion.identity);

                Spawn(spellCard.gameObject);
                spellCard.LoadCardData(randomLootData);
                spellCard.SetCardOwner(player);
                spellCard.SetCardParent(player.controlledHand.transform, false);
            }

            LootDeck.Remove(randomLootData);
        }
    }
}
