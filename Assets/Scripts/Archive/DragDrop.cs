using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragDrop : MonoBehaviour
{
    private bool isDragging = false;
    private bool isOverDropZone = false;
    private GameObject dropZone;
    private GameObject canvas;
    private GameObject startParent;
    private Vector2 startPosition;
    private OldGameManager gm;

    private string dropZoneTag;
    private CardDisplay cardDisplay;

    private void Awake()
    {
        gm = FindObjectOfType<OldGameManager>();
        canvas = GameObject.Find("Main Canvas");
        cardDisplay = gameObject.GetComponent<CardDisplay>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDragging)
        {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            transform.SetParent(canvas.transform,true);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        isOverDropZone = true;
        dropZone = collision.gameObject;
        dropZoneTag = collision.gameObject.tag;
    }


    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == dropZone)
        {
            isOverDropZone = false;
            dropZone = null;
            dropZoneTag = null;
        }
    }

    public void BeginDrag()
    {
        if (gameObject.tag != "DraftCard" || gm.player.currentGold >= cardDisplay.cost)     // only check for player gold if trying to drag a DraftCard
        {
            startPosition = transform.position;
            startParent = transform.parent.gameObject;
            isDragging = true;
        }

    }


    public void EndDrag()
    {
        // set as first/last sibling? may  help if player wants to reorder cards

        if (!isDragging) return;        // wasn't able to begin dragging a card, quit early

        isDragging = false;
        int slotIndex;

        if (isOverDropZone && transform.parent != dropZone.transform)   // no need to update parent if dragging and dropping into same zone
        {
            if (gameObject.tag == "DraftCard")
            {
                if (dropZoneTag == "Quest")                // prevent card moving from draft to quest location
                {
                    ResetCardPosition();
                    return;
                }
                else if (dropZoneTag == "Hand")
                {
                    slotIndex = this.GetComponent<CardDisplay>().slotIndex;
                    this.GetComponent<CardDisplay>().slotIndex = -1;
                    gm.ReplaceCard(slotIndex);

                    gm.player.currentGold -= cardDisplay.cost;
                    gm.goldChange.Invoke();
                    AssignNewCardParent();
                }
            }
            else
            {
                AssignNewCardParent();
                gm.questCardChange.Invoke();
            }
        }
        else
        {
            ResetCardPosition();
        }
    }

    private void ResetCardPosition()
    {
        transform.SetParent(startParent.transform, false);
        transform.position = startPosition;
    }

    private void AssignNewCardParent()
    {
        transform.SetParent(dropZone.transform, false);
        gameObject.tag = dropZoneTag + "Card";
    }
}
