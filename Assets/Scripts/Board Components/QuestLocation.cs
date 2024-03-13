using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestLocation : NetworkBehaviour
{
    [SerializeField]
    private QuestLane[] questLanes;

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


    [Server]
    public void StartGame()
    {
        ObserversInitializeQuestLocation();

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
    public void CalculatePowerTotal()
    {
        TotalPhysicalPower = 0;
        TotalMagicalPower = 0;

        for (int i = 0; i < questLanes.Length; i++)
        {
            TotalPhysicalPower += questLanes[i].PhysicalPower + questLanes[i].SpellPhysicalPower;
            TotalMagicalPower += questLanes[i].MagicalPower + questLanes[i].SpellMagicalPower;
        }

        if (TotalPhysicalPower >= QuestCard.PhysicalPower && TotalMagicalPower >= QuestCard.MagicalPower)
        {
            print("Quest Complete");
            print($"Physical Power: {TotalPhysicalPower} / {QuestCard.PhysicalPower}");
            print($"Magical Power: {TotalMagicalPower} / {QuestCard.MagicalPower}");
            CalculateQuestContributions();
            DistributeBardBonus();
        }
        else
        {
            print("Quest Incomplete");
            //TODO: Handle incomplete quest penalties
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
                DistributeQuestRewards(lane.Player, false);
            }
            return;
        }
        else if (primaryContributors.Count == 1)    //give all rewards to primary contributor, top secondary contributor(s) recieves half rewards
        {
            DistributeQuestRewards(primaryContributors[0].Player, true);

            if (secondaryContributors.Count == 0) return;
            int topSecondaryPower = secondaryContributors[0].EffectiveTotalPower;

            foreach (QuestLane lane in secondaryContributors)
            {
                if (lane.EffectiveTotalPower == topSecondaryPower)
                {
                    DistributeQuestRewards(lane.Player, false);
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
                    DistributeQuestRewards(lane.Player, true);
                }
                else if (secondPower != 0 && lane.EffectiveTotalPower == secondPower)
                {
                    DistributeQuestRewards(lane.Player, false);
                }
                else if (secondPower == 0)
                {
                    secondPower = lane.EffectiveTotalPower;
                    DistributeQuestRewards(lane.Player, false);
                }
                else break;
            }
            return;
        }
    }

    [Server]
    private void DistributeQuestRewards(Player player, bool primaryContributor)
    {
        if (primaryContributor)
        {
            player.ServerChangeGold(QuestCard.GoldReward);
            player.ServerChangeReputation(QuestCard.ReputationReward);
            Board.Instance.RewardLoot(player, QuestCard.LootReward);

            print($"Player {player.PlayerID} recieves {QuestCard.GoldReward} GP, {QuestCard.ReputationReward} Rep. and {QuestCard.LootReward} Loot for their contribution to the quest");
        }
        else
        {
            int secondaryGoldReward = QuestCard.GoldReward / 2;
            int secondaryReputationReward = QuestCard.ReputationReward / 2;

            player.ServerChangeGold(secondaryGoldReward);
            player.ServerChangeReputation(secondaryReputationReward);

            print($"Player {player.PlayerID} recieves {secondaryGoldReward} GP and {secondaryReputationReward} Rep. for their contribution to the quest");
        }
        
    }

    [Server]
    private void DistributeBardBonus()
    {
        foreach (QuestLane lane in questLanes)
        {
            if (lane.BardBonus > 0)
            {
                lane.Player.ServerChangeReputation(lane.BardBonus);
                print($"Player {lane.Player.PlayerID} recieves {lane.BardBonus} bonus Rep. for their Bard's contribution");
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
}
