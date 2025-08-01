using DG.Tweening;
using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
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
    [SerializeField] protected Card selectedCard;
    [SerializeReference] protected CardInteractionHandler hoveredCard;
    public List<Card> cardList;
    public CardHolderType HolderType { get; protected set; }

    protected GameObject previewCardSlot;
    protected RectTransform rect;
    protected bool isCrossing = false;
    int count = 0;

    protected virtual void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    [Server]
    public virtual void AddCard(Card card)
    {
        GameObject cardSlot;
        bool animateMove = false;
        if (previewCardSlot == null)
        {
            cardSlot = Instantiate(cardSlotPrefab);
            Spawn(cardSlot);
            cardSlot.transform.SetParent(transform);
            ObserversSetCardSlotParent(cardSlot);
        }
        else
        {
            cardSlot = previewCardSlot;
            animateMove = true;
            previewCardSlot = null;
        }

        card.SetCardParent(cardSlot.transform, true, this);
        ObserversResetCardPosition(card, animateMove);

        AddCardHandlerListeners(card.Owner, card);
    }

    [ObserversRpc]
    protected void ObserversSetCardSlotParent(GameObject cardSlot)
    {
        cardSlot.transform.SetParent(transform);
        cardSlot.name = "Card Slot " + count++;
    }

    [ObserversRpc]
    protected void ObserversResetCardPosition(Card card, bool animateMove = false)
    {
        if (animateMove && card.IsOwner)
        {
            card.CardHandler.PlayReturnTween("Return to Slot", Scale);
        }
        else
        {
            card.transform.localPosition = Vector3.zero;
            card.gameObject.GetComponent<Canvas>().overrideSorting = false;
            SetCardScale(card.gameObject);
        }
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
        selectedCard = cardHandler.Card;
    }

    protected void EndDrag(CardInteractionHandler cardHandler, bool returningToSlot)
    {
        //if (selectedCard == null)
        //{
        //    return;
        //}

        if (returningToSlot)
        {
            cardHandler.PlayReturnTween("Return to Slot", Scale);
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

    [ObserversRpc]
    protected void AddCardHandlerListeners(NetworkConnection connection, Card card)
    {
        CardInteractionHandler cardHandler = card.CardHandler;
        if (cardHandler == null)
            return;
        cardList.Add(card);
        cardHandler.PointerEnterEvent.AddListener(CardPointerEnter);
        cardHandler.PointerExitEvent.AddListener(CardPointerExit);
        cardHandler.BeginDragEvent.AddListener(BeginDrag);
        cardHandler.EndDragEvent.AddListener(EndDrag);
    }

    [ObserversRpc]
    protected void RemoveCardHandlerListeners(NetworkConnection connection, Card card)
    {
        if (connection == LocalConnection) selectedCard = null;

        cardList.Remove(card);
        CardInteractionHandler cardHandler = card.CardHandler;
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

    public bool CreatePreviewSlot(Card card)
    {
        if (cardList.Contains(card)) return false;
        if (QuestLane != null && QuestLane.IsQuestLaneFull()) return false;

        ServerCreatePreviewSlot();
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerCreatePreviewSlot()
    {
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
