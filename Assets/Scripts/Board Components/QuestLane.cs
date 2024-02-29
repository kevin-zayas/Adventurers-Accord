using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestLane : NetworkBehaviour
{
    [SerializeField]
    private QuestLocation questLocation;

    [field: SerializeField]
    [field: SyncVar]
    public GameObject DropZone { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int PhysicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int MagicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public QuestCard QuestCard { get; private set; }

    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;

    [ServerRpc(RequireOwnership = false)]
    public void ServerUpdatePower()
    {
        PhysicalPower = 0;
        MagicalPower = 0;

        for (int i = 0; i < DropZone.transform.childCount; i++)
        {
            Transform cardTransform = DropZone.transform.GetChild(i);
            Card card = cardTransform.GetComponent<Card>();

            PhysicalPower += card.PhysicalPower;
            MagicalPower += card.MagicalPower;
        }

        ObserversUpdatePower(PhysicalPower, MagicalPower);
        questLocation.CalculatePowerTotal();

    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversUpdatePower(int physicalPower, int magicalPower)
    {
        physicalPowerText.text = physicalPower.ToString();
        magicalPowerText.text = magicalPower.ToString();
    }
}
