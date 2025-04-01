using FishNet.Object;
using UnityEngine;

public abstract class CardDragDrop : NetworkBehaviour
{
    #region Serialized Fields
    [SerializeField] protected bool isDragging = false;

    [SerializeField] protected GameObject canvas;
    [SerializeField] protected GameObject dropZone;
    [SerializeField] protected Transform startParentTransform;
    [SerializeField] protected Vector2 startPosition;
    [SerializeField] protected Player player;
    [SerializeField] protected Card card;
    #endregion

    protected virtual void Awake()
    {
        canvas = GameObject.Find("Canvas");
    }

    protected virtual void Start()
    {
        if (!IsClientStarted) return;

        if (player == null)
        {
            player = GameManager.Instance.Players[LocalConnection.ClientId];
        }
    }

    /// <summary>
    /// Updates the position of the card while it is being dragged.
    /// </summary>
    protected virtual void Update()
    {
        if (isDragging)
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = (Vector2)worldPosition;
        }
    }

    /// <summary>
    /// Handles the collision enter event to detect potential drop zones.
    /// </summary>
    /// <param name="collision">The collision data associated with this event.</param>
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        dropZone = collision.gameObject;
        //print(dropZone);
    }

    /// <summary>
    /// Handles the collision exit event to clear the drop zone if the card exits it.
    /// </summary>
    /// <param name="collision">The collision data associated with this event.</param>
    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == dropZone)       // only excecute logic if the card is leaving the dropZone it just entered
        {
            dropZone = null;
            //print("exiting dropzone");
        }
    }

    /// <summary>
    /// Determines whether the drag operation can start. Derived classes should override this method to provide specific logic.
    /// </summary>
    /// <returns>True if the drag can start, otherwise false.</returns>
    protected virtual bool CanStartDrag()
    {
        if (Input.GetMouseButton(1)) return false; // Prevent dragging on right-click
        if (!card.IsDraftCard.Value && !IsOwner) return false; // Prevent dragging non-draft cards if not owner
        if (card.IsDraftCard.Value && !player.IsPlayerTurn.Value) return false;
        if (GameManager.Instance.CurrentPhase.Value == GameManager.Phase.GameOver) return false; // Prevent dragging after the game ends
        if (GameManager.Instance.CurrentPhase.Value == GameManager.Phase.Resolution) return false;
        if (card.IsDraftCard.Value && player.Gold.Value < card.Cost.Value) return false;    // Check player gold if dragging a DraftCard
        return true;
    }

    /// <summary>
    /// Begins the drag operation for the card.
    /// </summary>
    public virtual void BeginDrag()
    {
        startPosition = transform.position;
        startParentTransform = transform.parent;
        isDragging = true;

        transform.SetParent(canvas.transform, true);
    }

    /// <summary>
    /// Ends the drag operation for the card, handling card placement and validation.
    /// </summary>
    public virtual void EndDrag()
    {
        if (!isDragging) return;

        isDragging = false;

        if (dropZone == null || startParentTransform == dropZone.transform)
        {
            Debug.Log("Not over a valid drop zone or still in the starting zone");
            ResetCardPosition();
            return;
        }

        HandleEndDrag();
    }

    /// <summary>
    /// Handles the specific logic when the drag operation ends. Must be implemented by derived classes.
    /// </summary>
    protected abstract void HandleEndDrag();

    /// <summary>
    /// Resets the card's position to its original location before dragging.
    /// </summary>
    protected virtual void ResetCardPosition()
    {
        transform.position = startPosition;
    }

    /// <summary>
    /// Assigns the draft card to the player, updating the game state accordingly.
    /// </summary>
    protected virtual void AssignDraftCardToPlayer()
    {
        CardSlot cardSlot = startParentTransform.GetComponent<CardSlot>();

        card.ServerSetCardOwner(player);
        card.ServerSetCardParent(dropZone.transform, false);
        player.ServerChangePlayerGold(-card.Cost.Value);
        Board.Instance.ReplaceDraftCard(cardSlot.SlotIndex);
        GameManager.Instance.EndTurn(false);
    }
}
