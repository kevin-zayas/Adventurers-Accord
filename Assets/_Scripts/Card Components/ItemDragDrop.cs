using FishNet.Object;
using UnityEngine;

public class ItemDragDrop : NetworkBehaviour
{
    #region Serialized Fields
    [SerializeField] private bool isDragging = false;

    [SerializeField] private ItemCard itemCard;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject adventurerCard;

    [SerializeField] private Transform startParentTransform;
    [SerializeField] private Vector2 startPosition;
    #endregion

    private void Awake()
    {
        itemCard = GetComponent<ItemCard>();
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
        adventurerCard = collision.gameObject;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == adventurerCard)       // only excecute logic if the card is leaving the collision it just entered
        {
            adventurerCard = null;
        }
    }

    /// <summary>
    /// Begins the drag operation for the item card.
    /// </summary>
    public void BeginDrag()
    {
        if (Input.GetMouseButton(1)) return; // Prevent dragging on right-click
        if (GameManager.Instance.CurrentPhase == GameManager.Phase.GameOver) return;

        startPosition = transform.position;
        startParentTransform = transform.parent;
        isDragging = true;

        transform.SetParent(canvas.transform, true);
    }

    /// <summary>
    /// Ends the drag operation for the item card, handling card placement and validation.
    /// </summary>
    public void EndDrag()
    {
        if (!isDragging) return;

        isDragging = false;

        if (adventurerCard == null)
        {
            Debug.Log("No adventurer card selected.");
            ResetCardPosition();
            return;
        }

        AdventurerCard card = adventurerCard.GetComponent<AdventurerCard>();

        if (card.IsDraftCard || card.HasItem || !card.IsOwner)
        {
            Debug.Log("Cannot equip item: card already has an item, is a draft card, or is not owned by the player.");
            ResetCardPosition();
            return;
        }

        if (card.ParentTransform.CompareTag("Quest"))
        {
            Debug.Log("Cannot equip item: card is currently on a quest.");
            ResetCardPosition();
            return;
        }

        if ((itemCard.MagicalPower > 0 && card.OriginalMagicalPower == 0) || (itemCard.PhysicalPower > 0 && card.OriginalPhysicalPower == 0))
        {
            Debug.Log("Cannot equip item: card does not have the required power type.");
            ResetCardPosition();
            return;
        }

        ConfirmationPopUp popUp = PopUpManager.Instance.CreateConfirmationPopUp(true);
        popUp.InitializeEquipItemPopUp(card, this.gameObject);
    }

    /// <summary>
    /// Resets the item card's position to its original location before dragging.
    /// </summary>
    private void ResetCardPosition()
    {
        itemCard.ServerSetCardParent(startParentTransform, true);
        transform.position = startPosition;
    }
}
