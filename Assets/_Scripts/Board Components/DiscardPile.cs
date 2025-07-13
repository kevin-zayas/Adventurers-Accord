using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DiscardPile : CardHolder
{
    #region Singleton
    public static DiscardPile Instance { get; private set; }
    #endregion

    [SerializeField] private TMP_Text restingAdventurerCount;

    protected override void Start()
    {
        Instance = this;
        base.Start();
        HolderType = CardHolderType.Discard;
    }

    [Server]
    public override void AddCard(Card card)
    {
        AdventurerCard adventurerCard = card as AdventurerCard;

        adventurerCard.SetCardParent(transform, false, this);
        adventurerCard.ControllingPlayer.Value.DiscardPile.Add(adventurerCard);
        adventurerCard.ResetPotionPower();

        ObserversResetCardPosition(adventurerCard);
    }

    [Server]
    public override void MoveCard(Card card, CardHolder newCardHolder, Transform originalCardSlot = null)
    {
        newCardHolder.AddCard(card);
    }

    [Server]
    public void RecoverAdventurers()
    {
        foreach (Player player in GameManager.Instance.Players)
        {
            List<AdventurerCard> cardsToRemove = new List<AdventurerCard>();

            foreach (AdventurerCard card in player.DiscardPile)
            {
                if (card.CurrentRestPeriod.Value > 0) card.ChangeCurrentRestPeriod(-1);
                else
                {
                    MoveCard(card, player.ControlledHand.Value);
                    cardsToRemove.Add(card);
                    card.ResetCurrentRestPeriod();
                }
            }

            foreach (AdventurerCard card in cardsToRemove)
            {
                player.DiscardPile.Remove(card);
            }

            TargetUpdateRestingCount(player.Owner, player.DiscardPile.Count);
        }
    }

    [TargetRpc]
    private void TargetUpdateRestingCount(NetworkConnection connection, int discardPileCount)
    {
        restingAdventurerCount.text = discardPileCount.ToString();
    }
}
