using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PlayerDeck : MonoBehaviour
{
    public static List<OldCard> tierOneDeck = new List<OldCard>();
    public List<GameObject> tierOneDeckTracker;

    public static List<OldCard> tierTwoDeck = new List<OldCard>();
    public List<GameObject> tierTwoDeckTracker;

    public TMP_Text deckOneSizeText;
    public TMP_Text deckTwoSizeText;
    public int cardFrequency = 2;


    // Start is called before the first frame update
    void Start()
    {
        CreateDeck(tierOneDeck, OldCardDatabase.tierOneCardList);
        CreateDeck(tierTwoDeck, OldCardDatabase.tierTwoCardList);

        OnDeckSizeChange();

    }

    // Update is called once per frame
    void Update()
    {

    }

    void CreateDeck(List<OldCard> deck,List<OldCard> cardList)
    {
        for (int id = 0; id < cardList.Count; id++)
        {
            for (int x = 0; x < cardFrequency; x++)
            {
                deck.Add(cardList[id]);
            }
        }
    }

    public void OnDeckSizeChange()
    {
        deckOneSizeText.text = PlayerDeck.tierOneDeck.Count.ToString();
        deckTwoSizeText.text = PlayerDeck.tierTwoDeck.Count.ToString();
    }

}
