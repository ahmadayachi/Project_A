
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Dealer : State
{
    /// <summary>
    /// always clear after dealing 
    /// </summary>
    private List<CardInfo> _dealtCards = new List<CardInfo>();
    #region State    
    public override void Start<T>(T arg)
    {
        if (Extention.TryCastToStruct(arg,out DealerArguments DealerStructarg))
        {
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

    }
    #endregion
   
   
    private void DealCards(DealerArguments arguments)
    {
        ICardReceiver player;
        int randomCardIndex;
        //shuffle deck then deal to players better rng that way 
        for (int index = 0; index < arguments.Players.Count(); index++)
        {
            player = arguments.Players[index];

            for (byte jindex = 0; jindex < player.maxCards; jindex++)
            {
                randomCardIndex = Random.Range(0, arguments.DeckToDeal.Count());
                CardInfo randomCard = arguments.DeckToDeal[randomCardIndex];

                while (_dealtCards.Contains(randomCard))
                {
                    randomCardIndex = Random.Range(0, arguments.DeckToDeal.Count());
                    randomCard = arguments.DeckToDeal[randomCardIndex];
                }

                _dealtCards.Add(randomCard);
                player.AddCard(randomCard);
            }
        }

        // cleaning 
        _dealtCards.Clear();
    }
   

}
