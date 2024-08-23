using FishNet.Object;
using UnityEngine;

public class SpellDragDrop : NetworkBehaviour
{
    #region Serialized Fields
    [SerializeField] private bool isDragging = false;

    [SerializeField] private SpellCard spellCard;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject dropZone;

    [SerializeField] private Transform startParentTransform;
    [SerializeField] private Vector2 startPosition;
    #endregion

    private void Awake()
    {
        spellCard = GetComponent<SpellCard>();
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
        dropZone = collision.gameObject;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == dropZone)
        {
            dropZone = null;
        }
    }

    /// <summary>
    /// Begins the drag operation for the spell card.
    /// </summary>
    public void BeginDrag()
    {
        if (Input.GetMouseButton(1)) return; // Prevent dragging on right-click
        if (!IsOwner) return; // Ensure the player owns the card before dragging
        if (transform.parent.CompareTag("Quest")) return; // Prevent dragging if the card is already in a quest lane

        // Only allow dragging during the Dispatch or Magic phase
        if (GameManager.Instance.CurrentPhase != GameManager.Phase.Dispatch &&
            GameManager.Instance.CurrentPhase != GameManager.Phase.Magic)
        {
            Debug.Log("Can't move spells during this phase");
            return;
        }

        startPosition = transform.position;
        startParentTransform = transform.parent;
        isDragging = true;

        transform.SetParent(canvas.transform, true);
    }

    /// <summary>
    /// Ends the drag operation for the spell card, handling card placement and validation.
    /// </summary>
    public void EndDrag()
    {
        if (!isDragging) return;

        isDragging = false;

        if (dropZone == null || startParentTransform == dropZone.transform) // Don't update parent if dragging and dropping into the same zone
        {
            Debug.Log("Not over a valid drop zone or still in the starting zone");
            ResetCardPosition();
            return;
        }

        QuestLane questLane = dropZone.transform.parent.GetComponent<QuestLane>();

        if (questLane.QuestCard.BlockSpells)
        {
            Debug.Log("Spells can't be used on this Quest");
            ResetCardPosition();
            return;
        }

        if (questLane.DropZone.transform.childCount == 0)
        {
            Debug.Log("Spells can't be used on a lane with no Adventurers");
            ResetCardPosition();
            return;
        }

        HandleCardMovement();
    }

    /// <summary>
    /// Resets the spell card's position to its original location before dragging.
    /// </summary>
    private void ResetCardPosition()
    {
        spellCard.ServerSetCardParent(startParentTransform, true);
        transform.position = startPosition;
    }

    /// <summary>
    /// Handles the movement of the spell card to a new drop zone and updates the quest lane.
    /// </summary>
    private void HandleCardMovement()
    {
        QuestLane questLane = dropZone.transform.parent.GetComponent<QuestLane>();
        spellCard.ServerSetCardParent(dropZone.transform, false);
        questLane.ServerUpdateSpellEffects();

        if (GameManager.Instance.CurrentPhase == GameManager.Phase.Magic)
        {
            GameManager.Instance.RefreshEndRoundStatus();
        }
    }
}
