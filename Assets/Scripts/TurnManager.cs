using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public bool isYourTurn;
    public int yourTurn;
    private int playerCount = 2;
    public TMP_Text turnText;

    // Will move this to a rewards manager script eventually
    public int currentGold = 10;
    public TMP_Text goldText;
    


    // Start is called before the first frame update
    void Start()
    {
        isYourTurn = true;
        yourTurn = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (yourTurn == 1)
        {
            isYourTurn = true;
            turnText.text = "Your Turn";
        }
        else
        {
            isYourTurn = false;
            turnText.text = "Opponent Turn";
        }

        goldText.text = currentGold + " GP";
    }

    public void EndTurn()
    {
        yourTurn += 1;
        yourTurn %= playerCount;
        currentGold += 2;
    }
}
