using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundSummaryPopUp : NetworkBehaviour
{
    [SerializeField] Button closeButton;

    [field: SerializeField] public QuestSummary QuestSummary1 { get; private set; }
    [field: SerializeField] public QuestSummary QuestSummary2 { get; private set; }
    [field: SerializeField] public QuestSummary QuestSummary3 { get; private set; }



    // Start is called before the first frame update
    void Start()
    {
        closeButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });
    }

}
