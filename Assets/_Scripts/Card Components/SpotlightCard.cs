using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpotlightCard : NetworkBehaviour, IPointerDownHandler
{
    private GameObject canvas;
    private CardData cardData;
    
    private Vector2 originalSize;
    private Transform originalParent;
    private int originalLayer;
    private RectTransform cardRectTransform;

    private bool isEnlarged;
    private GameObject enlargedCard;

    private bool isSpotlighting;
    private GameObject spotlight;



    private void Start()
    {
        canvas = GameObject.Find("Canvas");
        cardRectTransform = gameObject.GetComponent<RectTransform>();
    }

    private void OnDisable()
    {
        DestroySpotlightCard();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerId == -1)
        {
            // LEFT CLICK
            if (isEnlarged)
            {
                DestroyEnlargedCard();
                return;
            }

            if (isSpotlighting) return;

            Vector2 spawnPosition = gameObject.transform.position;
            ServerSpawnCard(LocalConnection, gameObject, spawnPosition, false);
        }
        else if (eventData.pointerId == -2)
        {
            // RIGHT CLICK

            if (isEnlarged) DestroyEnlargedCard();

            if (isSpotlighting)
            {
                DestroySpotlightCard();
                return;
            }

            Vector2 spawnPosition = new Vector2(Screen.width/2, Screen.height/2);
            ServerSpawnCard(LocalConnection, gameObject, spawnPosition,true);
        }
    }

    //public void EnlargeCard()
    //{
    //    if (isEnlarged) return;
    //    isEnlarged = true;

    //    //Vector2 spawnPosition = gameObject.transform.position + new Vector3(0f, 300f);

    //    originalSize = cardRectTransform.localScale;
    //    originalParent = gameObject.transform.parent;
    //    originalLayer = gameObject.layer;

    //    cardRectTransform.localScale = new Vector2(2f, 2f);
    //    gameObject.transform.SetParent(canvas.transform, true);
    //    gameObject.layer = LayerMask.NameToLayer("Spotlight");
    //}

    [ServerRpc(RequireOwnership = false)]
    private void ServerSpawnCard(NetworkConnection connection, GameObject originalCard, Vector2 spawnPosition, bool isSpotlight)
    {
        GameObject card = Instantiate(originalCard, spawnPosition, Quaternion.identity);
        Spawn(card);

        card.GetComponent<Card>().TargetCopyCardData(connection, originalCard.GetComponent<Card>());

        if (isSpotlight) TargetRenderSpotlightCard(connection, card);
        else TargetRenderEnlargedCard(connection, card);
    }

    [TargetRpc]
    private void TargetRenderSpotlightCard(NetworkConnection connection, GameObject spotlightCard)
    {
        
        CanvasGroup canvasGroup = spotlightCard.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;     // turn off so the spotlight card doesnt register hover/clicks

        RectTransform spotlightRect = spotlightCard.GetComponent<RectTransform>();
        spotlightRect.localScale = new Vector2(3f, 3f);

        spotlightCard.gameObject.transform.SetParent(canvas.transform, true);
        spotlightCard.gameObject.layer = LayerMask.NameToLayer("Spotlight");

        spotlight = spotlightCard;
        isSpotlighting = true;
    }

    [TargetRpc]
    private void TargetRenderEnlargedCard(NetworkConnection connection, GameObject spotlightCard)
    {

        CanvasGroup canvasGroup = spotlightCard.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;     // turn off so the spotlight card doesnt register hover/clicks

        RectTransform spotlightRect = spotlightCard.GetComponent<RectTransform>();
        spotlightRect.localScale = new Vector2(2f, 2f);

        spotlightCard.gameObject.transform.SetParent(canvas.transform, true);
        spotlightCard.gameObject.layer = LayerMask.NameToLayer("Spotlight");

        enlargedCard = spotlightCard;
        isEnlarged = true;
    }

    public void DestroyEnlargedCard()
    {
        if (isEnlarged)
        {
            isEnlarged = false;
            ServerDespawnCard(enlargedCard);
        }
    }

    public void DestroySpotlightCard()
    {
        if (isSpotlighting)
        {
            isSpotlighting = false;
            ServerDespawnCard(spotlight);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerDespawnCard(GameObject spotlightCard)
    {
        if (spotlightCard)
        {
            Despawn(spotlightCard.gameObject);
        }
    }
}
