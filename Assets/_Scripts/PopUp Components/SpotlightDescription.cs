using FishNet.Connection;
using FishNet.Object;
using TMPro;
using UnityEngine;

public class SpotlightDescription : NetworkBehaviour

{
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] TMP_Text titleText;

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

    [ObserversRpc]
    public void ObserversSetParent(NetworkConnection connection, GameObject parent)
    {
        GetComponent<RectTransform>().SetParent(parent.transform, true);
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -140);
    }
}
