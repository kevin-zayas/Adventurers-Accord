using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using Mirror;

public class GameManager : MonoBehaviour
{
    public List<Card> tierOneDeck;
    public List<Card> tierTwoDeck;
    public Transform[] cardSlots;
    public bool[] availableCardSlots;
    public GameObject adventurerCard;
    public ResourceManager player;

    public UnityEvent goldChange;
    public UnityEvent repChange;
    public UnityEvent deckSizeChange;
    public UnityEvent questCardChange;

    public PlayerManager playerManager;

    // Start is called before the first frame update
    void Start()
    {
        //tierOneDeck = PlayerDeck.tierOneDeck;
        //tierTwoDeck = PlayerDeck.tierTwoDeck;

    }

    public void StartGame()
    {
        //for (int i = 0; i < availableCardSlots.Length; i++)
        //{
        //    DrawCard(i);
        //}

        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        print(networkIdentity);
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.CmdStart();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DrawCard(int slotIndex)
    {
        //List<Card> deck;
        //if (slotIndex > 3) { deck = tierTwoDeck; }
        //else { deck = tierOneDeck; }

        //if (deck.Count >= 1)
        //{
        //    if (availableCardSlots[slotIndex] == true)
        //    {
        //        Card randomCard = deck[Random.Range(0, deck.Count)];

        //        GameObject card = Instantiate(adventurerCard, Vector2.zero, Quaternion.identity);
        //        card.GetComponent<CardDisplay>().LoadCardData(randomCard);
        //        card.GetComponent<CardDisplay>().slotIndex = slotIndex;
        //        card.transform.SetParent(cardSlots[slotIndex].transform, false);
        //        card.tag = "DraftCard";

        //        availableCardSlots[slotIndex] = false;
        //        deck.Remove(randomCard);
        //        deckSizeChange.Invoke();
        //    }
           
        //}
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
        goldChange.Invoke();
    }

    public void ChangeReputation(int repDelta)
    {
        player.currentRep += repDelta;
        repChange.Invoke();
    }
}
