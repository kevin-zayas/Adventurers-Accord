using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotlightCard : MonoBehaviour
{
    private GameObject canvas;
    private GameObject spotlightCard;
    private RectTransform cardRect;


    private void Awake()
    {
        canvas = GameObject.Find("Main Canvas");
        cardRect = gameObject.GetComponent<RectTransform>();
    }

    public void OnClick()
    {
        if (!spotlightCard)
        {
            Vector2 spawnPosition = gameObject.transform.position;

            if (gameObject.tag == "HandCard")
            {
                spawnPosition += new Vector2(0, 90);    // shift Card in Hand up to prevent cutoff when spotlighting
            }

            spotlightCard = Instantiate(gameObject, spawnPosition, Quaternion.identity);
            spotlightCard.transform.SetParent(canvas.transform, true);
            spotlightCard.layer = LayerMask.NameToLayer("Spotlight");

            //could check if quest card to make more efficient
            RectTransform spotlightRect = spotlightCard.GetComponent<RectTransform>();
            spotlightRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cardRect.rect.width);    // manually set size since it spawns with 0,0 in quest drop zone
            spotlightRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cardRect.rect.height);
            spotlightRect.localScale = new Vector2(2, 2);

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
            //spotlightCard = null;
        }
    }
}
