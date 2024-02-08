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

    private GameManager gm;

    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        canvas = GameObject.Find("Main Canvas");
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
    }


    private void OnCollisionExit2D(Collision2D collision)
    {
        isOverDropZone = false;
        dropZone = null;
    }

    public void BeginDrag()
    {
        startPosition = transform.position;
        startParent = transform.parent.gameObject;
        isDragging = true;
    }


    public void EndDrag()
    {
        // set as first/last sibling? may  help if player wants to reorder cards

        isDragging = false;
        if (isOverDropZone && transform.parent != dropZone.transform)   // no need if dragging and dropping into same zone
        {
            transform.SetParent(dropZone.transform,false);

            int slotIndex = this.GetComponent<DisplayCard>().slotIndex;
            this.GetComponent<DisplayCard>().slotIndex = -1;
            gm.ReplaceCard(slotIndex);
        }
        else
        {
            transform.SetParent(startParent.transform, false);
            transform.position = startPosition;
            

        }
    }
}
