using UnityEngine;
using Fusion;
using System;
using System.Collections;

public class Player : NetworkBehaviour, IPlayer, IAfterSpawned
{
    #region Player fields

    private NetworkRunner _runner;
    private ChangeDetector _changeDetector;
    private State _playerState;
    private PlayerUIController _playerUIControler;

    [SerializeField]
    private PlayerUI _playerUI;

    private GameManager _gameManager;

    #endregion Player fields

    #region Player Networked Properties

    [Networked] public PlayerRef playerRef { get; set; }
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
    public NetworkObject NetworkObject { get => Object; }
    public Transform Transform { get => gameObject.transform; }

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
    public int HandCount { get => _hand.ValidCardsCount(); }
    public bool IsHandFull { get => (HandCount == CardsToDealCounter); }

    #endregion Player Properties

    private CallBackManager _callBackManager;
    private Coroutine _waitSimulationInit;
    private Coroutine _simulationBondingRoutine;
    private Coroutine _firstTickSyncRoutine;

    public override void Spawned()
    {
        _runner = Runner;
        _callBackManager = new CallBackManager();
        SetUpPlayerUIControler();
        SetUpPlayerState();

        Runner.SetIsSimulated(Object, true);
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        if (_name != string.Empty)
            gameObject.name = _name + ":" + _id;
        //SetUpPlayerBehaviour();
        if (_waitSimulationInit != null)
            StopCoroutine(_waitSimulationInit);
        _waitSimulationInit = StartCoroutine(WaitSimulation());
    }

    public void AfterSpawned()
    {
        if(_firstTickSyncRoutine != null)
            StopCoroutine(_firstTickSyncRoutine);
        _firstTickSyncRoutine = StartCoroutine(WaitSimulationAndSyncFirstTick());
    }

    public override void FixedUpdateNetwork()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(_name): _callBackManager.EnqueueOrExecute(_playerUIControler.SetPlayerName); break;
                case nameof(_iconID): _callBackManager.EnqueueOrExecute(_playerUIControler.SetPlayerIcon); break;
                case nameof(_hand): _callBackManager.EnqueueOrExecute(_playerUIControler.LoadPlayerCards); break;
            }
        }
    }

    #region Player Set Up Methods

    private void SetUpPlayerState()
    {
        _playerState = new PlayerState(_playerUIControler);
    }

    private IEnumerator WaitSimulation()
    {
        yield return new WaitUntil(PlayerReadyForCallBacks);
        _callBackManager.SetReady(true);
    }

    private IEnumerator WaitSimulationAndSyncFirstTick()
    {
        yield return new WaitUntil(PlayerReadyForCallBacks);
        _playerUIControler.SetPlayerName();
        _playerUIControler.SetPlayerIcon();
        _playerUIControler.LoadPlayerCards();
    }

    private bool PlayerReadyForCallBacks()
    {
        if (_playerUIControler == null) return false;
        if (_playerState == null) return false;
        if (_gameManager == null) return false;
        if (_gameManager.CardPool == null) return false;
        if (_gameManager.SimulationState != SimulationSetUpState.SetUpComplete) return false;
        return true;
    }

    public void InitPlayer(PlayerArguments playerArgs)
    {
        SetPlayerRef(playerArgs.PlayerRef);
        SetPlayerName(playerArgs.Name);
        SetPlayerID(playerArgs.ID);
        //SetCardCounter(playerArgs.CardCounter);
        SetPlayerIcon(playerArgs.IconID);
        SetIsplayerOut(playerArgs.isplayerOut);
        //BondPlayerSimulation(playerArgs.GameManager);
        PlusOneCard();
    }

    public void SetPlayerRef(PlayerRef playerRef)
    {
        if (playerRef == null || playerRef == PlayerRef.None)
        {
#if Log
            LogManager.LogError($"Invalid Player Player Ref =>{playerRef} player =>{this}");
#endif
            return;
        }
        this.playerRef = playerRef;
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

    public void BondPlayerSimulation(GameManager gameManager)
    {
        if (gameManager == null)
        {
#if Log
            LogManager.LogError("Player GameManager is Null!");
#endif
            return;
        }
        _gameManager = gameManager;
        if (_simulationBondingRoutine != null)
            StopCoroutine(_simulationBondingRoutine);
        _simulationBondingRoutine = StartCoroutine(BondPlayerSimulationRoutine());
    }

    private IEnumerator BondPlayerSimulationRoutine()
    {
#if Log
        LogManager.Log($"{this} is waiting for Simulation Set Up Complete", Color.yellow, LogManager.ValueInformationLog);
#endif
        yield return new WaitUntil(() => _gameManager.CardPool != null);
        _playerUIControler.SetUpCardPositionerCardPool(_gameManager.CardPool);

#if Log
        LogManager.Log($"{this} simulation Bonding is Complete", Color.green, LogManager.ValueInformationLog);
#endif
    }

    public void SetIsplayerOut(NetworkBool isPlayerOut)
    {
        if (isPlayerOut)
        {
#if Log
            LogManager.Log($"{this} is Out !", Color.yellow, LogManager.ValueInformationLog);
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
        if (_playerUIControler == null)
            _playerUIControler = new PlayerUIController(_playerUI, this);
    }

    #endregion Player Set Up Methods

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

        if (IsHandFull)
        {
#if Log
            LogManager.LogError("Add Card Failed!, Player should Hand Is Full!");
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

    #endregion method wrappers
}