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
        //TODO: find out what is causing spotlight to be inactive
        if (spotlightCard) spotlightCard.SetActive(true);
    }

    public void OnClick()
    {
        if (!spotlightCard)
        {
            Vector2 spawnPosition = gameObject.transform.position;

            spotlightCard = Instantiate(gameObject, spawnPosition, Quaternion.identity);
            spotlightCard.transform.SetParent(canvas.transform, true);
            spotlightCard.layer = LayerMask.NameToLayer("Spotlight");

            Card card = spotlightCard.GetComponent<Card>();
            if (card && card.HasItem) 
            {
                //print(card.transform.GetChild(1));       // show item card
                //card.transform.GetChild(1).gameObject.SetActive(true);
                //print(card.transform.GetChild(1).gameObject.activeSelf);
                //card.transform.GetChild(1).GetComponent<SpotlightCard>().OnClick();
            }

            RectTransform spotlightRect = spotlightCard.GetComponent<RectTransform>();
            //spotlightRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);   
            //spotlightRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);       // set card to original size in case an item was equipped

            
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
