using DG.Tweening;
using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class CardHolder : NetworkBehaviour
{
    [SerializeField] protected GameObject cardSlotPrefab;
    [SerializeField] protected CardInteractionHandler selectedCard;
    [SerializeReference] protected CardInteractionHandler hoveredCard;
    public List<CardInteractionHandler> cardHandlers;

    protected RectTransform rect;
    protected bool isCrossing = false;

    protected virtual void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    [Server]
    public virtual void AddCard(Card card)
    {
        GameObject cardSlot = Instantiate(cardSlotPrefab);
        Spawn(cardSlot);
        ObserversSetCardSlotParent(cardSlot);
        card.SetCardParent(cardSlot.transform, false);
        ObserversResetCardPosition(card);

        //CardInteractionHandler cardHandler = card.GetComponent<CardInteractionHandler>();
        //TargetEndDrag(card.Owner, cardHandler, false);

        AddCardHandlerListeners(card.Owner, card);
    }

    [ObserversRpc]
    protected void ObserversSetCardSlotParent(GameObject cardSlot)
    {
        cardSlot.transform.SetParent(transform);
    }

    [ObserversRpc]
    protected void ObserversResetCardPosition(Card card)
    {
        card.transform.localPosition = Vector3.zero;
        card.gameObject.GetComponent<Canvas>().overrideSorting = false;
    }


    [ServerRpc(RequireOwnership = false)]
    public virtual void MoveCard(Card card, CardHolder newCardHolder, Transform originalCardSlot = null)
    {
        RemoveCardHandlerListeners(card.Owner, card);
        CardInteractionHandler cardHandler = card.GetComponent<CardInteractionHandler>();

        //TargetEndDrag(card.Owner, cardHandler, false);      // why? - to reset selectedCard for swap logic
        newCardHolder.AddCard(card);
        Despawn(originalCardSlot.gameObject);
    }

    protected void BeginDrag(CardInteractionHandler cardHandler)
    {
        selectedCard = cardHandler;
    }


    protected void EndDrag(CardInteractionHandler cardHandler, bool returningToSlot)
    {
        if (selectedCard == null)
        {
            return;
        }

        if (returningToSlot)
        {
            selectedCard.transform.DOLocalMove(Vector3.zero, .15f).SetEase(Ease.OutBack);
        }


        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;
        
        selectedCard.gameObject.GetComponent<Canvas>().overrideSorting = false;
        selectedCard = null;
    }

    //[TargetRpc]
    //protected void TargetEndDrag(NetworkConnection connection, CardInteractionHandler cardHandler, bool returningToSlot)
    //{
    //    EndDrag(cardHandler, returningToSlot);
    //}

    protected void CardPointerEnter(CardInteractionHandler cardHandler)
    {
        hoveredCard = cardHandler;
    }

    protected void CardPointerExit(CardInteractionHandler cardHandler)
    {
        hoveredCard = null;     //not sure if hovered card is needed. but if so, it is currently not reset when a card is moved
    }

    protected virtual void Update()
    {

        if (selectedCard == null)
            return;
        if (isCrossing)
            return;

        //SwapCheck();
    }

    protected void SwapCheck()
    {
        for (int i = 0; i < cardHandlers.Count; i++)
        {

            if (selectedCard.transform.position.x > cardHandlers[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() < cardHandlers[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }

            if (selectedCard.transform.position.x < cardHandlers[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() > cardHandlers[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
        }
    }

    protected void Swap(int index)
    {
        isCrossing = true;

        Transform focusedParent = selectedCard.transform.parent;
        Transform crossedParent = cardHandlers[index].transform.parent;

        cardHandlers[index].transform.SetParent(focusedParent);
        cardHandlers[index].transform.localPosition = Vector3.zero;
        selectedCard.transform.SetParent(crossedParent);

        isCrossing = false;
    }

    [ObserversRpc]
    [TargetRpc]
    protected void AddCardHandlerListeners(NetworkConnection connection, Card card)
    {
        CardInteractionHandler cardHandler = card.GetComponent<CardInteractionHandler>();
        if (cardHandler == null)
            return;
        cardHandlers.Add(cardHandler);      //Might be able to just add the card itself. During swap just compare card positions?
        cardHandler.PointerEnterEvent.AddListener(CardPointerEnter);
        cardHandler.PointerExitEvent.AddListener(CardPointerExit);
        cardHandler.BeginDragEvent.AddListener(BeginDrag);
        cardHandler.EndDragEvent.AddListener(EndDrag);
    }

    [ObserversRpc]
    [TargetRpc]
    protected void RemoveCardHandlerListeners(NetworkConnection connection, Card card)
    {
        selectedCard = null;
        CardInteractionHandler cardHandler = card.GetComponent<CardInteractionHandler>();
        cardHandlers.Remove(cardHandler);
        cardHandler.PointerEnterEvent.RemoveListener(CardPointerEnter);
        cardHandler.PointerExitEvent.RemoveListener(CardPointerExit);
        cardHandler.BeginDragEvent.RemoveListener(BeginDrag);
        cardHandler.EndDragEvent.RemoveListener(EndDrag);
    }
}
