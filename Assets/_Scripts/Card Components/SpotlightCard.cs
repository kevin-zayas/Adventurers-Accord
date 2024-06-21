using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotlightCard : MonoBehaviour
{
    private GameObject canvas;
    private GameObject spotlightCard;
    private RectTransform cardRect;
    private float width;
    private float height;


    private void Awake()
    {
        canvas = GameObject.Find("Canvas");
        cardRect = gameObject.GetComponent<RectTransform>();
        width = cardRect.rect.width;
        height = cardRect.rect.height;
        
    }

    private void Update()
    {
        //TODO: find out what is causing spotlight to be inactive when initially spawned
        if (spotlightCard)
        {
            spotlightCard.SetActive(true);

            AdventurerCard card = spotlightCard.GetComponent<AdventurerCard>();

            if (card && card.HasItem) card.Item.gameObject.SetActive(true);
        }
        
    }

    public void OnHover()
    {
        if (!spotlightCard)
        {
            Vector2 spawnPosition = gameObject.transform.position;

            spotlightCard = Instantiate(gameObject, spawnPosition, Quaternion.identity);
            spotlightCard.transform.SetParent(canvas.transform, true);
            spotlightCard.layer = LayerMask.NameToLayer("Spotlight");

            RectTransform spotlightRect = spotlightCard.GetComponent<RectTransform>();
            spotlightRect.localScale = new Vector2(2f, 2f);

            CanvasGroup canvasGroup = spotlightCard.GetComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;     // turn off to allow player to click on card to hide spotlight
        }
        else
        {
            DestroySpotlightCard();
        }

    }

    public void DestroySpotlightCard()
    {
        if (spotlightCard)
        {
            Destroy(spotlightCard);
        }
    }
}
