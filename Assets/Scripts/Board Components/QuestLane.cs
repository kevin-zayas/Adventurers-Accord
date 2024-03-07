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
    public Player Player { get; private set; }

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
    public int EffectiveTotalPower { get; private set; }

    //[field: SerializeField]
    //[field: SyncVar]
    //public QuestCard QuestCard { get; private set; }

    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;

    [ServerRpc(RequireOwnership = false)]
    public void ServerUpdatePower()
    {
        PhysicalPower = 0;
        MagicalPower = 0;
        EffectiveTotalPower = 0;

        for (int i = 0; i < DropZone.transform.childCount; i++)
        {
            Transform cardTransform = DropZone.transform.GetChild(i);
            Card card = cardTransform.GetComponent<Card>();

            PhysicalPower += card.PhysicalPower + card.ItemPhysicalPower;
            MagicalPower += card.MagicalPower + card.ItemMagicalPower;
        }

        if (questLocation.QuestCard.MagicalPower > 0) EffectiveTotalPower += MagicalPower;
        if (questLocation.QuestCard.PhysicalPower > 0) EffectiveTotalPower += PhysicalPower;

        ObserversUpdatePower(PhysicalPower, MagicalPower);

    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversUpdatePower(int physicalPower, int magicalPower)
    {
        physicalPowerText.text = physicalPower.ToString();
        magicalPowerText.text = magicalPower.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetQuestLanePlayer(Player player)
    {
        Player = player;
    }

    [Server]
    public void ResetQuestLane()
    {
        PhysicalPower = 0;
        MagicalPower = 0;
        EffectiveTotalPower = 0;

        for (int i = 0; i < DropZone.transform.childCount; i++)
        {
            Transform cardTransform = DropZone.transform.GetChild(i);
            Card card = cardTransform.GetComponent<Card>();

            card.SetCardScale(new Vector3(2f, 2f, 1f));
            card.SetCardParent(card.ControllingPlayerHand.transform, false);
            
        }
        ObserversUpdatePower(PhysicalPower, MagicalPower);
    }

    //[Server]
    //public void AssignQuestCard(QuestCard questCard)
    //{
    //    QuestCard = questCard;
    //}

}
