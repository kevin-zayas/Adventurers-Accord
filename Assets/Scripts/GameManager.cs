using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<Card> deck = new List<Card>();
    public Transform[] cardSlots;
    public bool[] availableCardSlots;
    public GameObject AdventurerCard;
    public Dictionary<int, Card> dict = new();



    // Start is called before the first frame update
    void Start()
    {
        dict.Add(0, new Card());
        deck = PlayerDeck.staticDeck;
        GameObject card = Instantiate(AdventurerCard, cardSlots[0].transform.position, Quaternion.identity);
        card.transform.SetParent(cardSlots[0].transform); //experiment with true false
        card.GetComponent<DisplayCard>().LoadCardData(CardDatabase.cardDatabase[0]);



    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawCard()
    {
        if (deck.Count >= 1)
        {
            Card randomCard = deck[Random.Range(0, deck.Count)];

            for (int i = 0; i < availableCardSlots.Length; i++)
            {
                if (availableCardSlots[i] == true)
                {
                    //populate card from deck here. May want to use instantiate instead of setActive to avoid a large number of cards in the scene.
                    //randomCard.SetActive();
                    //randomCard.tranform.position = cardSlots[i].position;
                    
                    availableCardSlots[i] = false;
                    deck.Remove(randomCard);

                    return;
                }
            }
        }
    }
}
