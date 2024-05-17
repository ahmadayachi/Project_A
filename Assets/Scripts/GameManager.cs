using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager :NetworkBehaviour
{
    #region dealer
    private Dealer _dealer;
    #endregion
    #region Card Pool System
    private CardPool _cardsPool;
    public CardPool CardPool;
    #endregion
    #region Deck properties
    private const int BeloteDeckSize = 32;
    private byte _maxPlayerCards;
   /// <summary>
   /// The max amount of cards that can be dealt to a player, a player should be out if he carry more than this amount 
   /// </summary>
    public byte MaxPlayerCards { get => _maxPlayerCards;}
    #endregion
    #region Player Propertys 
    private const byte MaxPlayersNumber = 8;
    /// <summary>
    /// Array of  Active Players 
    /// </summary>
    [Networked, Capacity(MaxPlayersNumber)] 
    private NetworkArray<NetworkObject> _activeplayers { get;}
    [Networked, Capacity(MaxPlayersNumber)]
    private NetworkArray<string> _loosersIDs { get;}
    private byte _activePlayersNumber;
    public byte ActivePlayersNumber { get => _activePlayersNumber;}
    #endregion
    #region Cards Networked Properties 
    /// <summary>
    /// Array of the total cards that are dealt to players per Round 
    /// </summary>
    [Networked, Capacity(BeloteDeckSize)]
    private NetworkArray<byte> _dealtCards { get;}
    /// <summary>
    /// Array of card ranks that the previous player bet on
    /// </summary>
    [Networked, Capacity(BeloteDeckSize)]
    private NetworkArray<byte> _liveBet { get; }

    #endregion
    #region GameState properties
    [Networked] private GameState _gameState { get; set;}
    #endregion

    #region Runner
    private NetworkRunner _runner;
    #endregion




    #region Dealer Setup
    private void CreateDealer() => _dealer = new Dealer(StartRoutine,StopRoutine);
    #endregion
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
    #endregion
    #region methods to link with UI 
    public void SetNumberOfPlayers(int numberOfPlayers)
    {
        if(numberOfPlayers<=8 && numberOfPlayers>0)
        {
            _activePlayersNumber = (byte) numberOfPlayers;
#if Log
            LogManager.Log($"Player number is Set !, Player Number = {_activePlayersNumber}",Color.green,LogManager.ValueInformationLog);
#endif
        }
    }
    #endregion
    #region private Logic methods
    //Need Rework 
    private void SetMaxPlayerCards()
    {
        if ( _activePlayersNumber == 0)
        {
#if Log
            LogManager.LogError("player number need to be > 0 before setting the max player cards ");
#endif
            return;
        }
        byte playerCards = 1;
        while ((BeloteDeckSize - (playerCards * ActivePlayersNumber) > 0))
        {
            playerCards++;
        }
        _maxPlayerCards = (byte)(playerCards-1);
    }
    #endregion
    #region Method Wrappers
    /// <summary>
    /// wrapper method for runnner.spawn(objectToSPawn)
    /// </summary>
    /// <param name="objectRef"></param>
    /// <returns></returns>
    private NetworkObject SpawnObject (NetworkPrefabRef objectRef)
    {
        if(_runner == null)
        {
#if Log
            LogManager.LogError("spawning object failed! runner is null!");
#endif
            return null;
        }
        if(objectRef == null)
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
    #endregion
}
