using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AdventurerRegistryPopUp : NetworkBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject[] registryGroups;
    [SerializeField] Card cardPrefab;

    void Start()
    {
        closeButton.onClick.AddListener(() =>
        {
            ServerClosePopUp();
        });
    }

    [TargetRpc]
    public void TargetInitializeAdventurerRegistry(NetworkConnection connection)
    {
        PopulateRegistry(connection);
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        RectTransform rt = this.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
    }

    protected void PopulateRegistry(NetworkConnection connection)
    {
        foreach (CardData cardData in CardDatabase.Instance.tierOneAdventurers)
        {
            ServerAddCardToGroup(connection, cardData, 0);
        }

        foreach (CardData cardData in CardDatabase.Instance.tierTwoAdventurers)
        {
            ServerAddCardToGroup(connection, cardData, 1);

        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerAddCardToGroup(NetworkConnection connection, CardData cardData, int groupIndex)
    {
        Card card = Instantiate(cardPrefab, Vector2.zero, Quaternion.identity);
        Spawn(card.gameObject);
        card.LoadCardData(cardData);

        TargetSetCardParent(connection, card.gameObject, groupIndex);
        card.transform.SetParent(registryGroups[groupIndex].transform, false);
    }

    [TargetRpc]
    private void TargetSetCardParent(NetworkConnection connection, GameObject card, int groupIndex)
    {
        card.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
        card.transform.SetParent(registryGroups[groupIndex].transform, false);

        //remove the Begin Drag BaseEventData
        EventTrigger eventTrigger = card.GetComponent<EventTrigger>();
        if (eventTrigger != null)
        {
            foreach (EventTrigger.Entry entry in eventTrigger.triggers)
            {
                if (entry.eventID == EventTriggerType.BeginDrag)
                {
                    eventTrigger.triggers.Remove(entry);
                    break;
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerClosePopUp()
    {
        Despawn(gameObject);
    }
}
