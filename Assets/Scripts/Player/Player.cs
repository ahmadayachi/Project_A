using UnityEngine;
using Fusion;
using System;

public class Player : NetworkBehaviour, IPlayer
{
    #region Player Networked Properties
    [Networked] private string _name { get; set; }
    [Networked] private string _id { get; set; }
    [Networked] private byte _cardCounter { get; set; }
    [Networked] private NetworkBool _isPlayerOut { get; set; }

    [Networked, Capacity(15)]
    private NetworkArray<CardInfo> _playerHand { get; }
    #endregion

    #region Player Properties 
    #endregion
    private IPlayerBehaviour _playerState;
    public IPlayerBehaviour PlayerState { get => _playerState; }

    public string Name { get => _name; }

    public string ID { get => _id; }

    public CardInfo[] PlayerHand
    {
        get
        {
            CardInfo[] result = _playerHand.ToCardInfo();
#if Log
            LogManager.Log($"{this} playerHand is Empty!",Color.yellow,LogManager.PlayerLog);
#endif
            return result;
        }
    }

    public NetworkBool IsPlayerOut { get => _isPlayerOut;}

    public byte CardsCounter { get => _cardCounter; }

    private NetworkRunner _runner;
    public override void Spawned()
    {
        _runner = Runner;
        if(_name!=string.Empty)
            gameObject.name = _name+":"+_id;
        SetUpPlayerBehaviour();
    }
    private void SetUpPlayerBehaviour()
    {
        if (_runner.GameMode == GameMode.Single)
        {
            _playerState = new OfflinePlayerBehaviour();
        }
        else
        {
            _playerState = new OnlinePlayerBehaviour();
        }
    }

    public void SetPlayerName(string playerName)
    {
        if(playerName == string.Empty)
        {
#if Log
            LogManager.LogError("Player Name Cant be Empty !");
#endif
            return;
        }
        _name = playerName;
    }

    public void SetPlayerID(string playerID)
    {
        if (playerID == string.Empty)
        {
#if Log
            LogManager.LogError("Player id Cant be Empty !");
#endif
            return;
        }
        _id = playerID;
    }

    public void SetCardCounter(byte cardCounter)
    {
        if(cardCounter == 0)
        {
#if Log
            LogManager.LogError("Player Card Counter Cant be 0!");
#endif
            return;
        }
        _cardCounter = cardCounter;
    }

    public void ClearHand()
    {
        _playerHand.Clear();
    }

    public bool AddCard(CardInfo card)
    {

        if (IsPlayerOut)
        {
#if Log
            LogManager.Log($"{this} is out cant add card!",Color.blue,LogManager.PlayerLog);
#endif
            return false;
        }

        if(CardsCounter == _playerHand.ValidCardsCount())
        {
#if Log
            LogManager.LogError("Add Card Failed!, Player should be Out!");
#endif
            return false;
        }

        if (!_playerHand.AddCard(card))
        {
#if Log
            LogManager.LogError($"Adding {card} to player={this} Failed!");
#endif
            return false;
        }
        return true;
    }
}
