using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<Card> deck;
    public Transform[] cardSlots;
    public bool[] availableCardSlots;
    public GameObject AdventurerCard;



    // Start is called before the first frame update
    void Start()
    {
        deck = PlayerDeck.staticDeck;
        DrawCard();



    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawCard()
    {
        print(deck.Count);
        if (deck.Count >= 1)
        {
            Card randomCard = deck[Random.Range(0, deck.Count)];
            print(randomCard);

            for (int i = 0; i < availableCardSlots.Length; i++)
            {
                if (availableCardSlots[i] == true)
                {
                    //populate card from deck here. May want to use instantiate instead of setActive to avoid a large number of cards in the scene.
                    //randomCard.SetActive();
                    //randomCard.tranform.position = cardSlots[i].position;
                    GameObject card = Instantiate(AdventurerCard, cardSlots[i].transform.position, Quaternion.identity);
                    card.transform.SetParent(cardSlots[i].transform); //experiment with true false
                    card.GetComponent<DisplayCard>().LoadCardData(randomCard);

                    availableCardSlots[i] = false;
                    deck.Remove(randomCard);

                    return;
                }
            }
        }
    }
}
