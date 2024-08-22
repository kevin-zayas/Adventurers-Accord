using FishNet.Object.Synchronizing;
using FishNet.Object;
using UnityEngine;
using TMPro;
using FishNet.Connection;

public class ItemCardHeader : Card
{
    [field: SyncVar] public Transform Parent { get; private set; }
    [field: SyncVar] public int OriginalPhysicalPower { get; private set; }
    [field: SyncVar] public int OriginalMagicalPower { get; private set; }
    [field: SyncVar] public bool IsDisabled { get; private set; }

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text disableTypeText;

    public override void SetCardParent(Transform newParent, bool worldPositionStays) { }
    public override void ServerSetCardParent(Transform parent, bool worldPositionStays) { }
    protected override void ObserversSetCardParent(Transform parent, bool worldPositionStays) { }

    [Server]
    public override void LoadCardData(CardData cardData)
    {
        OriginalPhysicalPower = cardData.OriginalPhysicalPower;
        OriginalMagicalPower = cardData.OriginalMagicalPower;
        
        base.LoadCardData(cardData);
        //ObserversLoadCardData(cardData);
    }

    [ObserversRpc(BufferLast = true)]
    protected override void ObserversLoadCardData(CardData cardData)
    {
        physicalPowerText.text = cardData.PhysicalPower.ToString();
        magicalPowerText.text = cardData.MagicalPower.ToString();
        nameText.text = cardData.CardName;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerChangePhysicalPower(int powerDelta)
    {
        print(Parent);
        if (!Parent.transform.parent.CompareTag("Quest")) return;
        if (OriginalPhysicalPower > 0)
        {
            PhysicalPower += powerDelta;
            ObserversUpdatePowerText(PhysicalPower, MagicalPower);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerChangeMagicalPower(int powerDelta)
    {
        print(Parent);
        if (!Parent.transform.parent.CompareTag("Quest")) return;
        if (OriginalMagicalPower > 0)
        {
            MagicalPower += powerDelta;
            ObserversUpdatePowerText(PhysicalPower, MagicalPower);
        }
    }

    [ObserversRpc(BufferLast = true)]
    public void ObserversUpdatePowerText(int physicalPower, int magicalPower)
    {
        physicalPowerText.text = physicalPower.ToString();
        magicalPowerText.text = magicalPower.ToString();

        UpdatePowerTextColor(physicalPower, magicalPower, OriginalPhysicalPower, OriginalMagicalPower);
    }
    private void UpdatePowerTextColor(int physicalPower, int magicalPower, int originalPhysicalPower, int originalMagicalPower)
    {
        if (physicalPower > originalPhysicalPower) physicalPowerText.color = Color.green;
        else if (physicalPower < originalPhysicalPower) physicalPowerText.color = Color.red;
        else physicalPowerText.color = Color.white;

        if (magicalPower > originalMagicalPower) magicalPowerText.color = Color.green;
        else if (magicalPower < originalMagicalPower) magicalPowerText.color = Color.red;
        else magicalPowerText.color = Color.white;
    }

    [Server]
    public void ResetPower()
    {
        print($"{Name} Resetting Item Power");
        PhysicalPower = OriginalPhysicalPower;
        MagicalPower = OriginalMagicalPower;
        ObserversUpdatePowerText(PhysicalPower, MagicalPower);

        IsDisabled = false;
        ObserversSetDisable(false);
    }

    [Server]
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        ObserversSetActive(active);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversSetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    [Server]
    public void DisableItem(string disableType)
    {
        if (IsDisabled) return;

        PhysicalPower = 0;
        MagicalPower = 0;
        IsDisabled = true;

        ObserversUpdatePowerText(PhysicalPower, MagicalPower);
        ObserversSetDisable(true);
        ObserversSetDisableText(disableType);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversSetDisable(bool value)
    {
        gameObject.transform.GetChild(1).gameObject.SetActive(value);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversSetDisableText(string disableType)
    {
        disableTypeText.text = disableType;
    }

    [TargetRpc]
    public override void TargetCopyCardData(NetworkConnection connection, Card originalCard)
    {
        AdventurerCard card = originalCard as AdventurerCard;
        ItemCardHeader item = card.Item;

        physicalPowerText.text = item.PhysicalPower.ToString();
        magicalPowerText.text = item.MagicalPower.ToString();
        nameText.text = item.Name;

        UpdatePowerTextColor(item.PhysicalPower, item.MagicalPower, item.OriginalPhysicalPower, item.OriginalMagicalPower);
    }
}
