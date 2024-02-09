using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public List<Card> deck;
    public Transform[] cardSlots;
    public bool[] availableCardSlots;
    public GameObject AdventurerCard;
    public ResourceManager player;
    private PlayerDeck playerDeck;

    // Start is called before the first frame update
    void Start()
    {
        deck = PlayerDeck.staticDeck;
        playerDeck = gameObject.GetComponent<PlayerDeck>();
        playerDeck.deckSizeChange.Invoke();

    }

    public void StartGame()
    {
        for (int i = 0; i < availableCardSlots.Length; i++)
        {
            DrawCard(i);
        }
    }

    // Update is called once per frame
    void Update()
    {

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
                card.transform.SetParent(cardSlots[slotIndex].transform, false);
                card.GetComponent<DisplayCard>().slotIndex = slotIndex;

                availableCardSlots[slotIndex] = false;
                deck.Remove(randomCard);
                playerDeck.deckSizeChange.Invoke();
            }
           
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

    public void ChangeGold(int goldDelta)
    {
        player.currentGold += goldDelta;
        player.goldChange.Invoke();
    }

    public void ChangeReputation(int repDelta)
    {
        player.currentRep += repDelta;
        player.repChange.Invoke();
    }
}
