using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipItemPopUp : MonoBehaviour
{
    [SerializeField] Button cancelButton;
    [SerializeField] Button confirmButton;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] string titleTextString;

    private AdventurerCard adventurerCard;
    public GameObject itemCardObject;

    void Start()
    {
        cancelButton.onClick.AddListener(() =>
        {
            ItemCard itemCard = itemCardObject.GetComponent<ItemCard>();
            itemCard.ServerSetCardParent(itemCard.ControllingPlayerHand.transform, true);
            Destroy(gameObject);
        });

        confirmButton.onClick.AddListener(() =>
        {
            adventurerCard.ServerEquipItem(true, itemCardObject.GetComponent<ItemCard>().Data);
            itemCardObject.GetComponent<ItemCard>().ServerDespawnItem();
            Destroy(gameObject);
        });
    }

    public void InitializeEquipItemPopUp(AdventurerCard card, GameObject item)
    {
        adventurerCard = card;
        itemCardObject = item;

        titleText.text = string.Format(titleTextString, item.GetComponent<ItemCard>().CardName, card.CardName);

        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        RectTransform rt = this.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
    }
}
