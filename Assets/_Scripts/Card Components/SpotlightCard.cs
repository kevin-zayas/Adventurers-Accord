using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpotlightCard : NetworkBehaviour, IPointerDownHandler
{
    private GameObject canvas;    
    private Vector2 originalSize;
    private RectTransform cardRectTransform;

    private GameObject enlargedCard;
    private GameObject spotlightCard;

    private bool isDragging;



    private void Start()
    {
        canvas = GameObject.Find("Canvas");
        cardRectTransform = gameObject.GetComponent<RectTransform>();
    }

    //private void OnDisable()
    //{
    //    DestroySpotlightCard();
    //}

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isDragging) return;
        Vector2 spawnPosition;
        if (eventData.pointerId == -1)
        {
            //print("OnPointerDown" + enlargedCard);
            // LEFT CLICK
            if (enlargedCard)
            {
                DestroyEnlargedCard();
                return;
            }

            if (spotlightCard) return;

            spawnPosition = gameObject.transform.position;

            Card card = gameObject.GetComponent<Card>();

            if (card is ItemCardHeader itemHeader) ServerSpawnItemHeaderCard(LocalConnection, itemHeader, spawnPosition, false);
            else ServerSpawnCard(LocalConnection, gameObject, spawnPosition, false);
        }
        else if (eventData.pointerId == -2)
        {
            // RIGHT CLICK

            if (enlargedCard) DestroyEnlargedCard();

            if (spotlightCard)
            {
                DestroySpotlightCard();
                return;
            }

            spawnPosition = new(Screen.width / 2, Screen.height / 2);

            Card card = gameObject.GetComponent<Card>();

            if (card is ItemCardHeader itemHeader) ServerSpawnItemHeaderCard(LocalConnection, itemHeader, spawnPosition, true);
            else ServerSpawnCard(LocalConnection, gameObject, spawnPosition, true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerSpawnCard(NetworkConnection connection, GameObject originalCardObject, Vector2 spawnPosition, bool isSpotlight)
    {
        GameObject newCardObject = Instantiate(originalCardObject, spawnPosition, Quaternion.identity);

        RectTransform spotlightRect = newCardObject.GetComponent<RectTransform>();
        Spawn(newCardObject);

        Card card = newCardObject.GetComponent<Card>();
        card.TargetCopyCardData(connection, originalCardObject.GetComponent<Card>());

        if (card is AdventurerCard adventurerCard && adventurerCard.HasItem)
        {
            adventurerCard.Item.TargetCopyCardData(connection, originalCardObject.GetComponent<Card>());
        }

        if (isSpotlight) TargetRenderSpotlightCard(connection, newCardObject);
        else TargetRenderEnlargedCard(connection, newCardObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerSpawnItemHeaderCard(NetworkConnection connection, ItemCardHeader itemHeader, Vector2 spawnPosition, bool isSpotlight)
    {
        ItemCard newItem = Instantiate(CardDatabase.Instance.itemCardPrefab, spawnPosition, Quaternion.identity);
        Spawn(newItem.gameObject);

        newItem.TargetCopyItemHeaderData(connection, itemHeader);

        if (isSpotlight) TargetRenderSpotlightCard(connection, newItem.gameObject);
        else TargetRenderEnlargedCard(connection, newItem.gameObject);
    }

    [TargetRpc]
    private void TargetRenderSpotlightCard(NetworkConnection connection, GameObject card)
    {
        
        CanvasGroup canvasGroup = card.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;     // turn off so the spotlight card doesnt register hover/clicks

        RectTransform spotlightRect = card.GetComponent<RectTransform>();
        spotlightRect.localScale = new Vector2(3f, 3f);

        CorrectItemHeaderPosition(card);

        card.gameObject.transform.SetParent(canvas.transform, true);
        card.gameObject.layer = LayerMask.NameToLayer("Spotlight");

        spotlightCard = card;
    }

    [TargetRpc]
    private void TargetRenderEnlargedCard(NetworkConnection connection, GameObject card)
    {
        if (isDragging)
        {
            ServerDespawnCard(card);
            return;
        }

        CanvasGroup canvasGroup = card.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;     // turn off so the spotlight card doesnt register hover/clicks

        RectTransform spotlightRect = card.GetComponent<RectTransform>();
        spotlightRect.localScale = new Vector2(2f, 2f);

        CorrectItemHeaderPosition(card);

        card.gameObject.transform.SetParent(canvas.transform, true);
        card.gameObject.layer = LayerMask.NameToLayer("Spotlight");

        enlargedCard = card;

        // increase size of original card to ensure destroying enlarged card on pointer exit feels natural
        originalSize = cardRectTransform.localScale;
        cardRectTransform.localScale = new Vector2(2f, 2f);

    }

    private void CorrectItemHeaderPosition(GameObject card)
    {
        if (card.GetComponent<Card>() is AdventurerCard adventurerCard && adventurerCard.Item)
        {
            RectTransform spotlightRect = card.GetComponent<RectTransform>();
            spotlightRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 220f);

            RectTransform itemHeaderRect = adventurerCard.Item.GetComponent<RectTransform>();
            itemHeaderRect.anchoredPosition = Vector2.zero;
        }
    }

    public void DestroyEnlargedCard()
    {
        //print("destroy enlarged card");
        if (!enlargedCard) return;

        cardRectTransform.localScale = originalSize;

        enlargedCard.gameObject.SetActive(false);
        ServerDespawnCard(enlargedCard);

        cardRectTransform.localScale = originalSize;
    }

    public void DestroySpotlightCard()
    {
        //print("destroy spotlight card");
        ServerDespawnCard(spotlightCard);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerDespawnCard(GameObject card)
    {

        if (card)
        {
            Despawn(card.gameObject);
            //print("despawn card");
        }
    }

    public void OnBeginDrag()
    {
        //print("begin drag, isDragging True");
        isDragging = true;
        if (enlargedCard)
        {
            //print("enlarged card present during drag, destroying enlarged card");
            DestroyEnlargedCard();
        }
    }

    public void OnEndDrag()
    {
        //print("end drag, isDragging False");
        isDragging = false;
    }
}
