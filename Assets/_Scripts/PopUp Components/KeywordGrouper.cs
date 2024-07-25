using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeywordGrouper : NetworkBehaviour
{
    [SerializeField] SpotlightDescription keywordDescriptionPrefab;
    [SerializeField] GameObject layoutGroupObject;
    //private RectTransform grouperTransform;

    //private void Start()
    //{
    //    grouperTransform = GetComponent<RectTransform>();
    //}

    [Server]
    public void AddKeywordDescription(NetworkConnection connection, string keyword)
    {
        SpotlightDescription spotlightDescription = Instantiate(keywordDescriptionPrefab);
        Spawn(spotlightDescription.gameObject);
        spotlightDescription.TargetSetParent(connection, layoutGroupObject);
        spotlightDescription.TargetSetTitleText(connection, keyword);
        spotlightDescription.TargetSetDescriptionText(connection, CardDatabase.Instance.GetKeywordDefinition(keyword));

    }

    [TargetRpc]
    public void TargetSetParent(NetworkConnection connection, GameObject parent)
    {
        GetComponent<RectTransform>().SetParent(parent.transform, true);
    }

    [TargetRpc]
    public void TargetResizeKeywordGrouper(NetworkConnection connection) 
    {
        int keywordGrouperheight = 60 + 200 * transform.childCount;
        RectTransform rectTransform = layoutGroupObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, keywordGrouperheight);
    }

}
