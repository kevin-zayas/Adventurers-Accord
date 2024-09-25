using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Board : NetworkBehaviour
{
    #region Singleton
    public static Board Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [field: SerializeField] public CardSlot[] CardSlots { get; private set; }
    [field: SerializeField] public QuestLocation[] QuestLocations { get; private set; }

    [field: SerializeField] private List<CardData> T1Deck { get; } = new List<CardData>();
    [field: SerializeField] private List<CardData> T2Deck { get; } = new List<CardData>();
    [field: SerializeField] private List<CardData> LootDeck { get; } = new List<CardData>();
    [field: SerializeField] private List<CardData> L1QuestCardDeck { get; } = new List<CardData>();
    [field: SerializeField] private List<CardData> L2QuestCardDeck { get; } = new List<CardData>();
    [field: SerializeField] private List<CardData> L3QuestCardDeck { get; } = new List<CardData>();

    [SerializeField] private TMP_Text t1DeckTrackerText;
    [SerializeField] private TMP_Text t2DeckTrackerText;
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] public TMP_Text goldText;
    [SerializeField] public TMP_Text reputationText;
    #endregion

    #region Constants
    private readonly int cardFrequency = 3;
    private readonly int spellCardFrequency = 2;
    #endregion

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Initializes the game by constructing decks, drawing initial cards, and setting up quests.
    /// </summary>
    [Server]
    public void StartGame()
    {
        ConstructDecks();

        for (int i = 0; i < CardSlots.Length; i++)
        {
            DrawCard(i);
        }

        for (int i = 0; i < QuestLocations.Length; i++)
        {
            DrawQuestCard(i);
            QuestLocations[i].OnStartGame();
        }

        foreach (Player player in GameManager.Instance.Players)
        {
            RewardLoot(player, GameManager.Instance.StartingLoot);
        }
    }

    /// <summary>
    /// Draws a card from the appropriate deck and places it in the specified slot.
    /// </summary>
    /// <param name="slotIndex">The index of the slot where the card will be placed.</param>
    [Server]
    private void DrawCard(int slotIndex)
    {
        List<CardData> deck = slotIndex < 4 ? T1Deck : T2Deck;
        CardData randomCardData = deck[Random.Range(0, deck.Count)];

        AdventurerCard card = Instantiate(CardDatabase.Instance.adventurerCardPrefab, Vector2.zero, Quaternion.identity);
        Spawn(card.gameObject);

        card.LoadCardData(randomCardData);
        card.SetCardParent(CardSlots[slotIndex].transform, false);

        deck.Remove(randomCardData);
        ObserversUpdateDeckTrackers(T1Deck.Count, T2Deck.Count);
    }

    /// <summary>
    /// Draws a quest card from the appropriate quest deck and assigns it to the specified quest location.
    /// </summary>
    /// <param name="questSlotIndex">The index of the quest location where the card will be placed.</param>
    [Server]
    public void DrawQuestCard(int questSlotIndex)
    {
        List<CardData> questCardDeck = L1QuestCardDeck.Count > 0 ? L1QuestCardDeck :
                                      L2QuestCardDeck.Count > 0 ? L2QuestCardDeck :
                                      L3QuestCardDeck.Count > 0 ? L3QuestCardDeck : null;

        if (questCardDeck == null) return;

        QuestLocation questLocation = QuestLocations[questSlotIndex];
        CardData randomQuestData = questCardDeck[Random.Range(0, questCardDeck.Count)];
        QuestCard questCard = Instantiate(CardDatabase.Instance.questCardPrefab, Vector2.zero, Quaternion.identity);

        Spawn(questCard.gameObject);
        questCard.LoadCardData(randomQuestData);
        questLocation.AssignQuestCard(questCard);
        questCardDeck.Remove(randomQuestData);
    }

    /// <summary>
    /// Constructs the decks by adding cards to them based on the predefined frequencies.
    /// </summary>
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

    /// <summary>
    /// Replaces a draft card in the specified slot by drawing a new one.
    /// </summary>
    /// <param name="slotIndex">The index of the slot to replace the card in.</param>
    [ServerRpc(RequireOwnership = false)]
    public void ReplaceDraftCard(int slotIndex)
    {
        List<CardData> deck = slotIndex < 4 ? T1Deck : T2Deck;

        if (deck.Count > 0)
        {
            DrawCard(slotIndex);
        }
    }

    /// <summary>
    /// Updates the deck tracker text on all clients to reflect the current deck sizes.
    /// </summary>
    /// <param name="t1DeckSize">The current size of the T1 deck.</param>
    /// <param name="t2DeckSize">The current size of the T2 deck.</param>
    [ObserversRpc(BufferLast = true)]
    private void ObserversUpdateDeckTrackers(int t1DeckSize, int t2DeckSize)
    {
        t1DeckTrackerText.text = t1DeckSize.ToString();
        t2DeckTrackerText.text = t2DeckSize.ToString();
    }

    /// <summary>
    /// Updates the phase text on all clients to reflect the current game phase.
    /// </summary>
    /// <param name="phase">The name of the current phase.</param>
    /// <param name="gameOver">Indicates whether the game is over.</param>
    [ObserversRpc(BufferLast = true)]
    public void ObserversUpdatePhaseText(string phase, bool gameOver = false)
    {
        phaseText.GetComponent<FlashingEffect>().FlashEffect();
        phaseText.text = gameOver ? "Game Over!" : $"Phase : {phase}";
    }

    /// <summary>
    /// Checks all quests for completion and processes the end-of-quest logic.
    /// </summary>
    [Server]
    public void CheckQuestsForCompletion()
    {
        RoundSummaryPopUp popUp = PopUpManager.Instance.CreateRoundSummaryPopUp();
        Spawn(popUp.gameObject);

        for (int i = 0; i < QuestLocations.Length; i++)
        {
            QuestLocations[i].HandleEndOfQuest(popUp.QuestSummaries[i].Value);
        }

        popUp.ObserversInitializeRoundSummaryPopUp();
    }

    /// <summary>
    /// Resets all quest locations for the next round.
    /// </summary>
    [Server]
    public void ResetQuests()
    {
        foreach (QuestLocation location in QuestLocations)
        {
            location.ResetQuestLocation();
        }
    }

    /// <summary>
    /// Rewards a player with loot by drawing cards from the loot deck.
    /// </summary>
    /// <param name="player">The player to reward.</param>
    /// <param name="lootAmount">The amount of loot to reward.</param>
    [Server]
    public void RewardLoot(Player player, int lootAmount)
    {
        if (LootDeck.Count == 0) return;
        if (LootDeck.Count < lootAmount) lootAmount = LootDeck.Count;

        for (int i = 0; i < lootAmount; i++)
        {
            CardData randomLootData = LootDeck[Random.Range(0, LootDeck.Count)];

            if (randomLootData.CardType == "Magic Item")
            {
                ItemCard itemCard = Instantiate(CardDatabase.Instance.itemCardPrefab, Vector2.zero, Quaternion.identity);

                Spawn(itemCard.gameObject);
                itemCard.LoadCardData(randomLootData);
                itemCard.SetCardOwner(player);
                itemCard.SetCardParent(player.controlledHand.Value.transform, false);
            }
            else
            {
                SpellCard spellCard = Instantiate(CardDatabase.Instance.spellCardPrefab, Vector2.zero, Quaternion.identity);

                Spawn(spellCard.gameObject);
                spellCard.LoadCardData(randomLootData);
                spellCard.SetCardOwner(player);
                spellCard.SetCardParent(player.controlledHand.Value.transform, false);
            }

            LootDeck.Remove(randomLootData);
        }
    }
}
