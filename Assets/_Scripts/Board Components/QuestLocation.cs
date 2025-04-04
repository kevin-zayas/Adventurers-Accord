using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestLocation : NetworkBehaviour
{
    [SerializeField] private QuestLane[] questLanes;

    public enum QuestStatus { Default, Complete, Failed }

    [field: SerializeField] public QuestStatus Status { get; private set; }

    public QuestSummary QuestSummary { get; private set; }

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

        Status = QuestStatus.Default;

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
        Status = QuestStatus.Default;
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
    public bool MeetsQuestRequirements()
    {
        // Check if the Physical Power requirement is met or if no Physical Power is required
        bool physicalRequirementMet = (QuestCard.Value.PhysicalPower.Value == 0 || TotalPhysicalPower.Value >= QuestCard.Value.PhysicalPower.Value);

        // Check if the Magical Power requirement is met or if no Magical Power is required
        bool magicalRequirementMet = (QuestCard.Value.MagicalPower.Value == 0 || TotalMagicalPower.Value >= QuestCard.Value.MagicalPower.Value);

        return physicalRequirementMet && magicalRequirementMet; ;

    }

    [Server]
    public void HandleEndOfQuest(QuestSummary questSummary)
    {
        QuestSummary = questSummary;

        UpdateTotalPower();

        if (MeetsQuestRequirements())
        {
            Status = QuestStatus.Complete;
            QuestSummary.ObserversSetQuestInfo(QuestCard.Value.CardName.Value, "Complete!", TotalPhysicalPower.Value, QuestCard.Value.PhysicalPower.Value, TotalMagicalPower.Value, QuestCard.Value.MagicalPower.Value);
            CalculateQuestContributions(true);
            CheckGuildBonus();
            DistributeBardBonus();
            
            ReplaceQuestCard();
        }
        else
        {
            CheckGuildBonus();
            foreach (QuestLane lane in questLanes)
            {
                if (lane.QuestDropZone.transform.childCount > 0)
                {
                    QuestSummary.ObserversSetQuestInfo(QuestCard.Value.CardName.Value, "Failed", TotalPhysicalPower.Value, QuestCard.Value.PhysicalPower.Value, TotalMagicalPower.Value, QuestCard.Value.MagicalPower.Value);
                    CalculateFailedQuestPenalty();
                    ReplaceQuestCard();

                    return;
                }
            }

            QuestSummary.ObserversSetQuestInfo(QuestCard.Value.CardName.Value, "Unchallenged", TotalPhysicalPower.Value, QuestCard.Value.PhysicalPower.Value, TotalMagicalPower.Value, QuestCard.Value.MagicalPower.Value);
            return;

        }
    }

    [Server]
    public void CalculateQuestContributions(bool isQuestComplete)
    {
        List<QuestLane> laneList = new(questLanes);
        laneList.Sort((a, b) => b.EffectiveTotalPower.Value.CompareTo(a.EffectiveTotalPower.Value));

        List<QuestLane> primaryContributors = new();
        List<QuestLane> secondaryContributors = new();

        foreach (QuestLane lane in laneList)
        {
            if (CheckPrimaryContributor(lane))
            {
                primaryContributors.Add(lane);                                      //primary contributors are those who meet or exceed the quest requirements
            }
            else
            {
                if (lane.EffectiveTotalPower.Value > 0) secondaryContributors.Add(lane);
            }
        }
        CalculateQuestRwards(primaryContributors, secondaryContributors, isQuestComplete);
    }

    [Server]
    private bool CheckPrimaryContributor(QuestLane lane)
    {
        if (QuestCard.Value.PhysicalPower.Value > 0 && lane.TotalPhysicalPower.Value < QuestCard.Value.PhysicalPower.Value) return false;
        if (QuestCard.Value.MagicalPower.Value > 0 && lane.TotalMagicalPower.Value < QuestCard.Value.MagicalPower.Value) return false;
 
        return true;
    }

    [Server]
    private void CalculateQuestRwards(List<QuestLane> primaryContributors, List<QuestLane> secondaryContributors, bool isQuestComplete)
    {
        if (primaryContributors.Count == 0)         //all secondary contributors recieve half rewards
        {
            foreach (QuestLane lane in secondaryContributors)
            {
                if (isQuestComplete) DistributeQuestRewards(lane, false);
                else
                {
                    if (MeetsQuestRequirements()) lane.ObserversUpdateRewardIndicator("silver");
                    else lane.ObserversUpdateRewardIndicator("blank");
                }
            }
            return;
        }
        else                                      //give all rewards to primary contributors, top secondary contributor(s) recieves half rewards
        {
            foreach (QuestLane lane in primaryContributors)
            {
                if (isQuestComplete) DistributeQuestRewards(lane, true);
                else lane.ObserversUpdateRewardIndicator("gold");
            }

            if (secondaryContributors.Count == 0) return;
            int topSecondaryPower = secondaryContributors[0].EffectiveTotalPower.Value;

            foreach (QuestLane lane in secondaryContributors)
            {
                if (lane.EffectiveTotalPower.Value == topSecondaryPower)
                {
                    if (isQuestComplete) DistributeQuestRewards(lane, false);
                    else lane.ObserversUpdateRewardIndicator("silver");
                }
                else
                {
                    lane.ObserversUpdateRewardIndicator("blank");
                }
            }
            return;
        }
    }

    [Server]
    private void DistributeQuestRewards(QuestLane lane, bool primaryContributor)
    {
        Player player = lane.Player.Value;
        int goldReward;
        int reputationReward;
        int lootReward;

        if (primaryContributor)
        {
            goldReward = QuestCard.Value.GoldReward.Value;
            reputationReward = QuestCard.Value.ReputationReward.Value;
            lootReward = QuestCard.Value.LootReward.Value;
        }
        else
        {
            goldReward = QuestCard.Value.GoldReward.Value / 2;
            reputationReward = QuestCard.Value.ReputationReward.Value / 2;
            lootReward = (int)Mathf.Floor(QuestCard.Value.LootReward.Value / 2);
        }

        player.ChangePlayerGold(goldReward);
        player.ChangePlayerReputation(reputationReward);
        Board.Instance.RewardLoot(player, lootReward);

        print($"Player {player.PlayerID.Value} recieves {goldReward} GP, {reputationReward} Rep. and {lootReward} Loot for their contribution to the quest");
        QuestSummary.ObserversSetPlayerSummary(player.PlayerID.Value, lane.TotalPhysicalPower.Value, lane.TotalMagicalPower.Value, goldReward, reputationReward, lootReward);

    }

    [Server]
    private void CheckGuildBonus()
    {
        foreach (Player player in GameManager.Instance.Players)
        {
            if (player.isThievesGuild)
            {
                if (Status == QuestStatus.Complete)
                {
                    player.ChangePlayerGold(1);
                    print($"Thieves Guild Bonus - Player {player.PlayerID.Value} +1 GP - Quest Complete");
                }
                if (player.GuildBonusTracker[QuestLocationIndex]["stolenItems"] > 0)
                {
                    player.ChangePlayerGold(1);
                    print($"Thieves Guild Bonus - Player {player.PlayerID.Value} +1 GP - Stolen Item Count: {player.GuildBonusTracker[QuestLocationIndex]["stolenItems"]}");
                }
            }
            else if (player.isFightersGuild && Status == QuestStatus.Complete)
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
                    print($"Fighters Guild Bonus - Player {player.PlayerID.Value} +1 Rep. - Most Physical Power");
                }
            }
            else if (player.isMerchantsGuild && Status == QuestStatus.Complete)
            {
                if (player.GuildBonusTracker[QuestLocationIndex]["magicItemsDispatched"] >= 2)
                {
                    player.ChangePlayerReputation(1);
                    print($"Merchants Guild Bonus - Player {player.PlayerID.Value} +1 Rep. - Magic Item Count: {player.GuildBonusTracker[QuestLocationIndex]["magicItemsDispatched"]}");
                }
            }
            else if (player.isAssassinsGuild)
            {
                if (player.GuildBonusTracker[QuestLocationIndex]["curseSpellsPlayed"] > 0)
                {
                    player.ChangePlayerReputation(1);
                    print($"Assassins Guild Bonus - Player {player.PlayerID.Value} +1 Rep. - Curse Spell Count: {player.GuildBonusTracker[QuestLocationIndex]["curseSpellsPlayed"]}");
                }
                if (player.GuildBonusTracker[QuestLocationIndex]["poisonedAdventurers"] > 0)
                {
                    player.ChangePlayerGold(2);
                    print($"Assassins Guild Bonus - Player {player.PlayerID.Value} +2 GP. - Posoned Adventurer Count: {player.GuildBonusTracker[QuestLocationIndex]["poisonedAdventurers"]}");
                }
            }
        }
    }

    [Server]
    private void DistributeBardBonus()
    {
        int bardBonusGold;
        int bardBonusReputation;
        foreach (QuestLane lane in questLanes)
        {
            if (lane.BardBonus.Value > 0)
            {
                (bardBonusGold,bardBonusReputation) = bardBonusMap[lane.BardBonus.Value];
                lane.Player.Value.ChangePlayerGold(bardBonusGold);
                lane.Player.Value.ChangePlayerReputation(bardBonusReputation);
                QuestSummary.ObserversAddBardBonus(lane.Player.Value.PlayerID.Value, lane.TotalPhysicalPower.Value, lane.TotalMagicalPower.Value, bardBonusGold, bardBonusReputation);
            }
        }
    }

    [Server]
    private void CalculateFailedQuestPenalty()
    {
        foreach (QuestLane lane in questLanes)
        {
            int adventurerCount = lane.QuestDropZone.transform.childCount;
            if (adventurerCount > 0)
            {
                lane.Player.Value.ChangePlayerReputation(-adventurerCount);
                QuestSummary.ObserversSetPlayerSummary(lane.Player.Value.PlayerID.Value, lane.TotalPhysicalPower.Value, lane.TotalMagicalPower.Value, -adventurerCount);
                print($"Player {lane.Player.Value.PlayerID.Value} loses {adventurerCount} Rep. for failing the quest");
            }
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
            ResolveCard(cardList[0]);
            cardList.RemoveAt(0);
            return true;
        }
        
        return false;
    }

    [Server]
    private void ResolveCard(AdventurerCard card)
    {
        print("Resolving card: " + card.CardName.Value);
        if (!CheckResolutionValid(card))
        {
            GameManager.Instance.ServerCheckForUnresolvedCards();
            return;
        }

        ResolutionPopUp popUp = PopUpManager.Instance.CreateResolutionPopUp();

        Spawn(popUp.gameObject);
        GameManager.Instance.SetPlayerTurn(card.ControllingPlayer.Value);
        TargetResolveCard(card.Owner, popUp, card.CardName.Value);
        
    }

    [Server]
    private bool CheckResolutionValid(AdventurerCard card)
    {   
        if (card.CardName.Value == "Rogue")
        {
            //check for magic items in Quest Location
            foreach (QuestLane lane in questLanes)
            {
                foreach (Transform cardTransform in lane.QuestDropZone.transform)
                {
                    AdventurerCard childCard = cardTransform.GetComponent<AdventurerCard>();

                    if (childCard == card) continue;

                    if (childCard.HasItem.Value)
                    {
                        return true;
                    }
                }
            }
            return false;   //no items found in any lanes
        }
        return true;
    }

    [TargetRpc]
    public void TargetResolveCard(NetworkConnection networkConnection, ResolutionPopUp popUp, string cardName)
    {
        print("Sending popup to local client");
        popUp.InitializePopUp(this, cardName);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetAllowResolution(bool value)
    {
        AllowResolution.Value = value;
    }
}
