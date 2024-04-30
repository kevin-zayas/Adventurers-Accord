using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ResourceManager : MonoBehaviour
{
    public TMP_Text repText;
    public TMP_Text goldText;
    public float currentRep;
    public int currentGold;

    private OldGameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<OldGameManager>();
        currentRep = 5;
        currentGold = 10;
        gm.goldChange.Invoke();
        gm.repChange.Invoke();
;    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnGoldAmountChange()
    {
        goldText.text = currentGold + " GP";
    }

    public void OnRepAmountChange()
    {
        repText.text = currentRep + " rep.";
    }
}
