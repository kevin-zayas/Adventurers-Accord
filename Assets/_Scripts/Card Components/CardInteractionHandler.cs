using DG.Tweening;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public abstract class CardInteractionHandler : NetworkBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    #region Serialized Fields
    [SerializeField] protected bool isDragging = false;

    [SerializeField] protected GameObject canvas;
    [SerializeField] protected Canvas cardCanvas;
    [SerializeField] protected GameObject dropZone;
    [SerializeField] protected Transform startParentTransform;
    [SerializeField] protected Vector2 startPosition;
    [SerializeField] protected Player player;
    [SerializeField] protected Card card;

    [SerializeField] private float _animationDuration = 0.75f;
    #endregion

    [HideInInspector] public bool wasDragged;
    private Vector3 offset;

    [Header("Movement")]
    [SerializeField] private float moveSpeedLimit = 6000;

    [Header("Events")] 
    [HideInInspector] public UnityEvent<CardInteractionHandler> PointerEnterEvent;
    [HideInInspector] public UnityEvent<CardInteractionHandler> PointerExitEvent;
    [HideInInspector] public UnityEvent<CardInteractionHandler, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<CardInteractionHandler> PointerDownEvent;
    [HideInInspector] public UnityEvent<CardInteractionHandler> BeginDragEvent;
    [HideInInspector] public UnityEvent<CardInteractionHandler, bool> EndDragEvent;
    [HideInInspector] public UnityEvent<CardInteractionHandler, bool> SelectEvent;
    

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
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = worldPosition;
        }
        //ClampPosition();

        //if (isDragging)
        //{
        //    Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) - offset;
        //    Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        //    Vector2 velocity = direction * Mathf.Min(moveSpeedLimit, Vector2.Distance(transform.position, targetPosition) / Time.deltaTime);
        //    transform.Translate(velocity * Time.deltaTime);
        //}
    }

    void ClampPosition()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
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
    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDragEvent.Invoke(this);

        //startPosition = transform.position;
        startParentTransform = transform.parent.CompareTag("Slot") ? transform.parent.parent : transform.parent;
        //transform.SetParent(canvas.transform, true);

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = mousePosition - (Vector2)transform.position;
        isDragging = true;
        cardCanvas.overrideSorting = true;
        cardCanvas.sortingOrder = 100;
        //canvas.GetComponent<GraphicRaycaster>().enabled = false;
        //imageComponent.raycastTarget = false;

        wasDragged = true;

    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        
        if (dropZone == null || startParentTransform == dropZone.transform)
        {
            EndDragEvent.Invoke(this, true);

            if (!transform.parent.CompareTag("Slot"))       //only works for quests (until card holder is added)
            {
                PopUpManager.Instance.CreateToastPopUp("Invalid placement");
                ResetCardPosition();
            }
            return;
        }

        HandleEndDrag();
        //EndDragEvent.Invoke(this, false);

        //canvas.GetComponent<GraphicRaycaster>().enabled = true;
        //imageComponent.raycastTarget = true;

        StartCoroutine(FrameWait());

        IEnumerator FrameWait()
        {
            yield return new WaitForEndOfFrame();
            wasDragged = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterEvent.Invoke(this);
        //isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExitEvent.Invoke(this);
        //isHovering = false;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    /// <summary>
    /// Determines whether the drag operation can start. Derived classes should override this method to provide specific logic.
    /// </summary>
    /// <returns>True if the drag can start, otherwise false.</returns>
    protected virtual bool CanStartDrag()
    {
        if (card.IsClone) return false;
        if (Input.GetMouseButton(1)) return false; // Prevent dragging on right-click
        if (!card.IsDraftCard.Value && !IsOwner) // Prevent dragging non-draft cards if not owner
        {
            PopUpManager.Instance.CreateToastPopUp("You cannot move another player's card");
            return false;
        }
        if (card.IsDraftCard.Value && !player.IsPlayerTurn.Value)
        {
            PopUpManager.Instance.CreateToastPopUp("You can only purchase cards on your turn");
            return false;
        }
        if (card.IsDraftCard.Value && player.Gold.Value < card.Cost.Value) // Check player gold if dragging a DraftCard
        {
            PopUpManager.Instance.CreateToastPopUp("Insufficient Gold");
            return false;
        }
        if (player.IsAnimating) return false;
        return true;
    }

    /// <summary>
    /// Begins the drag operation for the card.
    /// </summary>
    //public virtual void BeginDrag()
    //{
    //    startPosition = transform.position;
    //    startParentTransform = transform.parent;
    //    isDragging = true;

    //    transform.SetParent(canvas.transform, true);
    //}

    /// <summary>
    /// Ends the drag operation for the card, handling card placement and validation.
    /// </summary>
    public virtual void EndDrag()
    {
        //if (!isDragging) return;

        //isDragging = false;

        //if (dropZone == null || startParentTransform == dropZone.transform)
        //{
        //    PopUpManager.Instance.CreateToastPopUp("Invalid placement");
        //    ResetCardPosition();
        //    return;
        //}

        //HandleEndDrag();
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
        transform.localPosition = Vector3.zero;
    }


    protected void OnCardPurchase()
    {
        player.SetIsAnimating(true);
        ServerPlayPurchaseAnimation(player.PlayerID.Value);
    }

    /// <summary>
    /// Assigns the draft card to the player, updating the game state accordingly.
    /// </summary>
    protected virtual void AssignDraftCardToPlayer()
    {
        DraftCardSlot cardSlot = startParentTransform.GetComponent<DraftCardSlot>();

        card.ServerSetCardOwner(player);
        //card.ServerSetCardParent(dropZone.transform, false);
        cardSlot.MoveCard(card, Hand.Instance.transform);
        //Hand.Instance.AddCard(card);
        card.transform.localPosition = Vector3.zero;

        player.ServerChangePlayerGold(-card.Cost.Value);
        Board.Instance.ServerReplaceDraftCard(cardSlot.SlotIndex);
        GameManager.Instance.EndTurn(false);
    }

    [ServerRpc(RequireOwnership = false)]
    protected void ServerPlayPurchaseAnimation(int playerID)
    {
        ObserversPlayMoveAnimation(playerID);
    }

    [ObserversRpc]
    protected void ObserversPlayMoveAnimation(int playerID)
    {
        Transform guildTransform = Board.Instance.GuildStatusList[playerID].gameObject.transform;
        Sequence sequence = DOTween.Sequence();
        if (LocalConnection.ClientId != playerID)
        {

            sequence.Append(transform.DOMove(guildTransform.position, _animationDuration).SetEase(Ease.OutSine));
            sequence.Join(transform.DOScale(Vector3.zero, _animationDuration).SetEase(Ease.InQuad));

        }
        else
        {
            sequence.Append(transform.DOJump(transform.position, 10f, 1, _animationDuration));
            sequence.OnComplete(() =>
            {
                AssignDraftCardToPlayer();
                player.SetIsAnimating(false);
            });
        }

        sequence.Play();
    }

    public int SiblingAmount()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.parent.childCount - 1 : 0;
    }

    public int ParentIndex()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.GetSiblingIndex() : 0;
    }

    public float NormalizedPosition()
    {
        return transform.parent.CompareTag("Slot") ? ExtensionMethods.Remap((float)ParentIndex(), 0, (float)(transform.parent.parent.childCount - 1), 0, 1) : 0;
    }
}
