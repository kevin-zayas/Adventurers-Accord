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

    public readonly SyncVar<QuestCard> QuestCard = new();

    public readonly SyncVar<int> TotalPhysicalPower = new();

    public readonly SyncVar<int> TotalMagicalPower = new();

    [field: SerializeField] public List<List<AdventurerCard>> CardsToResolvePerLane { get; private set; } = new List<List<AdventurerCard>>();

    public readonly SyncVar<bool> AllowResolution = new();

    private readonly Dictionary<int, Tuple<int,int>> bardBonusMap = new();

    [field: SerializeField] private TMP_Text totalPhysicalPowerText;
    [field: SerializeField] private TMP_Text totalMagicalPowerText;

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
        int questLocationWidth = 125 + 100 * playerCount;

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

        RectTransform rectTransform = transform.GetChild(0).GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(questLocationWidth, rectTransform.sizeDelta.y);
    }

    [Server]
    public void AssignQuestCard(QuestCard questCard)
    {
        
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
            TotalPhysicalPower.Value += questLanes[i].PhysicalPower.Value + questLanes[i].SpellPhysicalPower.Value;
            TotalMagicalPower.Value += questLanes[i].MagicalPower.Value + questLanes[i].SpellMagicalPower.Value;
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
            QuestSummary.ObserversSetQuestInfo(QuestCard.Value.CardName.Value, "Complete!", TotalPhysicalPower.Value, QuestCard.Value.PhysicalPower.Value, TotalMagicalPower.Value, QuestCard.Value.MagicalPower.Value);
            CalculateQuestContributions(true);
            DistributeBardBonus();

            ReplaceQuestCard();
        }
        else
        {
            foreach (QuestLane lane in questLanes)
            {
                if (lane.QuestDropZone.transform.childCount > 0)
                {
                    QuestSummary.ObserversSetQuestInfo(QuestCard.Value.CardName.Value, "Failed", TotalPhysicalPower.Value, QuestCard.Value.PhysicalPower.Value, TotalMagicalPower.Value, QuestCard.Value.MagicalPower.Value);
                    CalculateFailedQuestPenalty();
                    ReplaceQuestCard();

                    break;
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
        if (QuestCard.Value.PhysicalPower.Value > 0 && lane.PhysicalPower.Value + lane.SpellPhysicalPower.Value < QuestCard.Value.PhysicalPower.Value) return false;
        if (QuestCard.Value.MagicalPower.Value > 0 && lane.MagicalPower.Value + lane.SpellMagicalPower.Value < QuestCard.Value.MagicalPower.Value) return false;
 
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
        Player player = lane.Player;
        int goldReward;
        int reputationReward;
        int lootReward;

        if (primaryContributor)
        {
            goldReward = QuestCard.Value.GoldReward.Value;
            reputationReward = QuestCard.Value.ReputationReward.Value;
            lootReward = QuestCard.Value.LootReward.Value;
            //could set bool here to indicate player is primary contributor, which can be used to add a gold metal vs silver metal to the player's summary
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
        QuestSummary.ObserversSetPlayerSummary(player.PlayerID.Value, lane.PhysicalPower.Value + lane.SpellPhysicalPower.Value, lane.MagicalPower.Value + lane.SpellMagicalPower.Value, goldReward, reputationReward, lootReward);

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
                lane.Player.ChangePlayerGold(bardBonusGold);
                lane.Player.ChangePlayerReputation(bardBonusReputation);
                QuestSummary.ObserversAddBardBonus(lane.Player.PlayerID.Value, lane.PhysicalPower.Value + lane.SpellPhysicalPower.Value, lane.MagicalPower.Value + lane.SpellMagicalPower.Value, bardBonusGold, bardBonusReputation);
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
                lane.Player.ChangePlayerReputation(-adventurerCount);
                QuestSummary.ObserversSetPlayerSummary(lane.Player.PlayerID.Value, lane.PhysicalPower.Value + lane.SpellPhysicalPower.Value, lane.MagicalPower.Value + lane.SpellMagicalPower.Value, -adventurerCount);
                print($"Player {lane.Player.PlayerID.Value} loses {adventurerCount} Rep. for failing the quest");
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
