
using System;
using System.Collections;
using UnityEngine;

public class Dealer : State
{
    private const int _minHalfDeckSize = 10;
    public Coroutine DealingRoutine;
    private Func<IEnumerator, Coroutine> _startRoutine;
    private Action<Coroutine> _stopRoutine;
    
    public Dealer (Func<IEnumerator, Coroutine> startRoutine,Action<Coroutine> stoproutine)
    {
        _startRoutine = startRoutine;
        _stopRoutine = stoproutine;
    }

    #region State    
    public override void Start<T>(T arg)
    {
        if (Extention.TryCastToStruct(arg, out DealerStateArguments DealerStructarg))
        {
            //do the do here ?
            if (DealingRoutine != null)
                _stopRoutine?.Invoke(DealingRoutine);

            DealingRoutine =  _startRoutine?.Invoke(DealCards(DealerStructarg));
        }
        else
        {
#if Log
            LogManager.LogError("  wrong Dealer argument passed !");
#endif
        }
    }
    public override void ForceEnd()
    {
        if (DealingRoutine != null)
        {
            _stopRoutine?.Invoke(DealingRoutine);
            DealingRoutine = null;
        }

#if Log
        LogManager.Log("Dealing is forced to Stop!", Color.yellow, LogManager.DealerLog);
#endif
    }
    #endregion


    private IEnumerator DealCards(DealerStateArguments arguments)
    {
        //creating a new instance to freely manage our deck  
        int deckLength = arguments.DeckToDeal.Length;
        CardInfo[] deck = new CardInfo[deckLength];

        // filling our new instanse 
        Array.Copy(arguments.DeckToDeal, deck, deckLength);

        //starting by shuffling the deck as a whole 
        deck.Shuffle();
        yield return null;
        //proceeding to RiffleShuffle the deck 
         RiffleShuffle(deck);

        yield return null;

        //dealing cards 
        int playerCount = arguments.Players.Length;
        ICardReceiver player;
        int arrayIndex = 0;
        for (int index = 0; index < playerCount; index++)
        {
            player = arguments.Players[index];
            //jumping players that cant reciese cards 
            if (player.IsOut)
                continue;
            for (byte jindex = 0; jindex < player.CardsToDealCounter; jindex++)
            {
                //at this point a player should get his card
                if (player.AddCard(deck[arrayIndex]))
                    arrayIndex++;
                //if for some reason adding card fails Stop Dealing 
                //else
                //{
                //    ForceEnd();
                //    yield break;
                //} 
            }
        }

        //invoking callback for (some UI shet)
        arguments.OnDealerStateEnds?.Invoke();
        //reseting coroutine 
        DealingRoutine = null;
    }
    //revert to private 
    public void RiffleShuffle(CardInfo[] deck)
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
        int topHalfIndex = 0;
        bottomHalfIndex = 0;
        for (int deckIndex = 0; deckIndex < deckLength; deckIndex++)
        {
            // if can add from top half 
            if (topHalfIndex < cut && deckIndex < deckLength)
            {
                deck[deckIndex] = topHalf[topHalfIndex++];
            }
            //if can can add from bottom half 
            if (bottomHalfIndex < bottomHalflength && deckIndex < deckLength)
            {
                deck[++deckIndex] = BottomHalf[bottomHalfIndex++];
            }
        }
    }
   
}
