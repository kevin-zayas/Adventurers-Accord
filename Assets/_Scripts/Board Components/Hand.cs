using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Hand : CardHolder
{
    //public static Hand Instance { get; private set; }
    public readonly SyncVar<Player> controllingPlayer = new();
    public readonly SyncVar<int> playerID = new();

    protected override void Start()
    {
        base.Start();
        HolderType = CardHolderType.Hand;
    }

    protected void Update()
    {
        if (selectedCard == null)
            return;
        if (isCrossing)
            return;

        SwapCheck();
    }

    public void SwapCheck()
    {
        int selectedIndex = cardList.IndexOf(selectedCard);
        float selectedX = selectedCard.transform.position.x;
        int targetIndex = -1;

        if (selectedIndex > 0 && selectedX < cardList[selectedIndex - 1].transform.position.x)
            targetIndex = selectedIndex - 1;
        else if (selectedIndex < cardList.Count - 1 && selectedX > cardList[selectedIndex + 1].transform.position.x)
            targetIndex = selectedIndex + 1;

        if (targetIndex != -1)
        {
            isCrossing = true;
            ServerSwapCards(selectedIndex, targetIndex);
            PerformSwap(selectedIndex, targetIndex);
        }
    }

    [ServerRpc]
    private void ServerSwapCards(int selectedIndex, int targetIndex)
    {
        if (!IsServerInitialized) return;
        ObserversSwapCards(selectedIndex, targetIndex);
    }

    [ObserversRpc]
    private void ObserversSwapCards(int selectedIndex, int targetIndex)
    {
        if (IsOwner) return;
        PerformSwap(selectedIndex, targetIndex);
    }

    private void PerformSwap(int selectedIndex, int targetIndex)
    {
        Card selectedCard = cardList[selectedIndex];
        Card targetCard = cardList[targetIndex];

        cardList[selectedIndex] = targetCard;
        cardList[targetIndex] = selectedCard;

        Transform selectedSlot = selectedCard.transform.parent;     // instead of accessing transfom, could maybe get index of carslot
        Transform targetSlot = targetCard.transform.parent;         // this could allow card preview slot dragging without setparent issue

        selectedCard.transform.SetParent(targetSlot);
        targetCard.transform.SetParent(selectedSlot);

        if (IsOwner)
        {
            int dir = targetIndex > selectedIndex ? 1 : -1;
            targetCard.CardHandler.PlaySwapTween(dir);
            isCrossing = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerCreatePreviewSlot(Card card)
    {
        if (cardList.Contains(card)) return;

        if (previewCardSlot != null)
        {
            Debug.LogWarning("Preview card slot already exists. Removing the old one.");
            Despawn(previewCardSlot);
            previewCardSlot = null;
        }
        previewCardSlot = Instantiate(cardSlotPrefab);
        Spawn(previewCardSlot);
        previewCardSlot.transform.SetParent(transform);
        ObserversSetCardSlotParent(previewCardSlot);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerRemovePreviewSlot()
    {
        if (previewCardSlot != null)
        {
            Despawn(previewCardSlot);
            previewCardSlot = null;
        }
    }
}
