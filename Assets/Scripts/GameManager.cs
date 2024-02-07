using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public List<Card> deck;
    public Transform[] cardSlots;
    public bool[] availableCardSlots;
    public GameObject AdventurerCard;
    public TMP_Text deckSizeText;
    int deckSize;



    // Start is called before the first frame update
    void Start()
    {
        deck = PlayerDeck.staticDeck;
        //DrawCard();



    }

    // Update is called once per frame
    void Update()
    {
        //change to listener or event system to not update deck count every frame
        deckSize = PlayerDeck.staticDeck.Count;
        deckSizeText.text = deckSize.ToString();

    }

    public void DrawCard()
    {
        //print(deck.Count);
        if (deck.Count >= 1)
        {
            for (int i = 0; i < availableCardSlots.Length; i++)
            {
                if (availableCardSlots[i] == true)
                {
                    //if i is > 3, draw from tier 2 deck
                    Card randomCard = deck[Random.Range(0, deck.Count)];

                    GameObject card = Instantiate(AdventurerCard, cardSlots[i].transform.position, Quaternion.identity);
                    card.transform.SetParent(cardSlots[i].transform); //experiment with true false
                    card.GetComponent<DisplayCard>().LoadCardData(randomCard);
                    card.GetComponent<DisplayCard>().slotIndex = i;

                    availableCardSlots[i] = false;
                    deck.Remove(randomCard);

                    //return;
                }
            }
        }
    }

    public void CheckAvailableSlots()
    {
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i].transform.childCount == 1)
            {
                availableCardSlots[i] = true;
            }
        }
    }
}
