using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    //public Card container;
    //public int x;
    //public List<Card> deck = new List<Card>();
    public static List<Card> staticDeck = new List<Card>();
    public int cardFrequency = 2;
    float deckMaxSize;

    public List<GameObject> deckTracker;

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
        deckMaxSize = staticDeck.Count;
        //deck = staticDeck;

        //StartCoroutine(StartGame());

    }

    // Update is called once per frame
    void Update()
    {
        //staticDeck = deck;
        //print(staticDeck.Count);
        if (staticDeck.Count < deckMaxSize*.75)
        {
            deckTracker[0].SetActive(false);
        }
        if (staticDeck.Count < deckMaxSize*.5)
        {
            deckTracker[1].SetActive(false);
        }
        if (staticDeck.Count < deckMaxSize*.25)
        {
            deckTracker[2].SetActive(false);
        }
        if (staticDeck.Count == 0)
        {
            deckTracker[3].SetActive(false);
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

    //public void Shuffle()
    //{
    //    for (int i =0; i<deckSize; i++)
    //    {
    //        container = deck[i];
    //        int randomIndex = Random.Range(i, deckSize);
    //        deck[i] = deck[randomIndex];
    //        deck[randomIndex] = container;
    //    }
    //}

    IEnumerator Draw(int x)
    {
        for (int i = 0; i < x; i++)
        {
            yield return new WaitForSeconds(.5f);

            Instantiate(CardToHand, transform.position, transform.rotation);
        }
    }
}
