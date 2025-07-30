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

        //SwapCheck();
        UpdateDrag();
    }

    public void UpdateDrag()
    {
        int selectedIndex = cardList.IndexOf(selectedCard);
        float selectedX = selectedCard.transform.position.x;

        for (int i = 0; i < cardList.Count; i++)
        {
            if (i == selectedIndex) continue;

            Card targetCard = cardList[i];
            if (targetCard == null) continue;

            float targetX = targetCard.transform.position.x;
            float halfWidth = targetCard.GetComponent<RectTransform>().rect.width / 2f;

            bool crossedRight = selectedIndex < i && selectedX > targetX + halfWidth;
            bool crossedLeft = selectedIndex > i && selectedX < targetX - halfWidth;

            if (crossedRight || crossedLeft)
            {
                ServerSwapCards(selectedIndex, i);
                PerformSwap(selectedIndex, i); // Apply immediately for local visual feedback
                break;
            }
        }
    }

    [ServerRpc]
    private void ServerSwapCards(int selectedIndex, int targetIndex)
    {
        if (!IsServerInitialized) return;
        ObserversSwapCards(selectedIndex, targetIndex); // Sync other clients
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

        Transform selectedSlot = selectedCard.transform.parent;
        Transform targetSlot = targetCard.transform.parent;

        selectedCard.transform.SetParent(targetSlot);
        targetCard.transform.SetParent(selectedSlot);

        if (IsOwner) targetCard.CardHandler.PlayReturnTween("Swapping");
    }

    //protected void SwapCheck()
    //{
    //    for (int i = 0; i < cardList.Count; i++)
    //    {

    //        if (selectedCard.transform.position.x > cardList[i].transform.position.x)
    //        {
    //            if (selectedCard.CardHandler.ParentIndex() < cardList[i].CardHandler.ParentIndex())
    //            {
    //                Swap(i);
    //                break;
    //            }
    //        }

    //        if (selectedCard.transform.position.x < cardList[i].transform.position.x)
    //        {
    //            if (selectedCard.CardHandler.ParentIndex() > cardList[i].CardHandler.ParentIndex())
    //            {
    //                Swap(i);
    //                break;
    //            }
    //        }
    //    }
    //}

    //protected void Swap(int index)
    //{
    //    isCrossing = true;

    //    Transform focusedParent = selectedCard.transform.parent;
    //    Transform crossedParent = cardList[index].transform.parent;

    //    cardList[index].transform.SetParent(focusedParent);
    //    cardList[index].transform.localPosition = Vector3.zero;
    //    selectedCard.transform.SetParent(crossedParent);

    //    isCrossing = false;
    //}
}
