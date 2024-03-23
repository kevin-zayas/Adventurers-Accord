using FishNet.Object;
using UnityEngine;

public class AdventurerDragDrop : NetworkBehaviour
{
    [SerializeField] private bool isDragging = false;

    [SerializeField] private Card card;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject dropZone;
    [SerializeField] private string dropZoneTag;

    [SerializeField] private Transform startParentTransform;
    [SerializeField] private Vector2 startPosition;

    private void Awake()
    {
        card = this.GetComponent<Card>();
        canvas = GameObject.Find("Canvas");
    }

    private void Update()
    {
        if (isDragging)
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = (Vector2)worldPosition;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Quest") && !collision.gameObject.CompareTag("Hand")) return;

        dropZone = collision.gameObject;
        dropZoneTag = collision.gameObject.tag;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == dropZone)       // only excecute logic if the card is leaving the dropZone it just entered
        {
            dropZone = null;
            dropZoneTag = null;
        }
    }

    public void BeginDrag()
    {
        if (Input.GetMouseButton(1)) return;      // prevent dragging if right-clicking
        if (!card.IsDraftCard && !IsOwner) return;

        if (GameManager.Instance.Turn != LocalConnection.ClientId) return;      //only allow player to drag cards on their turn
        if (card.IsDraftCard && GameManager.Instance.CurrentPhase != GameManager.Phase.Draft) return;   

        Player player = GameManager.Instance.Players[LocalConnection.ClientId];

        if (!card.IsDraftCard || player.Gold >= card.Cost)         // only check for player gold if trying to drag a DraftCard
        {
            startPosition = transform.position;
            startParentTransform = transform.parent;
            isDragging = true;

            transform.SetParent(canvas.transform, true);
            transform.localScale = new Vector3(2f, 2f, 1f);
        }
    }

    public void EndDrag()
    {
        // set as first/last sibling? may  help if player wants to reorder cards
        if (!isDragging) return;

        isDragging = false;

        if (dropZone == null || startParentTransform == dropZone.transform)          // dont update parent if dragging and dropping into same zone
        {
            print("not over drop zone or same zone");
            ResetCardPosition();
            return;
        }

        if (card.IsDraftCard)
        {
            if (dropZoneTag == "Quest")                // prevent card moving from draft to quest location
            {
                print("cant move draft card to quest");
                ResetCardPosition();
            }
            else if (dropZoneTag == "Hand") AssignDraftCardToPlayer();
        }
        else
        {
            HandleCardMovement();
        }
    }

    private void ResetCardPosition()
    {
        card.ServerSetCardParent(startParentTransform, true);
        transform.position = startPosition;
    }

    private void AssignDraftCardToPlayer()
    {
        Player player = GameManager.Instance.Players[LocalConnection.ClientId];
        CardSlot cardSlot = startParentTransform.GetComponent<CardSlot>();

        card.ServerSetCardParent(dropZone.transform, false);
        card.ServerSetCardOwner(player);

        player.ServerChangePlayerGold(-card.Cost);
        Board.Instance.ReplaceCard(cardSlot.SlotIndex);
        GameManager.Instance.EndTurn(false);
    }

    private void HandleCardMovement()
    {
        if (GameManager.Instance.CurrentPhase != GameManager.Phase.Dispatch)
        {
            print("cant move card during this phase");
            ResetCardPosition();
            return;
        }

        card.ServerSetCardParent(dropZone.transform, false);
    }
}
