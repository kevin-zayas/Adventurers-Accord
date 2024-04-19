using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Card : NetworkBehaviour
{
    [field: SerializeField]
    [field: SyncVar] 
    public Player ControllingPlayer { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public Hand ControllingPlayerHand { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public Transform Parent { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool IsDraftCard { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public string Name { get; private set; }

    [SyncVar] private string Description;

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
    public int Cost { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public bool HasItem { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public ItemCardHeader Item { get; private set; }

    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image cardImage;
    
    private void Start()
    {
        nameText.text = Name;
        descriptionText.text = Description;
        physicalPowerText.text = PhysicalPower.ToString();
        magicalPowerText.text = MagicalPower.ToString();
        costText.text = Cost.ToString();

        if (IsServer)
        {
            IsDraftCard = true;
        }     
    }

    [Server]
    public void SetCardParent(Transform newParent, bool worldPositionStays)
    {
        OberserversSetCardParent(newParent, worldPositionStays);
        this.transform.SetParent(newParent, worldPositionStays);        //need to set the parent on the server immediately instead of waiting for the observers to set it

        if (this.Parent != null && this.Parent != newParent)
        {
            if (this.Parent.CompareTag("Quest")) OnQuestReturn(this.Parent); //issue when dragging card since parent is set to canvas
            this.Parent = newParent;
            if (newParent.CompareTag("Quest")) OnQuestDispatch(newParent);
        }
        else { this.Parent = newParent; }

    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetCardParent(Transform newParent, bool worldPositionStays)
    {
        SetCardParent(newParent, worldPositionStays);
    }

    [ObserversRpc(BufferLast = true)]
    private void OberserversSetCardParent(Transform newParent, bool worldPositionStays)
    {
        Vector3 scale;

        if (newParent.CompareTag("Hand")) scale = new Vector3(2f, 2f, 1f);
        else scale = new Vector3(1f, 1f, 1f);

        this.transform.localScale = scale;
        this.transform.SetParent(newParent, worldPositionStays);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetCardOwner(Player player)
    {
        ControllingPlayer = player;
        ControllingPlayerHand = player.controlledHand;
        GiveOwnership(player.Owner);
        IsDraftCard = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerEquipItem(bool hasItem, CardData itemCardData)
    {
        HasItem = hasItem;

        ItemCardHeader itemCardHeader = gameObject.transform.GetChild(1).GetComponent<ItemCardHeader>();
        Spawn(itemCardHeader.gameObject);

        itemCardHeader.LoadCardData(itemCardData);
        Item = itemCardHeader;

        if (Name == "Sorcerer") ResetPower();
        
        ObserversAdjustCardSize(220);    // increase card size to adjust for item header
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversAdjustCardSize(int height)
    {
        gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }


    [ServerRpc(RequireOwnership = false)]
    public void ServerChangePhysicalPower(int power)
    {
        ChangePhysicalPower(power);
    }

    [Server]
    public void ChangePhysicalPower(int power)
    {
        if (!Parent.CompareTag("Quest")) return;
        if (OriginalPhysicalPower > 0)
        {
            PhysicalPower += power;                                 //TODO: clamp power so its not less than 0
            ObserversUpdatePowerText(PhysicalPower, MagicalPower);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerChangeMagicalPower(int power)
    {
        ChangeMagicalPower(power);
    }

    [Server]
    public void ChangeMagicalPower(int power)
    {
        print(Parent.tag);
        if (!Parent.CompareTag("Quest")) return;
        if (OriginalMagicalPower > 0)
        {
            print(power);
            MagicalPower += power;
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
        //print($"{Name} Resetting Power");
        PhysicalPower = OriginalPhysicalPower;
        MagicalPower = OriginalMagicalPower;

        if (Name == "Sorcerer" && HasItem && !Item.IsDisabled) MagicalPower += 3;

        ObserversUpdatePowerText(PhysicalPower, MagicalPower);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerResetPower()
    {
        ResetPower();
    }

    [Server]
    public void DisableItem(string disableType)
    {
        if (!HasItem || Item.IsDisabled) return;

        Item.DisableItem(disableType);

        if (Name == "Sorcerer") ResetPower();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerDisableItem(string disableType)
    {
        DisableItem(disableType);
    }


    [Server]
    public void LoadCardData(CardData cardData)
    {
        PhysicalPower = cardData.physicalPower;
        MagicalPower = cardData.magicalPower;
        OriginalPhysicalPower = cardData.originalPhysicalPower;
        OriginalMagicalPower = cardData.originalMagicalPower;
        Name = cardData.cardName;
        Description = cardData.cardDescription;
        Cost = cardData.cost;

        ObserversLoadCardData(cardData);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversLoadCardData(CardData cardData)
    {
        physicalPowerText.text = cardData.physicalPower.ToString();
        magicalPowerText.text = cardData.magicalPower.ToString();
        nameText.text = cardData.cardName;
        descriptionText.text = cardData.cardDescription;
        costText.text = cardData.cost.ToString();

        //cardSprite.GetComponent<Image>().sprite = Resources.Load<Sprite>("NPC_characters/NPC_characters/07");
        cardImage.sprite = Resources.Load<Sprite>("Card_Sprites/"+cardData.cardName);
    }

    [Server]
    private void OnQuestDispatch(Transform newParent)
    {
        QuestLane questLane = newParent.parent.GetComponent<QuestLane>();
        questLane.AddAdventurerToQuestLane(this);
    }

    [Server]
    private void OnQuestReturn(Transform newParent)
    {
        //print($"{Name} Returning to hand");
        QuestLane questLane = newParent.parent.GetComponent<QuestLane>();
        questLane.RemoveAdventurerFromQuestLane(this);
        if (HasItem) Item.ResetPower();
        ResetPower();
    }

    public void OnResolutionClick()
    {
        if (GameManager.Instance.CurrentPhase != GameManager.Phase.Resolution) return;
        if (!GameManager.Instance.Players[LocalConnection.ClientId].IsPlayerTurn) return;

        QuestLane lane = Parent.parent.GetComponent<QuestLane>();
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
            //add warning popup?
            print("invalid target");
        }

    }

}
