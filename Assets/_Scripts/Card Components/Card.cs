using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.UI;

public abstract class Card : NetworkBehaviour
{
    #region SyncVars
    public SyncVar<string> CardName { get; protected set; }
    public SyncVar<string> CardDescription { get; protected set; }
    public SyncVar<int> PhysicalPower { get; protected set; }
    public SyncVar<int> MagicalPower { get; protected set; }
    public SyncVar<CardData> Data { get; protected set; }
    public SyncVar<Player> ControllingPlayer { get; protected set; }
    public SyncVar<Hand> ControllingPlayerHand { get; protected set; }
    #endregion

    [SerializeField] protected Image disableScreen;
    protected bool isClone = false;
    

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
        return;
    }

    public virtual void OnPointerExit()
    {
        return;
    }

    protected void ToggleDisableScreen(bool toggle)
    {
        disableScreen.gameObject.SetActive(toggle);
    }
}
