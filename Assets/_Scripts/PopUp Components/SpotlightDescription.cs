using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpotlightDescription : NetworkBehaviour

{
    [SerializeField] private TMP_Text descriptionText;

    public void SetDescriptionText(string description)
    {
        descriptionText.text = description;
    }
}
