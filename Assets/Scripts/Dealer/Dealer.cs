
using System;
using UnityEngine;

public class Dealer : State
{
    private const int _minHalfDeckSize = 10;
    private bool _stopDealing;

      
    #region State    
    public override void Start<T>(T arg)
    {
        if (Extention.TryCastToStruct(arg, out DealerArguments DealerStructarg))
        {
            _stopDealing = false;
            //do the do here ? 
            DealCards(DealerStructarg);
        }
        else
        {
#if Log
            Debug.LogError("wrong argument passed !");
#endif
        }
    }
    public override void ForceEnd()
    {
        _stopDealing = true;

#if Log
        Debug.Log("Dealing is forced to Stop!");
#endif
    }
    #endregion


    private void DealCards(DealerArguments arguments)
    {
        if (_stopDealing) return;
        //creating a new instance to freely manage our deck  
        int deckLength = arguments.DeckToDeal.Length;
        CardInfo[] deck = new CardInfo[deckLength];

        // filling our new instanse 
        Array.Copy(arguments.DeckToDeal, deck, deckLength);

        if (_stopDealing) return;

        //starting by shuffling the deck as a whole 
        deck.Shuffle();

        if (_stopDealing) return;

        //proceeding to RiffleShuffle the deck 
        RiffleShuffle(deck);

        if (_stopDealing) return;

        //dealing cards 
        int playerCount = arguments.Players.Length;
        ICardReceiver player;
        int arrayIndex = 0;
        for (int index = 0; index < playerCount; index++)
        {
            if (_stopDealing) return;

            player = arguments.Players[index];
            //jumping players that cant reciese cards 
            if (player.IsPlayerOut)
                continue;
            for (byte jindex = 0; jindex < player.MaxCards; jindex++)
            {
                player.AddCard(deck[arrayIndex]);
                arrayIndex++;
            }
        }
        
        if (_stopDealing) return;

        //invoking callback for (some UI shet)
        arguments.OnDealerStateEnds?.Invoke();
    }
    private void RiffleShuffle(CardInfo[] deck)
    {
        // creating a random cut which represent the point to slipt the deck 
        int deckLength = deck.Length;

        //creating a random seed 
        System.Random rng = new System.Random();
        int cut = rng.Next(_minHalfDeckSize, (deckLength - _minHalfDeckSize) + 1);

        //filling the two splits from the deck
        CardInfo[] topHalf = new CardInfo[cut];
        int bottomHalflength = deckLength - cut;
        CardInfo[] BottomHalf = new CardInfo[bottomHalflength];
        int bottomHalfIndex = 0;
        for (int index = 0; index < deckLength; index++)
        {
            if (_stopDealing) return;

            if (index < cut)
                topHalf[index] = deck[index];
            else
            {
                if (index > cut)
                    bottomHalfIndex++;
                BottomHalf[bottomHalfIndex] = deck[index];
            }
        }

        //clearing the deck just max simulation of this shuffle 
        Array.Clear(deck, 0, deckLength);

        //refilling the deck from the two splits 
        int firstIndex = 0;
        int secondIndex = firstIndex++;
        int topHalfIndex = 0;
        bottomHalfIndex = 0;
        while (secondIndex < deckLength)
        {
            if (_stopDealing) return;
            // if can add from top half 
            if (topHalfIndex < cut)
            {
                deck[firstIndex] = topHalf[topHalfIndex++];
                firstIndex += 2;
            }
            //if can can add from bottom half 
            if (bottomHalfIndex < bottomHalflength && secondIndex < deckLength)
            {
                deck[secondIndex] = BottomHalf[bottomHalfIndex++];
                secondIndex += 2;
            }
        }
    }
   
}
