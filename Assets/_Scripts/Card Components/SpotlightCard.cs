using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotlightCard : NetworkBehaviour
{
    private GameObject canvas;
    private Card enlargedCard;
    private bool isSpotlighting;

    [SyncVar] private CardData cardData;
    [SyncVar] private Card cardPrefab;


    private void Start()
    {
        canvas = GameObject.Find("Canvas");
    }

    public void OnHover()
    {
        if (isSpotlighting) return;
        isSpotlighting = true;

        Vector2 spawnPosition = gameObject.transform.position;
        ServerSpawnCard(LocalConnection, spawnPosition);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerSpawnCard(NetworkConnection connection, Vector2 spawnPosition)
    {
        if (cardData == null) cardData = gameObject.GetComponent<Card>().Data;

        

        enlargedCard = Instantiate(cardPrefab, spawnPosition, Quaternion.identity);
        Spawn(enlargedCard.gameObject);

        enlargedCard.LoadCardData(cardData);

        TargetRenderSpotlightCard(connection, enlargedCard);
    }

    [TargetRpc]
    private void TargetRenderSpotlightCard(NetworkConnection connection, Card enlargedCard)
    {
        CanvasGroup canvasGroup = enlargedCard.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;     // turn off so the spotlight card doesnt register hover/clicks

        RectTransform spotlightRect = enlargedCard.GetComponent<RectTransform>();
        spotlightRect.localScale = new Vector2(2f, 2f);

        enlargedCard.gameObject.transform.SetParent(canvas.transform, true);
        enlargedCard.gameObject.layer = LayerMask.NameToLayer("Spotlight");
    }

    public void DestroySpotlightCard()
    {
        if (!isSpotlighting) return;
        ServerDespawnCard();
        isSpotlighting = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerDespawnCard()
    {
        if (enlargedCard)
        {
            Despawn(enlargedCard.gameObject);
        }
    }
}
