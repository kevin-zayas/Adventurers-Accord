using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpotlightDescription : NetworkBehaviour

{
    [SerializeField] private TMP_Text descriptionText;

    [TargetRpc]
    public void TargetSetDescriptionText(NetworkConnection connection, string description)
    {

    }
}
