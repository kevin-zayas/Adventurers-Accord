using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeywordGrouper : NetworkBehaviour
{
    [SerializeField] SpotlightDescription keywordDescriptionPrefab;
    [SerializeField] GameObject layoutGroupObject;
    private RectTransform grouperTransform;

    private void Start()
    {
        grouperTransform = GetComponent<RectTransform>();
    }

    [Server]
    public void AddKeywordDescription(NetworkConnection connection, string keyword)
    {
        SpotlightDescription spotlightDescription = Instantiate(keywordDescriptionPrefab);
        Spawn(spotlightDescription.gameObject);
        spotlightDescription.TargetSetParent(connection, layoutGroupObject);
        spotlightDescription.TargetSetDescriptionTitle(connection, keyword);

    }

    [TargetRpc]
    public void TargetSetParent(NetworkConnection connection, GameObject parent)
    {
        GetComponent<RectTransform>().SetParent(parent.transform, true);
    }

}
