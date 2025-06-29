using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class SpellCard : Card
{
    #region SyncVars
    public readonly SyncVar<bool> IsNegativeEffect = new();
    public readonly SyncVar<bool> IsNumerical = new();
    #endregion

    #region UI Elements
    [SerializeField] private TMP_Text descriptionText;
    #endregion


    [Server]
    public override void SetCardParent(Transform parent, bool worldPositionStays)
    {
        Player player = ControllingPlayer.Value;

        if (parent.CompareTag("Hand") && player.IsMagesGuild && IsNumerical.Value)
        {
            ResetPower();
            PhysicalPower.Value = PhysicalPower.Value > 0 ? PhysicalPower.Value + 1 : PhysicalPower.Value < 0 ? PhysicalPower.Value - 1 : PhysicalPower.Value;
            MagicalPower.Value = MagicalPower.Value > 0 ? MagicalPower.Value + 1 : MagicalPower.Value < 0 ? MagicalPower.Value - 1 : MagicalPower.Value;

            ObserversUpdatePowerText(PhysicalPower.Value, MagicalPower.Value);

        }
        else if (parent.CompareTag("Quest") && player.IsMagesGuild)
        {
            QuestLane questLane = parent.parent.GetComponent<QuestLane>();
            int questIndex = questLane.QuestLocation.Value.QuestLocationIndex;
            player.GuildBonusTracker[questIndex]["spellsPlayed"]++;

            if (questLane.Player.Value != player)
            {
                questLane = questLane.QuestLocation.Value.GetQuestLanes()[player.PlayerID.Value];
            }

            if (player.GuildBonusTracker[questIndex]["spellsPlayed"] == 2 && questLane.CurrentAdventurerCount.Value > 0)
            {

                questLane.UpdateGuildBonusPower(0, 2);
                player.UpdateGuildRecapTracker("Arcane Momentum (+2 M. Power)", 1);
                print($"Mages Guild Bonus - Player {player.PlayerID.Value} +2 Magical Power");
            }
        }
        else if (parent.CompareTag("Quest") && player.IsAssassinsGuild && IsNegativeEffect.Value)
        {
            QuestLane questLane = parent.parent.GetComponent<QuestLane>();
            int questIndex = questLane.QuestLocation.Value.QuestLocationIndex;
            player.GuildBonusTracker[questIndex]["curseSpellsPlayed"]++;
        }
        base.SetCardParent(parent, worldPositionStays);
    }

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
        IsNegativeEffect.Value = cardData.IsNegativeEffect;
        IsNumerical.Value = cardData.IsNumerical;

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
        cardTypeText.text = cardData.CardType.ToString();
        costText.text = cardData.Cost.ToString();

        cardImage.sprite = CardDatabase.Instance.CardSpriteMap[cardData.CardName];
    }

    /// <summary>
    /// Updates the target client with the copied card data from the original card.
    /// </summary>
    /// <param name="connection">The network connection of the target client.</param>
    /// <param name="originalCard">The original card to copy data from.</param>
    [TargetRpc]
    public override void TargetCopyCardData(NetworkConnection connection, Card originalCard)
    {
        IsClone = true;
        SpellCard card = originalCard as SpellCard;

        cardImage.sprite = CardDatabase.Instance.CardSpriteMap[card.CardName.Value];

        physicalPowerText.text = card.PhysicalPower.Value.ToString();
        magicalPowerText.text = card.MagicalPower.Value.ToString();
        nameText.text = card.CardName.Value;
        descriptionText.text = card.CardDescription.Value;
        costText.text = card.Cost.Value.ToString();

        UpdatePowerTextColor(card.PhysicalPower.Value, card.MagicalPower.Value, card.OriginalPhysicalPower.Value, card.OriginalMagicalPower.Value);
    }

    public override bool ShouldToggleDisableScreen()
    {
        if (base.ShouldToggleDisableScreen()) return true;
        if (transform.parent.CompareTag("Quest")) return true;

        return false;
    }
}
