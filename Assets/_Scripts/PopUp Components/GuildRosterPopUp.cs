using FishNet.Connection;
using FishNet.Object;
using System;
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
    public void TargetInitializeGuildRoster(NetworkConnection connection, Player player, bool isViewingRival, bool enableBackButton)
    {
        if (isViewingRival) ServerPopulateRivalGuildRoster(connection, player);
        else ServerPopulateGuildRoster(connection, player);

        goBackButton.gameObject.SetActive(enableBackButton);

        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        RectTransform rt = this.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
    }

    [ServerRpc(RequireOwnership = false)]
    protected void ServerPopulateGuildRoster(NetworkConnection connection, Player player)
    {
        // Add active cards (from hand)
        foreach (Transform cardSlot in player.ControlledHand.Value.transform)
        {
            if (cardSlot.GetChild(0).TryGetComponent(out AdventurerCard handCard))
            {
                AddCardToRoster(connection, handCard.gameObject, "Active");
            }
        }

        // Add resting cards (from discard pile), sorted by cooldown
        player.DiscardPile.Sort((x, y) => x.CurrentRestPeriod.Value.CompareTo(y.CurrentRestPeriod.Value));
        foreach (var restingCard in player.DiscardPile)
        {
            AddCardToRoster(connection, restingCard.gameObject, "Resting");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected void ServerPopulateRivalGuildRoster(NetworkConnection connection, Player player)
    {
        List<AdventurerCard> rivalCards = new();

        // Active hand cards
        foreach (Transform cardSlot in player.ControlledHand.Value.transform)
        {
            if (cardSlot.GetChild(0).TryGetComponent(out AdventurerCard card))
            {
                rivalCards.Add(card);
            }
        }

        // Resting cards
        rivalCards.AddRange(player.DiscardPile);
        rivalCards.Sort((x, y) => x.CardName.Value.CompareTo(y.CardName.Value));

        // Add all to roster as "Active"
        foreach (var card in rivalCards)
        {
            AddCardToRoster(connection, card.gameObject, "Active");
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

        Transform parent = rosterGroup == "Active" ? activeRosterGroup.transform : restingRosterGroup.transform;
        newCardObject.transform.SetParent(parent, false);
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
