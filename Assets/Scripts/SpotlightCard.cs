using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotlightCard : MonoBehaviour
{
    private GameObject canvas;
    private GameObject spotlightCard;



    private void Awake()
    {
        canvas = GameObject.Find("Main Canvas");
    }

    public void OnClick()
    {
        if (!spotlightCard)
        {
            spotlightCard = Instantiate(gameObject, new Vector2(Input.mousePosition.x, Input.mousePosition.y), Quaternion.identity);
            spotlightCard.transform.SetParent(canvas.transform, true);
            spotlightCard.layer = LayerMask.NameToLayer("Spotlight");

            RectTransform rect = spotlightCard.GetComponent<RectTransform>();
            rect.localScale = new Vector2(2, 2);

            CanvasGroup canvasGroup = spotlightCard.GetComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            Destroy(spotlightCard);
        }

    }

    public void OnHoverExit()
    {
        if (spotlightCard)
        {
            Destroy(spotlightCard);
        }
    }
}
