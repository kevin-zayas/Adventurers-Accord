using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
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
        int questLocationWidth = 10 + 200 * playerCount;

        for (int i = 0; i < questLanes.Length; i++)
        {
            questLanes[i].gameObject.SetActive(i < playerCount);

            if (LocalConnection.ClientId == i)
            {
                questLanes[i].DropZone.GetComponent<BoxCollider2D>().enabled = true;
                questLanes[i].DropZone.GetComponent<Image>().color = Color.white;
            }
        }

        RectTransform rectTransform = transform.GetChild(0).GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(questLocationWidth, rectTransform.sizeDelta.y);
    }

    [Server]
    public void AssignQuestCard(QuestCard questCard)
    {
        questCard.questCardIndex = 0;

        Spawn(questCard.gameObject);
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
            TotalPhysicalPower += questLanes[i].PhysicalPower;
            TotalMagicalPower += questLanes[i].MagicalPower;
        }

        if (TotalPhysicalPower >= QuestCard.PhysicalPower && TotalMagicalPower >= QuestCard.MagicalPower)
        {
            print("Quest Complete");
            print($"Physical Power: {TotalPhysicalPower} / {QuestCard.PhysicalPower}");
            print($"Magical Power: {TotalMagicalPower} / {QuestCard.MagicalPower}");
        }
    }
}
