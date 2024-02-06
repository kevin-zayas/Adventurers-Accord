using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    public Card container;
    public int x;
    public static int deckSize;
    public List<Card> deck = new List<Card>();
    public static List<Card> staticDeck = new List<Card>();
    public int cardFrequency = 2;

    public GameObject cardInDeck1;
    public GameObject cardInDeck2;
    public GameObject cardInDeck3;
    public GameObject cardInDeck4;

    public GameObject CardToHand;
    public GameObject[] Clones;
    public GameObject Hand;


    // Start is called before the first frame update
    void Start()
    {
        for (int id = 0; id < CardDatabase.cardDatabase.Count; id++)
        {
            for (int x = 0; x < cardFrequency; x++)
            {
                staticDeck.Add(CardDatabase.cardDatabase[id]);
            }
        }
        deck = staticDeck;
        deckSize = staticDeck.Count;

        //StartCoroutine(StartGame());

    }

    // Update is called once per frame
    void Update()
    {
        //staticDeck = deck;
        if (deckSize < 15)
        {
            cardInDeck1.SetActive(false);
        }
        if (deckSize < 10)
        {
            cardInDeck2.SetActive(false);
        }
        if (deckSize < 5)
        {
            cardInDeck3.SetActive(false);
        }
        if (deckSize < 1)
        {
            cardInDeck4.SetActive(false);
        }

        if (TurnManager.startTurn)
        {
            //StartCoroutine(Draw(1));
            TurnManager.startTurn = false;
        }
    }

    //IEnumerator StartGame()
    //{
        //for (int i = 0; i < 2; i++)
        //{
        //    yield return new WaitForSeconds(1);

        //    Instantiate(CardToHand, transform.position, transform.rotation);
        //}
    //}

    public void Shuffle()
    {
        for (int i =0; i<deckSize; i++)
        {
            container = deck[i];
            int randomIndex = Random.Range(i, deckSize);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = container;
        }
    }

    IEnumerator Draw(int x)
    {
        for (int i = 0; i < x; i++)
        {
            yield return new WaitForSeconds(.5f);

            Instantiate(CardToHand, transform.position, transform.rotation);
        }
    }
}
