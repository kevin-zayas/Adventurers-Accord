using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellCard : Card
{
    #region SyncVars
    [field: SyncVar] public bool IsGreaseSpell { get; private set; }
    #endregion

    #region UI Elements
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image disableScreen;
    [SerializeField] private TMP_Text magicalPowerText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text physicalPowerText;
    #endregion

    /// <summary>
    /// Updates the card's parent transform on all clients, adjusting its scale if the parent is a Quest.
    /// </summary>
    /// <param name="parent">The new parent transform.</param>
    /// <param name="worldPositionStays">Whether to maintain the world position of the card.</param>
    [ObserversRpc(BufferLast = true)]
    protected override void ObserversSetCardParent(Transform parent, bool worldPositionStays)
    {
        if (parent.CompareTag("Quest"))
        {
            this.transform.localScale = new Vector3(.6f, .6f, 1f);
        }
        this.transform.SetParent(parent, worldPositionStays);
    }

    /// <summary>
    /// Loads the card data and updates the relevant SyncVars on the server.
    /// </summary>
    /// <param name="cardData">The card data to load.</param>
    [Server]
    public override void LoadCardData(CardData cardData)
    {
        IsGreaseSpell = cardData.IsGreaseSpell;

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
        descriptionText.text = cardData.CardDescription;

        cardImage.sprite = CardDatabase.Instance.SpriteMap[cardData.CardName];
    }

    /// <summary>
    /// Updates the target client with the copied card data from the original card.
    /// </summary>
    /// <param name="connection">The network connection of the target client.</param>
    /// <param name="originalCard">The original card to copy data from.</param>
    [TargetRpc]
    public override void TargetCopyCardData(NetworkConnection connection, Card originalCard)
    {
        SpellCard card = originalCard as SpellCard;

        cardImage.sprite = CardDatabase.Instance.SpriteMap[card.CardName];

        physicalPowerText.text = card.PhysicalPower.ToString();
        magicalPowerText.text = card.MagicalPower.ToString();
        nameText.text = card.CardName;
        descriptionText.text = card.CardDescription;
    }

    public override void OnHover()
    {
        if (transform.parent.CompareTag("Quest")) disableScreen.gameObject.SetActive(true); // Prevent dragging if the card is already in a quest lane

        // Only allow dragging during the Dispatch or Magic phase
        if (GameManager.Instance.CurrentPhase != GameManager.Phase.Dispatch &&
            GameManager.Instance.CurrentPhase != GameManager.Phase.Magic)
        {
            Debug.Log("Can't move spells during this phase");
            disableScreen.gameObject.SetActive(true);
        }
        else
        {
            print("can drag");
        }
    }

    public override void OnPointerExit()
    {
        disableScreen.gameObject.SetActive(false);
    }
}
