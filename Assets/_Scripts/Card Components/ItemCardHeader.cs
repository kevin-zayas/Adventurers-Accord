using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class ItemCardHeader : Card
{
    #region SyncVars
    public readonly SyncVar<bool> IsDisabled = new();
    [AllowMutableSyncTypeAttribute] public SyncVar<Transform> ParentCard = new();
    #endregion

    public bool equippedOnBattlemage = false;

    #region UI Elements
    [SerializeField] private TMP_Text disableTypeText;
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
    [Server]
    public void ChangePhysicalPower(int powerDelta)
    {
        if (!ParentCard.Value || !ParentCard.Value.parent.CompareTag("Quest")) return;
        if (OriginalPhysicalPower.Value > 0)
        {
            PhysicalPower.Value += powerDelta;
            ObserversUpdatePowerText(PhysicalPower.Value, MagicalPower.Value);
        }
    }

    /// <summary>
    /// Server-side method to change the card's magical power, only if it is part of a quest.
    /// </summary>
    /// <param name="powerDelta">The amount to change the magical power by.</param>
    [Server]
    public void ChangeMagicalPower(int powerDelta)
    {
        if (!ParentCard.Value || !ParentCard.Value.parent.CompareTag("Quest")) return;
        if (OriginalMagicalPower.Value > 0)
        {
            MagicalPower.Value += powerDelta;
            ObserversUpdatePowerText(PhysicalPower.Value, MagicalPower.Value);
        }
    }

    /// <summary>
    /// Resets the card's power to its original values and enables the card if it was disabled.
    /// </summary>
    [Server]
    public override void ResetPower()
    {
        base.ResetPower();
        if (equippedOnBattlemage) ApplyBalancedArsenal();
        IsDisabled.Value = false;
        ObserversSetDisable(false);
    }

    [Server]
    public void ApplyBalancedArsenal()
    {
        if (OriginalPhysicalPower.Value > OriginalMagicalPower.Value)
        {
            MagicalPower.Value = PhysicalPower.Value;
        }
        else
        {
            PhysicalPower.Value = MagicalPower.Value;
        }
        ObserversUpdatePowerText(PhysicalPower.Value, MagicalPower.Value);
    }

    /// <summary>
    /// Disables the card, setting its power to 0 and displaying the disable type on all clients.
    /// </summary>
    /// <param name="disableType">The type of disable action to apply.</param>
    [Server]
    public void DisableItem()
    {
        if (IsDisabled.Value) return;

        PhysicalPower.Value = 0;
        MagicalPower.Value = 0;
        IsDisabled.Value = true;

        ObserversUpdatePowerText(PhysicalPower.Value, MagicalPower.Value);
        ObserversSetDisable(true);
    }

    /// <summary>
    /// Updates the disable state of the card on all clients.
    /// </summary>
    /// <param name="value">Whether the card should be disabled or not.</param>
    [ObserversRpc(BufferLast = true)]
    private void ObserversSetDisable(bool value, string disableType = "Disabled")
    {
        gameObject.transform.GetChild(1).gameObject.SetActive(value);
        if (value) disableTypeText.text = disableType;
    }

    /// <summary>
    /// Copies the card data to the target client from the original card.
    /// </summary>
    /// <param name="connection">The network connection of the target client.</param>
    /// <param name="originalCard">The original card to copy data from.</param>
    [TargetRpc]
    public void TargetCopyCardData(NetworkConnection connection, Card originalCard, bool isItemDisabled)
    {
        IsClone = true;
        AdventurerCard card = originalCard as AdventurerCard;
        ItemCardHeader item = card.Item.Value;

        physicalPowerText.text = item.PhysicalPower.Value.ToString();
        magicalPowerText.text = item.MagicalPower.Value.ToString();
        nameText.text = item.CardName.Value;

        if (isItemDisabled)
        {
            gameObject.transform.GetChild(1).gameObject.SetActive(true);
            disableTypeText.text = "Disabled";
        }

        UpdatePowerTextColor(item.PhysicalPower.Value, item.MagicalPower.Value, item.OriginalPhysicalPower.Value, item.OriginalMagicalPower.Value);
    }
}
