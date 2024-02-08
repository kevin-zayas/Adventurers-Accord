using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragDrop : MonoBehaviour
{
    private bool isDragging = false;
    private bool isOverDropZone = false;
    private GameObject dropZone;
    private Vector2 startPosition;
    private GameManager gm;

    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
        //print(gm);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDragging)
        {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
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
        isDragging = true;
    }


    public void EndDrag()
    {
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
            transform.position = startPosition;
        }
    }
}
