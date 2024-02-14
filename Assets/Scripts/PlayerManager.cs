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
    public bool[] availableCardSlots;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer) { return; }
        uint id = NetworkClient.localPlayer.netId;
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        print($"id:  {id}");
        print($"networkIdentity: {networkIdentity}");

        hand = GameObject.Find("Hand");
        questLocation = GameObject.Find("Quest Location");

        GameObject T1Deck = GameObject.Find("T1 Deck");
        foreach (Transform child in T1Deck.transform.GetChild(0))
        {
            cardSlots.Add(child);
        }

        tierOneDeck = PlayerDeck.tierOneDeck;
        tierTwoDeck = PlayerDeck.tierTwoDeck;
    }

    [Server]
    public override void OnStartServer()
    {
        base.OnStartServer();

        tierOneDeck = PlayerDeck.tierOneDeck;
        tierTwoDeck = PlayerDeck.tierTwoDeck;

        availableCardSlots = new bool[4] { true, true, true, true };

        GameObject T1Deck = GameObject.Find("T1 Deck");
        foreach (Transform child in T1Deck.transform.GetChild(0))
        {
            cardSlots.Add(child);
        }


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
        for (int i = 0; i < 4; i++)
        {
            CmdDrawCard(i);
        }
    }

    //[Command]
    [Command(requiresAuthority =false)]
    public void CmdDrawCard(int slotIndex)
    {
        print("draw card");
        print($"networkIdentity: {NetworkClient.connection.identity}");
        List<Card> deck;
        if (slotIndex > 3) { deck = tierTwoDeck; }
        else { deck = tierOneDeck; }
        print("deck");
        if (deck.Count >= 1)
        {
            print("available slots");
            if (availableCardSlots[slotIndex] == true)
            {
                print("getting random card");
                Card randomCard = deck[Random.Range(0, deck.Count)];
                print("got random card");

                GameObject card = Instantiate(adventurerCard, Vector2.zero, Quaternion.identity);
                card.GetComponent<CardDisplay>().LoadCardData(randomCard);
                card.GetComponent<CardDisplay>().slotIndex = slotIndex;

                NetworkServer.Spawn(card, connectionToClient);

                //card.transform.SetParent(cardSlots[slotIndex].transform, false);
                card.tag = "DraftCard";
                RpcShowCard(card, slotIndex);
                

                availableCardSlots[slotIndex] = false;
                //deck.Remove(randomCard);
                //deckSizeChange.Invoke();
            }

        }
    }

    [ClientRpc]
    void RpcShowCard(GameObject card, int slotIndex)
    {
        print("show card");
        print($"networkIdentity: {NetworkClient.connection.identity}");
        print($"card tag: {card.tag}");
        if (isOwned)
        {
            print("is owned");
            //card.transform.SetParent(hand.transform, false);
            card.transform.SetParent(cardSlots[slotIndex].transform, false);
            //deck.Remove(randomCard);
        }
        else
        {
            print("not owned");
            //card.transform.SetParent(hand.transform, false);
            card.transform.SetParent(cardSlots[slotIndex].transform, false);
            
        }
    }

}
