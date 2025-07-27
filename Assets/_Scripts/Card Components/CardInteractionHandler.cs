using DG.Tweening;
using FishNet.Object;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class CardInteractionHandler : NetworkBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    #region Serialized Fields
    [SerializeField] protected bool isDragging = false;

    //[SerializeField] protected GameObject canvas;
    [SerializeField] protected Canvas cardCanvas;
    [SerializeField] protected GameObject dropZone;
    [SerializeField] protected CardHolder originalCardHolder;
    [SerializeField] protected Transform originalCardSlot;  //might not need this
    [SerializeField] protected Vector2 startPosition;
    [SerializeField] protected Player player;
    [SerializeField] protected Card card;

    [SerializeField] private float _animationDuration = 0.75f;
    #endregion

    [HideInInspector] public bool wasDragged;
    private Vector3 offset;

    #region Movement Variables
    [Header("Movement")]
    [SerializeField] private float followSpeed = 15f;
    [SerializeField] private float tiltStrength = 8f;
    [SerializeField] private float tiltLerpSpeed = 10f;
    [SerializeField] private float maxTilt = 35f;
    private Vector3 lastPosition;
    #endregion

    #region Events
    [HideInInspector] public UnityEvent<CardInteractionHandler> PointerEnterEvent;
    [HideInInspector] public UnityEvent<CardInteractionHandler> PointerExitEvent;
    [HideInInspector] public UnityEvent<CardInteractionHandler, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<CardInteractionHandler> PointerDownEvent;
    [HideInInspector] public UnityEvent<CardInteractionHandler> BeginDragEvent;
    [HideInInspector] public UnityEvent<CardInteractionHandler, bool> EndDragEvent;
    [HideInInspector] public UnityEvent<CardInteractionHandler, bool> SelectEvent;
    #endregion


    protected virtual void Awake()
    {
        //canvas = GameObject.Find("Canvas");
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
        // TODO: Move this logic to OnDrag, section outlogic to be resuable for card swapping animations. or maybe have similar logic in card Holder
        if (isDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);

            // Smooth follow
            Vector3 clampedTarget = ClampToScreen(worldPosition);
            transform.position = Vector3.Lerp(transform.position, clampedTarget, Time.deltaTime * 15f);

            // Tilt based on movement delta
            Vector3 delta = transform.position - lastPosition;
            float rawTilt = -delta.x * tiltStrength;
            float targetZRotation = Mathf.Clamp(rawTilt, -maxTilt, maxTilt);

            Quaternion targetRotation = Quaternion.Euler(0, 0, targetZRotation);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * tiltLerpSpeed);

            lastPosition = transform.position;
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * tiltLerpSpeed);
        }
    }

    Vector3 ClampToScreen(Vector2 targetPosition)
    {
        // Get screen bounds in world units
        Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Mathf.Abs(Camera.main.transform.position.z)));
        Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Mathf.Abs(Camera.main.transform.position.z)));

        // Get card's half size
        Vector2 halfSize = Vector2.zero;
        if (TryGetComponent(out RectTransform rect))
        {
            halfSize = rect.rect.size * 0.5f * rect.lossyScale;
        }

        float minX = bottomLeft.x + halfSize.x;
        float maxX = topRight.x - halfSize.x;
        float minY = bottomLeft.y + halfSize.y;
        float maxY = topRight.y - halfSize.y;

        float clampedX = Mathf.Clamp(targetPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(targetPosition.y, minY, maxY);

        return new Vector3(clampedX, clampedY, transform.position.z);
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
        if (!CanStartDrag()) return;
        BeginDragEvent.Invoke(this);

        originalCardSlot = transform.parent;
        originalCardHolder = originalCardSlot.parent.GetComponent<CardHolder>();

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = mousePosition - (Vector2)transform.position;
        isDragging = true;
        cardCanvas.overrideSorting = true;
        cardCanvas.sortingOrder = 100;

        card.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        //canvas.GetComponent<GraphicRaycaster>().enabled = false;
        //imageComponent.raycastTarget = false;

        wasDragged = true;
    }

    /// <summary>
    /// Determines whether the drag operation can start. Derived classes should override this method to provide specific logic.
    /// </summary>
    /// <returns>True if the drag can start, otherwise false.</returns>
    protected virtual bool CanStartDrag()
    {
        if (card.IsClone || Input.GetMouseButton(1) || player.IsAnimating)
            return false;

        if (!card.IsDraftCard.Value)
            return IsOwner || BlockDragWithMessage("You cannot move another player's card");

        if (!player.IsPlayerTurn.Value)
            return BlockDragWithMessage("You can only purchase cards on your turn");

        if (player.Gold.Value < card.Cost.Value)
            return BlockDragWithMessage("Insufficient Gold");

        return true;
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        if (dropZone == null || originalCardHolder.transform == dropZone.transform)
        {
            EndDragEvent.Invoke(this, true);
            return;
        }

        HandleEndDrag();

        //StartCoroutine(FrameWait());

        //IEnumerator FrameWait()
        //{
        //    yield return new WaitForEndOfFrame();
        //    wasDragged = false;
        //}
    }

    /// <summary>
    /// Handles the specific logic when the drag operation ends. Must be implemented by derived classes.
    /// </summary>
    protected abstract void HandleEndDrag();

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
        DraftCardHolder draftCardHolder = originalCardHolder as DraftCardHolder;

        card.ServerSetCardOwner(player);
        draftCardHolder.ServerMoveCard(card, player.ControlledHand.Value.GetComponent<CardHolder>());

        player.ServerChangePlayerGold(-card.Cost.Value);
        Board.Instance.ServerReplaceDraftCard(draftCardHolder.DraftCardIndex);
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

    protected bool BlockDragWithMessage(string message)
    {
        PopUpManager.Instance.CreateToastPopUp(message);
        return false;
    }

    protected virtual bool IsEndDragValid()
    {
        return true;
    }

    public void InvokeEndDrag()
    {
        EndDragEvent.Invoke(this, true);
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
