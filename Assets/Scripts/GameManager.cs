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
    }

    // Update is called once per frame
    void Update()
    {
        //change to listener or event system to not update deck count every frame
        deckSize = PlayerDeck.staticDeck.Count;
        deckSizeText.text = deckSize.ToString();

    }

    public void DrawCard(int slotIndex)
    {
        //print(deck.Count);
        if (deck.Count >= 1)
        {
            if (availableCardSlots[slotIndex] == true)
            {
                //if slotIndex is > 3, draw from tier 2 deck
                Card randomCard = deck[Random.Range(0, deck.Count)];

                GameObject card = Instantiate(AdventurerCard, Vector2.zero, Quaternion.identity);
                card.GetComponent<DisplayCard>().LoadCardData(randomCard);
                card.transform.SetParent(cardSlots[slotIndex].transform, false); //experiment with true false
                card.GetComponent<DisplayCard>().slotIndex = slotIndex;

                availableCardSlots[slotIndex] = false;
                deck.Remove(randomCard);
            }
           
        }
    }

    public void StartGame()
    {
        for (int i = 0; i < availableCardSlots.Length; i++)
        {
            DrawCard(i);
        }
    }

    public void ReplaceCard(int slotIndex)
    {
        if (slotIndex >= 0)
        {
            availableCardSlots[slotIndex] = true;
            DrawCard(slotIndex);
        }
    }

    //public void CheckAvailableSlots()
    //{
    //    for (int i = 0; i < cardSlots.Length; i++)
    //    {
    //        if (cardSlots[i].transform.childCount == 1)
    //        {
    //            availableCardSlots[i] = true;
    //        }
    //    }
    //}
}
