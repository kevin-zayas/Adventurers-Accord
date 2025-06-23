using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Hand : NetworkBehaviour
{
    public readonly SyncVar<Player> controllingPlayer = new();
    public readonly SyncVar<int> playerID = new();
    public int SlotIndex { get; private set; }

    [SerializeField] GameObject handSlotPrefab;

    [ObserversRpc]
    [TargetRpc]
    public void CreateHandSlot(NetworkConnection connection)
    {
        Instantiate(handSlotPrefab, transform);
        SlotIndex = transform.childCount - 1;
    }
}
