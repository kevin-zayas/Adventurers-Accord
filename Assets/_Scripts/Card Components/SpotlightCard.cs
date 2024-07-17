using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpotlightCard : NetworkBehaviour, IPointerDownHandler, IPointerExitHandler
{
    private GameObject canvas;

    //private GameObject enlargedCard;
    //private GameObject spotlightCard;

    private bool isDragging;
    public bool isEnlargedCard;
    public bool isSpotlightCard;

    public GameObject referenceCard;



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
        Vector2 spawnPosition;

        if (isSpotlightCard) gameObject.SetActive(false);
        

        if (eventData.pointerId == -1)
        {
            // LEFT CLICK

            if (isEnlargedCard) gameObject.SetActive(false);

            spawnPosition = gameObject.transform.position;
            Card card = gameObject.GetComponent<Card>();

            if (card is ItemCardHeader itemHeader) ServerSpawnItemHeaderCard(LocalConnection, itemHeader, spawnPosition, false);
            else ServerSpawnCard(LocalConnection, gameObject, spawnPosition, false);
        }
        else if (eventData.pointerId == -2)
        {
            // RIGHT CLICK

            spawnPosition = new(Screen.width / 2, Screen.height / 2);
            Card card = gameObject.GetComponent<Card>();

            if (card is ItemCardHeader itemHeader) ServerSpawnItemHeaderCard(LocalConnection, itemHeader, spawnPosition, true);
            else ServerSpawnCard(LocalConnection, gameObject, spawnPosition, true);
        }
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
        newCardObject.GetComponent<SpotlightCard>().referenceCard = sourceCardObject;

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

    private void CopyCardData(NetworkConnection connection, GameObject newCardObject, GameObject sourceCardObject)
    {
        Card originalCard;
        Card newCard = newCardObject.GetComponent<Card>();
        GameObject referenceCardObject = sourceCardObject.GetComponent<SpotlightCard>().referenceCard;
        
        // If there is a reference card, copy its data, otherwise use sourceCard
        originalCard = referenceCardObject ? referenceCardObject.GetComponent<Card>() : sourceCardObject.GetComponent<Card>();
        newCard.TargetCopyCardData(connection, originalCard);

        if (newCard is AdventurerCard adventurerCard && adventurerCard.HasItem)
        {
            adventurerCard.Item.TargetCopyCardData(connection, originalCard);
        }
    }


    [TargetRpc]
    private void TargetRenderSpotlightCard(NetworkConnection connection, GameObject card)
    {
        card.GetComponent<SpotlightCard>().isSpotlightCard = true;

        RectTransform spotlightRect = card.GetComponent<RectTransform>();
        spotlightRect.localScale = new Vector2(3f, 3f);

        //CorrectCardSize(card);

        spotlightRect.anchorMax = Vector2.one;
        spotlightRect.anchorMin = Vector2.zero;
        spotlightRect.offsetMin = Vector2.zero;
        spotlightRect.offsetMax = new Vector2(Screen.width,Screen.height);

        card.GetComponent<Image>().enabled = true;

        card.gameObject.transform.SetParent(canvas.transform, true);
        card.gameObject.layer = LayerMask.NameToLayer("Spotlight");
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

        //CorrectCardSize(card);

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

    //private void CorrectCardSize(GameObject card)
    //{
    //    if (card.GetComponent<Card>() is AdventurerCard adventurerCard && adventurerCard.Item.isActiveAndEnabled)
    //    {
    //        RectTransform spotlightRect = card.GetComponent<RectTransform>();
    //        spotlightRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 220f);

    //        RectTransform itemHeaderRect = adventurerCard.Item.GetComponent<RectTransform>();
    //        itemHeaderRect.anchoredPosition = Vector2.zero;
    //    }
    //}

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
        print("begin drag, isDragging True");
        isDragging = true;
    }

    public void OnEndDrag()
    {
        print("end drag, isDragging False");
        isDragging = false;
    }
}
