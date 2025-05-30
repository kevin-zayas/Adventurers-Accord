using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class AdventurerCard : Card
{
    #region SyncVars
    public readonly SyncVar<string> AbilityName = new();
    [AllowMutableSyncTypeAttribute] public SyncVar<bool> HasItem = new();
    [AllowMutableSyncTypeAttribute] public SyncVar<ItemCardHeader> Item = new();
    public readonly SyncVar<Transform> ParentTransform = new();
    public readonly SyncVar<int> RestPeriod = new();
    public readonly SyncVar<int> CurrentRestPeriod = new();
    public readonly SyncVar<bool> IsBlessed = new();
    private int potionPhysicalPower;
    private int potionMagicalPower;
    private int physicalPoisonTotal = 0;
    private int magicalPoisonTotal = 0;
    #endregion

    #region UI Elements
    [SerializeField] private GameObject abilityNameObject;
    [SerializeField] private TMP_Text abilityNameText;
    [SerializeField] private TMP_Text restPeriodText;
    #endregion

    #region Constants
    private const string Sorcerer = "Sorcerer";
    private const string Battlemage = "Battlemage";
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
        ObserversSetCardParent(newParent, worldPositionStays);
        transform.SetParent(newParent, worldPositionStays);

        if (ParentTransform.Value != newParent)
        {
            if (ParentTransform.Value != null && ParentTransform.Value.CompareTag(QuestTag)) OnQuestReturn(ParentTransform.Value);
            if (newParent != null && newParent.CompareTag(QuestTag)) OnQuestDispatch(newParent);
        }

        ParentTransform.Value = newParent;

    }

    /// <summary>
    /// Updates the card's parent transform on all clients, adjusting its scale based on the parent's tag.
    /// </summary>
    /// <param name="newParent">The new parent transform.</param>
    /// <param name="worldPositionStays">Whether to maintain the world position of the card.</param>
    [ObserversRpc(BufferLast = true)]
    protected override void ObserversSetCardParent(Transform newParent, bool worldPositionStays)
    {
        if (newParent == null) return;
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

        if (CardName.Value == Battlemage)
        {
            itemCardHeader.equippedOnBattlemage = true;
            itemCardHeader.ApplyBalancedArsenal();
        }
        if (CardName.Value == Sorcerer || ControllingPlayer.Value.isFightersGuild) ResetPower();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerGrantDivineBlessing()
    {
        IsBlessed.Value = true;
        ChangeCurrentRestPeriod(-1, true);

        if (physicalPoisonTotal > 0) ChangePhysicalPower(physicalPoisonTotal);
        if (magicalPoisonTotal > 0) ChangeMagicalPower(magicalPoisonTotal);
    }

    [Server]
    public void ChangeCurrentRestPeriod(int restPeriodDelta, bool updateObservers = false)
    {
        CurrentRestPeriod.Value = Mathf.Max(0, CurrentRestPeriod.Value + restPeriodDelta);
        if (updateObservers) ObserversUpdateRestPeriodText(CurrentRestPeriod.Value, RestPeriod.Value);
    }

    [Server]
    public void ResetCurrentRestPeriod()
    {
        CurrentRestPeriod.Value = RestPeriod.Value;
        ObserversUpdateRestPeriodText(CurrentRestPeriod.Value, RestPeriod.Value);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversUpdateRestPeriodText(int currentRestPeriod, int restPeriod)
    {
        restPeriodText.text = currentRestPeriod.ToString();
        UpdateRestPeriodTextColor(currentRestPeriod, restPeriod);
    }

    private void UpdateRestPeriodTextColor(int currentRestPeriod, int restPeriod)
    {
        if (currentRestPeriod == restPeriod) restPeriodText.color = Color.black;
        else restPeriodText.color = new Color32(54, 128, 48, 255);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerApplyPoison(bool physicalPoison, bool magicalPoison)
    {
        int poisonAmount;
        if (physicalPoison)
        {
            poisonAmount = Mathf.Min(2, PhysicalPower.Value);
            ChangePhysicalPower(-poisonAmount);
            physicalPoisonTotal += poisonAmount;
        }
        if (magicalPoison)
        {
            poisonAmount = Mathf.Min(2, MagicalPower.Value);
            ChangeMagicalPower(-poisonAmount);
            magicalPoisonTotal += poisonAmount;
        }
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
        if (ParentTransform.Value == null) return; // || !ParentTransform.Value.CompareTag(QuestTag)) return;

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
        if (ParentTransform.Value == null) return; // || !ParentTransform.Value.CompareTag(QuestTag)) return;

        if (OriginalMagicalPower.Value > 0)
        {
            MagicalPower.Value = Mathf.Max(0, MagicalPower.Value + powerChange); // Clamping value
            ObserversUpdatePowerText(PhysicalPower.Value, MagicalPower.Value);
        }
    }

    /// <summary>
    /// Resets the card's power to its original values and updates clients.
    /// </summary>
    [Server]
    public override void ResetPower()
    {
        PhysicalPower.Value = OriginalPhysicalPower.Value + potionPhysicalPower;
        MagicalPower.Value = OriginalMagicalPower.Value + potionMagicalPower;

        if (CardName.Value == Sorcerer && HasItem.Value && !Item.Value.IsDisabled.Value) MagicalPower.Value += 2;
        if (ControllingPlayer.Value.isFightersGuild && HasItem.Value && !Item.Value.IsDisabled.Value && Item.Value.PhysicalPower.Value > 0)
        {
            PhysicalPower.Value += 1;
        }

        ObserversUpdatePowerText(PhysicalPower.Value, MagicalPower.Value);
    }

    /// <summary>
    /// Disables the equipped item on the card and resets its power if necessary.
    /// </summary>
    /// <param name="disableType">The type of disable action to apply to the item.</param>
    [Server]
    public void DisableItem()
    {
        if (!HasItem.Value || Item.Value.IsDisabled.Value) return;

        Item.Value.DisableItem();

        if (CardName.Value == Sorcerer || ControllingPlayer.Value.isFightersGuild) ResetPower();
    }

    /// <summary>
    /// Server-side RPC to disable the equipped item on the card.
    /// </summary>
    /// <param name="disableType">The type of disable action to apply to the item.</param>
    [ServerRpc(RequireOwnership = false)]
    public void ServerDisableItem()
    {
        DisableItem();
    }

    /// <summary>
    /// Loads the card data and updates the relevant SyncVars on the server.
    /// </summary>
    /// <param name="cardData">The card data to load.</param>
    [Server]
    public override void LoadCardData(CardData cardData)
    {
        AbilityName.Value = cardData.AbilityName;
        RestPeriod.Value = cardData.RestPeriod;
        CurrentRestPeriod.Value = cardData.RestPeriod;

        base.LoadCardData(cardData);
    }

    /// <summary>
    /// Updates the card's visual representation on all clients based on the provided card data.
    /// </summary>
    /// <param name="cardData">The card data to load into the visual elements.</param>
    [ObserversRpc(BufferLast = true)]
    protected override void ObserversLoadCardData(CardData cardData)
    {
        physicalPowerText.text = cardData.PhysicalPower.ToString();
        magicalPowerText.text = cardData.MagicalPower.ToString();
        nameText.text = cardData.CardName;
        abilityNameText.text = cardData.AbilityName;
        cardTypeText.text = cardData.CardType;
        costText.text = cardData.Cost.ToString();
        restPeriodText.text = cardData.RestPeriod.ToString();

        cardImage.sprite = CardDatabase.Instance.CardSpriteMap[cardData.CardName];
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
        IsClone = true;
        AdventurerCard card = originalCard as AdventurerCard;

        cardImage.sprite = CardDatabase.Instance.CardSpriteMap[card.CardName.Value];

        physicalPowerText.text = card.PhysicalPower.Value.ToString();
        magicalPowerText.text = card.MagicalPower.Value.ToString();
        nameText.text = card.CardName.Value;
        abilityNameText.text = card.AbilityName.Value;
        costText.text = card.Cost.Value.ToString();

        restPeriodText.text = card.CurrentRestPeriod.Value.ToString();
        UpdateRestPeriodTextColor(card.CurrentRestPeriod.Value, card.RestPeriod.Value);

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

        Player player = ControllingPlayer.Value;
        int questIndex = questLane.QuestLocation.Value.QuestLocationIndex;
        player.DispatchedAdventurerCount++;

        if (player.isFightersGuild && OriginalPhysicalPower.Value > 0)
        {
            player.GuildBonusTracker[questIndex]["physAdventurers"]++;

            if (player.GuildBonusTracker[questIndex]["physAdventurers"] == 3)
            {
                questLane.UpdateGuildBonusPower(1, 0);
                player.UpdateGuildRecapTracker("Strength in Numbers (+1 P. Power)", 1);
                Debug.Log($"Fighters Guild Bonus - Player {player.PlayerID.Value} +1 Physical Power");
            }
        }
        else if (player.isMerchantsGuild && HasItem.Value)
        {
            player.GuildBonusTracker[questIndex]["magicItemsDispatched"]++;
        }

    }

    /// <summary>
    /// Handles the logic when the card is returned from a quest.
    /// Removes the card from the quest lane and resets power on the server.
    /// </summary>
    /// <param name="previousParent">The previous parent transform, expected to be a quest lane.</param>
    [Server]
    private void OnQuestReturn(Transform previousParent)
    {
        QuestLane questLane = previousParent.parent.GetComponent<QuestLane>();
        questLane.RemoveAdventurerFromQuestLane(this);

        if (IsBlessed.Value)
        {
            IsBlessed.Value = false;
            ObserversUpdateRestPeriodText(RestPeriod.Value, RestPeriod.Value);
        }

        if (HasItem.Value) Item.Value.ResetPower();
        physicalPoisonTotal = magicalPoisonTotal = 0;
        ResetPower();

        Player player = ControllingPlayer.Value;
        player.DispatchedAdventurerCount--;
        int questIndex = questLane.QuestLocation.Value.QuestLocationIndex;

        if (player.isFightersGuild && OriginalPhysicalPower.Value > 0)
        {
            player.GuildBonusTracker[questIndex]["physAdventurers"]--;

            if (player.GuildBonusTracker[questIndex]["physAdventurers"] == 2)
            {
                questLane.UpdateGuildBonusPower(-1, 0);
                player.UpdateGuildRecapTracker("Strength in Numbers (+1 P. Power)", -1);
                Debug.Log($"Fighters Guild Bonus Disabled - Player {player.PlayerID.Value}");
            }
        }
        else if (player.isMerchantsGuild && HasItem.Value)
        {
            player.GuildBonusTracker[questIndex]["magicItemsDispatched"]--;
        }
    }

    /// <summary>
    /// Handles the resolution click event for the card, validating if it can be selected based on the current game state.
    /// </summary>
    public void OnResolutionClick()
    {
        if (GameManager.Instance.CurrentPhase.Value != GameManager.Phase.Resolution) return;
        if (!Player.Instance.IsPlayerTurn.Value) return;
        if (ParentTransform.Value == null) return;

        QuestLane lane = ParentTransform.Value.parent.GetComponent<QuestLane>();
        if (lane == null || !lane.QuestLocation.Value.AllowResolution.Value) return;

        string resolutionType = PopUpManager.Instance.CurrentResolutionType;
        if (resolutionType == null) return;
        bool isNotPlayer = ControllingPlayer.Value != Player.Instance;
        bool hasActiveItem = HasItem.Value && !Item.Value.IsDisabled.Value;
        bool hasPower = MagicalPower.Value > 0 || PhysicalPower.Value > 0;
        bool isWolf = CardName.Value == "Wolf";

        bool validTarget = (resolutionType == "Rogue" && isNotPlayer && hasActiveItem) ||
                           (resolutionType == "Assassin" && isNotPlayer && !IsBlessed.Value && hasPower) ||
                           (resolutionType == "Cleric" && !IsBlessed.Value && !isWolf);

        if (validTarget) PopUpManager.Instance.CurrentResolutionPopUp.SetConfirmSelectionState(this);
        else PopUpManager.Instance.CreateToastPopUp("Invalid target");
    }

    public void OnPotionResolutionClick()
    {
        //if (!Player.Instance.IsPlayerTurn.Value) return;
        if (ParentTransform.Value == null) return;
        if (ControllingPlayer.Value != Player.Instance) return; // only allow potions to be used on your own cards

        QuestLane lane = ParentTransform.Value.parent.GetComponent<QuestLane>();
        if (lane == null || !lane.QuestLocation.Value.AllowResolution.Value) return;

        string resolutionType = PopUpManager.Instance.CurrentResolutionType;
        if (resolutionType == null) return;
        bool isWolf = CardName.Value == "Wolf";
        bool hasPower = OriginalMagicalPower.Value > 0 || OriginalPhysicalPower.Value > 0;

        bool validTarget = (resolutionType == "Healing Potion" && CurrentRestPeriod.Value > 0 && !isWolf) ||
                           (resolutionType == "Potion of Power" && hasPower);

        if (validTarget) PopUpManager.Instance.CurrentResolutionPopUp.SetConfirmSelectionState(this);
        else PopUpManager.Instance.CreateToastPopUp("Invalid target");
    }

    public override bool ShouldToggleDisableScreen()
    {
        if (base.ShouldToggleDisableScreen()) return true;
        if (CardName.Value == "Wolf") ToggleDisableScreen(true);

        return false;
    }

    [Server]
    public void ApplyPotionPhysicalPower(int power)
    {
        if (ParentTransform.Value == null) return;

        if (OriginalPhysicalPower.Value > 0)
        {
            potionPhysicalPower += power;
            PhysicalPower.Value += power;
            ObserversUpdatePowerText(PhysicalPower.Value, MagicalPower.Value);
        }
    }

    [Server]
    public void ApplyPotionMagicalPower(int power)
    {
        if (ParentTransform.Value == null) return;

        if (OriginalMagicalPower.Value > 0)
        {
            potionMagicalPower += power;
            MagicalPower.Value += power;
            ObserversUpdatePowerText(PhysicalPower.Value, MagicalPower.Value);
        }
    }

    [Server]
    public void ResetPotionPower()
    {
        potionPhysicalPower = potionMagicalPower = 0;
        ResetPower();
    }
}