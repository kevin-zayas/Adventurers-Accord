using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    public GameObject SpellDropZone { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int PhysicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int MagicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int SpellPhysicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int SpellMagicalPower { get; private set; }

    [field: SerializeField]
    [field: SyncVar]
    public int EffectiveTotalPower { get; private set; }

    private readonly Dictionary<string, int> adventurerEffects = new();

    [field: SyncVar]
    public bool ClericProtection { get; private set; }

    //[field: SerializeField]
    //[field: SyncVar]
    //public QuestCard QuestCard { get; private set; }

    [SerializeField] private TMP_Text physicalPowerText;
    [SerializeField] private TMP_Text magicalPowerText;

    private void Start()
    {
        adventurerEffects.Add("Bard", 0);
        adventurerEffects.Add("Cleric", 0);
        adventurerEffects.Add("Rogue", 0);

        //adventurerEffects.Add("Assassin", 0);
        //adventurerEffects.Add("Enchanter", 0);
        //adventurerEffects.Add("Tinkerer", 0);
    }

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

        if (questLocation.QuestCard.MagicalPower > 0) EffectiveTotalPower += MagicalPower + SpellPhysicalPower;
        if (questLocation.QuestCard.PhysicalPower > 0) EffectiveTotalPower += PhysicalPower + SpellMagicalPower;

        ObserversUpdatePower(PhysicalPower + SpellPhysicalPower, MagicalPower + SpellMagicalPower);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUpdateSpellEffects()
    {
        SpellPhysicalPower = 0;
        SpellMagicalPower = 0;
        EffectiveTotalPower = 0;

        for (int i = 0; i < SpellDropZone.transform.childCount; i++)
        {
            Transform spellCardTransform = SpellDropZone.transform.GetChild(i);
            SpellCard spellCard = spellCardTransform.GetComponent<SpellCard>();

            SpellPhysicalPower += spellCard.PhysicalPower;
            SpellMagicalPower += spellCard.MagicalPower;
        }

        if (questLocation.QuestCard.MagicalPower > 0) EffectiveTotalPower += MagicalPower + SpellPhysicalPower;
        if (questLocation.QuestCard.PhysicalPower > 0) EffectiveTotalPower += PhysicalPower + SpellMagicalPower;

        ObserversUpdatePower(PhysicalPower + SpellPhysicalPower, MagicalPower + SpellMagicalPower);
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
        SpellPhysicalPower = 0;
        SpellMagicalPower = 0;
        EffectiveTotalPower = 0;

        while (DropZone.transform.childCount > 0)
        {
            Transform cardTransform = DropZone.transform.GetChild(0);
            Card card = cardTransform.GetComponent<Card>();

            //card.ResetPower();
            card.SetCardParent(card.ControllingPlayerHand.transform, false);
        }
        ClearSpellEffects();
        //ClearAdventurerEffects();
        ObserversUpdatePower(PhysicalPower, MagicalPower);
    }

    [Server]
    private void ClearSpellEffects()
    {
        for (int i = 0; i < SpellDropZone.transform.childCount; i++)
        {
            Transform spellCardTransform = SpellDropZone.transform.GetChild(i);
            SpellCard spellCard = spellCardTransform.GetComponent<SpellCard>();

            spellCard.Despawn();
        }
    }

    [Server]
    public void AddAdventurerToQuestLane(Card card)
    {
        if (adventurerEffects.ContainsKey(card.Name)) adventurerEffects[card.Name]++;

        switch (card.Name)
        {
            case "Bard":
                //bardInspiration = true;
                break;
            case "Cleric":
                ClericProtection = true;
                if (adventurerEffects["Cleric"] == 1) UpdateDrainEffects();
                break;
            case "Rogue":
                //stickyFingers = true;
                break;

        }

        if (questLocation.QuestCard.Drain && !ClericProtection)
        {
            card.ServerChangePhysicalPower(-questLocation.QuestCard.PhysicalDrain);
            card.ServerChangeMagicalPower(-questLocation.QuestCard.MagicalDrain);
        }

        ServerUpdatePower();
    }

    [Server]
    public void RemoveAdventurerFromQuestLane(Card card)
    {
        if (adventurerEffects.ContainsKey(card.Name)) adventurerEffects[card.Name]--;

        switch (card.Name)
        {
            case "Bard":
                //bardInspiration = false;
                break;
            case "Cleric":
                if (adventurerEffects["Cleric"] == 0)
                {
                    ClericProtection = false;
                    UpdateDrainEffects();
                }
                break;
            case "Rogue":
                //stickyFingers = false;
                break;

        }

        ServerUpdatePower();
    }

    [Server]
    private void UpdateDrainEffects()
    {
        if (!questLocation.QuestCard.Drain) return;

        foreach (Transform cardTransform in DropZone.transform)
        {
            Card card = cardTransform.GetComponent<Card>();
            if (card.Name == "Cleric") continue;

            if (ClericProtection)
            {
                card.ServerChangePhysicalPower(questLocation.QuestCard.PhysicalDrain);      //reverse drain
                card.ServerChangeMagicalPower(questLocation.QuestCard.MagicalDrain);    
            }
            else
            {
                print("Applying Drain");
                card.ServerChangePhysicalPower(-questLocation.QuestCard.PhysicalDrain);
                card.ServerChangeMagicalPower(-questLocation.QuestCard.MagicalDrain);
            }
        }
    }

    //[Server]
    //public void AssignQuestCard(QuestCard questCard)
    //{
    //    QuestCard = questCard;
    //}

}
