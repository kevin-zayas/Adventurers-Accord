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
        if (eventData.pointerId == -1)
        {
            print("OnPointerDown" + enlargedCard);
            // LEFT CLICK
            if (enlargedCard)
            {
                DestroyEnlargedCard();
                return;
            }

            if (spotlightCard) return;

            Vector2 spawnPosition = gameObject.transform.position;
            ServerSpawnCard(LocalConnection, gameObject, spawnPosition, false);
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

            Vector2 spawnPosition = new(Screen.width / 2, Screen.height / 2);
            ServerSpawnCard(LocalConnection, gameObject, spawnPosition, true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerSpawnCard(NetworkConnection connection, GameObject originalCard, Vector2 spawnPosition, bool isSpotlight)
    {
        GameObject cardObject = Instantiate(originalCard, spawnPosition, Quaternion.identity);
        Spawn(cardObject);

        Card card = cardObject.GetComponent<Card>();
        card.TargetCopyCardData(connection, originalCard.GetComponent<Card>());
        if (card is AdventurerCard adventurerCard && adventurerCard.HasItem)
        {
            adventurerCard.Item.TargetCopyCardData(connection, originalCard.GetComponent<Card>());
        }

        if (isSpotlight) TargetRenderSpotlightCard(connection, cardObject);
        else TargetRenderEnlargedCard(connection, cardObject);
    }

    [TargetRpc]
    private void TargetRenderSpotlightCard(NetworkConnection connection, GameObject card)
    {
        
        CanvasGroup canvasGroup = card.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;     // turn off so the spotlight card doesnt register hover/clicks

        RectTransform spotlightRect = card.GetComponent<RectTransform>();
        spotlightRect.localScale = new Vector2(3f, 3f);

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

        card.gameObject.transform.SetParent(canvas.transform, true);
        card.gameObject.layer = LayerMask.NameToLayer("Spotlight");

        enlargedCard = card;

        // increase size of original card to ensure destroying enlarged card on pointer exit feels natural
        originalSize = cardRectTransform.localScale;
        cardRectTransform.localScale = new Vector2(2f, 2f);

    }

    public void DestroyEnlargedCard()
    {
        print("destroy enlarged card");
        if (!enlargedCard) return;

        cardRectTransform.localScale = originalSize;

        enlargedCard.gameObject.SetActive(false);
        ServerDespawnCard(enlargedCard);

        cardRectTransform.localScale = originalSize;
    }

    public void DestroySpotlightCard()
    {
        print("destroy spotlight card");
        ServerDespawnCard(spotlightCard);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerDespawnCard(GameObject card)
    {

        if (card)
        {
            Despawn(card.gameObject);
            print("despawn card");
        }
    }

    public void OnBeginDrag()
    {
        print("begin drag, isDragging True");
        isDragging = true;
        if (enlargedCard)
        {
            print("enlarged card present during drag, destroying enlarged card");
            DestroyEnlargedCard();
        }
    }

    public void OnEndDrag()
    {
        print("end drag, isDragging False");
        isDragging = false;
    }
}
