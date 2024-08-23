using FishNet.Object;
using UnityEngine;

public class AdventurerDragDrop : NetworkBehaviour
{
    #region Serialized Fields
    [SerializeField] private bool isDragging = false;

    [SerializeField] private AdventurerCard card;
    [SerializeField] private GameObject canvas;
    [SerializeField] private Player player;

    [SerializeField] private GameObject dropZone;
    [SerializeField] private Transform startParentTransform;
    [SerializeField] private Vector2 startPosition;
    #endregion

    private void Start()
    {
        card = GetComponent<AdventurerCard>();
        canvas = GameObject.Find("Canvas");

        if (!IsClient) return;

        if (player == null)
        {
            player = GameManager.Instance.Players[LocalConnection.ClientId];
        }
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
        if (collision.gameObject.CompareTag("Quest") && GameManager.Instance.CurrentPhase != GameManager.Phase.Dispatch) return;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Magic Items")) return; // Prevent dragging onto Magic Item

        dropZone = collision.gameObject;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == dropZone)       // only excecute logic if the card is leaving the dropZone it just entered
        {
            dropZone = null;
        }
    }

    /// <summary>
    /// Begins the drag operation for the card.
    /// </summary>
    public void BeginDrag()
    {
        if (Input.GetMouseButton(1)) return; // Prevent dragging on right-click
        if (!card.IsDraftCard && !IsOwner) return; // Prevent dragging non-draft cards if not owner
        if (GameManager.Instance.CurrentPhase == GameManager.Phase.Resolution || GameManager.Instance.CurrentPhase == GameManager.Phase.GameOver) return;
        if (!player.IsPlayerTurn) return; // Allow dragging only during player's turn
        if (card.IsDraftCard && GameManager.Instance.CurrentPhase != GameManager.Phase.Recruit) return;

        if (!card.IsDraftCard || player.Gold >= card.Cost) // Check player gold if dragging a DraftCard
        {
            startPosition = transform.position;
            startParentTransform = transform.parent;
            isDragging = true;

            transform.SetParent(canvas.transform, true);
            transform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Ends the drag operation for the card, handling card placement and validation.
    /// </summary>
    public void EndDrag()
    {
        if (!isDragging) return;

        isDragging = false;

        if (dropZone == null || startParentTransform == dropZone.transform)
        {
            print("not over valid drop zone or still in starting zone");
            ResetCardPosition();
            return;
        }

        if (card.IsDraftCard)
        {
            AssignDraftCardToPlayer();
        }
        else
        {
            HandleCardMovement();
        }
    }

    /// <summary>
    /// Resets the card's position to its original location before dragging.
    /// </summary>
    private void ResetCardPosition()
    {
        card.ServerSetCardParent(startParentTransform, true);
        transform.position = startPosition;
    }

    /// <summary>
    /// Assigns the draft card to the player, updating the game state accordingly.
    /// </summary>
    private void AssignDraftCardToPlayer()
    {
        CardSlot cardSlot = startParentTransform.GetComponent<CardSlot>();

        card.ServerSetCardParent(dropZone.transform, false);
        card.ServerSetCardOwner(player);

        player.ServerChangePlayerGold(-card.Cost);
        Board.Instance.ReplaceDraftCard(cardSlot.SlotIndex);
        GameManager.Instance.EndTurn(false);
    }

    /// <summary>
    /// Handles the movement of the card to a new drop zone.
    /// </summary>
    private void HandleCardMovement()
    {
        card.ServerSetCardParent(dropZone.transform, false);
    }
}
