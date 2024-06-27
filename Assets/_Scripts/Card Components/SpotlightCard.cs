using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotlightCard : NetworkBehaviour
{
    private GameObject canvas;
    private CardData cardData;
    //private bool isSpotlighting;
    private bool isEnlarged;
    private Vector2 originalSize;
    private Transform originalParent;
    private int originalLayer;
    private RectTransform cardRectTransform;

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

    public void OnClick()
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

    //[ServerRpc(RequireOwnership = false)]
    //private void ServerSpawnCard(NetworkConnection connection, GameObject instantiatedCard, Vector2 spawnPosition)
    //{
    //    //GameObject instantiatedCard = Instantiate(card, spawnPosition, Quaternion.identity);
    //    //enlargedCard = card;
    //    Spawn(instantiatedCard);

    //    //enlargedCard.LoadCardData(card.Data);

    //    TargetRenderSpotlightCard(connection, instantiatedCard);
    //}

    //[TargetRpc]
    //private void TargetRenderSpotlightCard(NetworkConnection connection, GameObject enlargedCard)
    //{
    //    CanvasGroup canvasGroup = enlargedCard.GetComponent<CanvasGroup>();
    //    canvasGroup.blocksRaycasts = false;     // turn off so the spotlight card doesnt register hover/clicks

    //    RectTransform spotlightRect = enlargedCard.GetComponent<RectTransform>();
    //    spotlightRect.localScale = new Vector2(2f, 2f);

    //    enlargedCard.gameObject.transform.SetParent(canvas.transform, true);
    //    enlargedCard.gameObject.layer = LayerMask.NameToLayer("Spotlight");


    //}

    public void DestroySpotlightCard()
    {
        if (!isEnlarged) return;
        isEnlarged = false;
        cardRectTransform.localScale = originalSize;
        gameObject.transform.SetParent(originalParent, true);
        gameObject.layer = originalLayer;
    }

    //[ServerRpc(RequireOwnership = false)]
    //private void ServerDespawnCard()
    //{
    //    if (enlargedCard)
    //    {
    //        Despawn(enlargedCard.gameObject);
    //    }
    //}
}
