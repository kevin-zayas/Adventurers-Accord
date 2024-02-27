using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestLane : NetworkBehaviour
{
    [field: SerializeField] 
    public GameObject DropZone { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int PhysicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int MagicalPower { get; private set; }

    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;
}
