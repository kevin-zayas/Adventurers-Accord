using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdventurerCard : Card
{
    #region SyncVars
    [field: SyncVar] public int Cost { get; private set; }
    [field: SyncVar] public bool HasItem { get; private set; }
    [field: SyncVar] public ItemCardHeader Item { get; private set; }
    [field: SyncVar] public bool IsDraftCard { get; private set; }
    [field: SyncVar] public int OriginalMagicalPower { get; private set; }
    [field: SyncVar] public int OriginalPhysicalPower { get; private set; }
    [field: SyncVar] public Transform ParentTransform { get; private set; }
    #endregion

    #region UI Elements
    [SerializeField] private GameObject abilityNameObject;
    [SerializeField] private TMP_Text abilityNameText;
    [SerializeField] private TMP_Text cardTypeText;
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text physicalPowerText;
    #endregion

    #region Cached Components
    private RectTransform _rectTransform;
    #endregion

    #region General Variables
    public string AbilityName { get; private set; }
    #endregion

    #region Constants
    private const string Sorcerer = "Sorcerer";
    private const string QuestTag = "Quest";
    #endregion

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (IsServer)
        {
            IsDraftCard = true;
        }
    }

    /// <summary>
    /// Sets the parent transform of the card on the server and updates the associated state.
    /// </summary>
    /// <param name="newParent">The new parent transform to set.</param>
    /// <param name="worldPositionStays">Whether to maintain the world position of the card.</param>
    [Server]
    public override void SetCardParent(Transform newParent, bool worldPositionStays)
    {
        ObserversSetCardParent(newParent, worldPositionStays);
        transform.SetParent(newParent, worldPositionStays);

        if (ParentTransform != null && ParentTransform != newParent)
        {
            if (ParentTransform.CompareTag(QuestTag)) OnQuestReturn(ParentTransform);
            ParentTransform = newParent;
            if (newParent.CompareTag(QuestTag)) OnQuestDispatch(newParent);
        }
        else
        {
            ParentTransform = newParent;
        }
    }

    /// <summary>
    /// Server-side RPC to set the parent transform of the card.
    /// </summary>
    /// <param name="newParent">The new parent transform.</param>
    /// <param name="worldPositionStays">Whether to maintain the world position of the card.</param>
    [ServerRpc(RequireOwnership = false)]
    public override void ServerSetCardParent(Transform newParent, bool worldPositionStays)
    {
        SetCardParent(newParent, worldPositionStays);
    }

    /// <summary>
    /// Updates the card's parent transform on all clients, adjusting its scale based on the parent's tag.
    /// </summary>
    /// <param name="newParent">The new parent transform.</param>
    /// <param name="worldPositionStays">Whether to maintain the world position of the card.</param>
    [ObserversRpc(BufferLast = true)]
    protected override void ObserversSetCardParent(Transform newParent, bool worldPositionStays)
    {
        Vector3 scale = newParent.CompareTag(QuestTag) ? new Vector3(.6f, .6f, 1f) : new Vector3(1f, 1f, 1f);
        transform.localScale = scale;
        transform.SetParent(newParent, worldPositionStays);
    }

    /// <summary>
    /// Server-side RPC to set the card's owner and update related properties.
    /// </summary>
    /// <param name="player">The player who will own the card.</param>
    [ServerRpc(RequireOwnership = false)]
    public void ServerSetCardOwner(Player player)
    {
        ControllingPlayer = player;
        ControllingPlayerHand = player.controlledHand;
        GiveOwnership(player.Owner);
        IsDraftCard = false;
    }

    /// <summary>
    /// Server-side RPC to equip an item to the card.
    /// </summary>
    /// <param name="hasItem">Whether the card has an item equipped.</param>
    /// <param name="itemCardData">Data of the item card to equip.</param>
    [ServerRpc(RequireOwnership = false)]
    public void ServerEquipItem(bool hasItem, CardData itemCardData)
    {
        HasItem = hasItem;

        var itemCardHeader = transform.GetChild(0).transform.GetChild(1).GetComponent<ItemCardHeader>();
        Spawn(itemCardHeader.gameObject);
        itemCardHeader.LoadCardData(itemCardData);
        Item = itemCardHeader;

        if (CardName == Sorcerer) ResetPower();
    }

    /// <summary>
    /// Server-side RPC to change the physical power of the card.
    /// </summary>
    /// <param name="power">The amount to change the physical power by.</param>
    [ServerRpc(RequireOwnership = false)]
    public void ServerChangePhysicalPower(int power)
    {
        ChangePhysicalPower(power);
    }

    /// <summary>
    /// Changes the physical power of the card and updates clients if the card is on a quest.
    /// </summary>
    /// <param name="power">The amount to change the physical power by.</param>
    [Server]
    public void ChangePhysicalPower(int power)
    {
        if (ParentTransform == null || !ParentTransform.CompareTag(QuestTag)) return;

        if (OriginalPhysicalPower > 0)
        {
            PhysicalPower = Mathf.Max(0, PhysicalPower + power); // Clamping value
            ObserversUpdatePowerText(PhysicalPower, MagicalPower);
        }
    }

    /// <summary>
    /// Server-side RPC to change the magical power of the card.
    /// </summary>
    /// <param name="powerChange">The amount to change the magical power by.</param>
    [ServerRpc(RequireOwnership = false)]
    public void ServerChangeMagicalPower(int powerChange)
    {
        ChangeMagicalPower(powerChange);
    }

    /// <summary>
    /// Changes the magical power of the card and updates clients if the card is on a quest.
    /// </summary>
    /// <param name="powerChange">The amount to change the magical power by.</param>
    [Server]
    public void ChangeMagicalPower(int powerChange)
    {
        if (ParentTransform == null || !ParentTransform.CompareTag(QuestTag)) return;

        if (OriginalMagicalPower > 0)
        {
            MagicalPower = Mathf.Max(0, MagicalPower + powerChange); // Clamping value
            ObserversUpdatePowerText(PhysicalPower, MagicalPower);
        }
    }

    /// <summary>
    /// Updates the power text on all clients, reflecting changes to physical and magical power.
    /// </summary>
    /// <param name="physicalPower">The updated physical power.</param>
    /// <param name="magicalPower">The updated magical power.</param>
    [ObserversRpc(BufferLast = true)]
    public void ObserversUpdatePowerText(int physicalPower, int magicalPower)
    {
        physicalPowerText.text = physicalPower.ToString();
        magicalPowerText.text = magicalPower.ToString();
        UpdatePowerTextColor(physicalPower, magicalPower, OriginalPhysicalPower, OriginalMagicalPower);
    }

    /// <summary>
    /// Updates the color of the power text based on comparison with the original power values.
    /// </summary>
    /// <param name="physicalPower">The current physical power.</param>
    /// <param name="magicalPower">The current magical power.</param>
    private void UpdatePowerTextColor(int physicalPower, int magicalPower, int originalPhysical, int originalMagical)
    {
        physicalPowerText.color = physicalPower > originalPhysical ? Color.green : physicalPower < OriginalPhysicalPower ? Color.red : Color.white;
        magicalPowerText.color = magicalPower > originalMagical ? Color.green : magicalPower < OriginalMagicalPower ? Color.red : Color.white;
    }

    /// <summary>
    /// Resets the card's power to its original values and updates clients.
    /// If the card is a "Sorcerer" and has an item equipped, the magical power is increased.
    /// </summary>
    [Server]
    public void ResetPower()
    {
        PhysicalPower = OriginalPhysicalPower;
        MagicalPower = OriginalMagicalPower;

        if (CardName == Sorcerer && HasItem && !Item.IsDisabled) MagicalPower += 2;

        ObserversUpdatePowerText(PhysicalPower, MagicalPower);
    }

    /// <summary>
    /// Disables the equipped item on the card and resets its power if necessary.
    /// </summary>
    /// <param name="disableType">The type of disable action to apply to the item.</param>
    [Server]
    public void DisableItem(string disableType)
    {
        if (!HasItem || Item.IsDisabled) return;

        Item.DisableItem(disableType);

        if (CardName == Sorcerer) ResetPower();
    }

    /// <summary>
    /// Server-side RPC to disable the equipped item on the card.
    /// </summary>
    /// <param name="disableType">The type of disable action to apply to the item.</param>
    [ServerRpc(RequireOwnership = false)]
    public void ServerDisableItem(string disableType)
    {
        DisableItem(disableType);
    }

    /// <summary>
    /// Loads the card data and updates the relevant SyncVars on the server.
    /// </summary>
    /// <param name="cardData">The card data to load.</param>
    [Server]
    public override void LoadCardData(CardData cardData)
    {
        OriginalPhysicalPower = cardData.OriginalPhysicalPower;
        OriginalMagicalPower = cardData.OriginalMagicalPower;
        Cost = cardData.Cost;
        AbilityName = cardData.AbilityName;

        base.LoadCardData(cardData);
    }

    /// <summary>
    /// Updates the card's visual representation on all clients based on the provided card data.
    /// </summary>
    /// <param name="cardData">The card data to load into the visual elements.</param>
    [ObserversRpc(BufferLast = true)]
    protected override void ObserversLoadCardData(CardData cardData)
    {
        cardImage.sprite = CardDatabase.Instance.SpriteMap[cardData.CardName];

        physicalPowerText.text = cardData.PhysicalPower.ToString();
        magicalPowerText.text = cardData.MagicalPower.ToString();
        nameText.text = cardData.CardName;
        abilityNameText.text = cardData.AbilityName;
        cardTypeText.text = cardData.CardType;
        costText.text = cardData.Cost.ToString();

        if (cardData.AbilityName == "") abilityNameObject.SetActive(false);
    }

    /// <summary>
    /// Updates the target client with the copied card data from the original card.
    /// </summary>
    /// <param name="connection">The network connection of the target client.</param>
    /// <param name="originalCard">The original card to copy data from.</param>
    [TargetRpc]
    public override void TargetCopyCardData(NetworkConnection connection, Card originalCard)
    {
        AdventurerCard card = originalCard as AdventurerCard;

        cardImage.sprite = CardDatabase.Instance.SpriteMap[card.CardName];

        physicalPowerText.text = card.PhysicalPower.ToString();
        magicalPowerText.text = card.MagicalPower.ToString();
        nameText.text = card.CardName;
        abilityNameText.text = card.AbilityName;
        costText.text = card.Cost.ToString();

        UpdatePowerTextColor(card.PhysicalPower, card.MagicalPower, card.OriginalPhysicalPower, card.OriginalMagicalPower);

        if (card.AbilityName == "") abilityNameObject.SetActive(false);
    }

    /// <summary>
    /// Handles the logic when the card is dispatched to a quest.
    /// Adds the card to the quest lane on the server.
    /// </summary>
    /// <param name="newParent">The new parent transform, expected to be a quest lane.</param>
    [Server]
    private void OnQuestDispatch(Transform newParent)
    {
        QuestLane questLane = newParent.parent.GetComponent<QuestLane>();
        questLane.AddAdventurerToQuestLane(this);
    }

    /// <summary>
    /// Handles the logic when the card is returned from a quest.
    /// Removes the card from the quest lane and resets power on the server.
    /// </summary>
    /// <param name="newParent">The previous parent transform, expected to be a quest lane.</param>
    [Server]
    private void OnQuestReturn(Transform newParent)
    {
        QuestLane questLane = newParent.parent.GetComponent<QuestLane>();
        questLane.RemoveAdventurerFromQuestLane(this);
        if (HasItem) Item.ResetPower();
        ResetPower();
    }

    /// <summary>
    /// Handles the resolution click event for the card, validating if it can be selected based on the current game state.
    /// </summary>
    public void OnResolutionClick()
    {
        if (GameManager.Instance.CurrentPhase != GameManager.Phase.Resolution) return;
        if (!GameManager.Instance.Players[LocalConnection.ClientId].IsPlayerTurn) return;

        QuestLane lane = ParentTransform.parent.GetComponent<QuestLane>();
        if (!lane.QuestLocation.AllowResolution) return;

        if (PopUpManager.Instance.CurrentResolutionPopUp.ResolutionType == "Rogue" && HasItem && !Item.IsDisabled)
        {
            PopUpManager.Instance.CurrentResolutionPopUp.SetConfirmSelectionState(this);
        }
        else if (PopUpManager.Instance.CurrentResolutionPopUp.ResolutionType == "Assassin" && !lane.ClericProtection && (MagicalPower > 0 || PhysicalPower > 0))
        {
            PopUpManager.Instance.CurrentResolutionPopUp.SetConfirmSelectionState(this);
        }
        else
        {
            Debug.Log("Invalid target");
        }
    }
}