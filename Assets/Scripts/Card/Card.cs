using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour, ICard
{
    #region Card Fields

    [SerializeField] private CardUI _cardUI;
    private byte _rank;
    private byte _id;
    private CardSuit _suit;
    private ICardUIControler _cardUIControler;

    #endregion Card Fields

    #region ICard Properties

    public CardUI CardUI { get => _cardUI; }
    public byte Rank { get => _rank; }
    public byte ID { get => _id; }
    public CardSuit Suit { get => _suit; }
    public ICardUIControler CardUIControl { get => _cardUIControler; }

    #endregion ICard Properties

    private void Awake()
    {
        SetUpCardUIControler();
    }

    public void SetUpCardUIControler()
    {
        if (_cardUIControler != null) return;
        _cardUIControler = new CardUIControler(this, CardUI);
    }

    private bool SetRank(byte rank)
    {
        if (!Extention.IsAValidBeloteRank(rank))
        {
#if Log
            LogManager.LogError($"{rank} this rank is not a valid belote rank !");
#endif
            return false;
        }
        _rank = rank;
        return true;
    }

   

    public bool SetID(byte id)
    {
        if (id == 0)
        {
#if Log
            LogManager.LogError($"Card ID cant be 0!");
#endif
            return false;
        }
        _id = id;
        return true;
    }

    public bool SetSuite(CardSuit suite)
    {
        if (suite == CardSuit.NoSuit)
        {
#if Log
            LogManager.LogError($"{suite} is Invalid!");
#endif
            return false;
        }
        _suit = suite;
        return true;
    }

    public void Enable(CardInfo card)
    {
        if (!card.IsValid)
        {
#if Log
            LogManager.LogError($"Cant Enable an Invalid Card !{card}");
#endif
            return;
        }
        //enabling ui if all card fields are set
        if (SetRank(card.Rank) && SetID(card.ID) && SetSuite(card.Suit))
        {
            _cardUIControler.SetCardRankSprite();
#if Log
            LogManager.Log($"{this} is enabled !", Color.green, LogManager.ValueInformationLog);
#endif
        }
        // resetting if an invalid field dected
        else
        {
#if Log
            LogManager.LogError($"Cant Enable Card, some values and invalid!{card}");
#endif
            Disable();
        }
    }

    public void Disable()
    {
        _rank = 0;
        _id = 0;
        _suit = 0;
        _cardUIControler.ResetCardRankSprite();
    }

    public CardInfo ToCardInfo()
    {
        CardInfo info = new CardInfo();
        //if card is reseted the cardinfo should not be valid
        if (_id == 0) return info;
        info.ID = _id;
        info.Rank = _rank;
        info.Suit = _suit;
        info.IsValid = true;
        return info;
    }

    public override string ToString()
    {
        return $"ID: {ID}, Rank: {Rank}, Suit: {Suit}";
    }
}