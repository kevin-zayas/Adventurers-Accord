using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
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
        if (isSwapping)
            return;

        if (Time.time - swapTimer > swapDelay) SwapCheck();
    }

    public void SwapCheck()
    {
        isSwapping = true;
        int currentIndex = cardList.IndexOf(selectedCard);
        float currentX = selectedCard.transform.position.x;
        int targetIndex = -1;

        if (currentIndex > 0 && currentX < cardList[currentIndex - 1].transform.position.x)     //maybe use parent transform position instead of card position?
            targetIndex = currentIndex - 1;
        else if (currentIndex < cardList.Count - 1 && currentX > cardList[currentIndex + 1].transform.position.x)
            targetIndex = currentIndex + 1;

        if (targetIndex != -1 && currentIndex != -1)
        {
            //if (selectedCard == null) return;

            //PerformSwap(currentIndex, targetIndex);
            ServerSwapCards(currentIndex, targetIndex);
        }
        else
        {
            isSwapping = false;
        }
    }

    [ServerRpc]
    private void ServerSwapCards(int currentIndex, int targetIndex)
    {
        ObserversSwapCards(currentIndex, targetIndex);
    }

    [ObserversRpc]
    private void ObserversSwapCards(int currentIndex, int targetIndex)
    {
        //if (IsOwner) return;
        PerformSwap(currentIndex, targetIndex);
    }

    private void PerformSwap(int currentIndex, int targetIndex)
    {
        Card currentCard = cardList[currentIndex];
        Card targetCard = cardList[targetIndex];

        Transform currentSlot = transform.GetChild(currentIndex);
        Transform targetSlot = targetCard.transform.parent;

        targetCard.transform.SetParent(currentSlot);

        if (draggedPreviewCard == null) currentCard.transform.SetParent(targetSlot);
        else previewCardSlot = targetSlot.gameObject;

        cardList[currentIndex] = targetCard;
        cardList[targetIndex] = currentCard;

        if (IsOwner)
        {
            int dir = targetIndex > currentIndex ? 1 : -1;
            targetCard.CardHandler.PlaySwapTween(dir);
            isSwapping = false;
            //swapTimer = Time.time;    //might need this to prevent rapid swap bug
        }
    }
}
