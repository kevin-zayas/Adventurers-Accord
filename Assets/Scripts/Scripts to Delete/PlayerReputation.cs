using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerReputation : MonoBehaviour
{
    public TMP_Text repText;
    public float currentRep;


    // Start is called before the first frame update
    void Start()
    {
        currentRep = 5;
    }

    // Update is called once per frame
    void Update()
    {
        repText.text = currentRep + " rep.";
    }
}