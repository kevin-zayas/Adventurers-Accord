using DG.Tweening;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Hand : NetworkBehaviour
{
    public readonly SyncVar<Player> controllingPlayer = new();
    public readonly SyncVar<int> playerID = new();
    public int SlotIndex { get; private set; }

    [SerializeField] GameObject handSlotPrefab;
    [SerializeField] private CardInteractionHandler selectedCard;
    [SerializeReference] private CardInteractionHandler hoveredCard;
    public List<CardInteractionHandler> cardHandlers;

    private RectTransform rect;
    private bool isCrossing = false;


    private void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    [ObserversRpc]
    [TargetRpc]
    public void CreateHandSlot(NetworkConnection connection)
    {
        Instantiate(handSlotPrefab, transform);
        SlotIndex = transform.childCount - 1;
    }

    [Server]
    public void AddCardToHand(Card card)
    {
        GameObject handSlot = Instantiate(handSlotPrefab, transform);
        Spawn(handSlot);
        card.SetCardParent(handSlot.transform, false);

        CardInteractionHandler cardHandler = card.GetComponent<CardInteractionHandler>();
        cardHandlers.Add(cardHandler);      //Might be able to just add the card itself. During swap just compare cartd positions?
        cardHandler.PointerEnterEvent.AddListener(CardPointerEnter);
        cardHandler.PointerExitEvent.AddListener(CardPointerExit);
        cardHandler.BeginDragEvent.AddListener(BeginDrag);
        cardHandler.EndDragEvent.AddListener(EndDrag);
    }

    private void BeginDrag(CardInteractionHandler card)
    {
        selectedCard = card;
    }


    void EndDrag(CardInteractionHandler card, bool returningToHand)
    {
        if (selectedCard == null)
            return;

        if (returningToHand) selectedCard.transform.DOLocalMove(Vector3.zero, .15f).SetEase(Ease.OutBack);

        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;
        selectedCard = null;

    }

    void CardPointerEnter(CardInteractionHandler card)
    {
        hoveredCard = card;
    }

    void CardPointerExit(CardInteractionHandler card)
    {
        hoveredCard = null;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            foreach (CardInteractionHandler cardHandler in this.cardHandlers)
            {
                //cardHandler.Deselect();
            }
        }

        if (selectedCard == null)
            return;
        if (isCrossing)
            return;
        
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

    void Swap(int index)
    {
        isCrossing = true;

        Transform focusedParent = selectedCard.transform.parent;
        Transform crossedParent = cardHandlers[index].transform.parent;

        cardHandlers[index].transform.SetParent(focusedParent);
        cardHandlers[index].transform.localPosition = Vector3.zero;
        selectedCard.transform.SetParent(crossedParent);

        isCrossing = false;

        //if (cardHandlers[index].cardVisual == null)
        //    return;

        //bool swapIsRight = cardHandlers[index].ParentIndex() > selectedCard.ParentIndex();
        //cardHandlers[index].cardVisual.Swap(swapIsRight ? -1 : 1);

        ////Updated Visual Indexes
        //foreach (CardHandler card in cardHandlers)
        //{
        //    card.cardVisual.UpdateIndex();
        //}
    }
}
