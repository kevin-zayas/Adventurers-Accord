using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Card : NetworkBehaviour
{
    #region SyncVars
    public readonly SyncVar<string> CardName = new();
    public readonly SyncVar<string> CardDescription = new();
    public readonly SyncVar<int> Cost = new();
    public readonly SyncVar<int> PhysicalPower = new();
    public readonly SyncVar<int> MagicalPower = new();
    public readonly SyncVar<int> OriginalMagicalPower = new();
    public readonly SyncVar<int> OriginalPhysicalPower = new();
    public readonly SyncVar<CardData> Data = new();
    public readonly SyncVar<Player> ControllingPlayer = new();
    public readonly SyncVar<Hand> ControllingPlayerHand = new();
    public bool IsClone { get; protected set; }
    [AllowMutableSyncTypeAttribute] public SyncVar<bool> IsDraftCard = new();
    #endregion

    [SerializeField] protected Image disableScreen;
    [SerializeField] protected Image hoverScreen;
    [SerializeField] protected TMP_Text magicalPowerText;
    [SerializeField] protected TMP_Text physicalPowerText;
    [SerializeField] protected TMP_Text cardTypeText;
    [SerializeField] protected Image cardImage;
    [SerializeField] protected TMP_Text costText;
    [SerializeField] protected TMP_Text nameText;
    protected Player player;

    public enum CardType
    {
        Adventurer,
        MagicItem,
        Spell,
        Potion,
        Quest,
        Summon
    }

    private void Start()
    {
        player = Player.Instance;
    }

    /// <summary>
    /// Despawns the card on the server.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ServerDespawnCard()
    {
        this.Despawn();
    }

    /// <summary>
    /// Sets the card's owner and updates the controlling player's hand.
    /// </summary>
    /// <param name="player">The player who will own the card.</param>
    [Server]
    public void SetCardOwner(Player player)
    {
        ControllingPlayer.Value = player;
        ControllingPlayerHand.Value = player.controlledHand.Value;
        GiveOwnership(player.Owner);
        IsDraftCard.Value = false;
    }

    /// <summary>
    /// Sets the parent transform of the card on the server and updates the state for observers.
    /// </summary>
    /// <param name="parentTransform">The new parent transform to set.</param>
    /// <param name="worldPositionStays">Whether to maintain the world position of the card.</param>
    [Server]
    public virtual void SetCardParent(Transform parentTransform, bool worldPositionStays)
    {
        ObserversSetCardParent(parentTransform, worldPositionStays);
        this.transform.SetParent(parentTransform, worldPositionStays);
    }

    /// <summary>
    /// Server-side RPC to set the parent transform of the card.
    /// </summary>
    /// <param name="parentTransform">The new parent transform.</param>
    /// <param name="worldPositionStays">Whether to maintain the world position of the card.</param>
    [ServerRpc(RequireOwnership = false)]
    public virtual void ServerSetCardParent(Transform parentTransform, bool worldPositionStays)
    {
        SetCardParent(parentTransform, worldPositionStays);
    }

    /// <summary>
    /// Updates the card's parent transform on all clients.
    /// </summary>
    /// <param name="parentTransform">The new parent transform.</param>
    /// <param name="worldPositionStays">Whether to maintain the world position of the card.</param>
    [ObserversRpc(BufferLast = true)]
    protected virtual void ObserversSetCardParent(Transform parentTransform, bool worldPositionStays)
    {
        this.transform.SetParent(parentTransform, worldPositionStays);
    }

    /// <summary>
    /// Server-side RPC to set the card's owner and update related properties.
    /// </summary>
    /// <param name="owningPlayer">The player who will own the card.</param>
    [ServerRpc(RequireOwnership = false)]
    public virtual void ServerSetCardOwner(Player owningPlayer)
    {
        ControllingPlayer.Value = owningPlayer;
        ControllingPlayerHand.Value = owningPlayer.controlledHand.Value;
        GiveOwnership(owningPlayer.Owner);
        IsDraftCard.Value = false;
    }

    /// <summary>
    /// Updates the card's scale on all clients.
    /// </summary>
    /// <param name="scale">The new scale to set.</param>
    [ObserversRpc]
    public void ObserversSetCardScale(Vector2 scale)
    {
        GetComponent<RectTransform>().localScale = scale;
    }

    /// <summary>
    /// Resets the card's power to its original values and updates clients.
    /// </summary>
    [Server]
    public virtual void ResetPower()
    {
        PhysicalPower.Value = OriginalPhysicalPower.Value;
        MagicalPower.Value = OriginalMagicalPower.Value;

        ObserversUpdatePowerText(PhysicalPower.Value, MagicalPower.Value);
    }

    /// <summary>
    /// Updates the power text on all clients, reflecting changes to physical and magical power.
    /// </summary>
    /// <param name="physicalPower">The updated physical power.</param>
    /// <param name="magicalPower">The updated magical power.</param>
    [ObserversRpc(BufferLast = true)]
    protected virtual void ObserversUpdatePowerText(int physicalPower, int magicalPower)
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
    protected virtual void UpdatePowerTextColor(int physicalPower, int magicalPower, int originalPhysical, int originalMagical)
    {
        physicalPowerText.color = physicalPower > originalPhysical ? Color.green : physicalPower < originalPhysical ? Color.red : Color.white;
        magicalPowerText.color = magicalPower > originalMagical ? Color.green : magicalPower < originalMagical ? Color.red : Color.white;
    }

    /// <summary>
    /// Loads the card data and updates the relevant SyncVars on the server.
    /// </summary>
    /// <param name="cardData">The card data to load.</param>
    [Server]
    public virtual void LoadCardData(CardData cardData)
    {
        PhysicalPower.Value = cardData.PhysicalPower;
        MagicalPower.Value = cardData.MagicalPower;
        OriginalPhysicalPower.Value = cardData.OriginalPhysicalPower;
        OriginalMagicalPower.Value = cardData.OriginalMagicalPower;
        CardName.Value = cardData.CardName;
        Cost.Value = cardData.Cost;
        CardDescription.Value = cardData.CardDescription;
        Data.Value = cardData;

        ObserversLoadCardData(cardData);
    }

    /// <summary>
    /// Copies the card data from the source card to the newly spawned card.
    /// </summary>
    /// <param name="connection">The network connection of the client receiving the card data.</param>
    /// <param name="newCardObject">The new card object to receive the copied data.</param>
    /// <param name="sourceCardObject">The source card object to copy data from.</param>
    public void CopyCardData(NetworkConnection connection, GameObject newCardObject, GameObject sourceCardObject)
    {
        Card originalCard;
        Card newCard = newCardObject.GetComponent<Card>();
        GameObject referenceCardObject = sourceCardObject.GetComponent<SpotlightCard>().referenceCard;

        // If there is a reference card, copy its data, otherwise use sourceCard
        originalCard = referenceCardObject ? referenceCardObject.GetComponent<Card>() : sourceCardObject.GetComponent<Card>();
        newCardObject.GetComponent<SpotlightCard>().referenceCard = originalCard.gameObject;

        // Copy data to a new Item Card
        if (newCard is ItemCard itemCard && originalCard is ItemCardHeader itemCardHeader)
        {
            itemCard.TargetCopyItemHeaderData(connection, itemCardHeader);
        }
        else
        {
            newCard.TargetCopyCardData(connection, originalCard);
        }

        // Copy data into Adventurer Card's Item Header
        if (newCard is AdventurerCard adventurerCard && adventurerCard.HasItem.Value)
        {
            adventurerCard.Item.Value.TargetCopyCardData(connection, originalCard);
            adventurerCard.Item.Value.GetComponent<SpotlightCard>().referenceCard = originalCard.GetComponent<AdventurerCard>().Item.Value.gameObject;
        }
    }

    /// <summary>
    /// Updates the card's visual representation on all clients based on the provided card data.
    /// </summary>
    /// <param name="cardData">The card data to load into the visual elements.</param>
    [ObserversRpc(BufferLast = true)]
    protected virtual void ObserversLoadCardData(CardData cardData) { }

    /// <summary>
    /// Updates the target client with the copied card data from the original card.
    /// </summary>
    /// <param name="connection">The network connection of the target client.</param>
    /// <param name="originalCard">The original card to copy data from.</param>
    [TargetRpc]
    public virtual void TargetCopyCardData(NetworkConnection connection, Card originalCard) { }

    public virtual void OnHover()
    {
        if (IsClone) return;  // Do not show hover screen for enlarged/spotlight cards

        if (ShouldToggleDisableScreen()) ToggleDisableScreen(true);
        else ToggleHoverScreen(true);
    }

    public virtual bool ShouldToggleDisableScreen()
    {
        if (IsDraftCard.Value && player.Gold.Value < Cost.Value) return true; // Check player gold if hovering a DraftCard
        else return false;
    }

    public virtual void OnPointerExit()
    {
        ToggleHoverScreen(false);
        if (disableScreen) ToggleDisableScreen(false);
    }

    protected void ToggleDisableScreen(bool toggle)
    {
        disableScreen.gameObject.SetActive(toggle);
        if (toggle) hoverScreen.gameObject.SetActive(false);
    }

    protected void ToggleHoverScreen(bool toggle)
    {
        hoverScreen.gameObject.SetActive(toggle);
    }

    public override bool Equals(object obj)
    {
        if (obj is Card otherCard)
        {
            return this.GetInstanceID() == otherCard.GetInstanceID();
        }
        return false;
    }

    public override int GetHashCode()
    {
        return this.GetInstanceID().GetHashCode();
    }
}
