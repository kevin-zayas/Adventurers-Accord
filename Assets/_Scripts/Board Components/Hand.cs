using DG.Tweening;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
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
    //[SerializeField] GameObject handSlotPrefab;
    //[SerializeField] private CardInteractionHandler selectedCard;
    //[SerializeReference] private CardInteractionHandler hoveredCard;
    //public List<CardInteractionHandler> cardHandlers;

    //private RectTransform rect;
    //private bool isCrossing = false;

    //[Server]
    //public void AddCard(Card card)
    //{
    //    GameObject handSlot = Instantiate(handSlotPrefab, transform);
    //    Spawn(handSlot);
    //    card.SetCardParent(handSlot.transform, false);

    //    CardInteractionHandler cardHandler = card.GetComponent<CardInteractionHandler>();
    //    cardHandlers.Add(cardHandler);      //Might be able to just add the card itself. During swap just compare card positions?
    //    cardHandler.PointerEnterEvent.AddListener(CardPointerEnter);
    //    cardHandler.PointerExitEvent.AddListener(CardPointerExit);
    //    cardHandler.BeginDragEvent.AddListener(BeginDrag);
    //    cardHandler.EndDragEvent.AddListener(EndDrag);
    //}

    //[ServerRpc]
    //public void MoveCard(Card card, Transform newTransform)
    //{
    //    CardInteractionHandler cardHandler = card.GetComponent<CardInteractionHandler>();
    //    cardHandlers.Remove(cardHandler);
    //    cardHandler.PointerEnterEvent.RemoveListener(CardPointerEnter);
    //    cardHandler.PointerExitEvent.RemoveListener(CardPointerExit);
    //    cardHandler.BeginDragEvent.RemoveListener(BeginDrag);
    //    cardHandler.EndDragEvent.RemoveListener(EndDrag);

    //    EndDrag(cardHandler, false);
    //    GameObject slot = card.transform.parent.gameObject;
    //    card.SetCardParent(newTransform, false);
    //    Despawn(slot);

    //}

    //protected void BeginDrag(CardInteractionHandler card)
    //{
    //    selectedCard = card;
    //}


    //protected void EndDrag(CardInteractionHandler cardHandler, bool returningToSlot)
    //{
    //    if (selectedCard == null)
    //        return;

    //    if (returningToSlot) selectedCard.transform.DOLocalMove(Vector3.zero, .15f).SetEase(Ease.OutBack);

    //    rect.sizeDelta += Vector2.right;
    //    rect.sizeDelta -= Vector2.right;
    //    selectedCard = null;
    //    cardHandler.gameObject.GetComponent<Canvas>().overrideSorting = false;

    //}

    //protected void CardPointerEnter(CardInteractionHandler cardHandler)
    //{
    //    hoveredCard = cardHandler;
    //}

    //protected void CardPointerExit(CardInteractionHandler cardHandler)
    //{
    //    hoveredCard = null;
    //}

    //void Update()
    //{
    //    if (Input.GetMouseButtonDown(1))
    //    {
    //        foreach (CardInteractionHandler cardHandler in this.cardHandlers)
    //        {
    //            //cardHandler.Deselect();
    //        }
    //    }

    //    if (selectedCard == null)
    //        return;
    //    if (isCrossing)
    //        return;

    //    SwapCheck();
    //}

    //protected void SwapCheck()
    //{
    //    for (int i = 0; i < cardHandlers.Count; i++)
    //    {

    //        if (selectedCard.transform.position.x > cardHandlers[i].transform.position.x)
    //        {
    //            if (selectedCard.ParentIndex() < cardHandlers[i].ParentIndex())
    //            {
    //                Swap(i);
    //                break;
    //            }
    //        }

    //        if (selectedCard.transform.position.x < cardHandlers[i].transform.position.x)
    //        {
    //            if (selectedCard.ParentIndex() > cardHandlers[i].ParentIndex())
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
    //    Transform crossedParent = cardHandlers[index].transform.parent;

    //    cardHandlers[index].transform.SetParent(focusedParent);
    //    cardHandlers[index].transform.localPosition = Vector3.zero;
    //    selectedCard.transform.SetParent(crossedParent);

    //    isCrossing = false;

    //    //if (cardHandlers[index].cardVisual == null)
    //    //    return;

    //    //bool swapIsRight = cardHandlers[index].ParentIndex() > selectedCard.ParentIndex();
    //    //cardHandlers[index].cardVisual.Swap(swapIsRight ? -1 : 1);

    //    ////Updated Visual Indexes
    //    //foreach (CardHandler card in cardHandlers)
    //    //{
    //    //    card.cardVisual.UpdateIndex();
    //    //}
    //}
}
