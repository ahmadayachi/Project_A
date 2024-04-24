using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardUIControler : ICardUIControler
{
    Card _card;
    CardUI _cardUI;
    public CardUIControler (Card card,CardUI cardUI)
    {
        _card = card;
        _cardUI = cardUI;
    }
    public void SetCardRankSprite()
    {
        Sprite Suite = AssetLoader.DeckContainerInstance.GetSuitSprite(_card.Rank, _card.Suit);
        if( Suite == null )
        {
#if Log
            LogManager.LogError("Could not Fetch Suit Sprite from Deck Container!");
#endif
            return;
        }
        _cardUI.CardRank.enabled = true;
        _cardUI.CardRank.sprite = Suite;
    }
    public void ResetCardRankSprite()
    {
        _cardUI.CardRank.sprite = null;
        _cardUI.CardRank.enabled = false;
    }
}
