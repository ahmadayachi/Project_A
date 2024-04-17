using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakePlayer : ICardReceiver
{
    private string _id;
    public string ID { get => _id; }
    private NetworkBool _isPlayerOut;
    public NetworkBool IsPlayerOut { get => _isPlayerOut; }

    private byte _maxCards;
    public byte MaxCards { get => _maxCards; }
    private CardInfo[] _playerHand;
    public CardInfo[] PlayerHand { get; }
    public void SetID(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
#if Log
            Debug.LogError("Cant Set Invalid ID to Fake Player !");
#endif
            return;
        }
        _id = id;
    }
    public void AddCard(CardInfo card)
    {
        if (card.IsValid)
        {
#if Log
            Debug.LogError($"Card is not Valid ! cant Add to Player id = {this.ID}");
#endif
        }
        _playerHand.AddCard(card);
    }
    public void SetUpPlayerHand()
    {
        if (MaxCards > 0)
            _playerHand = new CardInfo[MaxCards];
    }
}
