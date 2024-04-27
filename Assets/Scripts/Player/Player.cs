using UnityEngine;
using Fusion;
using System;

public class Player : NetworkBehaviour, IPlayer
{
    #region Player fields

    private NetworkRunner _runner;
    private ChangeDetector _changeDetector;
    private IPlayerBehaviour _playerState;
    private IPlayerUIControler _playerUIControler;
    [SerializeField] 
    private PlayerUI _playerUI;
    private GameManager _gameManager;
    #endregion Player fields

    #region Player Networked Properties

    [Networked] private string _name { get; set; }
    [Networked] private string _id { get; set; }
    [Networked] private byte _cardCounter { get; set; }
    [Networked] private NetworkBool _isPlayerOut { get; set; }
    [Networked] private byte _iconID { get; set; }
    /// <summary>
    /// an array of player Card ID's 
    /// </summary>
    [Networked, Capacity(15)]
    private NetworkArray<byte> _playerHand { get; }

    #endregion Player Networked Properties

    #region Player Properties

    public IPlayerBehaviour PlayerState { get => _playerState; }
    public IPlayerUIControler PlayerUIControler { get => _playerUIControler; }
    public string Name { get => _name; }

    public string ID { get => _id; }
    public byte IconID { get => _iconID; }

    public CardInfo[] PlayerHand
    {
        get
        {
            CardInfo[] result = _playerHand.ToCardInfo();
            if (result == null)
            {
#if Log
                LogManager.Log($"{this} playerHand is Empty!", Color.yellow, LogManager.PlayerLog);
#endif
            }
            return result;
        }
    }

    public NetworkBool IsPlayerOut { get => _isPlayerOut; }

    public byte CardsCounter { get => _cardCounter; }

    #endregion Player Properties

    public override void Spawned()
    {
        _runner = Runner;
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        if (_name != string.Empty)
            gameObject.name = _name + ":" + _id;
        SetUpPlayerBehaviour();
        SetUpPlayerUIControler();
    }

    public override void FixedUpdateNetwork()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(_iconID): _playerUIControler.SetPlayerIcon(); break;
                case nameof(_playerHand): _playerUIControler.LoadPlayerCards(); break;
            }
        }
    }
    #region Player Set Up Methods    
    public void InitPlayer(PlayerArguments playerArgs)
    {
        SetPlayerName(playerArgs.Name);
        SetPlayerID(playerArgs.ID);
        SetCardCounter(playerArgs.CardCounter);
        SetPlayerIcon(playerArgs.IconID);
        SetIsplayerOut(playerArgs.isplayerOut);
        SetPlayerGameManager(playerArgs.GameManager);
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
    public void SetPlayerName(string playerName)
    {
        if (playerName == string.Empty)
        {
#if Log
            LogManager.LogError("Player Name Cant be Empty !");
#endif
            return;
        }
        _name = playerName;
    }
    public void SetCardCounter(byte cardCounter)
    {
        if (cardCounter == 0)
        {
#if Log
            LogManager.LogError("Player Card Counter Cant be 0!");
#endif
            return;
        }
        _cardCounter = cardCounter;
    }
    public void SetPlayerIcon(byte IconID)
    {
        _iconID = IconID;
    }
    public void SetPlayerGameManager(GameManager gameManager) 
    {
        if(gameManager == null)
        {
#if Log
            LogManager.LogError("Player GameManager is Null!");
#endif
            return;
        }
        _gameManager = gameManager;
    }
    public void SetIsplayerOut(NetworkBool isPlayerOut)
    {
        if (isPlayerOut)
        {
#if Log
            LogManager.Log($"{this} is Out !",Color.yellow,LogManager.ValueInformationLog);
#endif
        }
        _isPlayerOut = isPlayerOut;
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
    private void SetUpPlayerUIControler()
    {
        if (_playerUIControler != null)
            _playerUIControler = new PlayerUIController(_playerUI, this);
    }
    #endregion

    public void ClearHand()
    {
        _playerHand.Clear();
    }

    public bool AddCard(CardInfo card)
    {
        if (IsPlayerOut)
        {
#if Log
            LogManager.Log($"{this} is out cant add card!", Color.blue, LogManager.PlayerLog);
#endif
            return false;
        }

        if (CardsCounter == _playerHand.ValidCardsCount())
        {
#if Log
            LogManager.LogError("Add Card Failed!, Player should be Out!");
#endif
            return false;
        }

        if (!_playerHand.AddCardID(card))
        {
#if Log
            LogManager.LogError($"Adding {card} to player={this} Failed!");
#endif
            return false;
        }
        return true;
    }
    public override string ToString()
    {
        return $"Name:{_name}/ ID:{_id}";
    }
}