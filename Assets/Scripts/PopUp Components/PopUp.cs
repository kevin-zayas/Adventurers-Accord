using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUp : MonoBehaviour
{
    [SerializeField] Button leftButton;
    [SerializeField] Button rightButton;
    [SerializeField] TMP_Text leftButtonText;
    [SerializeField] TMP_Text rightButtonText;

    [SerializeField] Button closeButton;
    [SerializeField] TMP_Text title;
    [SerializeField] TMP_Text message;
    [SerializeField] Image noticeImage;

    public void CreateRoguePopUp(Transform questLocation)
    {
        noticeImage.gameObject.SetActive(false);
        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);

        title.text = "Sticky Fingers";
        message.text = "Please choose an Adventurer to 'borrow' an item from.";

        closeButton.onClick.AddListener(() =>
        {
            //create popup to confirm not stealing this turn
            Destroy(gameObject);
        });

        transform.SetParent(questLocation);
    }
}
