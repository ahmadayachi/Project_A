//#define USINGSPRITE
using UnityEngine;

public class CardUIControler
{
    Card _card;
    CardUI _cardUI;
    const string X = "X";
    public CardUIControler(Card card, CardUI cardUI)
    {
        _card = card;
        _cardUI = cardUI;
    }

    public void SetCardRankSprite()
    {
        Material Suite = AssetLoader.DeckContainerInstance.GetSuitMaterial(_card.Rank, _card.Suit);
        if (Suite == null)
        {
#if Log
            LogManager.LogError("Could not Fetch Suit from Deck Container!");
#endif
            return;
        }
        _cardUI.CardRank.enabled = true;
        _cardUI.CardRank.material = Suite;
    }
    public void ResetCardRankSprite()
    {
        _cardUI.CardRank.material = null;
        _cardUI.CardRank.enabled = false;
    }
    public void SetCardCounter(int counter)
    {
        _cardUI.CardCounterText.gameObject.SetActive(true);
        _cardUI.CardCounterText.text = X + counter.ToString();
    }
    public void ResetCardCounter()
    {
        _cardUI.CardCounterText.text = string.Empty;
        _cardUI.CardCounterText.gameObject.SetActive(false);
    }

#if USINGSPRITE
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
#endif
}
