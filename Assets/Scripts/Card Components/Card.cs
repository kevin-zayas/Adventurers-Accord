using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

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
    
    [SyncVar]
    public int draftCardIndex;

    [field: SerializeField]
    [field: SyncVar] 
    public int PhysicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int MagicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int Cost { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public Vector3 CurrentScale { get; private set;}

    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text costText;

    private void Start()
    {
        physicalPowerText.text = PhysicalPower.ToString();
        magicalPowerText.text = MagicalPower.ToString();
        costText.text = Cost.ToString();

        if (IsServer)
        {
            IsDraftCard = true;
        }     
    }

    [Server]
    public void SetCardParent(Transform parent, bool worldPositionStays)
    {
        OberserversSetCardParent(parent, worldPositionStays, CurrentScale);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetCardParent(Transform parent, bool worldPositionStays)
    {
        OberserversSetCardParent(parent, worldPositionStays, CurrentScale);
        this.transform.SetParent(parent, worldPositionStays);
        this.Parent = parent;
    }

    [ObserversRpc(BufferLast = true)]
    private void OberserversSetCardParent(Transform parent, bool worldPositionStays, Vector3 scale)
    {
        this.transform.localScale = scale;
        this.transform.SetParent(parent, worldPositionStays);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetCardOwner(Player owner)
    {
        ControllingPlayer = owner;
        ControllingPlayerHand = owner.controlledHand;
        IsDraftCard = false;
    }

    [Server]
    public void SetCardScale(Vector3 scale)
    {
        //ObserversSetCardScale(scale);
        this.CurrentScale = scale;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetCardScale(Vector3 scale)
    {
        //ObserversSetCardScale(scale);
        this.CurrentScale = scale;
    }

    //[ObserversRpc(BufferLast = true)]
    //private void ObserversSetCardScale(Vector3 scale)
    //{
    //    this.transform.localScale = scale;
    //    //this.CurrentScale = scale;
    //}

}
