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
    public QuestLocation[] QuestLocations { get; private set; }

    [field: SerializeField]
    private List<CardData> T1Deck { get; } = new List<CardData>();

    [field: SerializeField]
    private List<CardData> T2Deck { get; } = new List<CardData>();

    [field: SerializeField]
    private List<CardData> LootDeck { get; } = new List<CardData>();

    [field: SerializeField]
    private List<CardData> L1QuestCardDeck { get; } = new List<CardData>();

    [field: SerializeField]
    private List<CardData> L2QuestCardDeck { get; } = new List<CardData>();

    [field: SerializeField]
    private List<CardData> L3QuestCardDeck { get; } = new List<CardData>();

    private readonly int cardFrequency = 3;
    private readonly int spellCardFrequency = 2;

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

        for(int i = 0; i < QuestLocations.Length; i++)
        {
            DrawQuestCard(i);
            QuestLocations[i].OnStartGame();
        }

        foreach (Player player in GameManager.Instance.Players)         //for testing item/spells
        {
            RewardLoot(player, GameManager.Instance.StartingLoot);
        }
    }

    [Server]
    private void DrawCard(int slotIndex)
    {
        List<CardData> deck; 

        if (slotIndex < 4) deck = T1Deck;
        else deck = T2Deck;

        CardData randomCardData= deck[Random.Range(0, deck.Count)];

        AdventurerCard card = Instantiate(CardDatabase.Instance.adventurerCardPrefab, Vector2.zero, Quaternion.identity);
        Spawn(card.gameObject);

        card.LoadCardData(randomCardData);
        card.SetCardParent(CardSlots[slotIndex].transform, false);

        deck.Remove(randomCardData);
        ObserversUpdateDeckTrackers(T1Deck.Count, T2Deck.Count);
    }

    [Server]
    public void DrawQuestCard(int questSlotIndex)
    {
        List<CardData> questCardDeck;

        if (L1QuestCardDeck.Count > 0) questCardDeck = L1QuestCardDeck;
        else if (L2QuestCardDeck.Count > 0) questCardDeck = L2QuestCardDeck;
        else if (L3QuestCardDeck.Count > 0) questCardDeck = L3QuestCardDeck;
        else return;

        QuestLocation questLocation = QuestLocations[questSlotIndex];
        CardData randomQuestData = questCardDeck[Random.Range(0, questCardDeck.Count)];
        QuestCard questCard = Instantiate(CardDatabase.Instance.questCardPrefab, Vector2.zero, Quaternion.identity);

        Spawn(questCard.gameObject);
        questCard.LoadCardData(randomQuestData);
        questLocation.AssignQuestCard(questCard);
        questCardDeck.Remove(randomQuestData);
    }

    [Server]
    private void ConstructDecks()
    {
        for (int i = 0; i < cardFrequency; i++)
        {
            T1Deck.AddRange(CardDatabase.Instance.tierOneCards);
            T2Deck.AddRange(CardDatabase.Instance.tierTwoCards);
            LootDeck.AddRange(CardDatabase.Instance.itemCards);
        }

        for (int i = 0; i < spellCardFrequency; i++)
        {
            LootDeck.AddRange(CardDatabase.Instance.spellCards);
        }

        LootDeck.AddRange(CardDatabase.Instance.rareItemCards);

        L1QuestCardDeck.AddRange(CardDatabase.Instance.levelOneQuestCards);
        L2QuestCardDeck.AddRange(CardDatabase.Instance.levelTwoQuestCards);
        L3QuestCardDeck.AddRange(CardDatabase.Instance.levelThreeQuestCards);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReplaceDraftCard(int slotIndex)
    {
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
    public void ObserversUpdatePhaseText(string phase, bool gameOver = false)
    {
        if (gameOver)
        {
            phaseText.text = "Game Over!";
            return;
        }
        phaseText.text = $"Phase : {phase}";
    }

    [Server]
    public void CheckQuestsForCompletion()
    {
        RoundSummaryPopUp popUp = PopUpManager.Instance.CreateRoundSummaryPopUp();
        Spawn(popUp.gameObject);

        for (int i = 0; i < QuestLocations.Length; i++)
        {

            QuestLocations[i].CheckQuestCompletion(popUp.QuestSummaries[i]);
        }

        
        popUp.ObserversInitializeRoundSummaryPopUp();
    }

    [Server]
    public void ResetQuests()
    {
        foreach (QuestLocation location in QuestLocations)
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

            if (randomLootData.CardType == "Magic Item")
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
