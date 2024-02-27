using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestLocation : NetworkBehaviour
{
    [SerializeField]
    private GameObject[] dropZones;

    [Server]
    public void StartGame()
    {
        ObserversInitializeQuestLocation();
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversInitializeQuestLocation()
    {
        int playerCount = GameManager.Instance.Players.Count;

        for (int i = 0; i < dropZones.Length; i++)
        {
            dropZones[i].SetActive(i < playerCount);
        }

        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(10+ 200 * playerCount, rectTransform.sizeDelta.y);


    }
}
