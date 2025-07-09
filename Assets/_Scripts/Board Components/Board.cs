using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Card;

public class Board : NetworkBehaviour
{
    #region Singleton
    public static Board Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [field: SerializeField] public DraftCardHolder[] DraftCardSlots { get; private set; }
    [field: SerializeField] public QuestLocation[] QuestLocations { get; private set; }
    [field: SerializeField] public GuildStatus[] GuildStatusList{ get; private set; }
    [field: SerializeField] private List<CardData> T1Deck { get; } = new List<CardData>();
    [field: SerializeField] private List<CardData> T2Deck { get; } = new List<CardData>();
    [field: SerializeField] private List<CardData> ShopLootDeck { get; } = new List<CardData>();
    [field: SerializeField] private List<CardData> RewardLootDeck { get; } = new List<CardData>();
    [field: SerializeField] private List<CardData> L1QuestCardDeck { get; } = new List<CardData>();
    [field: SerializeField] private List<CardData> L2QuestCardDeck { get; } = new List<CardData>();
    [field: SerializeField] private List<CardData> L3QuestCardDeck { get; } = new List<CardData>();

    [SerializeField] private TMP_Text t1DeckTrackerText;
    [SerializeField] private TMP_Text t2DeckTrackerText;
    [SerializeField] private TMP_Text lootDeckTrackerText;
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private Button restingAdventurerButton;
    [SerializeField] GameObject handSlotPrefab;
    public TMP_Text goldText;
    public TMP_Text reputationText;
    #endregion

    #region Constants
    private readonly int cardFrequency = 3;
    private readonly int spellCardFrequency = 2;
    #endregion

    private void Awake()
    {
        Instance = this;
        restingAdventurerButton.onClick.AddListener(() =>
        {
            PopUpManager.Instance.ServerCreateGuildRosterPopUp(LocalConnection, Player.Instance, false);
        });
    }

    /// <summary>
    /// Initializes the game by constructing decks, drawing initial cards, and setting up quests.
    /// </summary>
    [Server]
    public void StartGame()
    {
        ConstructDecks();

        for (int i = 0; i < DraftCardSlots.Length; i++)
        {
            DrawDraftCard(i);
        }

        for (int i = 0; i < QuestLocations.Length; i++)
        {
            DrawQuestCard(i);
            QuestLocations[i].OnStartGame();
        }

        DealStartingHand();
        ObserversInitializeGuildStatus();
    }

    [ObserversRpc]
    private void ObserversInitializeGuildStatus()
    {
        for (int i = 0; i < GameManager.Instance.Players.Count; i++)
        {
            GuildStatusList[i].InitializeGuildStatus(GameManager.Instance.Players[i]);
        }
    }

    [ObserversRpc]
    public void ObserversUpdateTurnMarker(int? playerID = null)
    {
        for (int i = 0; i < GameManager.Instance.Players.Count; i++)
        {
            if (playerID == null)
            {
                GuildStatusList[i].SetTurnIndicator(true);
            }
            else
            {
                GuildStatusList[i].SetTurnIndicator(i == playerID);
            }
        }
    }

    [ObserversRpc]
    public void ObserversUpdateTurnMarker(int playerID, bool isTurn)
    {
        GuildStatusList[playerID].SetTurnIndicator(isTurn);
    }

    /// <summary>
    /// Draws a card from the appropriate deck and places it in the specified slot.
    /// </summary>
    /// <param name="slotIndex">The index of the slot where the card will be placed.</param>
    [Server]
    private void DrawDraftCard(int slotIndex)
    {
        if (slotIndex > 7)
        {
            DrawShopLootCard(slotIndex);
            return;
        }
        List<CardData> deck = slotIndex < 4 ? T1Deck : T2Deck;
        CardData randomCardData = deck[Random.Range(0, deck.Count)];

        SpawnCard(randomCardData, DraftCardSlots[slotIndex]);
        deck.Remove(randomCardData);
        ObserversUpdateDeckTrackers(T1Deck.Count, T2Deck.Count);
    }

    /// <summary>
    /// Draws a card from the loot deck and places it in the specified slot.
    /// </summary>
    /// <param name="slotIndex">The index of the slot where the card will be placed.</param>
    [Server]
    private void DrawShopLootCard(int slotIndex)
    {
        CardData randomLootData = ShopLootDeck[Random.Range(0, ShopLootDeck.Count)];
        
        Card lootCard = SpawnCard(randomLootData, DraftCardSlots[slotIndex]);
        ObserversSetCardLayer(lootCard.gameObject);

        ShopLootDeck.Remove(randomLootData);
        ObserversUpdateLootDeckTracker(ShopLootDeck.Count);
    }

    [ObserversRpc]
    private void ObserversSetCardLayer(GameObject card)
    {
        card.layer = LayerMask.NameToLayer("Draft Card");
    }

    /// <summary>
    /// Draws a quest card from the appropriate quest deck and assigns it to the specified quest location.
    /// </summary>
    /// <param name="questSlotIndex">The index of the quest location where the card will be placed.</param>
    [Server]
    public void DrawQuestCard(int questSlotIndex)
    {
        List<CardData> questCardDeck = CardDatabase.Instance.TestQuestCards.Count > 0 ? CardDatabase.Instance.TestQuestCards
            : L1QuestCardDeck.Count > 0 ? L1QuestCardDeck
            : L2QuestCardDeck.Count > 0 ? L2QuestCardDeck
            : L3QuestCardDeck.Count > 0 ? L3QuestCardDeck
            : null;

        if (questCardDeck == null) return;

        QuestLocation questLocation = QuestLocations[questSlotIndex];
        CardData randomQuestData = questCardDeck[Random.Range(0, questCardDeck.Count)];
        QuestCard questCard = Instantiate(CardDatabase.Instance.questCardPrefab, Vector2.zero, Quaternion.identity);

        Spawn(questCard.gameObject);
        questCard.LoadCardData(randomQuestData);
        questLocation.AssignQuestCard(questCard);
        questCardDeck.Remove(randomQuestData);
    }

    [Server]
    public void CheckAllQuestsComplete()
    {
       foreach (QuestLocation questLocation in QuestLocations)
        {
            if (questLocation.Status != QuestLocation.QuestStatus.Completed)
            {
                print("active quest found, not all quests are complete");
                return;
            }
        }

        GameManager.Instance.SetPhaseGameOver();
        return;
    }

    /// <summary>
    /// Constructs the decks by adding cards to them based on the predefined frequencies.
    /// </summary>
    [Server]
    private void ConstructDecks()
    {
        for (int i = 0; i < cardFrequency; i++)
        {
            T1Deck.AddRange(CardDatabase.Instance.tierOneAdventurers);
            T2Deck.AddRange(CardDatabase.Instance.tierTwoAdventurers);
            ShopLootDeck.AddRange(CardDatabase.Instance.itemCards);
            RewardLootDeck.AddRange(CardDatabase.Instance.itemCards);
        }

        for (int i = 0; i < spellCardFrequency; i++)
        {
            ShopLootDeck.AddRange(CardDatabase.Instance.spellCards);
            RewardLootDeck.AddRange(CardDatabase.Instance.spellCards);
        }

        ShopLootDeck.AddRange(CardDatabase.Instance.rareItemCards);
        RewardLootDeck.AddRange(CardDatabase.Instance.rareItemCards);
        L1QuestCardDeck.AddRange(CardDatabase.Instance.levelOneQuestCards);
        L2QuestCardDeck.AddRange(CardDatabase.Instance.levelTwoQuestCards);
        L3QuestCardDeck.AddRange(CardDatabase.Instance.levelThreeQuestCards);
    }

    /// <summary>
    /// Replaces a draft card in the specified slot by drawing a new one.
    /// </summary>
    /// <param name="slotIndex">The index of the slot to replace the card in.</param>
    [ServerRpc(RequireOwnership = false)]
    public void ServerReplaceDraftCard(int slotIndex)
    {
        List<CardData> deck = slotIndex < 4 ? T1Deck : slotIndex < 8 ? T2Deck : ShopLootDeck;

        if (deck.Count > 0)
        {
            DrawDraftCard(slotIndex);
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
        t1DeckTrackerText.text = $"{t1DeckSize} Recruits Remaining";
        t2DeckTrackerText.text = $"{t2DeckSize} Recruits Remaining";
    }

    /// <summary>
    /// Updates the loot deck tracker text on all clients to reflect the current deck sizes.
    /// </summary>
    /// <param name="lootDeckSize">The current size of the T1 deck.</param>
    [ObserversRpc(BufferLast = true)]
    private void ObserversUpdateLootDeckTracker(int lootDeckSize)
    {
        lootDeckTrackerText.text = $"{lootDeckSize} Wares Remaining";
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
        phaseText.text = gameOver ? "Game Over!" : $"{phase} Phase";
    }

    /// <summary>
    /// Checks all quests for completion and processes the end-of-quest logic.
    /// </summary>
    [Server]
    public void CheckQuestsForCompletion()
    {
        List<QuestSummaryData> questSummaries = new();

        for (int i = 0; i < QuestLocations.Length; i++)
        {
            if (QuestLocations[i].QuestCard.Value == null) continue;
            QuestLocations[i].HandleEndOfQuest(questSummaries);
        }

        Dictionary<int, PlayerRoundSummaryData> playerSummaries = CreatePlayerRoundSummary(questSummaries);
        PopUpManager.Instance.CreateRoundSummaryPopUp(playerSummaries, questSummaries);
        GameManager.Instance.EndPhase();
    }

    private Dictionary<int, PlayerRoundSummaryData> CreatePlayerRoundSummary(List<QuestSummaryData> questSummaries)
    {
        Dictionary<int, PlayerRoundSummaryData> playerSummaries = new();

        foreach (Player player in GameManager.Instance.Players)
        {
            PlayerRoundSummaryData playerSummary = new($"Player {player.PlayerID.Value + 1}");
            playerSummaries.Add(player.PlayerID.Value, playerSummary);
            if (player.DispatchedAdventurerCount == 0)
            {
                int oddJobGold = 1 + (GameManager.Instance.RoundNumber.Value + 1) / 2;
                playerSummary.AddBonusReward("Odd Jobs", oddJobGold, 0, 0);
                player.ChangePlayerGold(oddJobGold);
                player.UpdateGuildRecapTracker("Odd Jobs (Count)", 1);
                player.UpdateGuildRecapTracker("Odd Jobs (Gold)", oddJobGold);
            }
        }

        foreach (QuestSummaryData questSummary in questSummaries)
        {
            foreach (int playerID in questSummary.PlayerQuestSummaries.Keys)
            {
                PlayerRoundSummaryData playerSummary = playerSummaries[playerID];
                PlayerRoundSummaryData playerQuestSummary = questSummary.PlayerQuestSummaries[playerID];
                playerSummary.UpdatePlayerSummary(playerQuestSummary);
            }
        }
        return playerSummaries;
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
        if (RewardLootDeck.Count == 0) return;
        if (RewardLootDeck.Count < lootAmount) lootAmount = RewardLootDeck.Count;

        for (int i = 0; i < lootAmount; i++)
        {
            CardData randomLootData = RewardLootDeck[Random.Range(0, RewardLootDeck.Count)];
            SpawnCard(randomLootData, player.ControlledHand.Value, player);
            if (randomLootData.CardType == CardType.MagicItem)
            {
                player.UpdateGuildRecapTracker("Magic Items (Loot)", 1);
            }
            else if (randomLootData.CardType == CardType.Spell)
            {
                player.UpdateGuildRecapTracker("Spells (Loot)", 1);
                if (randomLootData.IsNegativeEffect)
                {
                    player.UpdateGuildRecapTracker("Curse Spells (Loot)", 1);
                }
            }

        }
    }

    [Server]
    public void DealStartingHand()
    {
        foreach (Player player in GameManager.Instance.Players)
        {
            List<CardData> startingHand = CardDatabase.Instance.TestRoster.Count > 0
            ? CardDatabase.Instance.TestRoster
            : CardDatabase.Instance.GuildRosterMap[player.GuildType];

            foreach (CardData cardData in startingHand)
            {
                SpawnCard(cardData, player.ControlledHand.Value, player);
            }
        }
    }

    [Server]
    private Card SpawnCard(CardData cardData, CardHolder cardHolder, Player player = null)
    {
        Card cardPrefab = cardData.CardType switch
        {
            CardType.MagicItem => CardDatabase.Instance.itemCardPrefab,
            CardType.Spell => CardDatabase.Instance.spellCardPrefab,
            CardType.Potion => CardDatabase.Instance.potionCardPrefab,
            _ => CardDatabase.Instance.adventurerCardPrefab,
        };
        Card card = Instantiate(cardPrefab, Vector2.zero, Quaternion.identity);

        Spawn(card.gameObject);
        card.LoadCardData(cardData);
        if (player != null) card.SetCardOwner(player);
        cardHolder.AddCard(card);
        
        return card;
    }
}
