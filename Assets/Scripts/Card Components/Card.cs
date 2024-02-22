using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Card : NetworkBehaviour
{
    [SyncVar]
    public Player controllingPlayer;

    [SyncVar(OnChange = nameof(CardParentChanged))]
    public Transform parent;

    private void CardParentChanged(Transform oldValue, Transform newValue, bool asServer)
    {
        //print($"Old parent is {oldValue}");
        //print($"New parent is {newValue}");
        //print($"Is server? {asServer}");
        if (asServer) return;

        if (newValue == null) print("Card parent is null");

        this.transform.SetParent(newValue, false);
    }

    
    
}
