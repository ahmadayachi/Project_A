using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    #region dealer

    private Dealer _dealer;

    #endregion dealer

    #region betHandler

    private BetHandler _betHandler;

    #endregion betHandler

    #region Card Pool System

    private CardPool _cardsPool;
    public CardPool CardPool;

    #endregion Card Pool System
    #region Doubt
    private Doubt _doubt;
    #endregion

    #region Deck properties
    private const int DoubleStandartDeckSize = 104;
    private byte _maxPlayerCards;
    /// <summary>
    /// The max amount of cards that can be dealt to a player, a player should be out if he carry more than this amount
    /// </summary>
    public byte MaxPlayerCards { get => _maxPlayerCards; }

    #endregion Deck properties

    #region Player Propertys

    private const byte MaxPlayersNumber = 8;

    /// <summary>
    /// Array of  Active Players
    /// </summary>
    [Networked, Capacity(MaxPlayersNumber)]
    private NetworkArray<NetworkObject> _activeplayers { get; }

    [Networked, Capacity(MaxPlayersNumber - 1)]
    private NetworkArray<string> _loosersIDs { get; }

    [Networked] private string _winnerID { get; set; }
    [Networked] private string _currentPlayerID { get; set; }
    private byte _activePlayersNumber;
    public byte ActivePlayersNumber { get => _activePlayersNumber; }

    #endregion Player Propertys

    #region Cards Networked Properties

    /// <summary>
    /// Array of the total cards that are dealt to players per Round
    /// </summary>
    [Networked, Capacity(DoubleStandartDeckSize)]
    private NetworkArray<byte> _dealtCards { get; }
    private List<byte> _dealtCardsList = new List<byte>();
    #endregion Cards Networked Properties

    #region GameState properties

    [Networked] private GameState _gameState { get; set; }
    private byte _dealtCardsNumber;
    public byte DealtCardsNumber { get => _dealtCardsNumber; }

    #endregion GameState properties

    #region Runner

    private NetworkRunner _runner;

    #endregion Runner

    #region Live Bet Props

    /// <summary>
    /// Array of card ranks that the previous player bet on
    /// </summary>
    [Networked, Capacity(DoubleStandartDeckSize)]
    private NetworkArray<byte> _liveBet { get; }

    private List<DiffusedRankInfo> _diffusedBet = new List<DiffusedRankInfo>();

    /// <summary>
    /// Id of the Player who set the Live Bet
    /// </summary>
    [Networked] private string _liveBetPlayerID { get; set; }

    #endregion Live Bet Props
    #region State Props
    private State _currentState;
    #endregion




    #region Dealer Setup

    private void CreateDealer() => _dealer = new Dealer(StartRoutine, StopRoutine);

    #endregion Dealer Setup
    #region Doubt Setup
    private void CreateDoubt() => _doubt = new Doubt(DoubtOverLogic, StartRoutine, StopRoutine);
    #endregion

    #region BetHandler Setup

    private void CreateBetHandler() => _betHandler = new BetHandler();

    #endregion BetHandler Setup

    #region Cards Pool Setup

    private void CreateCardPool()
    {
        var cardPrefab = AssetLoader.PrefabContainer.CardPrefab;
        if (cardPrefab == null)
        {
#if Log
            LogManager.LogError(" Failed Fetching CardPrefab from Prefab Container !");
#endif
            return;
        }
        if (_activePlayersNumber == 0 || _maxPlayerCards == 0)
        {
#if Log
            LogManager.LogError($" Failed Creating CardPool! active player number {_activePlayersNumber} max player cards {_maxPlayerCards}");
#endif
            return;
        }
        CardPoolArguments poolArgs = new CardPoolArguments();
        poolArgs.CardPrefab = cardPrefab;
        poolArgs.MaxPlayerCards = _maxPlayerCards;
        poolArgs.ActivePlayerCount = _maxPlayerCards;
        _cardsPool = new CardPool(poolArgs);
    }

    #endregion Cards Pool Setup

    #region methods to link with UI

    public void SetNumberOfPlayers(int numberOfPlayers)
    {
        if (numberOfPlayers <= 8 && numberOfPlayers > 0)
        {
            _activePlayersNumber = (byte)numberOfPlayers;
#if Log
            LogManager.Log($"Player number is Set !, Player Number = {_activePlayersNumber}", Color.green, LogManager.ValueInformationLog);
#endif
        }
    }

    #endregion methods to link with UI

    #region private Logic methods

    private void SetMaxPlayerCards()
    {
        if (_activePlayersNumber == 0)
        {
#if Log
            LogManager.LogError("player number need to be > 0 before setting the max player cards ");
#endif
            return;
        }
        byte playerCards = 1;
        int currentDeckSize = CardManager.Deck.Length;
        while ((currentDeckSize - (playerCards * ActivePlayersNumber) > 0))
        {
            playerCards++;
        }
        _maxPlayerCards = (byte)(playerCards - 1);
    }

    #endregion private Logic methods

    #region Mono Method Wrappers

    /// <summary>
    /// wrapper method for runnner.spawn(objectToSPawn)
    /// </summary>
    /// <param name="objectRef"></param>
    /// <returns></returns>
    private NetworkObject SpawnObject(NetworkPrefabRef objectRef)
    {
        if (_runner == null)
        {
#if Log
            LogManager.LogError("spawning object failed! runner is null!");
#endif
            return null;
        }
        if (objectRef == null)
        {
#if Log
            LogManager.LogError("spawning object failed! object is null!");
#endif
            return null;
        }

        return _runner.Spawn(objectRef);
    }

    /// <summary>
    /// wrapper for a start coroutine
    /// </summary>
    /// <param name="routine"></param>
    /// <returns></returns>
    private Coroutine StartRoutine(IEnumerator routine)
    {
        return StartCoroutine(routine);
    }

    /// <summary>
    /// wrapper for StopCoroutine
    /// </summary>
    /// <param name="routineCash"></param>
    private void StopRoutine(Coroutine routineCash)
    {
        StopCoroutine(routineCash);
    }

    #endregion Method Wrappers
    #region Doubt Over Logic
    private IEnumerator DoubtOverLogic (DoubtState doubtState)
    {
        yield return null;
    } 
    #endregion
    #region Player Commands Methods
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ConfirmBet(byte[] bet, string playerID)
    {
        //blocking invalid args
        if (string.IsNullOrEmpty(playerID) || bet == null)
        {
#if Log
            LogManager.LogError($"Blocking Confirm Rpc ! Invalid Args Found! CurrentPlayerID is :=>{_currentPlayerID}");
#endif
            return;
        }
        //only current player can confirm bet
        if (_currentPlayerID != playerID)
        {
#if Log
            LogManager.Log($"Blocking Confirm Rpc ! player with ID:= {playerID} is not the Current PLayer!,Current PLayer ID:={_currentPlayerID}", Color.red, LogManager.GameModeLogs);
#endif
            return;
        }
        //the previous bet should always be sorted
        byte[] liveBet = _liveBet.ToByteArray();
        ValidatorArguments betArgs = new ValidatorArguments(bet, liveBet, _dealtCardsNumber);
        bool isValid = _betHandler.ChainValidateBet(betArgs);
        //bet has to be valid
        if (!isValid)
        {
#if Log
            LogManager.Log($"Blocking Confirm Rpc ! player with ID:= {playerID} Sent an Invalid Bet!, Bet=:{string.Join(",", liveBet)}", Color.red, LogManager.GameModeLogs);
#endif
            return;
        }
        //making sure the array is sorted before confirming
        Extention.BetDiffuser(bet, _diffusedBet);
        byte[] sotedBet = _diffusedBet.ToByteArray();
        int sortedBetLength = sotedBet.Length;
        //cleaning Network Array
        for (int index = 0; index < _liveBet.Length; index++)
        {
            _liveBet.Set(index, 0);
        }
        //adding Bet
        for (int index = 0; (index < sortedBetLength); index++)
        {
            _liveBet.Set(index, sotedBet[index]);
        }
        //setting live bet player id 
        _liveBetPlayerID = playerID;

    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Doubt(string playerID)
    {
        //blocking invalid args
        if (string.IsNullOrEmpty(playerID))
        {
#if Log
            LogManager.LogError($"Blocking Confirm Rpc ! Invalid Args Found! CurrentPlayerID is :=>{_currentPlayerID}");
#endif
            return;
        }
        //only current player can confirm bet
        if (_currentPlayerID != playerID)
        {
#if Log
            LogManager.Log($"Blocking Confirm Rpc ! player with ID:= {playerID} is not the Current Player!,Current Player ID:={_currentPlayerID}", Color.red, LogManager.GameModeLogs);
#endif
            return;
        }

        if (_doubt == null)
        {
#if Log
            LogManager.LogError($"Blocking Confirm Rpc ! Doubt State is not Initialized! CurrentPlayerID is :=>{_currentPlayerID}");
#endif
            return;
        }

        byte[] liveBet = _liveBet.ToByteArray();
        _dealtCards.ToByteList(_dealtCardsList);
        //invoking Doubt State
        DoubtStateArguments stateArguments = new DoubtStateArguments(_dealtCardsList, liveBet);
        ChangeState(_doubt, stateArguments);
    }
    #endregion GameMode Methods
    #region state contol methods  
    public void ChangeState<T>(State newState,T newStateArgs) where T:struct
    {
        // froce end current state
        _currentState?.ForceEnd();

        // starting the new state
        newState.Start(newStateArgs);
        _currentState = newState;
    }
    #endregion
}