//#define DECKCHECK

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class Dealer : State
{
    private const int _minHalfDeckSize = 8;
    public Coroutine DealingRoutine;
    private Func<IEnumerator, Coroutine> _startRoutine;
    private Action<Coroutine> _stopRoutine;

    public Dealer(Func<IEnumerator, Coroutine> startRoutine, Action<Coroutine> stoproutine)
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

            DealingRoutine = _startRoutine?.Invoke(DealCards(DealerStructarg));
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

    #endregion State

    private IEnumerator DealCards(DealerStateArguments arguments)
    {
        //creating a new instance to freely manage our deck
        int deckLength = arguments.DeckToDeal.Length;
        CardInfo[] deck = new CardInfo[deckLength];

        // filling our new instanse
        Array.Copy(arguments.DeckToDeal, deck, deckLength);
        yield return null;
        //starting by shuffling the deck as a whole
        deck.Shuffle();
        yield return null;
        //proceeding to RiffleShuffle the deck
        deck.RiffleShuffle();

        yield return null;
#if DECKCHECK
        if (!DeckIsReadyTOServe(deck))
        {
#if Log
            foreach (var item in deck)
            {
                LogManager.Log($"{item}", Color.red);
            }
            LogManager.LogError("Deck is not Ready to Deal to Players !");
#endif
            ForceEnd();

            yield break;
        }
#endif
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
                //if for some reason adding card fails Stop Dealing
                if (!player.AddCard(deck[arrayIndex++]))
                {
                    ForceEnd();
                    yield break;
                }
            }
        }

        yield return new WaitForSeconds(1f);

        //invoking callback for (some UI shet)
        arguments.OnDealerStateEnds?.Invoke();
        //reseting coroutine
        DealingRoutine = null;
    }

    private bool DeckIsReadyTOServe(CardInfo[] deck)
    {
        if (deck.IsEmpty())
            return false;
        for (int index = 0; index < deck.Length; index++)
        {
            if (!deck[index].IsValid || !Extention.IsAValidCardRank(deck[index].Rank))
                return false;
        }
        return true;
    }
}