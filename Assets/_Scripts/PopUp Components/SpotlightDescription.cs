using FishNet.Connection;
using FishNet.Object;
using TMPro;
using UnityEngine;

public class SpotlightDescription : NetworkBehaviour

{
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] TMP_Text titleText;
    //private RectTransform descriptionTransform;

    //private void Start()
    //{
    //    descriptionTransform = GetComponent<RectTransform>();
    //}

    [TargetRpc]
    public void TargetSetDescriptionText(NetworkConnection connection, string text)
    {
        descriptionText.text = text;
    }

    [TargetRpc]
    public void TargetSetTitleText(NetworkConnection connection, string text)
    {
        titleText.text = text;
    }

    [TargetRpc]
    public void TargetSetParent(NetworkConnection connection, GameObject parent)
    {
        GetComponent<RectTransform>().SetParent(parent.transform, true);
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -140);
    }
}
