using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellDragDrop : NetworkBehaviour
{
    [SerializeField] private bool isDragging = false;

    [SerializeField] private SpellCard spellCard;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject dropZone;
    [SerializeField] private string dropZoneTag;

    [SerializeField] private Transform startParentTransform;
    [SerializeField] private Vector2 startPosition;

    private void Awake()
    {
        spellCard = this.GetComponent<SpellCard>();
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
        print(IsOwner);
        if (!IsOwner) return;

        //only allow player to drag cards during dispatch on their turn or during magic phase
        if ((GameManager.Instance.CurrentPhase != GameManager.Phase.Dispatch || GameManager.Instance.Turn != LocalConnection.ClientId) &&
            GameManager.Instance.CurrentPhase != GameManager.Phase.Magic)
        {
            print("not your turn or cant move spells during this phase");
            return;
        }

        //Player player = GameManager.Instance.Players[LocalConnection.ClientId];
  
        startPosition = transform.position;
        startParentTransform = transform.parent;
        isDragging = true;

        transform.SetParent(canvas.transform, true);
        transform.localScale = new Vector3(2f, 2f, 1f);

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

        //if ((GameManager.Instance.CurrentPhase != GameManager.Phase.Dispatch || GameManager.Instance.Turn != LocalConnection.ClientId) &&
        //    GameManager.Instance.CurrentPhase != GameManager.Phase.Magic)
        //{
        //    print("not your turn or cant move spells during this phase");
        //    ResetCardPosition();
        //    return;
        //}
        HandleCardMovement();

    }

    private void ResetCardPosition()
    {
        spellCard.ServerSetCardParent(startParentTransform, true);
        transform.position = startPosition;
    }

    private void HandleCardMovement()
    {
        QuestLane questLane;

        if (dropZoneTag == "Quest")
        {
            questLane = dropZone.transform.parent.GetComponent<QuestLane>();
            spellCard.ServerSetCardParent(dropZone.transform, false);
        }
        else
        {
            questLane = startParentTransform.parent.GetComponent<QuestLane>();
            spellCard.ServerSetCardParent(dropZone.transform, false);
        }
        
        questLane.ServerUpdatePower();
    }
}
