using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class ItemCardHeader : Card
{
    #region SyncVars
    public SyncVar<bool> IsDisabled { get; private set; }
    public SyncVar<int> OriginalMagicalPower { get; private set; }
    public SyncVar<int> OriginalPhysicalPower { get; private set; }
    public SyncVar<Transform> ParentTransform { get; private set; }
    #endregion

    #region UI Elements
    [SerializeField] private TMP_Text disableTypeText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text physicalPowerText;
    #endregion

    /// <summary>
    /// Loads the card data and updates the relevant SyncVars on the server.
    /// </summary>
    /// <param name="cardData">The card data to load.</param>
    [Server]
    public override void LoadCardData(CardData cardData)
    {
        OriginalPhysicalPower.Value = cardData.OriginalPhysicalPower;
        OriginalMagicalPower.Value = cardData.OriginalMagicalPower;

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
    }

    /// <summary>
    /// Server-side method to change the card's physical power, only if it is part of a quest.
    /// </summary>
    /// <param name="powerDelta">The amount to change the physical power by.</param>
    [ServerRpc(RequireOwnership = false)]
    public void ServerChangePhysicalPower(int powerDelta)
    {
        if (!ParentTransform.Value || !ParentTransform.Value.parent.CompareTag("Quest")) return;
        if (OriginalPhysicalPower.Value > 0)
        {
            PhysicalPower += powerDelta;
            ObserversUpdatePowerText(PhysicalPower, MagicalPower);
        }
    }

    /// <summary>
    /// Server-side method to change the card's magical power, only if it is part of a quest.
    /// </summary>
    /// <param name="powerDelta">The amount to change the magical power by.</param>
    [ServerRpc(RequireOwnership = false)]
    public void ServerChangeMagicalPower(int powerDelta)
    {
        if (!ParentTransform.Value || !ParentTransform.Value.parent.CompareTag("Quest")) return;
        if (OriginalMagicalPower.Value > 0)
        {
            MagicalPower += powerDelta;
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

        UpdatePowerTextColor(physicalPower, magicalPower, OriginalPhysicalPower.Value, OriginalMagicalPower.Value);
    }

    /// <summary>
    /// Updates the color of the power text based on comparison with the original power values.
    /// </summary>
    /// <param name="physicalPower">The current physical power.</param>
    /// <param name="magicalPower">The current magical power.</param>
    /// <param name="originalPhysicalPower">The original physical power.</param>
    /// <param name="originalMagicalPower">The original magical power.</param>
    private void UpdatePowerTextColor(int physicalPower, int magicalPower, int originalPhysicalPower, int originalMagicalPower)
    {
        physicalPowerText.color = physicalPower > originalPhysicalPower ? Color.green :
                                  physicalPower < originalPhysicalPower ? Color.red : Color.white;

        magicalPowerText.color = magicalPower > originalMagicalPower ? Color.green :
                                 magicalPower < originalMagicalPower ? Color.red : Color.white;
    }

    /// <summary>
    /// Resets the card's power to its original values and enables the card if it was disabled.
    /// </summary>
    [Server]
    public void ResetPower()
    {
        PhysicalPower = OriginalPhysicalPower.Value;
        MagicalPower = OriginalMagicalPower.Value;
        ObserversUpdatePowerText(PhysicalPower, MagicalPower);

        IsDisabled.Value = false;
        ObserversSetDisable(false);
    }

    /// <summary>
    /// Sets the card's active state and notifies all clients.
    /// </summary>
    /// <param name="active">Whether the card should be active or not.</param>
    [Server]
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        ObserversSetActive(active);
    }

    /// <summary>
    /// Updates the card's active state on all clients.
    /// </summary>
    /// <param name="active">The active state to set on all clients.</param>
    [ObserversRpc(BufferLast = true)]
    private void ObserversSetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    /// <summary>
    /// Disables the card, setting its power to 0 and displaying the disable type on all clients.
    /// </summary>
    /// <param name="disableType">The type of disable action to apply.</param>
    [Server]
    public void DisableItem(string disableType)
    {
        if (IsDisabled.Value) return;

        PhysicalPower = 0;
        MagicalPower = 0;
        IsDisabled.Value = true;

        ObserversUpdatePowerText(PhysicalPower, MagicalPower);
        ObserversSetDisable(true);
        ObserversSetDisableText(disableType);
    }

    /// <summary>
    /// Updates the disable state of the card on all clients.
    /// </summary>
    /// <param name="value">Whether the card should be disabled or not.</param>
    [ObserversRpc(BufferLast = true)]
    private void ObserversSetDisable(bool value)
    {
        gameObject.transform.GetChild(1).gameObject.SetActive(value);
    }

    /// <summary>
    /// Sets the disable type text on all clients.
    /// </summary>
    /// <param name="disableType">The type of disable action to display.</param>
    [ObserversRpc(BufferLast = true)]
    private void ObserversSetDisableText(string disableType)
    {
        disableTypeText.text = disableType;
    }

    /// <summary>
    /// Copies the card data to the target client from the original card.
    /// </summary>
    /// <param name="connection">The network connection of the target client.</param>
    /// <param name="originalCard">The original card to copy data from.</param>
    [TargetRpc]
    public override void TargetCopyCardData(NetworkConnection connection, Card originalCard)
    {
        isClone = true;
        AdventurerCard card = originalCard as AdventurerCard;
        ItemCardHeader item = card.Item;

        physicalPowerText.text = item.PhysicalPower.ToString();
        magicalPowerText.text = item.MagicalPower.ToString();
        nameText.text = item.CardName;

        UpdatePowerTextColor(item.PhysicalPower, item.MagicalPower, item.OriginalPhysicalPower.Value, item.OriginalMagicalPower.Value);
    }
}
