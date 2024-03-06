using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDragDrop : NetworkBehaviour
{
    [SerializeField] private bool isDragging = false;

    [SerializeField] private ItemCard item;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject adventurerCard;
    [SerializeField] private string dropZoneTag;

    [SerializeField] private Transform startParentTransform;
    [SerializeField] private Vector2 startPosition;

    private Vector3 enlargedScale = new(2f, 2f, 1);
    private Vector3 originalScale = new(1, 1, 1);

    private void Awake()
    {
        item = this.GetComponent<ItemCard>();
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
        dropZoneTag = collision.gameObject.tag;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == adventurerCard)       // only excecute logic if the card is leaving the dropZone it just entered
        {
            adventurerCard = null;
            dropZoneTag = null;
        }
    }

    public void BeginDrag()
    {
        if (Input.GetMouseButton(1)) return;      // prevent dragging if right-clicking
        //if (!IsOwner) return;                   // might not be neccessary if player can only drag on their turn
        if (item.IsEquipped) return;

        if (GameManager.Instance.Turn != LocalConnection.ClientId) return;      //only allow player to drag cards on their turn
        //if (card.IsDraftCard && GameManager.Instance.CurrentPhase != GameManager.Phase.Draft) return;

        //Player player = GameManager.Instance.Players[Owner.ClientId];

        //if (!card.IsDraftCard || player.Gold >= card.Cost)
        //{
        startPosition = transform.position;
        startParentTransform = transform.parent;
        isDragging = true;

        transform.SetParent(canvas.transform, true);
        transform.localScale = enlargedScale;
        //}
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

        if (card.IsDraftCard || card.HasItem)  // check if over owned card
        {
            print("not over adventurer card");
            ResetCardPosition();
            return;
        }

        if (item.MagicalPower > 0 && card.OriginalMagicalPower == 0 || item.PhysicalPower > 0 && card.OriginalPhysicalPower == 0)
        { 
            ResetCardPosition();
            return;
        }
        //card.ServerChangeMagicalPower(item.MagicalPower);
        //card.ServerChangePhysicalPower(item.PhysicalPower);
        card.ServerSetItem(true,this.GetComponent<ItemCard>());
        this.Despawn();
    }

    private void ResetCardPosition()
    {
        item.ServerSetCardParent(startParentTransform, true);
        transform.position = startPosition;
    }

    private void AssignCardToPlayer()
    {
        Player player = GameManager.Instance.Players[Owner.ClientId];
        item.ServerSetCardScale(enlargedScale);

        item.ServerSetCardParent(adventurerCard.transform, false);
        item.ServerSetCardOwner(adventurerCard.GetComponent<Hand>().controllingPlayer);


        //player.ServerChangeGold(-card.Cost);
        //Board.Instance.ReplaceCard(card.draftCardIndex);
        GameManager.Instance.EndTurn(false);
    }

    private void HandleOwnedCardMovement()
    {
        QuestLane questLane;

        if (GameManager.Instance.CurrentPhase != GameManager.Phase.Dispatch)
        {
            print("cant move card during this phase");
            ResetCardPosition();
            return;
        }

        if (dropZoneTag == "Quest")
        {
            questLane = adventurerCard.transform.parent.GetComponent<QuestLane>();
            item.ServerSetCardScale(originalScale);
        }
        else
        {
            questLane = startParentTransform.parent.GetComponent<QuestLane>();
            item.ServerSetCardScale(enlargedScale);
        }
        item.ServerSetCardParent(adventurerCard.transform, false);
        questLane.ServerUpdatePower();
    }
}
