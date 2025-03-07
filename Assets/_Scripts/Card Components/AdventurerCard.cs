using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdventurerCard : Card
{
    #region SyncVars
    public readonly SyncVar<string> AbilityName = new();
    [AllowMutableSyncTypeAttribute] public SyncVar<bool> HasItem = new();
    [AllowMutableSyncTypeAttribute] public SyncVar<ItemCardHeader> Item = new();
    public readonly SyncVar<int> OriginalMagicalPower = new();
    public readonly SyncVar<int> OriginalPhysicalPower = new();
    public readonly SyncVar<Transform> ParentTransform = new();
    public readonly SyncVar<int> CoolDown = new();
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

    #region Constants
    private const string Sorcerer = "Sorcerer";
    private const string QuestTag = "Quest";
    #endregion

    private void Start()
    {
        //if (IsServerInitialized)
        //{
        //    IsDraftCard.Value = true;
        //}
        //else      //may need to add this back when putting it on dedicated server
        //{
            player = GameManager.Instance.Players[LocalConnection.ClientId];
        //}
    }

    /// <summary>
    /// Sets the parent transform of the card on the server and updates the associated state.
    /// </summary>
    /// <param name="newParent">The new parent transform to set.</param>
    /// <param name="worldPositionStays">Whether to maintain the world position of the card.</param>
    [Server]
    public override void SetCardParent(Transform newParent, bool worldPositionStays)
    {
        //print($"is newParent null (sever) - {newParent == null}");
        ObserversSetCardParent(newParent, worldPositionStays);
        transform.SetParent(newParent, worldPositionStays);

        if (ParentTransform.Value != null && ParentTransform.Value != newParent)
        {
            if (ParentTransform.Value.CompareTag(QuestTag)) OnQuestReturn(ParentTransform.Value);
            ParentTransform.Value = newParent;
            if (newParent.CompareTag(QuestTag)) OnQuestDispatch(newParent);
        }
        else
        {
            ParentTransform.Value = newParent;
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
        //print($"is newParent null (observer) - {newParent == null}");
        //print(newParent.tag);
        Vector3 scale = newParent.CompareTag(QuestTag) ? new Vector3(.6f, .6f, 1f) : new Vector3(1f, 1f, 1f);
        transform.localScale = scale;
        transform.SetParent(newParent, worldPositionStays);
    }

    /// <summary>
    /// Server-side RPC to equip an item to the card.
    /// </summary>
    /// <param name="hasItem">Whether the card has an item equipped.</param>
    /// <param name="itemCardData">Data of the item card to equip.</param>
    [ServerRpc(RequireOwnership = false)]
    public void ServerEquipItem(bool hasItem, CardData itemCardData)
    {
        HasItem.Value = hasItem;

        var itemCardHeader = transform.GetChild(0).transform.GetChild(1).GetComponent<ItemCardHeader>();
        Spawn(itemCardHeader.gameObject);
        itemCardHeader.LoadCardData(itemCardData);
        Item.Value = itemCardHeader;

        if (CardName.Value == Sorcerer) ResetPower();
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
        if (ParentTransform.Value == null || !ParentTransform.Value.CompareTag(QuestTag)) return;

        if (OriginalPhysicalPower.Value > 0)
        {
            PhysicalPower.Value = Mathf.Max(0, PhysicalPower.Value + power); // Clamping value
            ObserversUpdatePowerText(PhysicalPower.Value, MagicalPower.Value);
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
        if (ParentTransform.Value == null || !ParentTransform.Value.CompareTag(QuestTag)) return;

        if (OriginalMagicalPower.Value > 0)
        {
            MagicalPower.Value = Mathf.Max(0, MagicalPower.Value + powerChange); // Clamping value
            ObserversUpdatePowerText(PhysicalPower.Value, MagicalPower.Value);
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
        UpdatePowerTextColor(physicalPower, magicalPower, OriginalPhysicalPower.Value, OriginalMagicalPower.Value);
    }

    /// <summary>
    /// Updates the color of the power text based on comparison with the original power values.
    /// </summary>
    /// <param name="physicalPower">The current physical power.</param>
    /// <param name="magicalPower">The current magical power.</param>
    private void UpdatePowerTextColor(int physicalPower, int magicalPower, int originalPhysical, int originalMagical)
    {
        physicalPowerText.color = physicalPower > originalPhysical ? Color.green : physicalPower < OriginalPhysicalPower.Value ? Color.red : Color.white;
        magicalPowerText.color = magicalPower > originalMagical ? Color.green : magicalPower < OriginalMagicalPower.Value ? Color.red : Color.white;
    }

    /// <summary>
    /// Resets the card's power to its original values and updates clients.
    /// If the card is a "Sorcerer" and has an item equipped, the magical power is increased.
    /// </summary>
    [Server]
    public void ResetPower()
    {
        PhysicalPower.Value = OriginalPhysicalPower.Value;
        MagicalPower.Value = OriginalMagicalPower.Value;

        if (CardName.Value == Sorcerer && HasItem.Value && !Item.Value.IsDisabled.Value) MagicalPower.Value += 2;

        ObserversUpdatePowerText(PhysicalPower.Value, MagicalPower.Value);
    }

    /// <summary>
    /// Disables the equipped item on the card and resets its power if necessary.
    /// </summary>
    /// <param name="disableType">The type of disable action to apply to the item.</param>
    [Server]
    public void DisableItem(string disableType)
    {
        if (!HasItem.Value || Item.Value.IsDisabled.Value) return;

        Item.Value.DisableItem(disableType);

        if (CardName.Value == Sorcerer) ResetPower();
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
        OriginalPhysicalPower.Value = cardData.OriginalPhysicalPower;
        OriginalMagicalPower.Value = cardData.OriginalMagicalPower;
        Cost.Value = cardData.Cost;
        AbilityName.Value = cardData.AbilityName;

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
        isClone = true;
        AdventurerCard card = originalCard as AdventurerCard;

        cardImage.sprite = CardDatabase.Instance.SpriteMap[card.CardName.Value];

        physicalPowerText.text = card.PhysicalPower.Value.ToString();
        magicalPowerText.text = card.MagicalPower.Value.ToString();
        nameText.text = card.CardName.Value;
        abilityNameText.text = card.AbilityName.Value;
        costText.text = card.Cost.Value.ToString();

        UpdatePowerTextColor(card.PhysicalPower.Value, card.MagicalPower.Value, card.OriginalPhysicalPower.Value, card.OriginalMagicalPower.Value);

        if (card.AbilityName.Value == "") abilityNameObject.SetActive(false);
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
        if (HasItem.Value) Item.Value.ResetPower();
        ResetPower();
    }

    /// <summary>
    /// Handles the resolution click event for the card, validating if it can be selected based on the current game state.
    /// </summary>
    public void OnResolutionClick()
    {
        if (GameManager.Instance.CurrentPhase.Value != GameManager.Phase.Resolution) return;
        if (!GameManager.Instance.Players[LocalConnection.ClientId].IsPlayerTurn.Value) return;
        if (ParentTransform.Value == null) return;

        QuestLane lane = ParentTransform.Value.parent.GetComponent<QuestLane>();
        if (!lane.QuestLocation.Value.AllowResolution.Value) return;

        if (PopUpManager.Instance.CurrentResolutionPopUp.Value.ResolutionType == "Rogue" && HasItem.Value && !Item.Value.IsDisabled.Value)
        {
            PopUpManager.Instance.CurrentResolutionPopUp.Value.SetConfirmSelectionState(this);
        }
        else if (PopUpManager.Instance.CurrentResolutionPopUp.Value.ResolutionType == "Assassin" && !lane.ClericProtection.Value && (MagicalPower.Value > 0 || PhysicalPower.Value > 0))
        {
            PopUpManager.Instance.CurrentResolutionPopUp.Value.SetConfirmSelectionState(this);
        }
        else
        {
            Debug.Log("Invalid target");
        }
    }

    public override bool ShouldToggleDisableScreen()
    {
        if (base.ShouldToggleDisableScreen()) return true;
        if (CardName.Value == "Wolf") ToggleDisableScreen(true);

        return false;
    }
}