using DG.Tweening;
using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardHolder : NetworkBehaviour
{
    public virtual QuestLane QuestLane => null;
    public virtual Vector3 Scale => Vector3.one;

    public enum CardHolderType
    {
        Draft,
        Hand,
        Quest,
        Spell,
        Discard
    }
    [SerializeField] protected GameObject cardSlotPrefab;
    [SerializeField] protected CardInteractionHandler selectedCard;
    [SerializeReference] protected CardInteractionHandler hoveredCard;
    public List<CardInteractionHandler> cardHandlers;
    public CardHolderType HolderType { get; protected set; }

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
        cardSlot.transform.SetParent(transform);
        ObserversSetCardSlotParent(cardSlot);
        card.SetCardParent(cardSlot.transform, false, this);
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
        SetCardScale(card.gameObject);
    }

    protected void SetCardScale(GameObject card)
    {
        card.transform.DOScale(Scale, 0.2f).SetEase(Ease.OutBack);
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void ServerMoveCard(Card card, CardHolder newCardHolder, Transform originalCardSlot = null)
    {
        MoveCard(card, newCardHolder, originalCardSlot);
    }

    [Server]
    public virtual void MoveCard(Card card, CardHolder newCardHolder, Transform originalCardSlot = null)
    {
        RemoveCardHandlerListeners(card.Owner, card);

        if (newCardHolder) newCardHolder.AddCard(card);
        else Despawn(card.gameObject);

        originalCardSlot.SetParent(null);
        Despawn(originalCardSlot.gameObject);
    }

    protected void BeginDrag(CardInteractionHandler cardHandler)
    {
        selectedCard = cardHandler;
    }

    protected void EndDrag(CardInteractionHandler cardHandler, bool returningToSlot)
    {
        //if (selectedCard == null)
        //{
        //    return;
        //}

        if (returningToSlot)
        {
            cardHandler.PlayReturnTween("Return to Slot");
        }

        rect.sizeDelta += Vector2.right;        // TODO: try removing this
        rect.sizeDelta -= Vector2.right;

        selectedCard = null;
    }

    protected void CardPointerEnter(CardInteractionHandler cardHandler)
    {
        //hoveredCard = cardHandler;
    }

    protected void CardPointerExit(CardInteractionHandler cardHandler)
    {
        //hoveredCard = null;
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
    protected void RemoveCardHandlerListeners(NetworkConnection connection, Card card)
    {
        if (connection == LocalConnection) selectedCard = null;

        CardInteractionHandler cardHandler = card.GetComponent<CardInteractionHandler>();
        cardHandlers.Remove(cardHandler);
        cardHandler.PointerEnterEvent.RemoveListener(CardPointerEnter);
        cardHandler.PointerExitEvent.RemoveListener(CardPointerExit);
        cardHandler.BeginDragEvent.RemoveListener(BeginDrag);
        cardHandler.EndDragEvent.RemoveListener(EndDrag);
    }

    public bool IsQuest()
    {
        return HolderType == CardHolderType.Quest;
    }

    public bool IsDraft()
    {
        return HolderType == CardHolderType.Draft;
    }

    public bool IsHand()
    {
        return HolderType == CardHolderType.Hand;
    }

    public bool IsSpell()
    {
        return HolderType == CardHolderType.Spell;
    }
}
