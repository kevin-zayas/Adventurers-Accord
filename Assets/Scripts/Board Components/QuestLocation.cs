using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QuestLocation : NetworkBehaviour
{
    [SerializeField]
    private QuestLane[] questLanes;

    public enum QuestStatus { Default, Complete, Failed }

    [field: SerializeField]
    public QuestStatus Status { get; private set; }

    [field: SerializeField]
    public QuestSummary QuestSummary { get; private set; }

    [field: SerializeField]
    private CardSlot questCardSlot;

    [field: SerializeField]
    public QuestCard QuestCard { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int TotalPhysicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int TotalMagicalPower { get; private set; }

    [field: SerializeField]
    public List<List<Card>> CardsToResolvePerLane { get; private set; } = new List<List<Card>>();

    [field: SerializeField]
    [field: SyncVar]
    public bool AllowResolution { get; private set; }

    [Server]
    public void StartGame()
    {
        ObserversInitializeQuestLocation();

        Status = QuestStatus.Default;

        foreach (Player player in GameManager.Instance.Players)
        {
            CardsToResolvePerLane.Add(new List<Card>());
        }

    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversInitializeQuestLocation()
    {
        int playerCount = GameManager.Instance.Players.Count;
        int questLocationWidth = 25 + 200 * playerCount;

        for (int i = 0; i < questLanes.Length; i++)
        {
            questLanes[i].gameObject.SetActive(i < playerCount);

            if (LocalConnection.ClientId == i)
            {
                questLanes[i].DropZone.GetComponent<BoxCollider2D>().enabled = true;
                questLanes[i].DropZone.GetComponent<Image>().color = Color.white;

                questLanes[i].ServerSetQuestLanePlayer(GameManager.Instance.Players[i]);
            }
        }

        RectTransform rectTransform = transform.GetChild(0).GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(questLocationWidth, rectTransform.sizeDelta.y);
    }

    [Server]
    public void AssignQuestCard(QuestCard questCard)
    {
        questCard.questCardIndex = 0;

        //Spawn(questCard.gameObject);
        questCard.SetCardParent(questCardSlot.transform, false);
        QuestCard = questCard;
    }

    [Server]
    public void CheckQuestCompletion(QuestSummary questSummary)
    {
        QuestSummary = questSummary;
        TotalPhysicalPower = 0;
        TotalMagicalPower = 0;
        bool adventurersPresent = false;

        for (int i = 0; i < questLanes.Length; i++)
        {
            TotalPhysicalPower += questLanes[i].PhysicalPower + questLanes[i].SpellPhysicalPower;
            TotalMagicalPower += questLanes[i].MagicalPower + questLanes[i].SpellMagicalPower;
        }

        if (TotalPhysicalPower >= QuestCard.PhysicalPower && TotalMagicalPower >= QuestCard.MagicalPower)
        {
            //print("Quest Complete");
            //print($"Physical Power: {TotalPhysicalPower} / {QuestCard.PhysicalPower}");
            //print($"Magical Power: {TotalMagicalPower} / {QuestCard.MagicalPower}");

            //Status = QuestStatus.Complete;
            QuestSummary.ObserversSetQuestInfo(QuestCard.Name, "Complete!", TotalPhysicalPower, QuestCard.PhysicalPower, TotalMagicalPower, QuestCard.MagicalPower);
            CalculateQuestContributions();
            DistributeBardBonus();
        }
        else
        {
            foreach (QuestLane lane in questLanes)
            {
                if (lane.DropZone.transform.childCount > 0)
                {
                    adventurersPresent = true;
                    break;
                }
            }

            if (adventurersPresent)
            {
                //print("Quest Failed");
                QuestSummary.ObserversSetQuestInfo(QuestCard.Name, "Failed", TotalPhysicalPower, QuestCard.PhysicalPower, TotalMagicalPower, QuestCard.MagicalPower);
                CalculateFailedQuestPenalty();
                return;
            }

            QuestSummary.ObserversSetQuestInfo(QuestCard.Name, "Unchallenged", TotalPhysicalPower, QuestCard.PhysicalPower, TotalMagicalPower, QuestCard.MagicalPower);
            return;

        }
    }

    [Server]
    private void CalculateQuestContributions()
    {
        List<QuestLane> laneList = new(questLanes);
        laneList.Sort((a, b) => b.EffectiveTotalPower.CompareTo(a.EffectiveTotalPower));

        List<QuestLane> primaryContributors = new();
        List<QuestLane> secondaryContributors = new();

        foreach (QuestLane lane in laneList)
        {
            if (lane.MagicalPower + lane.SpellMagicalPower >= QuestCard.MagicalPower && lane.PhysicalPower + lane.SpellPhysicalPower >= QuestCard.PhysicalPower)
            {
                primaryContributors.Add(lane);                                      //primary contributors are those who meet or exceed the quest requirements
            }
            else
            {
                if (lane.EffectiveTotalPower > 0) secondaryContributors.Add(lane);
            }
        }
        CalculateQuestRwards(primaryContributors, secondaryContributors);
    }

    [Server]
    private void CalculateQuestRwards(List<QuestLane> primaryContributors, List<QuestLane> secondaryContributors)
    {
        if (primaryContributors.Count == 0)         //all secondary contributors recieve half rewards
        {
            foreach (QuestLane lane in secondaryContributors)
            {
                DistributeQuestRewards(lane, false);
            }
            return;
        }
        else if (primaryContributors.Count == 1)    //give all rewards to primary contributor, top secondary contributor(s) recieves half rewards
        {
            DistributeQuestRewards(primaryContributors[0], true);

            if (secondaryContributors.Count == 0) return;
            int topSecondaryPower = secondaryContributors[0].EffectiveTotalPower;

            foreach (QuestLane lane in secondaryContributors)
            {
                if (lane.EffectiveTotalPower == topSecondaryPower)
                {
                    DistributeQuestRewards(lane, false);
                }
                else break;
            }
            return;
        }
        else if (primaryContributors.Count > 1)         //top primary contributors recieve full rewards, next highest primary contributor(s) recieves half rewards
        {
            int topPower = primaryContributors[0].EffectiveTotalPower;
            int secondPower = 0;

            foreach (QuestLane lane in primaryContributors)
            {
                if (lane.EffectiveTotalPower == topPower)
                {
                    DistributeQuestRewards(lane, true);
                }
                else if (secondPower != 0 && lane.EffectiveTotalPower == secondPower)
                {
                    DistributeQuestRewards(lane, false);
                }
                else if (secondPower == 0)
                {
                    secondPower = lane.EffectiveTotalPower;
                    DistributeQuestRewards(lane, false);
                }
                else break;
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
            goldReward = QuestCard.GoldReward;
            reputationReward = QuestCard.ReputationReward;
            lootReward = QuestCard.LootReward;
            //could set bool here to indicate player is primary contributor, which can be used to add a gold metal vs silver metal to the player's summary
        }
        else
        {
            goldReward = QuestCard.GoldReward / 2;
            reputationReward = QuestCard.ReputationReward / 2;
            lootReward = 0;
        }

        player.ChangePlayerGold(goldReward);
        player.ChangePlayerReputation(reputationReward);
        Board.Instance.RewardLoot(player, lootReward);

        print($"Player {player.PlayerID} recieves {goldReward} GP, {reputationReward} Rep. and {lootReward} Loot for their contribution to the quest");
        QuestSummary.ObserversSetPlayerSummary(player.PlayerID, lane.PhysicalPower + lane.SpellPhysicalPower, lane.MagicalPower + lane.SpellMagicalPower, goldReward, reputationReward, lootReward);

    }

    [Server]
    private void DistributeBardBonus()
    {
        foreach (QuestLane lane in questLanes)
        {
            if (lane.BardBonus > 0)
            {
                lane.Player.ChangePlayerReputation(lane.BardBonus);
                lane.Player.ChangePlayerGold(lane.BardBonus);
                QuestSummary.ObserversAddBardBonus(lane.Player.PlayerID, lane.PhysicalPower + lane.SpellPhysicalPower, lane.MagicalPower + lane.SpellMagicalPower, lane.BardBonus);
            }
        }
    }

    [Server]
    private void CalculateFailedQuestPenalty()
    {
        foreach (QuestLane lane in questLanes)
        {
            int adventurerCount = lane.DropZone.transform.childCount;
            if (adventurerCount > 0)
            {
                lane.Player.ChangePlayerReputation(-adventurerCount);
                QuestSummary.ObserversSetPlayerSummary(lane.Player.PlayerID, lane.PhysicalPower + lane.SpellPhysicalPower, lane.MagicalPower + lane.SpellMagicalPower, -adventurerCount);
                print($"Player {lane.Player.PlayerID} loses {adventurerCount} Rep. for failing the quest");
            }
        }
    }

    [Server]
    public void ResetQuestLocation()
    {
        TotalPhysicalPower = 0;
        TotalMagicalPower = 0;

        for (int i = 0; i < questLanes.Length; i++)
        {
            questLanes[i].ResetQuestLane();
        }
    }

    [Server]
    public bool HasUnresolvedCards(int laneIndex)
    {
        List<Card> cardList;

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
    private void ResolveCard(Card card)
    {
        print("Resolving card: " + card.Name);
        ResolutionPopUp popUp = PopUpManager.Instance.CreateResolutionPopUp();

        Spawn(popUp.gameObject);
        GameManager.Instance.SetPlayerTurn(card.ControllingPlayer);
        TargetResolveCard(card.Owner, popUp, card.Name);
        
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
        AllowResolution = value;
    }
}
