using Fusion;
using UnityEngine;

public class FakePlayer : ICardReceiver
{
    private string _id;
    public string ID { get => _id; }
    private NetworkBool _isPlayerOut;
    public NetworkBool IsPlayerOut { get => _isPlayerOut; }

    private byte _maxCards;
    public byte MaxCards { get => _maxCards; }
    private byte _cardsCounter;
    public byte CardsCounter { get => _cardsCounter; }
    private CardInfo[] _playerHand;
    public CardInfo[] PlayerHand { get => _playerHand; }
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
    public void SetCardCounter(byte CardsCounter)
    {
        if (CardsCounter > 0 && CardsCounter <=_maxCards)
            _cardsCounter = CardsCounter;
    }
    public void SetMaxCards(byte maxCards)
    {
        if (maxCards > 0)
            _maxCards = maxCards;
    }
    public void SetUpPlayerHand()
    {
        if (CardsCounter > 0)
            _playerHand = new CardInfo[CardsCounter];
    }
    public bool AddCard(CardInfo card)
    {
        if (!card.IsValid)
        {
#if Log
            Debug.LogError($"Card is not Valid ! cant Add to Player id = {this.ID}");
#endif
            return false;
        }

        if (!_playerHand.AddCard(card))
        {
#if Log
            Debug.LogError($"No available spot in the array. player:{this} Card to Add{card}");
#endif
        }
#if Log
            Debug.Log($"card Is Added !. player:{this} Card to Add{card}");
#endif
        return true;
    }
    public void RemoveCard(CardInfo card)
    {
        if (!card.IsValid)
        {
#if Log
            Debug.LogError($"Card is not Valid ! cant remove cardfrom Player id = {this.ID}");
#endif
            return;
        }

        if (_playerHand.RemoveCard(card))
        {
#if Log
            Debug.Log($"Card is Removed Succesfully !{this} removed card {card}");
#endif
        }
        else
        {
#if Log
            Debug.LogError($"Card is not Removed!{this}  card {card}");
#endif
        }
    }
    public override string ToString()
    {
        return $"fake player ID: {this.ID}";
    }
}
