using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuildRosterPopUp : NetworkBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject activeRosterGroup;
    [SerializeField] private GameObject restingRosterGroup;
    // Start is called before the first frame update
    void Start()
    {
        closeButton.onClick.AddListener(() =>
        {
            ServerClosePopUp();
        });
    }

    [TargetRpc]
    public void TargetInitializeGuildRoster(NetworkConnection connection)
    {
        ServerPopulateGuildRoster(connection);

        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        RectTransform rt = this.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
    }

    [ServerRpc(RequireOwnership = false)]
    protected void ServerPopulateGuildRoster(NetworkConnection connection)
    {
        Player player = GameManager.Instance.Players[connection.ClientId];

        foreach (Transform handCard in player.controlledHand.Value.transform)
        {
            if (handCard.GetComponent<AdventurerCard>() != null)
            {
                AddCardToRoster(connection, handCard.gameObject, "Active");
            }
        }

        foreach (AdventurerCard restingCard in player.DiscardPile)
        {
            AddCardToRoster(connection, restingCard.gameObject, "Resting");
        }
    }
    [Server]
    private void AddCardToRoster(NetworkConnection connection, GameObject rosterCardObject, string rosterGroup)
    {
        print($"Adding {rosterCardObject.GetComponent<AdventurerCard>().CardName.Value} to {rosterGroup}");
        GameObject newCardObject = Instantiate(rosterCardObject, Vector2.zero, Quaternion.identity);
        Spawn(newCardObject);

        Card newCard = newCardObject.GetComponent<Card>();
        newCard.CopyCardData(connection, newCardObject, rosterCardObject);

        TargetSetCardParent(connection, newCardObject, rosterGroup);
    }


    [TargetRpc]
    private void TargetSetCardParent(NetworkConnection connection, GameObject card, string rosterGroup)
    {
        if (rosterGroup == "Resting")
        {
            card.transform.SetParent(restingRosterGroup.transform, false);
        }
        else
        {
            card.transform.SetParent(activeRosterGroup.transform, false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerClosePopUp()
    {
        Despawn(gameObject);
    }
}
