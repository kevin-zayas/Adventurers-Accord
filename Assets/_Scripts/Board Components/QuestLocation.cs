using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestLocation : NetworkBehaviour
{
    [SerializeField] private QuestLane[] questLanes;

    public enum QuestStatus { Unchallenged, Completed, Failed }

    [field: SerializeField] public QuestStatus Status { get; private set; }

    [field: SerializeField] private CardSlot questPreviewSlot;

    private QuestCard previewQuestCard;

    [field: SerializeField] private CardSlot questCardSlot;
    [field: SerializeField] public int QuestLocationIndex { get; private set; }

    public readonly SyncVar<QuestCard> QuestCard = new();

    public readonly SyncVar<int> TotalPhysicalPower = new();

    public readonly SyncVar<int> TotalMagicalPower = new();

    [field: SerializeField] public List<List<AdventurerCard>> CardsToResolvePerLane { get; private set; } = new List<List<AdventurerCard>>();

    public readonly SyncVar<bool> AllowResolution = new();

    private readonly Dictionary<int, Tuple<int,int>> bardBonusMap = new();

    [field: SerializeField] private TMP_Text totalPhysicalPowerText;
    [field: SerializeField] private TMP_Text totalMagicalPowerText;

    [SerializeField] private RectTransform questLocationGroupRect;
    [SerializeField] private RectTransform questLaneGroupRect;

    [Server]
    public void OnStartGame()
    {
        ObserversInitializeQuestLocation();

        Status = QuestStatus.Unchallenged;

        foreach (Player player in GameManager.Instance.Players)
        {
            CardsToResolvePerLane.Add(new List<AdventurerCard>());
        }

        bardBonusMap.Add(1, new Tuple<int, int>(1, 0));
        bardBonusMap.Add(2, new Tuple<int, int>(2, 1));
        bardBonusMap.Add(3, new Tuple<int, int>(4, 2));

    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversInitializeQuestLocation()
    {
        int playerCount = GameManager.Instance.Players.Count;
        float questLaneGroupWidth = 15 + 127.5f * playerCount;
        float questLocationWidth = (questLaneGroupWidth < 400) ? 400 : questLaneGroupWidth;

        for (int i = 0; i < questLanes.Length; i++)
        {
            questLanes[i].gameObject.SetActive(i < playerCount);

            if (LocalConnection.ClientId == i)
            {
                questLanes[i].QuestDropZone.GetComponent<BoxCollider2D>().enabled = true;
                questLanes[i].QuestDropZone.GetComponent<Image>().color = Color.white;

                questLanes[i].ServerSetQuestLanePlayer(GameManager.Instance.Players[i]);
            }
        }
        questLaneGroupRect.sizeDelta = new Vector2(questLaneGroupWidth, questLaneGroupRect.sizeDelta.y);

        RectTransform questLocationRect = this.GetComponent<RectTransform>();
        questLocationRect.sizeDelta = new Vector2(questLocationWidth, questLocationRect.sizeDelta.y);

        questLocationGroupRect.sizeDelta = new Vector2(125 + 3 * questLocationWidth, questLocationGroupRect.sizeDelta.y);

    }

    [Server]
    public QuestLane[] GetQuestLanes()
    {
        return questLanes;
    }

    [Server]
    public void AssignQuestCard(QuestCard questCard)
    {
        Status = QuestStatus.Unchallenged;
        CreatePreviewCard(questCard);

        questCard.SetCardParent(questCardSlot.transform, false);
        questCard.ObserversSetCardScale(new Vector2(1.15f, 1.15f));
        QuestCard.Value = questCard;

        foreach (QuestLane lane in questLanes) lane.AssignQuestCard(questCard);
    }

    [Server]
    private void CreatePreviewCard(QuestCard questCard)
    {
        QuestCard previewCard = Instantiate(CardDatabase.Instance.questCardPrefab, Vector2.zero, Quaternion.identity);
        Spawn(previewCard.gameObject);

        previewCard.LoadCardData(questCard.Data.Value);
        previewCard.SetCardParent(questPreviewSlot.transform, false);
        previewQuestCard = previewCard;
    }

    [Server]
    private void ReplaceQuestCard()
    {
        Despawn(previewQuestCard.gameObject);
        Despawn(QuestCard.Value.gameObject);
        Board.Instance.DrawQuestCard(questCardSlot.SlotIndex);
    }

    [Server]
    public void UpdateTotalPower()
    {
        TotalPhysicalPower.Value = 0;
        TotalMagicalPower.Value = 0;

        for (int i = 0; i < questLanes.Length; i++)
        {
            TotalPhysicalPower.Value += questLanes[i].TotalPhysicalPower.Value;
            TotalMagicalPower.Value += questLanes[i].TotalMagicalPower.Value;
        }

        ObserversUpdateTotalPower(TotalPhysicalPower.Value, TotalMagicalPower.Value);
    }

    [ObserversRpc]
    private void ObserversUpdateTotalPower(int totalPhysicalPower, int totalMagicalPower)
    {
        totalPhysicalPowerText.text = totalPhysicalPower.ToString();
        totalMagicalPowerText.text = totalMagicalPower.ToString();
    }

    [Server]
    public bool AreQuestReqsMet()
    {
        // Check if the Physical Power requirement is met or if no Physical Power is required
        bool physicalRequirementMet = (QuestCard.Value.PhysicalPower.Value == 0 || TotalPhysicalPower.Value >= QuestCard.Value.PhysicalPower.Value);

        // Check if the Magical Power requirement is met or if no Magical Power is required
        bool magicalRequirementMet = (QuestCard.Value.MagicalPower.Value == 0 || TotalMagicalPower.Value >= QuestCard.Value.MagicalPower.Value);

        return physicalRequirementMet && magicalRequirementMet;
    }

    [Server]
    public void HandleEndOfQuest(List<QuestSummaryData> questSummaries)
    {
        UpdateTotalPower();
        QuestSummaryData questSummaryData = new(this);
        questSummaries.Add(questSummaryData);

        if (!HasDispatchedAdventurers()) return;

        if (AreQuestReqsMet())
        {
            Status = QuestStatus.Completed;
            CalculateQuestContributions(true,questSummaryData);
            DistributeBardBonus(questSummaryData);
        }
        else
        {
            Status = QuestStatus.Failed;
            CalculateFailedQuestPenalty(questSummaryData);
        }

        questSummaryData.Status = Status;
        CheckGuildBonus(questSummaryData);
        ReplaceQuestCard();
        return;
    }

    [Server]
    public void CalculateQuestContributions(bool isRoundOver, QuestSummaryData questSummaryData = null)
    {
        foreach (QuestLane lane in questLanes)
        {
            if (DoesMeetFullReqs(lane))
            {
                if (isRoundOver) DistributeQuestRewards(lane, true, questSummaryData);
                lane.ObserversUpdateRewardIndicator("gold");
            }
            else if (AreQuestReqsMet() && DoesMeetHalflReqs(lane))
            {
                if (isRoundOver) DistributeQuestRewards(lane, false, questSummaryData);
                lane.ObserversUpdateRewardIndicator("silver");
            }
            else
            {
                lane.ObserversUpdateRewardIndicator("blank");
                if (isRoundOver && lane.EffectiveTotalPower.Value > 0)
                {
                    lane.Player.Value.UpdateGuildRecapTracker("Quests Completed", 1);

                    PlayerRoundSummaryData playerSummary = new($"Player {lane.Player.Value.PlayerID.Value + 1}");
                    playerSummary.UpdatePlayerSummary(0, 0, 0, lane.TotalPhysicalPower.Value, lane.TotalMagicalPower.Value);
                    questSummaryData.PlayerQuestSummaries[lane.Player.Value.PlayerID.Value] = playerSummary;
                }
            }
        }
    }

    [Server]
    private bool DoesMeetFullReqs(QuestLane lane)
    {
        if (QuestCard.Value.PhysicalPower.Value > 0 && lane.TotalPhysicalPower.Value < QuestCard.Value.PhysicalPower.Value) return false;
        if (QuestCard.Value.MagicalPower.Value > 0 && lane.TotalMagicalPower.Value < QuestCard.Value.MagicalPower.Value) return false;
 
        return true;
    }

    [Server]
    private bool DoesMeetHalflReqs(QuestLane lane)
    {
        int questPowerTotal = QuestCard.Value.PhysicalPower.Value + QuestCard.Value.MagicalPower.Value;
        return (lane.EffectiveTotalPower.Value >= questPowerTotal / 2);      // Use 2.0f to avoid integer division truncation if needed
    }

    [Server]
    private bool HasDispatchedAdventurers()
    {
        foreach (QuestLane lane in questLanes)
        {
            if (lane.QuestDropZone.transform.childCount > 0) return true;
        }
        return false;
    }

    [Server]
    private void DistributeQuestRewards(QuestLane lane, bool fullRewards, QuestSummaryData questSummaryData)
    {
        Player player = lane.Player.Value;

        (int goldReward, int reputationReward, int lootReward) = fullRewards
            ? (QuestCard.Value.GoldReward.Value, QuestCard.Value.ReputationReward.Value, QuestCard.Value.LootReward.Value)
            : (QuestCard.Value.GoldReward.Value / 2, QuestCard.Value.ReputationReward.Value / 2, (int)Mathf.Floor(QuestCard.Value.LootReward.Value / 2));
        
        player.ChangePlayerGold(goldReward);
        player.ChangePlayerReputation(reputationReward);
        Board.Instance.RewardLoot(player, lootReward);

        PlayerRoundSummaryData playerSummary = new($"Player {player.PlayerID.Value + 1}");
        playerSummary.UpdatePlayerSummary(goldReward, reputationReward, lootReward, lane.TotalPhysicalPower.Value, lane.TotalMagicalPower.Value);
        questSummaryData.PlayerQuestSummaries[player.PlayerID.Value] = playerSummary;

        player.UpdateGuildRecapTracker("Quests Completed", 1);
        player.UpdateGuildRecapTracker("Quest Reward (Gold)", goldReward);
        player.UpdateGuildRecapTracker("Quest Reward (Rep)", reputationReward);
        player.UpdateGuildRecapTracker("Quest Reward (Loot)", lootReward);
        if (fullRewards) player.UpdateGuildRecapTracker("Quests Completed (Full Rewards)", 1);
        else player.UpdateGuildRecapTracker("Quests Completed (Half Rewards)", 1);

    }

    [Server]
    private void CheckGuildBonus(QuestSummaryData questSummaryData)
    {
        if (Status == QuestStatus.Unchallenged) return;

        foreach (int playerID in questSummaryData.PlayerQuestSummaries.Keys)
        {
            Player player = GameManager.Instance.Players[playerID];
            PlayerRoundSummaryData questPlayerSummaryData = questSummaryData.PlayerQuestSummaries[playerID];

            if (player.IsThievesGuild)
            {
                if (Status == QuestStatus.Completed)
                {
                    player.ChangePlayerGold(1);
                    player.UpdateGuildRecapTracker("First to the Spoils (Gold)", 1);
                    questPlayerSummaryData.AddBonusReward("First to the Spoils", 1, 0, 0);

                    if (UnityEngine.Random.Range(1, 5) == 1) // 25% chance
                    {
                        Board.Instance.RewardLoot(player, 1);
                        player.UpdateGuildRecapTracker("First to the Spoils (Loot)", 1);
                        questPlayerSummaryData.AddBonusReward("First to the Spoils", 0, 0, 1);
                    }
                       
                }
                if (player.GuildBonusTracker[QuestLocationIndex]["disabledItems"] > 0)
                {
                    player.ChangePlayerGold(1);
                    player.UpdateGuildRecapTracker("Sleight of Hand (Gold)", 1);
                    questPlayerSummaryData.AddBonusReward("Sleight of Hand", 1, 0, 0);
                }
            }
            else if (player.IsFightersGuild && Status == QuestStatus.Completed)
            {
                int playerPhysPower = questLanes[player.PlayerID.Value].TotalPhysicalPower.Value;
                if (playerPhysPower <= 0) continue; // No bonus if no physical power

                foreach (QuestLane lane in questLanes)
                {

                    if (lane.QuestDropZone.transform.childCount == 0) continue;
                    if (lane.Player.Value.PlayerID.Value == player.PlayerID.Value) continue;

                    if (lane.TotalPhysicalPower.Value >= playerPhysPower)
                    {
                        player.GuildBonusTracker[QuestLocationIndex]["mostPhysPower"] = 0;
                        break;
                    }
                }
                if (player.GuildBonusTracker[QuestLocationIndex]["mostPhysPower"] == 1)
                {
                    player.ChangePlayerReputation(1);
                    player.UpdateGuildRecapTracker("Path to Glory (Rep)", 1);
                    questPlayerSummaryData.AddBonusReward("Path to Glory", 0, 1, 0);
                }
            }
            else if (player.IsMerchantsGuild && Status == QuestStatus.Completed)
            {
                if (player.GuildBonusTracker[QuestLocationIndex]["magicItemsDispatched"] >= 2)
                {
                    player.ChangePlayerReputation(1);
                    player.UpdateGuildRecapTracker("Show of Wealth (Rep)", 1);
                    questPlayerSummaryData.AddBonusReward("Show of Wealth", 0, 1, 0);
                }
            }
            else if (player.IsAssassinsGuild)
            {
                if (player.GuildBonusTracker[QuestLocationIndex]["curseSpellsPlayed"] > 0)
                {
                    player.ChangePlayerReputation(1);
                    player.UpdateGuildRecapTracker("Whispered Influence (Rep)", 1);
                    questPlayerSummaryData.AddBonusReward("Whispered Influence", 0, 1, 0);
                }

                if (player.GuildBonusTracker[QuestLocationIndex]["poisonedAdventurers"] > 0)
                {
                    player.ChangePlayerGold(2);
                    player.UpdateGuildRecapTracker("Deadly Bounty (Gold)", 2);
                    questPlayerSummaryData.AddBonusReward("Deadly Bounty", 2, 0, 0);
                }
            }
        }

        foreach (Player player in GameManager.Instance.Players)
        {
            if (questSummaryData.PlayerQuestSummaries.ContainsKey(player.PlayerID.Value)) continue;
            
            if (player.IsAssassinsGuild && player.GuildBonusTracker[QuestLocationIndex]["curseSpellsPlayed"] > 0)
            {
                PlayerRoundSummaryData playerSummary = new($"Player {player.PlayerID.Value + 1}");
                playerSummary.UpdatePlayerSummary(0, 0, 0, 0, 0);
                questSummaryData.PlayerQuestSummaries[player.PlayerID.Value] = playerSummary;

                player.ChangePlayerReputation(1);
                player.UpdateGuildRecapTracker("Whispered Influence (Rep)", 1);
                playerSummary.AddBonusReward("Whispered Influence", 0, 1, 0);
            }
        }
    }

    [Server]
    private void DistributeBardBonus(QuestSummaryData questSummaryData)
    {
        int bardBonusGold;
        int bardBonusReputation;
        foreach (QuestLane lane in questLanes)
        {
            if (lane.BardBonus.Value > 0)
            {
                Player player = lane.Player.Value;
                (bardBonusGold,bardBonusReputation) = bardBonusMap[lane.BardBonus.Value];
                player.ChangePlayerGold(bardBonusGold);
                player.ChangePlayerReputation(bardBonusReputation);
                player.UpdateGuildRecapTracker("Bardsong (Gold)", bardBonusGold);
                player.UpdateGuildRecapTracker("Bardsong (Rep)", bardBonusReputation);
                questSummaryData.PlayerQuestSummaries[player.PlayerID.Value].AddBonusReward("Bardsong", bardBonusGold, bardBonusReputation, 0);
            }
        }
    }

    [Server]
    private void CalculateFailedQuestPenalty(QuestSummaryData questSummaryData)
    {
        int goldPenalty = QuestCard.Value.GoldPenalty.Value;
        int reputationPenalty = QuestCard.Value.ReputationPenalty.Value;
        int restPeriodPenalty = QuestCard.Value.RestPeriodPenalty.Value;
        Player player;

        foreach (QuestLane lane in questLanes)
        {
            if (lane.QuestDropZone.transform.childCount == 0) continue;

            player = lane.Player.Value;
            player.ChangePlayerGold(goldPenalty);
            player.ChangePlayerReputation(reputationPenalty);
            player.UpdateGuildRecapTracker("Quest Penalty (Gold)", goldPenalty);
            player.UpdateGuildRecapTracker("Quest Penalty (Rep)", reputationPenalty);
            player.UpdateGuildRecapTracker("Quests Failed", 1);

            foreach (Transform cardSlotTransform in lane.QuestDropZone.transform)
            {
                AdventurerCard adventurerCard = cardSlotTransform.GetChild(0).GetComponent<AdventurerCard>();
                adventurerCard.ChangeCurrentRestPeriod(restPeriodPenalty);
            }

            PlayerRoundSummaryData playerSummary = new($"Player {player.PlayerID.Value + 1}");
            playerSummary.UpdatePlayerSummary(goldPenalty, reputationPenalty, 0, lane.TotalPhysicalPower.Value, lane.TotalMagicalPower.Value);
            questSummaryData.PlayerQuestSummaries[player.PlayerID.Value] = playerSummary;
        }
    }

    [Server]
    public void ResetQuestLocation()
    {
        TotalPhysicalPower.Value = 0;
        TotalMagicalPower.Value = 0;

        for (int i = 0; i < questLanes.Length; i++)
        {
            questLanes[i].ResetQuestLane();
        }
    }

    [Server]
    public bool HasUnresolvedCards(int laneIndex)
    {
        List<AdventurerCard> cardList;

        cardList = CardsToResolvePerLane[laneIndex];
        if (cardList.Count > 0)
        {
            AdventurerCard card = cardList[0];
            cardList.RemoveAt(0); 
            ResolveCard(card, laneIndex);
            return true;
        }
        return false;
    }

    [Server]
    private void ResolveCard(AdventurerCard card, int laneIndex)
    {
        card.ControllingPlayer.Value.UpdateGuildRecapTracker($"{card.CardName.Value} Resolutions Possible", 1);
        if (!IsResolutionValid(card.CardName.Value, laneIndex))
        {
            GameManager.Instance.CheckForUnresolvedCards();
            return;
        }

        PopUpManager.Instance.CreateResolutionPopUp(card.Owner, card.CardName.Value, this);
        GameManager.Instance.SetPlayerTurn(card.ControllingPlayer.Value);
    }

    [Server]
    private bool IsResolutionValid(string cardName, int laneIndex)
    {   
        if (cardName == "Cleric") return true; // Cleric can always resolve

        foreach (QuestLane lane in questLanes)
        {
            if (lane == questLanes[laneIndex]) continue;    //skip resolution card's lane

            foreach (Transform cardSlotTransform in lane.QuestDropZone.transform)
            {
                AdventurerCard card = cardSlotTransform.GetChild(0).GetComponent<AdventurerCard>();

                if (cardName == "Rogue" && card.HasItem.Value) return true;
                else if (cardName == "Assassin" && !card.IsBlessed.Value) return true;
            }
        }
        return false;   //no valid targets found
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetAllowResolution(bool value)
    {
        AllowResolution.Value = value;
    }
}
