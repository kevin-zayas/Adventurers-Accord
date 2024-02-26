using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OldQuestManager : MonoBehaviour
{
    public TMP_Text physicalPowerText;
    public TMP_Text magicalPowerText;
    public int physicalPowerTotal;
    public int magicalPowerTotal;

    private OldGameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<OldGameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnQuestCardChange()
    {
        GameObject card;
        int cardCount = gameObject.transform.childCount;

        physicalPowerTotal = 0;
        magicalPowerTotal = 0;

        for (int i = 0; i < cardCount; i++)
        {
            card = gameObject.transform.GetChild(i).gameObject;
            physicalPowerTotal += card.GetComponent<CardDisplay>().physPower;
            magicalPowerTotal += card.GetComponent<CardDisplay>().magPower;
        }

        physicalPowerText.text = physicalPowerTotal.ToString();
        magicalPowerText.text = magicalPowerTotal.ToString();
    }
}
