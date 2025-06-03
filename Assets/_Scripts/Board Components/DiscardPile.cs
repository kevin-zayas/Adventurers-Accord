using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DiscardPile : NetworkBehaviour
{
    #region Singleton
    public static DiscardPile Instance { get; private set; }
    #endregion

    [SerializeField] private TMP_Text restingAdventurerCount;

    void Start()
    {
        Instance = this;
    }

    [Server]
    public void DiscardCard(AdventurerCard card, Player player)
    {
        card.SetCardParent(gameObject.transform, false);
        player.DiscardPile.Add(card);
        card.ResetPotionPower();
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
                    card.SetCardParent(player.controlledHand.Value.transform, false);
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
