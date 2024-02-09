using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public bool isYourTurn;
    public int turnTracker;
    private int playerCount = 2;
    public TMP_Text turnText;

    public static bool startTurn;

    // Will move this to a rewards manager script eventually
    public int currentGold;
    public TMP_Text goldText;
    


    // Start is called before the first frame update
    void Start()
    {
        isYourTurn = true;
        turnTracker = 1;
        currentGold = 10;
    }

    // Update is called once per frame
    void Update()
    {
        if (turnTracker == 1)
        {
            isYourTurn = true;
            turnText.text = "Your Turn";

        }
        else
        {
            isYourTurn = false;
            turnText.text = "Opponent Turn";
        }

        //goldText.text = currentGold + " GP";
    }

    public void EndTurn()
    {
        turnTracker += 1;
        turnTracker %= playerCount;
        if (turnTracker == 1)
        {
            startTurn = true;
        }
    }
}
