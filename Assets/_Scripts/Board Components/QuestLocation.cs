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
    public void HandleEndOfQuest(Dictionary<int,PlayerRoundSummaryData> playerSummaries, List<QuestSummaryData> questSummaries)
    {
        UpdateTotalPower();
        QuestSummaryData questSummaryData = new(this);
        questSummaries.Add(questSummaryData);

        if (!HasDispatchedAdventurers()) return;

        if (AreQuestReqsMet())
        {
            Status = QuestStatus.Completed;
            CalculateQuestContributions(true,playerSummaries,questSummaryData);
            DistributeBardBonus(playerSummaries, questSummaryData);
        }
        else
        {
            
            Status = QuestStatus.Failed;
            CalculateFailedQuestPenalty(playerSummaries, questSummaryData);
        }

        questSummaryData.Status = Status;
        CheckGuildBonus(playerSummaries, questSummaryData);
        ReplaceQuestCard();
        return;
    }

    [Server]
    public void CalculateQuestContributions(bool isRoundOver, Dictionary<int,PlayerRoundSummaryData> playerSummaries = null, QuestSummaryData questSummaryData = null)
    {
        foreach (QuestLane lane in questLanes)
        {
            if (DoesMeetFullReqs(lane))
            {
                if (isRoundOver) DistributeQuestRewards(lane, true, playerSummaries, questSummaryData);
                lane.ObserversUpdateRewardIndicator("gold");
            }
            else if (AreQuestReqsMet() && DoesMeetHalflReqs(lane))
            {
                if (isRoundOver) DistributeQuestRewards(lane, false, playerSummaries, questSummaryData);
                lane.ObserversUpdateRewardIndicator("silver");
            }
            else
            {
                lane.ObserversUpdateRewardIndicator("blank");
                if (isRoundOver && lane.EffectiveTotalPower.Value > 0)
                {
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
    private void DistributeQuestRewards(QuestLane lane, bool fullRewards, Dictionary<int, PlayerRoundSummaryData> playerSummaries, QuestSummaryData questSummaryData)
    {
        Player player = lane.Player.Value;

        (int goldReward, int reputationReward, int lootReward) = fullRewards
            ? (QuestCard.Value.GoldReward.Value, QuestCard.Value.ReputationReward.Value, QuestCard.Value.LootReward.Value)
            : (QuestCard.Value.GoldReward.Value / 2, QuestCard.Value.ReputationReward.Value / 2, (int)Mathf.Floor(QuestCard.Value.LootReward.Value / 2));
        
        player.ChangePlayerGold(goldReward);
        player.ChangePlayerReputation(reputationReward);
        Board.Instance.RewardLoot(player, lootReward);

        print($"Player {player.PlayerID.Value} recieves {goldReward} GP, {reputationReward} Rep. and {lootReward} Loot for their contribution to the quest");

        PlayerRoundSummaryData playerSummary = new($"Player {player.PlayerID.Value + 1}");
        playerSummary.UpdatePlayerSummary(goldReward, reputationReward, lootReward, lane.TotalPhysicalPower.Value, lane.TotalMagicalPower.Value);
        questSummaryData.PlayerQuestSummaries[player.PlayerID.Value] = playerSummary;

        playerSummaries[player.PlayerID.Value].UpdatePlayerSummary(goldReward, reputationReward, lootReward);

    }

    [Server]
    private void CheckGuildBonus(Dictionary<int, PlayerRoundSummaryData> playerSummaries, QuestSummaryData questSummaryData)
    {
        if (Status == QuestStatus.Unchallenged) return;

        //foreach (Player player in GameManager.Instance.Players)
        foreach (int playerID in questSummaryData.PlayerQuestSummaries.Keys)
        {
            Player player = GameManager.Instance.Players[playerID];
            PlayerRoundSummaryData playerSummaryData = playerSummaries[player.PlayerID.Value];
            PlayerRoundSummaryData questPlayerSummaryData = questSummaryData.PlayerQuestSummaries[playerID];

            if (player.isThievesGuild)
            {
                if (Status == QuestStatus.Completed)
                {
                    player.ChangePlayerGold(1);
                    playerSummaryData.AddBonusReward("First to the Spoils", 1, 0, 0);
                    questPlayerSummaryData.AddBonusReward("First to the Spoils", 1, 0, 0);
                    print($"Thieves Guild Bonus - Player {player.PlayerID.Value} +1 GP - Quest Complete");

                    if (UnityEngine.Random.Range(1, 5) == 1) // 25% chance
                    {
                        Board.Instance.RewardLoot(player, 1);
                        playerSummaryData.AddBonusReward("First to the Spoils", 0, 0, 1);
                        questPlayerSummaryData.AddBonusReward("First to the Spoils", 0, 0, 1);
                        print($"Thieves Guild Bonus - Player {player.PlayerID.Value} +1 Loot - Quest Complete");
                    }
                       
                }
                if (player.GuildBonusTracker[QuestLocationIndex]["stolenItems"] > 0)
                {
                    player.ChangePlayerGold(1);
                    playerSummaryData.AddBonusReward("Sleight of Hand", 1, 0, 0);
                    questPlayerSummaryData.AddBonusReward("Sleight of Hand", 1, 0, 0);
                    print($"Thieves Guild Bonus - Player {player.PlayerID.Value} +1 GP - Stolen Item Count: {player.GuildBonusTracker[QuestLocationIndex]["stolenItems"]}");
                }
            }
            else if (player.isFightersGuild && Status == QuestStatus.Completed)
            {
                int playerPhysPower = questLanes[player.PlayerID.Value].TotalPhysicalPower.Value;

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
                    playerSummaryData.AddBonusReward("Path to Glory", 0, 1, 0);
                    questPlayerSummaryData.AddBonusReward("Path to Glory", 0, 1, 0);
                    print($"Fighters Guild Bonus - Player {player.PlayerID.Value} +1 Rep. - Most Physical Power");
                }
            }
            else if (player.isMerchantsGuild && Status == QuestStatus.Completed)
            {
                if (player.GuildBonusTracker[QuestLocationIndex]["magicItemsDispatched"] >= 2)
                {
                    player.ChangePlayerReputation(1);
                    playerSummaryData.AddBonusReward("Show of Wealth", 0, 1, 0);
                    questPlayerSummaryData.AddBonusReward("Show of Wealth", 0, 1, 0);
                    print($"Merchants Guild Bonus - Player {player.PlayerID.Value} +1 Rep. - Magic Item Count: {player.GuildBonusTracker[QuestLocationIndex]["magicItemsDispatched"]}");
                }
            }
            else if (player.isAssassinsGuild)
            {
                if (player.GuildBonusTracker[QuestLocationIndex]["curseSpellsPlayed"] > 0)
                {
                    player.ChangePlayerReputation(1);
                    playerSummaryData.AddBonusReward("Whispered Influence", 0, 1, 0);
                    questPlayerSummaryData.AddBonusReward("Whispered Influence", 0, 1, 0);
                    print($"Assassins Guild Bonus - Player {player.PlayerID.Value} +1 Rep. - Curse Spell Count: {player.GuildBonusTracker[QuestLocationIndex]["curseSpellsPlayed"]}");
                }
                if (player.GuildBonusTracker[QuestLocationIndex]["poisonedAdventurers"] > 0)
                {
                    player.ChangePlayerGold(2);
                    playerSummaryData.AddBonusReward("Deadly Bounty", 2, 0, 0);
                    questPlayerSummaryData.AddBonusReward("Deadly Bounty", 2, 0, 0);
                    print($"Assassins Guild Bonus - Player {player.PlayerID.Value} +2 GP. - Posoned Adventurer Count: {player.GuildBonusTracker[QuestLocationIndex]["poisonedAdventurers"]}");
                }
            }
        }
    }

    [Server]
    private void DistributeBardBonus(Dictionary<int, PlayerRoundSummaryData> playerSummaries, QuestSummaryData questSummaryData)
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
                playerSummaries[player.PlayerID.Value].AddBonusReward("Bardsong", bardBonusGold, bardBonusReputation, 0);
                questSummaryData.PlayerQuestSummaries[player.PlayerID.Value].AddBonusReward("Bardsong", bardBonusGold, bardBonusReputation, 0);
            }
        }
    }

    [Server]
    private void CalculateFailedQuestPenalty(Dictionary<int, PlayerRoundSummaryData> playerSummaries, QuestSummaryData questSummaryData)
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

            foreach (Transform cardTransform in lane.QuestDropZone.transform)
            {
                AdventurerCard adventurerCard = cardTransform.GetComponent<AdventurerCard>();
                adventurerCard.ChangeCurrentRestPeriod(restPeriodPenalty);
                print($"Player {player.PlayerID.Value +1} - Adventurer {adventurerCard.CardName.Value} - Increase Rest Period by {restPeriodPenalty}");
            }

            print($"Player {player.PlayerID.Value} loses {reputationPenalty} Rep. for failing the quest");
            PlayerRoundSummaryData playerSummary = new($"Player {player.PlayerID.Value + 1}");
            playerSummary.UpdatePlayerSummary(goldPenalty, reputationPenalty, 0, lane.TotalPhysicalPower.Value, lane.TotalMagicalPower.Value);
            questSummaryData.PlayerQuestSummaries[player.PlayerID.Value] = playerSummary;

            playerSummaries[player.PlayerID.Value].UpdatePlayerSummary(goldPenalty, reputationPenalty, 0);
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
        print("Resolving card: " + card.CardName.Value);
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
        if (cardName == "Rogue")
        {
            foreach (QuestLane lane in questLanes)
            {
                if (lane == questLanes[laneIndex]) continue;    //skip resolution card's lane

                foreach (Transform cardTransform in lane.QuestDropZone.transform)
                {
                    AdventurerCard card = cardTransform.GetComponent<AdventurerCard>();

                    if (card.HasItem.Value)
                    {
                        return true;
                    }
                }
            }
            return false;   //no items found in any lanes       ----TODO: Fix server issue when this is false
        }
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetAllowResolution(bool value)
    {
        AllowResolution.Value = value;
    }
}
