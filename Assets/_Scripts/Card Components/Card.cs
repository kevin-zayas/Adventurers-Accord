using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
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
    public readonly SyncVar<CardData> Data = new();
    public readonly SyncVar<Player> ControllingPlayer = new();
    public readonly SyncVar<Hand> ControllingPlayerHand = new();
    [AllowMutableSyncTypeAttribute] public SyncVar<bool> IsDraftCard = new();
    #endregion

    [SerializeField] protected Image disableScreen;
    [SerializeField] protected Image hoverScreen;
    [SerializeField] protected bool isClone = false;
    protected Player player;

    private void Start()
    {
        if (IsServerInitialized && !ControllingPlayer.Value)
        {
            IsDraftCard.Value = true;
        }
        //else      //may need to add this back when putting it on dedicated server
        //{
        player = GameManager.Instance.Players[LocalConnection.ClientId];
        //}
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
        ObserverSetCardOwner(player);
    }

    [ObserversRpc]
    private void ObserverSetCardOwner(Player player)
    {
        ControllingPlayer.Value = player;
        ControllingPlayerHand.Value = player.controlledHand.Value;
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
    }

    /// <summary>
    /// Server-side RPC to set the parent transform of the card.
    /// </summary>
    /// <param name="parentTransform">The new parent transform.</param>
    /// <param name="worldPositionStays">Whether to maintain the world position of the card.</param>
    [ServerRpc(RequireOwnership = false)]
    public virtual void ServerSetCardParent(Transform parentTransform, bool worldPositionStays)
    {
        ObserversSetCardParent(parentTransform, worldPositionStays);
        this.transform.SetParent(parentTransform, worldPositionStays);
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
    /// Loads the card data and updates the relevant SyncVars on the server.
    /// </summary>
    /// <param name="cardData">The card data to load.</param>
    [Server]
    public virtual void LoadCardData(CardData cardData)
    {
        PhysicalPower.Value = cardData.PhysicalPower;
        MagicalPower.Value = cardData.MagicalPower;
        CardName.Value = cardData.CardName;
        CardDescription.Value = cardData.CardDescription;
        Data.Value = cardData;

        ObserversLoadCardData(cardData);
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
        if (isClone) return;  // Do not show hover screen for enlarged/spotlight cards

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
}
