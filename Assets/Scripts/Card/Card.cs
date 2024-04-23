using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour, ICard
{
    #region Card Properties
    private byte _rank { get; set; }
    public byte Rank { get => _rank; }
    private byte _id { get; set; }
    public byte ID { get => _id; }
    private byte _suit { get; set; }
    public byte Suit { get => _suit; }
    private ICardUI _cardUI { get; set;}
    public ICardUI CardUI { get => _cardUI; }
    #endregion
    private void Awake()
    {
        SetCardUI();
    }
    private void SetCardUI()
    {
        if (_cardUI != null) return;
        ICardUI cardUI = GetComponent<ICardUI>();
        if (cardUI == null)
        {
#if Log
            LogManager.LogError("There Is No UICard Component attached to Card GameObject!");
#endif
            return;
        }
        _cardUI = CardUI;
    }
    public void SetRank(byte rank)
    {
        _rank = rank;
    }
    public void SetID(byte id)
    {
        _id = id;
    }
    public void SetSuite(byte suite)
    {
        _suit = (byte)suite;
    }

    public void Enable()
    {
        throw new System.NotImplementedException();
    }

    public void Disable()
    {
        throw new System.NotImplementedException();
    }

    public CardInfo ToCardInfo()
    {
        CardInfo info = new CardInfo();
        info.ID = _id;
        info.Rank = _rank;
        info.Suit = _suit;
        info.IsValid = true;
        return info;
    }
}
