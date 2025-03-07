using FishNet.Object;
using Unity.VisualScripting;

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
        card.CurrentCoolDown.Value = card.CoolDown.Value;
    }

    [Server]
    public void RecoverAdventurers()
    {
        foreach (Player player in GameManager.Instance.Players)
        {
            foreach (AdventurerCard card in player.DiscardPile)
            {
                if (card.CurrentCoolDown.Value > 0) card.CurrentCoolDown.Value--;
                else
                {
                    card.SetCardParent(player.controlledHand.Value.transform, false);
                }
            }
        }
        //GameManager.Instance.EndPhase();
    }
}
