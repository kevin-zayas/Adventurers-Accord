using FishNet.Object.Synchronizing;
using FishNet.Object;
using UnityEngine;
using TMPro;

public class ItemCardHeader : NetworkBehaviour
{
    [field: SerializeField]
    [field: SyncVar]
    public Player ControllingPlayer { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public Transform Parent { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public string Name { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int PhysicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int MagicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int OriginalPhysicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int OriginalMagicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool IsDisabled { get; private set; }

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text disableTypeText;

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetCardOwner(Player owner)
    {
        ControllingPlayer = owner;
        //ControllingPlayerHand = owner.controlledHand;
    }

    [ObserversRpc(BufferLast = true)]
    public void ObserversSetItemInfo(int physicalPower, int magicalPower, string name)
    {
        physicalPowerText.text = physicalPower.ToString();
        magicalPowerText.text = magicalPower.ToString();
        nameText.text = name;
    }

    [Server]
    public void LoadCardData(CardData cardData)
    {
        PhysicalPower = cardData.physicalPower;
        MagicalPower = cardData.magicalPower;
        OriginalPhysicalPower = cardData.originalPhysicalPower;
        OriginalMagicalPower = cardData.originalMagicalPower;
        Name = cardData.cardName;

        ObserversLoadCardData(cardData);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversLoadCardData(CardData cardData)
    {
        physicalPowerText.text = cardData.physicalPower.ToString();
        magicalPowerText.text = cardData.magicalPower.ToString();
        nameText.text = cardData.cardName;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerChangePhysicalPower(int powerDelta)
    {
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
        if(!Parent.transform.parent.CompareTag("Quest")) return;
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

        if (physicalPower > OriginalPhysicalPower) physicalPowerText.color = Color.green;
        else if (physicalPower < OriginalPhysicalPower) physicalPowerText.color = Color.red;
        else physicalPowerText.color = Color.white;

        if (magicalPower > OriginalMagicalPower) magicalPowerText.color = Color.green;
        else if (magicalPower < OriginalMagicalPower) magicalPowerText.color = Color.red;
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

    [ServerRpc(RequireOwnership = false)]
    public void ServerResetPower()
    {
        ResetPower();
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
}
