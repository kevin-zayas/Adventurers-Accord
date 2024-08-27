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
    #endregion

    protected virtual void Awake()
    {
        canvas = GameObject.Find("Canvas");
    }

    protected virtual void Update()
    {
        if (isDragging)
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = (Vector2)worldPosition;
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        dropZone = collision.gameObject;
    }

    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == dropZone)       // only excecute logic if the card is leaving the dropZone it just entered
        {
            dropZone = null;
        }
    }

    /// <summary>
    /// Determines whether the drag operation can start. Derived classes should override this method to provide specific logic.
    /// </summary>
    protected virtual bool CanStartDrag()
    {
        if (Input.GetMouseButton(1)) return false; // Prevent dragging on right-click

        return true;
    }

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

    protected abstract void HandleEndDrag();

    protected virtual void ResetCardPosition()
    {
        transform.position = startPosition;
    }

    protected abstract void HandleCardMovement();
}
