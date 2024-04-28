using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPositioner : MonoBehaviour
{
    private List<ICard> _loadedCards = new List<ICard>();
    private CardPool _cardPool;
    public void LoadCards(CardInfo[] cards)
    {
        for(int index = 0; index < cards.Length; index++)
        {
            if (!cards[index].IsValid || IsCardLoaded(cards[index])) continue;
           ICard loadedCard = _cardPool.CreateACard(cards[index]);
            if(loadedCard==null)
            {
#if Log
                LogManager.LogError($"Pooling Card Failed ! cardinfo ={cards[index]}");
#endif
                return;
            }
            _loadedCards.Add(loadedCard);
        }
        //position loaded cards here 
    }

    public void Init(CardPool cardPool) =>_cardPool = cardPool;
    private bool IsCardLoaded(CardInfo card)
    {
        foreach(ICard icard in _loadedCards)
        {
            if(Extention.AreSameCard(icard.ToCardInfo(),card))
                return true;
        }
        return false;
    }
}
