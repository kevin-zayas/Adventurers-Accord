using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : NetworkBehaviour
{
    public static Board Instance { get; private set; }

    [field: SerializeField]
    public CardSlot[] cardSlots { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool[] availableCardSlots { get; private set; }

    [SerializeField]
    private Card cardPrefab;

    private void Awake()
    {
        Instance = this;
    }

    [Server]
    public void StartGame()
    {
        for (int i = 0; i < cardSlots.Length; i++)
        {
            Transform slot = cardSlots[i].transform;
            Card card = Instantiate(cardPrefab, Vector2.zero, Quaternion.identity);
            card.parent = slot;

            Spawn(card.gameObject);
            availableCardSlots[i] = false;
        }
    }
}
