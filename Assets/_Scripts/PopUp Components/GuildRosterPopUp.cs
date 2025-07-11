using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GuildRosterPopUp : NetworkBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button goBackButton;
    [SerializeField] private GameObject activeRosterGroup;
    [SerializeField] private GameObject restingRosterGroup;

    [SerializeField] private GameObject cooldownDisplayPrefab;
    [SerializeField] private Toggle showActiveAdventurersToggle;

    void Start()
    {
        closeButton.onClick.AddListener(() =>
        {
            ServerClosePopUp();
        });

        goBackButton.onClick.AddListener(() =>
        {
            ServerClosePopUp(true, LocalConnection);
        });

        if (showActiveAdventurersToggle == null) return;

        showActiveAdventurersToggle.onValueChanged.AddListener((value) =>
        {
            activeRosterGroup.SetActive(value);
            PlayerPrefs.SetInt("ShowActiveAdventurers", value ? 1 : 0);
        });

        showActiveAdventurersToggle.isOn = PlayerPrefs.GetInt("ShowActiveAdventurers", 0) == 1;
    }

    [TargetRpc]
    public void TargetInitializeGuildRoster(NetworkConnection connection, Player player, bool isViewingRival)
    {
        if (isViewingRival) ServerPopulateRivalGuildRoster(connection, player);
        else ServerPopulateGuildRoster(connection, player);

        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        RectTransform rt = this.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
    }

    [ServerRpc(RequireOwnership = false)]
    protected void ServerPopulateGuildRoster(NetworkConnection connection, Player player)
    {
        bool hasAdventurerCard = false;
        foreach (Transform handCard in player.ControlledHand.Value.transform)
        {
            if (handCard.GetComponent<AdventurerCard>() != null)
            {
                hasAdventurerCard = true;
                AddCardToRoster(connection, handCard.gameObject, "Active");
            }
        }

        //if (!hasAdventurerCard) TargetDisableRoster(connection, "Active");
        //if (player.DiscardPile.Count == 0)
        //{
        //    TargetDisableRoster(connection, "Resting");
        //    return;
        //}

        // Sort the player's discard pile by current cooldown
        player.DiscardPile.Sort((x, y) => x.CurrentRestPeriod.Value.CompareTo(y.CurrentRestPeriod.Value));
        foreach (AdventurerCard restingCard in player.DiscardPile)
        {
            AddCardToRoster(connection, restingCard.gameObject, "Resting");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected void ServerPopulateRivalGuildRoster(NetworkConnection connection, Player player)
    {
        List<AdventurerCard> rivalRoster = new List<AdventurerCard>();

        foreach (Transform handCard in player.ControlledHand.Value.transform)
        {
            if (handCard.GetComponent<AdventurerCard>() != null)
            {
                rivalRoster.Add(handCard.GetComponent<AdventurerCard>());
            }
        }
        foreach (AdventurerCard restingCard in player.DiscardPile)
        {
            rivalRoster.Add(restingCard);
        }
        // sort the list of rival cards alphabetically by card name
        rivalRoster.Sort((x, y) => x.CardName.Value.CompareTo(y.CardName.Value));

        foreach (AdventurerCard rivalCard in rivalRoster)
        {
            AddCardToRoster(connection, rivalCard.gameObject, "Active");
        }
    }

    [Server]
    private void AddCardToRoster(NetworkConnection connection, GameObject rosterCardObject, string rosterGroup)
    {
        GameObject newCardObject = Instantiate(rosterCardObject, Vector2.zero, Quaternion.identity);
        Spawn(newCardObject);

        Card newCard = newCardObject.GetComponent<Card>();
        newCard.CopyCardData(connection, newCardObject, rosterCardObject);

        int currentCooldown = rosterCardObject.GetComponent<AdventurerCard>().CurrentRestPeriod.Value + 1;
        TargetSetCardParent(connection, newCardObject, rosterGroup, currentCooldown);

        if (rosterGroup == "Active")
        {
            newCardObject.transform.SetParent(activeRosterGroup.transform, false);
        }
        else
        {
            newCardObject.transform.SetParent(restingRosterGroup.transform, false);
        }
    }


    [TargetRpc]
    private void TargetSetCardParent(NetworkConnection connection, GameObject card, string rosterGroup, int currentCooldown)
    {
        card.transform.localScale = new Vector3(1.25f, 1.25f, 1f);

        if (rosterGroup == "Resting")
        {
            card.transform.SetParent(restingRosterGroup.transform, false);
            GameObject cooldownDisplay = Instantiate(cooldownDisplayPrefab, Vector2.zero, Quaternion.identity);
            cooldownDisplay.transform.SetParent(card.transform, false);
            cooldownDisplay.transform.localPosition = new Vector3(0, -100, 0);

            TMP_Text displayText = cooldownDisplay.GetComponent<CooldownDisplay>().cooldownText;
            displayText.text = $"{currentCooldown} Round";
            if (currentCooldown > 1) displayText.text += "s";
        }
        else
        {
            card.transform.SetParent(activeRosterGroup.transform, false);
        }
    }

    [TargetRpc]
    private void TargetDisableRoster(NetworkConnection connection, string rosterGroup)
    {
        if (rosterGroup == "Active")
        {
            activeRosterGroup.SetActive(false);
        }
        else
        {
            restingRosterGroup.SetActive(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerClosePopUp(bool launchScoreboard = false, NetworkConnection connection = null)
    {
        if (launchScoreboard) PopUpManager.Instance.CreateScoreBoardPopUp(connection);
        Despawn(gameObject);
    }
}
