using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragDrop : NetworkBehaviour
{
    [SerializeField] private bool isDragging = false;

    [SerializeField] private bool isOverDropZone = false;

    [SerializeField] private Card card;

    [SerializeField] private GameObject canvas;

    [SerializeField] private GameObject dropZone;

    [SerializeField] private Transform startParentTransform;

    [SerializeField] private Vector2 startPosition;

    [SerializeField] private string dropZoneTag;



    private void Awake()
    {
        card = this.GetComponent<Card>();
        canvas = GameObject.Find("Canvas");
    }

    private void Update()
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
        dropZoneTag = collision.gameObject.tag;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == dropZone)       // only excecute logic if the card is leaving the dropZone it just entered
        {
            isOverDropZone = false;
            dropZone = null;
            dropZoneTag = null;
        }
    }

    public void BeginDrag()
    {
        //if (gameObject.tag != "DraftCard" || gm.player.currentGold >= cardDisplay.cost)     // only check for player gold if trying to drag a DraftCard
        //{
        startPosition = transform.position;
        startParentTransform = transform.parent;
        isDragging = true;
        transform.SetParent(canvas.transform, true);
        //}
    }

    public void EndDrag()
    {
        // set as first/last sibling? may  help if player wants to reorder cards

        if (!isDragging) return;

        isDragging = false;
        int slotIndex;

        if (isOverDropZone && startParentTransform != dropZone.transform)   // dont update parent if dragging and dropping into same zone
        {
            if (gameObject.tag == "DraftCard")
            {
                if (dropZoneTag == "Quest")                // prevent card moving from draft to quest location
                {
                    transform.position = startPosition;
                    card.ServerSetCardParent(startParentTransform, true);
                    return;
                }
                else if (dropZoneTag == "Hand")
                {
                    slotIndex = card.slotIndex;
                    card.slotIndex = -1;
                    if (Board.Instance == null) print("Board is null");

                    //gm.player.currentGold -= cardDisplay.cost;
                    //gm.goldChange.Invoke();
                    print("94");

                    //ServerAssignCardParent(dropZone.transform, dropZoneTag);
                    card.ServerSetCardParent(dropZone.transform, false);
                    Board.Instance.ReplaceCard(slotIndex);
                }
            }
            else
            {
                print("101");
                card.ServerSetCardParent(dropZone.transform, false);
                //gm.questCardChange.Invoke();
            }
        }
        else
        {
            card.ServerSetCardParent(startParentTransform, true);
            transform.position = startPosition;


        }
    }

    //[ServerRpc(RequireOwnership = false)]
    //private void ServerResetCardPosition(Transform startParent)
    //{
    //    card.parent = null;  // this is a hack to get around the fact that the syncvar is not being updated
    //    print("Resetting position");
    //    //transform.SetParent(startParent, false);
    //    card.parent = startParent;
    //    transform.position = startPosition;
    //}

    //[ServerRpc(RequireOwnership = false)]
    //private void ServerAssignCardParent(Transform parent, string tag)
    //{
    //    print("Assigning parent");
    //    if (parent == null) print("Parent is null");
    //    //transform.SetParent(dropZone.transform, false);
    //    card.parent = parent;
    //    gameObject.tag = tag + "Card";
    //}
}
