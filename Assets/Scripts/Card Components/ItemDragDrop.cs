using FishNet.Object;
using UnityEngine;

public class ItemDragDrop : NetworkBehaviour
{
    [SerializeField] private bool isDragging = false;

    [SerializeField] private ItemCard itemCard;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject adventurerCard;

    [SerializeField] private Transform startParentTransform;
    [SerializeField] private Vector2 startPosition;

    private Vector3 enlargedScale = new(2f, 2f, 1);

    private void Awake()
    {
        itemCard = this.GetComponent<ItemCard>();
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
        if (collision.gameObject == adventurerCard)       // only excecute logic if the card is leaving the card it just entered
        {
            adventurerCard = null;
        }
    }

    public void BeginDrag()
    {
        if (Input.GetMouseButton(1)) return;      // prevent dragging if right-clicking           
        //if (item.IsEquipped) return;

        if (GameManager.Instance.CurrentPhase == GameManager.Phase.Dispatch && 
            GameManager.Instance.Turn != LocalConnection.ClientId) return;      //only allow player to drag cards on their turn when in dispatch phase
   
        startPosition = transform.position;
        startParentTransform = transform.parent;
        isDragging = true;

        transform.SetParent(canvas.transform, true);
        transform.localScale = enlargedScale;

    }

    public void EndDrag()
    {
        // set as first/last sibling? may  help if player wants to reorder cards
        if (!isDragging) return;

        isDragging = false;

        if (adventurerCard == null)
        {
            ResetCardPosition();
            return;
        }

        Card card = adventurerCard.GetComponent<Card>();

        if (card.IsDraftCard || card.HasItem || !card.IsOwner)
        {
            print("Card already has item or over unowned card");
            ResetCardPosition();
            return;
        }

        if (card.Parent.CompareTag("Quest"))       // if card is on a quest, don't allow item to be equipped
        {
            print("Card is on a quest");
            ResetCardPosition();
            return;
        }

        if (itemCard.MagicalPower > 0 && card.OriginalMagicalPower == 0 || itemCard.PhysicalPower > 0 && card.OriginalPhysicalPower == 0)
        { 
            print("Card does not have the required power type");
            ResetCardPosition();
            return;
        }

        card.ServerEquipItem(true,this.GetComponent<ItemCard>());
        this.GetComponent<ItemCard>().ServerDespawnItem();
    }

    private void ResetCardPosition()
    {
        itemCard.ServerSetCardParent(startParentTransform, true);
        transform.position = startPosition;
    }
}
