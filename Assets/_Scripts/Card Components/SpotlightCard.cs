using FishNet.Connection;
using FishNet.Object;
using GameKit.Dependencies.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpotlightCard : NetworkBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler
{
    #region General Variables
    private GameObject canvas;
    public GameObject referenceCard;

    private bool isClicking;
    private bool isDragging;
    public bool isEnlargedCard;
    public bool isSpotlightCard;
    #endregion

    #region UI Elements
    [SerializeField] private KeywordGrouper keywordGrouperPrefab;
    [SerializeField] private SpotlightDescription spotlightDescriptionPrefab;
    #endregion

    private void Start()
    {
        // Cache the Canvas GameObject for later use
        canvas = GameObject.Find("Canvas");
    }

    private void OnDisable()
    {
        // Despawn the card if it is either an enlarged or spotlight card
        if (isEnlargedCard || isSpotlightCard)
        {
            ServerDespawnCard(gameObject);
        }
    }

    /// <summary>
    /// Handles pointer down events. Disables the card if it's enlarged
    /// </summary>
    /// <param name="eventData">Event data associated with the pointer down event.</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerId == -1 && isEnlargedCard)
        {
            gameObject.SetActive(false); // Disable enlarged card on left-click down to prevent dragging
        }
        isClicking = true;
    }

    /// <summary>
    /// Handles pointer up events. Spawns a spotlight or enlarged version of the card depending on context.
    /// </summary>
    /// <param name="eventData">Event data associated with the pointer up event.</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isClicking || isDragging) return;
        if (isSpotlightCard) gameObject.SetActive(false);

        bool isSpotlight = eventData.pointerId != -1;
        Vector2 spawnPosition = transform.position;
        Card card = GetComponent<Card>();

        if (card is ItemCardHeader itemHeader)
        {
            ServerSpawnItemHeaderCard(LocalConnection, itemHeader, spawnPosition, isSpotlight);
        }
        else
        {
            ServerSpawnCard(LocalConnection, gameObject, spawnPosition, isSpotlight);
        }
    }

    /// <summary>
    /// Handles pointer exit events. Disables the card if it's enlarged.
    /// </summary>
    /// <param name="eventData">Event data associated with the pointer exit event.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        isClicking = false;
        if (isEnlargedCard)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Spawns a new card on the server and copies the relevant data from the source card.
    /// </summary>
    /// <param name="connection">The network connection of the client requesting the spawn.</param>
    /// <param name="sourceCardObject">The source card object to copy data from.</param>
    /// <param name="spawnPosition">The position where the new card should be spawned.</param>
    /// <param name="isSpotlight">Whether the card should be rendered as a spotlight card.</param>
    [ServerRpc(RequireOwnership = false)]
    private void ServerSpawnCard(NetworkConnection connection, GameObject sourceCardObject, Vector2 spawnPosition, bool isSpotlight)
    {
        GameObject newCardObject = Instantiate(sourceCardObject, spawnPosition, Quaternion.identity);
        Spawn(newCardObject);

        CopyCardData(connection, newCardObject, sourceCardObject);
        GameObject originalCard = newCardObject.GetComponent<SpotlightCard>().referenceCard;

        if (isSpotlight)
        {
            ObserversRenderSpotlightCard(connection, newCardObject, originalCard);
        }
        else
        {
            ObserversRenderEnlargedCard(connection, newCardObject);
        }
    }

    /// <summary>
    /// Spawns a new item header card on the server and copies the relevant data from the source item header.
    /// </summary>
    /// <param name="connection">The network connection of the client requesting the spawn.</param>
    /// <param name="sourceItemHeader">The source item header to copy data from.</param>
    /// <param name="spawnPosition">The position where the new item header should be spawned.</param>
    /// <param name="isSpotlight">Whether the item should be rendered as a spotlight item.</param>
    [ServerRpc(RequireOwnership = false)]
    private void ServerSpawnItemHeaderCard(NetworkConnection connection, ItemCardHeader sourceItemHeader, Vector2 spawnPosition, bool isSpotlight)
    {
        ItemCard newItem = Instantiate(CardDatabase.Instance.itemCardPrefab, spawnPosition, Quaternion.identity);
        Spawn(newItem.gameObject);

        GameObject referenceCardObject = sourceItemHeader.GetComponent<SpotlightCard>().referenceCard;
        ItemCardHeader originalItemHeader = referenceCardObject ? referenceCardObject.GetComponent<ItemCardHeader>() : sourceItemHeader;

        newItem.TargetCopyItemHeaderData(connection, originalItemHeader);
        newItem.GetComponent<SpotlightCard>().referenceCard = originalItemHeader.gameObject;

        if (isSpotlight)
        {
            ObserversRenderSpotlightCard(connection, newItem.gameObject, originalItemHeader.gameObject);
        }
        else
        {
            ObserversRenderEnlargedCard(connection, newItem.gameObject);
        }
    }

    /// <summary>
    /// Copies the card data from the source card to the newly spawned card.
    /// </summary>
    /// <param name="connection">The network connection of the client receiving the card data.</param>
    /// <param name="newCardObject">The new card object to receive the copied data.</param>
    /// <param name="sourceCardObject">The source card object to copy data from.</param>
    private void CopyCardData(NetworkConnection connection, GameObject newCardObject, GameObject sourceCardObject)
    {
        Card originalCard;
        Card newCard = newCardObject.GetComponent<Card>();
        GameObject referenceCardObject = sourceCardObject.GetComponent<SpotlightCard>().referenceCard;

        // If there is a reference card, copy its data, otherwise use sourceCard
        originalCard = referenceCardObject ? referenceCardObject.GetComponent<Card>() : sourceCardObject.GetComponent<Card>();
        newCardObject.GetComponent<SpotlightCard>().referenceCard = originalCard.gameObject;

        // Copy data to a new Item Card
        if (newCard is ItemCard itemCard && originalCard is ItemCardHeader itemCardHeader)
        {
            itemCard.TargetCopyItemHeaderData(connection, itemCardHeader);
        }
        else
        {
            newCard.TargetCopyCardData(connection, originalCard);
        }

        // Copy data into Adventurer Card's Item Header
        if (newCard is AdventurerCard adventurerCard && adventurerCard.HasItem.Value)
        {
            AdventurerCard card = originalCard as AdventurerCard;
            bool isItemDisabled = card.Item.Value.IsDisabled.Value;
            adventurerCard.Item.Value.TargetCopyCardData(connection, originalCard, isItemDisabled);
            adventurerCard.Item.Value.GetComponent<SpotlightCard>().referenceCard = originalCard.GetComponent<AdventurerCard>().Item.Value.gameObject;
        }
    }

    /// <summary>
    /// Renders the spotlight version of the card on the target client.
    /// </summary>
    /// <param name="connection">The network connection of the target client.</param>
    /// <param name="newCardObject">The new card object to be rendered as a spotlight card.</param>
    /// <param name="originalCardObject">The original card object for reference.</param>
    [ObserversRpc]
    private void ObserversRenderSpotlightCard(NetworkConnection connection, GameObject newCardObject, GameObject originalCardObject)
    {
        if (LocalConnection != connection)
        {
            newCardObject.transform.SetPosition(true, new Vector2(960, -300));  //this prevents collisions with another player's spotlight card
            return;
        }
        SpotlightCard spotlightCard = newCardObject.GetComponent<SpotlightCard>();
        spotlightCard.isSpotlightCard = true;

        if (!string.IsNullOrEmpty(originalCardObject.GetComponent<Card>().CardDescription.Value))
        {
            ServerSpawnSpotlightDescription(connection, newCardObject, originalCardObject);
        }

        DisableCooldownDisplay(newCardObject);
        RectTransform spotlightTransform = newCardObject.GetComponent<RectTransform>();
        spotlightTransform.localScale = new Vector2(3f, 3f);
        newCardObject.transform.SetParent(canvas.transform, true);

        // Expand raycast blocker to full screen
        spotlightTransform.anchorMax = Vector2.one;
        spotlightTransform.anchorMin = Vector2.zero;
        spotlightTransform.offsetMin = Vector2.zero;
        spotlightTransform.offsetMax = new Vector2(0f, 125f);

        newCardObject.GetComponent<Image>().enabled = true;
        newCardObject.layer = LayerMask.NameToLayer("Spotlight");

        newCardObject.GetComponent<Card>().OnPointerExit();
    }

    /// <summary>
    /// Spawns the spotlight description and keyword grouper on the server and sends it to the target client.
    /// </summary>
    /// <param name="connection">The network connection of the target client.</param>
    /// <param name="spotlightCard">The spotlight card that will contain the description and keywords.</param>
    /// <param name="originalCardObject">The original card object for reference.</param>
    [ServerRpc(RequireOwnership = false)]
    private void ServerSpawnSpotlightDescription(NetworkConnection connection, GameObject spotlightCard, GameObject originalCardObject)
    {
        SpotlightDescription spotlightDescription = Instantiate(spotlightDescriptionPrefab);
        Spawn(spotlightDescription.gameObject);
        spotlightDescription.ObserversSetParent(connection, spotlightCard);

        string cardDescription = originalCardObject.GetComponent<Card>().CardDescription.Value;
        spotlightDescription.TargetSetDescriptionText(connection, cardDescription);

        // check database for keywords that need a description
        List<string> keywordList = CardDatabase.Instance.GetCardKeywords(originalCardObject.GetComponent<Card>().CardName.Value);
        if (keywordList == null) return;

        // if there are any keywords, create and spawn description grouper
        KeywordGrouper keywordGrouper = Instantiate(keywordGrouperPrefab);
        Spawn(keywordGrouper.gameObject);
        keywordGrouper.ObserversSetParent(connection, spotlightCard);

        foreach (string keyword in keywordList)
        {
            keywordGrouper.AddKeywordDescription(connection, keyword);
        }

        keywordGrouper.TargetResizeKeywordGrouper(connection);
    }

    /// <summary>
    /// Renders the enlarged version of the card on the target client.
    /// </summary>
    /// <param name="connection">The network connection of the target client.</param>
    /// <param name="card">The card object to be rendered as an enlarged card.</param>
    [ObserversRpc]
    private void ObserversRenderEnlargedCard(NetworkConnection connection, GameObject card)
    {
        if (isDragging)
        {
            ServerDespawnCard(card);
            return;
        }

        if (LocalConnection != connection)
        {
            card.transform.SetPosition(true, new Vector2(960, -300));  //this prevents collisions with another player's enlarged card
            return;
        }

        card.GetComponent<SpotlightCard>().isEnlargedCard = true;
        DisableCooldownDisplay(card);

        RectTransform spotlightTransform = card.GetComponent<RectTransform>();
        spotlightTransform.anchorMax = new Vector2(0.5f, 0.5f);         //Set anchor to middle of card to keep consistent across use cases
        spotlightTransform.anchorMin = new Vector2(0.5f, 0.5f);
        spotlightTransform.localScale = new Vector2(2f, 2f);

        card.transform.SetParent(canvas.transform, true);
        PreventEnlargedCardCutoff(spotlightTransform);
        card.layer = LayerMask.NameToLayer("Spotlight");

        card.GetComponent<Card>().OnPointerExit();
    }

    /// <summary>
    /// Prevents the enlarged card from being cut off the screen by adjusting its position.
    /// </summary>
    /// <param name="rt">The RectTransform of the card being adjusted.</param>
    private void PreventEnlargedCardCutoff(RectTransform rt)
    {
        Vector2 position = rt.anchoredPosition;

        RectTransform canvasTransform = canvas.GetComponent<RectTransform>();
        float canvasWidth = canvasTransform.rect.width/2;
        float canvasHeight = canvasTransform.rect.height/2;

        float x = Mathf.Clamp(position.x, -canvasWidth + rt.sizeDelta.x, canvasWidth - rt.sizeDelta.x);
        float y = Mathf.Clamp(position.y, -canvasHeight + rt.sizeDelta.y, canvasHeight - rt.sizeDelta.y);

        rt.anchoredPosition = new Vector2(x, y);
    }

    private void DisableCooldownDisplay(GameObject card)
    {
        //if card is an adventurer card, check for cooldown display gameobject and disable it
        if (card.GetComponent<AdventurerCard>())
        {
            Transform cooldownDisplay = card.transform.Find("CooldownDisplay(Clone)");
            if (cooldownDisplay)
            {
                cooldownDisplay.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Despawns the card on the server.
    /// </summary>
    /// <param name="card">The card GameObject to be despawned.</param>
    [ServerRpc(RequireOwnership = false)]
    private void ServerDespawnCard(GameObject card)
    {
        if (card)
        {
            Despawn(card);
        }
    }

    /// <summary>
    /// Handles the beginning of a drag event.
    /// </summary>
    public void OnBeginDrag()
    {
        isDragging = true;
    }

    /// <summary>
    /// Handles the end of a drag event.
    /// </summary>
    public void OnEndDrag()
    {
        isDragging = false;
    }
}
