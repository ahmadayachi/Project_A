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
    private CardSuite _suite { get; set; }
    public CardSuite Suite { get => _suite; }
    private ICardUI _cardUI { get; set;}
    public ICardUI CardUI { get => _cardUI; }
    #endregion
    public void SetRank(byte rank)
    {
        _rank = rank;
    }
    public void SetID(byte id)
    {
        _id = id;
    }
    public void SetSuite(CardSuite suite)
    {
        _suite = suite;
    }

    public void Enable()
    {
        throw new System.NotImplementedException();
    }

    public void Disable()
    {
        throw new System.NotImplementedException();
    }
}
