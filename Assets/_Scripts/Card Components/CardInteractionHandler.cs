using DG.Tweening;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class CardInteractionHandler : NetworkBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
{
    #region Serialized Fields
    protected bool isDragging = false;

    //[SerializeField] protected GameObject canvas;
    [SerializeField] protected Canvas cardCanvas;
    protected GameObject dropZone;
    protected CardHolder originalCardHolder;
    [SerializeField] protected Player player;
    [SerializeField] protected Card card;
    public Card Card => card;

    [SerializeField] private float _animationDuration = 0.75f;
    #endregion

    [HideInInspector] public bool wasDragged;
    private bool cardIsAnimating;
    public CardHolder previewSlotCardHolder;

    #region Card Animation Parameters
    [Header("Card Animation Parameters")]
    [SerializeField] private float scaleOnHover = 1.15f;
    [SerializeField] private float hoverEnlargeTimer = .8f;
    [SerializeField] private float scaleOnEnlarge = 1.8f;
    [SerializeField] private float scaleDuration = .15f;

    [SerializeField] private float hoverPunchAngle = 5;
    [SerializeField] private float punchDuration = .15f;
    [SerializeField] private int hoverPunchVibrato = 10;

    [SerializeField] private float returnMoveDuration = 0.25f;
    [SerializeField] private float swapMoveDuration = 0.2f;
    [SerializeField] private float swapRotateDuration = 0.15f;
    [SerializeField] private float swapAngle = 30f;
    #endregion

    #region Movement Paremters
    [Header("Movement Paremeters")]
    private readonly float followSpeed = 15f;
    private readonly float tiltStrength = 8f;
    private readonly float tiltLerpSpeed = 10f;
    private readonly float maxTilt = 35f;
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
        // TODO: Move this logic to OnDrag
        if (isDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);

            // Smooth follow
            Vector3 clampedTarget = ClampToScreen(worldPosition);
            transform.position = Vector3.Lerp(transform.position, clampedTarget, Time.deltaTime * followSpeed);

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

    private Vector3 ClampToScreen(Vector2 targetPosition)
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
        if (isDragging && dropZone.TryGetComponent(out CardHolder cardHolder))
        {
            //if (previewSlotCardHolder != null)
            //    previewSlotCardHolder.RemovePreviewSlot();

            if (cardHolder.CreatePreviewSlot(card))
                previewSlotCardHolder = cardHolder;
        }
    }

    /// <summary>
    /// Handles the collision exit event to clear the drop zone if the card exits it.
    /// </summary>
    /// <param name="collision">The collision data associated with this event.</param>
    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        // only excecute logic if the card is leaving the dropZone it just entered
        if (collision.gameObject == dropZone)
        {
            if (previewSlotCardHolder != null)
            {
                previewSlotCardHolder.RemovePreviewSlot();
                previewSlotCardHolder = null;
            }
            dropZone = null;
            //print("exiting dropzone");
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanStartDrag()) return;
        BeginDragEvent.Invoke(this);

        DOTween.Kill(this);
        player.IsDragging = isDragging = true;
        originalCardHolder = transform.parent.parent.GetComponent<CardHolder>();

        cardCanvas.overrideSorting = true;
        cardCanvas.sortingOrder = 100;

        card.transform.DOScale(Vector3.one, scaleDuration).SetId(this)
            .SetEase(Ease.OutBack)
            .OnStart(() =>
            {
                //Debug.Log($"[Tween Start] BeginDrag scale on {card.CardName.Value}");
            })
            .OnComplete(() =>
            {
                //Debug.Log($"[Tween Complete] BeginDrag scale complete on {card.CardName.Value}");
            })
            .OnKill(() =>
            {
                //Debug.Log($"[Tween Killed] Forcing BeginDrag scale on {card.CardName.Value}");
                card.transform.localScale = Vector3.one;
            });
    }

    /// <summary>
    /// Determines whether the drag operation can start. Derived classes should override this method to provide specific logic.
    /// </summary>
    /// <returns>True if the drag can start, otherwise false.</returns>
    protected virtual bool CanStartDrag()
    {
        if (card.IsClone || Input.GetMouseButton(1) || player.IsAnimating || cardIsAnimating)
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
        player.IsDragging = isDragging = false;
        previewSlotCardHolder = null;

        if (dropZone == null || originalCardHolder.transform == dropZone.transform)
        {
            EndDragEvent.Invoke(this, true);
            return;
        }

        HandleEndDrag();
    }

    /// <summary>
    /// Handles the specific logic when the drag operation ends. Must be implemented by derived classes.
    /// </summary>
    protected abstract void HandleEndDrag();

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (player.IsDragging || cardIsAnimating || card.IsClone) return;
        DOTween.Kill(this);

        var sequence = DOTween.Sequence().SetId(this);

        // Immediate scale and punch rotation
        sequence.Append(transform.DOScale(transform.localScale * scaleOnHover, scaleDuration).SetEase(Ease.OutBack));
        sequence.Join(transform.DOPunchRotation(Vector3.forward * hoverPunchAngle, punchDuration, hoverPunchVibrato, 1));

        //Delay before main hover scale and canvas sorting override
        sequence.AppendInterval(hoverEnlargeTimer);
        sequence.Append(transform.DOScale(Vector3.one * scaleOnEnlarge, scaleDuration)
            .SetEase(Ease.OutBack)
            .OnStart(() =>
            {
                //Debug.Log($"[Tween Start] Pointer Enter Scale up on {card.CardName.Value}");
                cardCanvas.overrideSorting = true;
                cardCanvas.sortingOrder = 100;
            })
            .OnUpdate(() =>
            {
                //Debug.Log($"[Tween Update] Clamping position on {gameObject.name}");
                transform.position = ClampToScreen(transform.position);
            })
            .OnComplete(() =>
            {
                //Debug.Log($"[Tween Complete] Pointer Enter Scale up on {card.CardName.Value}");
            })
            .OnKill(() =>
            {
                //Debug.Log($"[Tween Killed] Pointer Enter Scale up on {card.CardName.Value}");
            }));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (player.IsDragging || cardIsAnimating) return;
        PlayReturnTween("Pointer Exit Scale down");
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
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
        cardIsAnimating = true;
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
        ObserversPlayPurchaseAnimation(playerID);
    }

    [ObserversRpc]
    protected void ObserversPlayPurchaseAnimation(int playerID)
    {
        Transform guildTransform = Board.Instance.GuildStatusList[playerID].transform;
        Sequence sequence = DOTween.Sequence();

        if (LocalConnection.ClientId != playerID)
        {
            sequence.Append(transform.DOMove(guildTransform.position, _animationDuration).SetEase(Ease.OutSine))
                .Join(transform.DOScale(Vector3.zero, _animationDuration).SetEase(Ease.InQuad));
        }
        else
        {
            sequence.Append(transform.DOJump(transform.position, 15f, 1, _animationDuration))
                .OnComplete(() =>
                {
                    AssignDraftCardToPlayer();
                    player.SetIsAnimating(false);
                    cardIsAnimating = false;
                });
        }
    }

    public void PlayReturnTween(string contextLabel, Vector3? scale = null)
    {
        if (cardIsAnimating || card.IsClone) return;
        DOTween.Kill(this);

        Vector3 originalScale = scale ?? card.CurrentCardHolder.Value.Scale;
        cardIsAnimating = true;

        DOTween.Sequence().SetTarget(transform).SetId(this)
            .Append(transform.DOScale(originalScale, returnMoveDuration).SetEase(Ease.OutBack))
            .Join(transform.DOLocalMove(Vector3.zero, returnMoveDuration).SetEase(Ease.OutBack))
            .OnStart(() =>
            {
                //Debug.Log($"[Tween Start] {contextLabel} on card: {card.CardName.Value}");
            })
            .OnComplete(() =>
            {
                //Debug.Log($"[Tween Complete] {contextLabel} finished: {card.CardName.Value}");
            })
            .OnKill(() =>
            {
                //Debug.Log($"[Tween Killed] Forcing {contextLabel} on: {card.CardName.Value}");
                transform.localScale = originalScale;
                transform.localPosition = Vector3.zero;
                gameObject.GetComponent<Canvas>().overrideSorting = false;
                cardIsAnimating = false;
            });
    }

    public void PlaySwapTween(int dir)
    {
        DOTween.Kill(this);

        DOTween.Sequence().SetTarget(transform).SetId(this)
            .Append(transform.DOLocalRotate(dir * swapAngle * Vector3.forward, swapRotateDuration).SetEase(Ease.OutCubic))
            .Join(transform.DOLocalMove(Vector3.zero, swapMoveDuration).SetEase(Ease.InOutCubic))
            .Append(transform.DOLocalRotate(Vector3.zero, swapRotateDuration).SetEase(Ease.InOutCubic))
            .OnKill(() =>
            {
                transform.localRotation = Quaternion.identity;
            });
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
        return transform.parent.GetSiblingIndex();
    }

    public float NormalizedPosition()
    {
        return transform.parent.CompareTag("Slot") ? ExtensionMethods.Remap((float)ParentIndex(), 0, (float)(transform.parent.parent.childCount - 1), 0, 1) : 0;
    }
}
