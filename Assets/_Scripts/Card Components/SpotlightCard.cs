using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpotlightCard : NetworkBehaviour, IPointerDownHandler, IPointerExitHandler
{
    private GameObject canvas;
    public GameObject referenceCard;

    private bool isDragging;
    public bool isEnlargedCard;
    public bool isSpotlightCard;

    [SerializeField] private SpotlightDescription spotlightDescriptionPrefab;

    private void Start()
    {
        canvas = GameObject.Find("Canvas");
    }

    private void OnDisable()
    {
        if (isEnlargedCard || isSpotlightCard) ServerDespawnCard(gameObject);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isDragging) return;
        if (isSpotlightCard) gameObject.SetActive(false);

        bool isSpotlight = true;

        if (eventData.pointerId == -1)
        {
            // LEFT CLICK
            if (isEnlargedCard) gameObject.SetActive(false);
            isSpotlight = false;
        }

        Vector2 spawnPosition = gameObject.transform.position;
        Card card = gameObject.GetComponent<Card>();

        if (card is ItemCardHeader itemHeader) ServerSpawnItemHeaderCard(LocalConnection, itemHeader, spawnPosition, isSpotlight);
        else ServerSpawnCard(LocalConnection, gameObject, spawnPosition, isSpotlight);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isEnlargedCard)
        {
            gameObject.SetActive(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerSpawnCard(NetworkConnection connection, GameObject sourceCardObject, Vector2 spawnPosition, bool isSpotlight)
    {
        GameObject newCardObject = Instantiate(sourceCardObject, spawnPosition, Quaternion.identity);
        Spawn(newCardObject);

        CopyCardData(connection, newCardObject, sourceCardObject);
        GameObject originalCard = newCardObject.GetComponent<SpotlightCard>().referenceCard;

        if (isSpotlight) TargetRenderSpotlightCard(connection, newCardObject, originalCard);
        else TargetRenderEnlargedCard(connection, newCardObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerSpawnItemHeaderCard(NetworkConnection connection, ItemCardHeader sourceItemHeader, Vector2 spawnPosition, bool isSpotlight)
    {
        ItemCard newItem = Instantiate(CardDatabase.Instance.itemCardPrefab, spawnPosition, Quaternion.identity);
        Spawn(newItem.gameObject);

        ItemCardHeader originalItemHeader;
        GameObject referenceCardObject = sourceItemHeader.GetComponent<SpotlightCard>().referenceCard;
        originalItemHeader = referenceCardObject ? referenceCardObject.GetComponent<ItemCardHeader>() : sourceItemHeader;

        newItem.TargetCopyItemHeaderData(connection, originalItemHeader);
        newItem.GetComponent<SpotlightCard>().referenceCard = originalItemHeader.gameObject;

        if (isSpotlight) TargetRenderSpotlightCard(connection, newItem.gameObject, originalItemHeader.gameObject);
        else TargetRenderEnlargedCard(connection, newItem.gameObject);
    }

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
        else newCard.TargetCopyCardData(connection, originalCard);

        // Copy data into Adventurer Card's Item Header
        if (newCard is AdventurerCard adventurerCard && adventurerCard.HasItem)
        {
            adventurerCard.Item.TargetCopyCardData(connection, originalCard);
            adventurerCard.Item.GetComponent<SpotlightCard>().referenceCard = originalCard.GetComponent<AdventurerCard>().Item.gameObject;
        }
    }


    [TargetRpc]
    private void TargetRenderSpotlightCard(NetworkConnection connection, GameObject newCardObject, GameObject originalCardObject)
    {
        SpotlightCard spotlightCard = newCardObject.GetComponent<SpotlightCard>();
        spotlightCard.isSpotlightCard = true;

        if (originalCardObject.GetComponent<Card>().Description != "") ServerSpawnSpotlightDescription(connection, newCardObject, originalCardObject);

        RectTransform spotlightTransform = newCardObject.GetComponent<RectTransform>();
        spotlightTransform.localScale = new Vector2(3f, 3f);

        // Expand raycast blocker to full screen
        spotlightTransform.anchorMax = Vector2.one;
        spotlightTransform.anchorMin = Vector2.zero;
        spotlightTransform.offsetMin = Vector2.zero;
        spotlightTransform.offsetMax = new Vector2(Screen.width, Screen.height + 125f);

        newCardObject.GetComponent<Image>().enabled = true;

        newCardObject.gameObject.transform.SetParent(canvas.transform, true);
        newCardObject.gameObject.layer = LayerMask.NameToLayer("Spotlight");
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerSpawnSpotlightDescription(NetworkConnection connection, GameObject spotlightCard, GameObject originalCardObject)
    {
        SpotlightDescription spotlightDescription = Instantiate(spotlightDescriptionPrefab, new Vector2(Screen.width / 2, Screen.height / 2 - 355f), Quaternion.identity);
        Spawn(spotlightDescription.gameObject);
        TargetRenderSpotlightDescription(connection, spotlightCard, originalCardObject, spotlightDescription.gameObject);

        //try seting position above instead of in targetRender to try to descrease delay.
        // move logic to populate description to here so that we can also populate the side panel of keyword descriptions
    }

    [TargetRpc]
    private void TargetRenderSpotlightDescription(NetworkConnection connection, GameObject spotlightCard, GameObject originalCardObject, GameObject description)
    {
        RectTransform descriptionTransform= description.GetComponent<RectTransform>();
        //descriptionTransform.anchoredPosition = new Vector2(Screen.width/2, Screen.height/2 - 355f);
        descriptionTransform.SetParent(spotlightCard.transform, true);

        string cardDescription = originalCardObject.GetComponent<Card>().Description;
        description.GetComponent<SpotlightDescription>().SetDescriptionText(cardDescription);
    }

    [TargetRpc]
    private void TargetRenderEnlargedCard(NetworkConnection connection, GameObject card)
    {
        if (isDragging)
        {
            ServerDespawnCard(card);
            return;
        }

        card.GetComponent<SpotlightCard>().isEnlargedCard = true;

        RectTransform spotlightRect = card.GetComponent<RectTransform>();
        spotlightRect.localScale = new Vector2(2f, 2f);
        PreventEnlargedCardCutoff(spotlightRect);

        card.gameObject.transform.SetParent(canvas.transform, true);
        card.gameObject.layer = LayerMask.NameToLayer("Spotlight");
    }

    private void PreventEnlargedCardCutoff(RectTransform rt)
    {
        Vector2 position = rt.anchoredPosition;

        float x = position.x;
        x = Mathf.Clamp(x, 0, Screen.width- rt.sizeDelta.x);

        float y = position.y;
        y = Mathf.Clamp(y, rt.sizeDelta.y, Screen.height - rt.sizeDelta.y);

        position = new Vector2(x, y);
        rt.anchoredPosition = position;
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
    }

    public void OnEndDrag()
    {
        //print("end drag, isDragging False");
        isDragging = false;
    }
}
