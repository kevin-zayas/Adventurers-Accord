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
    [SerializeField] private string dropZoneTag;

    [SerializeField] private Transform startParentTransform;
    [SerializeField] private Vector2 startPosition;

    private Vector3 enlargedScale = new(2f, 2f, 1);
    private Vector3 originalScale = new(1, 1, 1);
    private Vector3 currentScale;

    private void Awake()
    {
        card = this.GetComponent<Card>();
        canvas = GameObject.Find("Canvas");
        currentScale = transform.localScale;
    }

    private void Update()
    {
        if (isDragging)
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector2(worldPosition.x, worldPosition.y);
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
        if (Input.GetMouseButton(1)) return;      // prevent dragging if right-clicking
        if (!IsOwner) return;  // might not be neccessary if player can only drag on their turn

        if (GameManager.Instance.Turn != LocalConnection.ClientId) return;      //only allow player to drag cards on their turn
        if (card.IsDraftCard && GameManager.Instance.CurrentPhase != GameManager.Phase.Draft) return;   

        Player player = GameManager.Instance.Players[Owner.ClientId];

        if (!card.IsDraftCard || player.Gold >= card.Cost)         // only check for player gold if trying to drag a DraftCard
        {
            startPosition = transform.position;
            startParentTransform = transform.parent;
            isDragging = true;
            transform.SetParent(canvas.transform, true);
            transform.localScale = enlargedScale;
        }
    }

    public void EndDrag()
    {
        // set as first/last sibling? may  help if player wants to reorder cards
        if (!isDragging) return;

        isDragging = false;
        int slotIndex;

        if (!isOverDropZone || startParentTransform == dropZone.transform)          // dont update parent if dragging and dropping into same zone
        {
            card.ServerSetCardParent(startParentTransform, true);
            transform.position = startPosition;
            transform.localScale = currentScale;
            return;
        }

        if (card.IsDraftCard)
        {
            if (dropZoneTag == "Quest")                // prevent card moving from draft to quest location
            {
                transform.position = startPosition;
                card.ServerSetCardParent(startParentTransform, true);
                transform.localScale = originalScale;
                return;
            }
            else if (dropZoneTag == "Hand")
            {
                Player player = GameManager.Instance.Players[Owner.ClientId];
                slotIndex = card.draftCardIndex;
                //card.slotIndex = -1;                      // TODO: either make this ServerRpc or use availableCardSlots to draw new cards

                card.ServerSetCardParent(dropZone.transform, false);
                card.ServerSetCardOwner(dropZone.GetComponent<Hand>().controllingPlayer);

                card.transform.localScale = enlargedScale;
                currentScale = enlargedScale;

                player.ServerChangeGold(-card.Cost);
                Board.Instance.ReplaceCard(slotIndex);
                GameManager.Instance.EndTurn(false);
            }
        }
        else
        {
            if (GameManager.Instance.CurrentPhase == GameManager.Phase.Dispatch)
            {
                card.ServerSetCardParent(dropZone.transform, false);
                if (dropZoneTag == "Quest")
                {
                    dropZone.transform.parent.GetComponent<QuestLane>().ServerUpdatePower();
                    transform.localScale = originalScale;
                    currentScale = originalScale;
                }
                else
                {
                    startParentTransform.parent.GetComponent<QuestLane>().ServerUpdatePower();
                    transform.localScale = enlargedScale;
                    currentScale = enlargedScale;
                }
            }
            else
            {
                card.ServerSetCardParent(startParentTransform, true);
                transform.position = startPosition;
                transform.localScale = currentScale;
                return;
            }
            
        }
    }
}
