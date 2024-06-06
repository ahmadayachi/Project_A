using UnityEngine;
using Fusion;
using System;
using System.Collections;

public class Player : NetworkBehaviour, IPlayer
{
    #region Player fields

    private NetworkRunner _runner;
    private ChangeDetector _changeDetector;
    private State _playerState;
    private IPlayerUIControler _playerUIControler;
    [SerializeField] 
    private PlayerUI _playerUI;
    private GameManager _gameManager;
    #endregion Player fields

    #region Player Networked Properties

    [Networked] private string _name { get; set; }
    [Networked] private string _id { get; set; }
    /// <summary>
    /// how many cards should the player Get 
    /// </summary>
    [Networked] private byte _cardToDealCounter { get; set; }
    [Networked] private NetworkBool _isOut { get; set; }
    [Networked] private byte _iconID { get; set; }
    /// <summary>
    /// an array of player Card ID's 
    /// </summary>
    [Networked, Capacity(15)]
    private NetworkArray<byte> _hand { get; }

    #endregion Player Networked Properties

    #region Player Properties

    public State PlayerState { get => _playerState; }
    public IPlayerUIControler PlayerUIControler { get => _playerUIControler; }
    public string Name { get => _name; }
    public string ID { get => _id; }
    public byte IconID { get => _iconID; }
    public bool IsLocalPlayer { get => Object.HasInputAuthority; }
    public CardInfo[] Hand
    {
        get
        {
            CardInfo[] result = _hand.ToCardInfo();
            if (result == null)
            {
#if Log
                LogManager.Log($"{this} playerHand is Empty!", Color.yellow, LogManager.PlayerLog);
#endif
            }
            return result;
        }
    }

    public NetworkBool IsOut { get => _isOut; }
    public byte CardsToDealCounter { get => _cardToDealCounter; }
    public int HandCount { get => _hand.ValidCardsCount();}
    public bool IsHandFull { get =>(HandCount==CardsToDealCounter); }
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
                case nameof(_hand): _playerUIControler.LoadPlayerCards(); break;
            }
        }
    }
    #region Player Set Up Methods    
    public void InitPlayer(PlayerArguments playerArgs)
    {
        SetPlayerName(playerArgs.Name);
        SetPlayerID(playerArgs.ID);
        //SetCardCounter(playerArgs.CardCounter);
        SetPlayerIcon(playerArgs.IconID);
        SetIsplayerOut(playerArgs.isplayerOut);
        SetPlayerGameManager(playerArgs.GameManager);
        PlusOneCard();
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
        _cardToDealCounter = cardCounter;
    }
    /// <summary>
    /// adds one to the totall Cards Counter 
    /// </summary>
    public void PlusOneCard()
    {
        _cardToDealCounter++;
    }
    public void ClearCardsCounter()
    {
        _cardToDealCounter = 0;
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
        _isOut = isPlayerOut;
    }
    private void SetUpPlayerBehaviour()
    {
        //if (_runner.GameMode == GameMode.Single)
        //{
        //    _playerState = new OfflinePlayerBehaviour();
        //}
        //else
        //{
        //    _playerState = new OnlinePlayerBehaviour();
        //}
    }
    private void SetUpPlayerUIControler()
    {
        if (_playerUIControler != null)
            _playerUIControler = new PlayerUIController(_playerUI, this,_gameManager.CardPool);
    }
    #endregion

    public void ClearHand()
    {
        _hand.Clear();
    }

    public bool AddCard(CardInfo card)
    {
        if (IsOut)
        {
#if Log
            LogManager.Log($"{this} is out cant add card!", Color.blue, LogManager.PlayerLog);
#endif
            return false;
        }

        if (CardsToDealCounter == HandCount)
        {
#if Log
            LogManager.LogError("Add Card Failed!, Player should be Out!");
#endif
            return false;
        }

        if (!_hand.AddCardID(card))
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
        return $"Name:{_name}/ ID:{_id}/ CardCounter{_cardToDealCounter}/ IsOut{_isOut}";
    }
    #region method wrappers
    public Coroutine Startroutine(IEnumerator coroutin)
    {
        return StartCoroutine(coroutin);
    }
    public void StopRoutine(Coroutine coroutin)
    {
        StopCoroutine(coroutin);
    }
    #endregion
}