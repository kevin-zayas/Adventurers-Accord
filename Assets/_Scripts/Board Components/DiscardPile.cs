using FishNet.Object;
using System.Collections.Generic;

public class DiscardPile : NetworkBehaviour
{
    #region Singleton
    public static DiscardPile Instance { get; private set; }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    [Server]
    public void DiscardCard(AdventurerCard card, Player player)
    {
        card.SetCardParent(gameObject.transform, false);
        player.DiscardPile.Add(card);
        //card.CurrentRestPeriod.Value += card.RestPeriod.Value;
    }

    [Server]
    public void RecoverAdventurers()
    {
        foreach (Player player in GameManager.Instance.Players)
        {
            List<AdventurerCard> cardsToRemove = new List<AdventurerCard>();

            foreach (AdventurerCard card in player.DiscardPile)
            {
                if (card.CurrentRestPeriod.Value > 0) card.CurrentRestPeriod.Value--;
                else
                {
                    card.SetCardParent(player.controlledHand.Value.transform, false);
                    cardsToRemove.Add(card);
                    card.CurrentRestPeriod.Value = card.RestPeriod.Value;
                }
            }

            foreach (AdventurerCard card in cardsToRemove)
            {
                player.DiscardPile.Remove(card);
            }
        }
        //GameManager.Instance.EndPhase();
    }
}
