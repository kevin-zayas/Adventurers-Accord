using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PlayerDeck : MonoBehaviour
{
    public static List<Card> staticDeck = new List<Card>();
    public List<GameObject> deckTracker;

    public TMP_Text deckSizeText;
    public int cardFrequency = 2;
    private float deckMaxSize;

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
        OnDeckSizeChange();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnDeckSizeChange()
    {
        if (staticDeck.Count < deckMaxSize * .75)
        {
            deckTracker[0].SetActive(false);
        }
        if (staticDeck.Count < deckMaxSize * .5)
        {
            deckTracker[1].SetActive(false);
        }
        if (staticDeck.Count < deckMaxSize * .25)
        {
            deckTracker[2].SetActive(false);
        }
        if (staticDeck.Count == 0)
        {
            deckTracker[3].SetActive(false);
        }

        deckSizeText.text = PlayerDeck.staticDeck.Count.ToString();
    }

}
