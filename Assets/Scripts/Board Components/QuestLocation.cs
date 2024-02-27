using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestLocation : NetworkBehaviour
{
    [SerializeField]
    private QuestLane[] questLanes;

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
}
