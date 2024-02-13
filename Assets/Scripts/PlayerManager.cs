using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    public GameObject adventurerCard;

    public GameObject hand;
    public GameObject questLocation;

    public List<Card> tierOneDeck;
    public List<Card> tierTwoDeck;
    public List<Transform> cardSlots;
    public List<bool> availableCardSlots;

    public override void OnStartClient()
    {
        base.OnStartClient();

        hand = GameObject.Find("Hand");
        questLocation = GameObject.Find("Quest Location");

        GameObject T1Deck = GameObject.Find("T1 Deck");
        foreach (Transform child in T1Deck.transform.GetChild(0))
        {
            cardSlots.Add(child);
            availableCardSlots.Add(true);
        }

        //tierOneDeck = PlayerDeck.tierOneDeck;
        //tierTwoDeck = PlayerDeck.tierTwoDeck;
    }

    [Server]
    public override void OnStartServer()
    {
        base.OnStartServer();

        tierOneDeck = PlayerDeck.tierOneDeck;
        tierTwoDeck = PlayerDeck.tierTwoDeck;
    }

    [Command]
    public void CmdStart()
    {
        print("start");
        print($"networkIdentity: {NetworkClient.connection.identity}");

        //print($"isClient: {isClient}");
        //print($"isServer: {isServer}");
        //print($"isLocalPlayer: {isLocalPlayer}");
        //print($"isOwned: {isOwned}");
        for (int i = 0; i < 1; i++)
        {
            CmdDrawCard(i);
        }
    }

    [Command]
    public void CmdDrawCard(int slotIndex)
    {
        print("draw card");
        print($"networkIdentity: {NetworkClient.connection.identity}");
        List<Card> deck;
        if (slotIndex > 3) { deck = tierTwoDeck; }
        else { deck = tierOneDeck; }

        if (deck.Count >= 1)
        {
            //if (availableCardSlots[slotIndex] == true)
            {
                Card randomCard = deck[Random.Range(0, deck.Count)];

                GameObject card = Instantiate(adventurerCard, Vector2.zero, Quaternion.identity);
                NetworkServer.Spawn(card, connectionToClient);


                card.GetComponent<CardDisplay>().LoadCardData(randomCard);
                card.GetComponent<CardDisplay>().slotIndex = slotIndex;
                //card.transform.SetParent(cardSlots[slotIndex].transform, false);
                card.tag = "DraftCard";
                RpcShowCard(card, slotIndex);
                

                //availableCardSlots[slotIndex] = false;
                deck.Remove(randomCard);
                //deckSizeChange.Invoke();
            }

        }
    }

    [ClientRpc]
    void RpcShowCard(GameObject card, int slotIndex)
    {
        print("show card");
        print($"networkIdentity: {NetworkClient.connection.identity}");
        if (card.tag == "DraftCard")
        {
            if (isOwned)
            {
                print("is owned");
                card.transform.SetParent(hand.transform, false);
            }
            else
            {
                print("not owned");
            }

            //card.transform.SetParent(hand.transform, false);
        }
    }
}
