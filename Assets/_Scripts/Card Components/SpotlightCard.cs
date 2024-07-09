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
    //private bool isSpotlighting;
    private bool isEnlarged;
    private Vector2 originalSize;
    private Transform originalParent;
    private int originalLayer;
    private RectTransform cardRectTransform;
    private bool isSpotlighting;
    private GameObject spotlight;

    [SyncVar] private Card cardPrefab;


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
            EnlargeCard();
        }
        else if (eventData.pointerId == -2)
        {
            // RIGHT CLICK
            // shrink card here
            Vector2 spawnPosition = gameObject.transform.position;
            ServerSpawnCard(LocalConnection, gameObject, spawnPosition);
        }
    }

    public void EnlargeCard()
    {
        if (isEnlarged) return;
        isEnlarged = true;

        //Vector2 spawnPosition = gameObject.transform.position + new Vector3(0f, 300f);

        originalSize = cardRectTransform.localScale;
        originalParent = gameObject.transform.parent;
        originalLayer = gameObject.layer;

        cardRectTransform.localScale = new Vector2(2f, 2f);
        gameObject.transform.SetParent(canvas.transform, true);
        gameObject.layer = LayerMask.NameToLayer("Spotlight");
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerSpawnCard(NetworkConnection connection, GameObject card, Vector2 spawnPosition)
    {
        GameObject spotlightCard = Instantiate(card, spawnPosition, Quaternion.identity);
        //enlargedCard = card;
        Spawn(spotlightCard);

        //enlargedCard.LoadCardData(card.Data);

        TargetRenderSpotlightCard(connection, spotlightCard);
    }

    [TargetRpc]
    private void TargetRenderSpotlightCard(NetworkConnection connection, GameObject spotlightCard)
    {
        CanvasGroup canvasGroup = spotlightCard.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;     // turn off so the spotlight card doesnt register hover/clicks

        RectTransform spotlightRect = spotlightCard.GetComponent<RectTransform>();
        spotlightRect.localScale = new Vector2(2f, 2f);

        spotlightCard.gameObject.transform.SetParent(canvas.transform, true);
        spotlightCard.gameObject.layer = LayerMask.NameToLayer("Spotlight");

        spotlight = spotlightCard;
        isSpotlighting = true;


    }

    public void DestroySpotlightCard()
    {
        if (isEnlarged)
        {
            isEnlarged = false;
            cardRectTransform.localScale = originalSize;
            gameObject.transform.SetParent(originalParent, true);
            gameObject.layer = originalLayer;
        }

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
