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
        dropZone = collision.gameObject;
    }

    //private void OnCollisionStay2D(Collision2D collision)
    //{
    //    if (dropZone != null) return;        
    //    dropZone = collision.gameObject;
    //}

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == dropZone)       // only excecute logic if the card is leaving the dropZone it just entered
        {
            dropZone = null;
        }
    }

    public void BeginDrag()
    {
        if (Input.GetMouseButton(1)) return;      // prevent dragging if right-clicking
        if (!IsOwner) return;
        if (transform.parent.CompareTag("Quest")) return;      // prevent dragging if card is already in a quest lane

        //only allow player to drag cards during dispatch or magic phase
        if (GameManager.Instance.CurrentPhase == GameManager.Phase.Draft)       // Should spells be used during Resolution phase?
        {
            print("cant move spells during this phase");
            return;
        }
  
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

        QuestLane questLane = dropZone.transform.parent.GetComponent<QuestLane>();
        if (questLane.QuestLocation.QuestCard.BlockSpells)
        {
            print("Spells cant be used on this Quest");
            ResetCardPosition();
            return;
        }

        HandleCardMovement();
    }

    private void ResetCardPosition()
    {
        spellCard.ServerSetCardParent(startParentTransform, true);
        transform.position = startPosition;
    }

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
