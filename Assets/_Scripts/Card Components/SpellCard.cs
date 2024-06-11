using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellCard : Card
{
    [field: SerializeField]
    [field: SyncVar]
    public bool IsGreaseSpell { get; private set; }

    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image cardImage;

    private void Start()
    {
        nameText.text = Name;
        descriptionText.text = Description;
        physicalPowerText.text = PhysicalPower.ToString();
        magicalPowerText.text = MagicalPower.ToString();
    }

    //[Server]
    //public void SetCardParent(Transform parent, bool worldPositionStays)
    //{
    //    OberserversSetCardParent(parent, worldPositionStays);
    //}

    //[ServerRpc(RequireOwnership = false)]
    //public void ServerSetCardParent(Transform parent, bool worldPositionStays)
    //{
    //    OberserversSetCardParent(parent, worldPositionStays);
    //    this.transform.SetParent(parent, worldPositionStays);
    //}

    [ObserversRpc(BufferLast = true)]
    protected override void OberserversSetCardParent(Transform parent, bool worldPositionStays)
    {
        if (parent.CompareTag("Quest"))
        {
            this.transform.localScale = new Vector3(.5f, .5f, 1f);
        }
        this.transform.SetParent(parent, worldPositionStays);
    }

    [Server]
    public void LoadCardData(CardData cardData)
    {
        PhysicalPower = cardData.PhysicalPower;
        MagicalPower = cardData.MagicalPower;
        Name = cardData.CardName;
        Description = cardData.CardDescription;
        IsGreaseSpell = cardData.IsGreaseSpell;

        ObserversLoadCardData(cardData);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversLoadCardData(CardData cardData)
    {
        physicalPowerText.text = cardData.PhysicalPower.ToString();
        magicalPowerText.text = cardData.MagicalPower.ToString();
        nameText.text = cardData.CardName;
        descriptionText.text = cardData.CardDescription;

        if (cardData.PhysicalPower > 0 || cardData.MagicalPower > 0) cardImage.sprite = Resources.Load<Sprite>("ItemSpell_Sprites/PositiveSpell");
        else cardImage.sprite = Resources.Load<Sprite>("ItemSpell_Sprites/NegativeSpell");
    }
}
