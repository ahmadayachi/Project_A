using Fusion;
using System;
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

    #endregion Doubt

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
    public IPlayer LocalPlayer;
    public IPlayer CurrentPlayer;
    private byte _activePlayersNumber;
    public byte ActivePlayersNumber { get => _activePlayersNumber; }

    public IPlayer[] Players;
    private int _playerIndex;

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

    [Networked] private byte _doubtSceneTimer { get; set;}
    public byte DoubtSceneTimer { get => _doubtSceneTimer;}

    #endregion Live Bet Props

    #region State Props

    private State _currentState;

    #endregion State Props

    #region Simulation Props

    public NetworkRunner GameRunner { get; set; }
    public bool IsHost { get => GameRunner.IsServer; }
    public bool IsClient { get => GameRunner.IsClient; }
    public GameMode GameMode { get => GameRunner.GameMode; }

    #endregion Simulation Props

    #region Dealer Setup

    private void CreateDealer() => _dealer = new Dealer(StartRoutine, StopRoutine);

    #endregion Dealer Setup

    #region Doubt Setup

    private void CreateDoubt() => _doubt = new Doubt(DoubtOverLogic, StartRoutine, StopRoutine);

    #endregion Doubt Setup

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
        if (GameRunner == null)
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

        return GameRunner.Spawn(objectRef);
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

    #endregion Mono Method Wrappers

    #region Doubt Over Logic

    private IEnumerator DoubtOverLogic(DoubtState doubtState)
    {
        // Calculate and Set Doubt Scene Time 
        CaluCulateDoubtSceneTimer();
        // Updating Clients and Host UI 
        _gameState = GameState.Doubting;
        //wait for the Doubt Scene Ui 
        yield return new WaitForSeconds(_doubtSceneTimer);

        //punishing Doubt looser 
        string playerToPunishID = doubtState == DoubtState.WinDoubt ? _liveBetPlayerID : _currentPlayerID;
        IPlayer playerToPunish;
        if (TryFindPlayer(playerToPunishID, out playerToPunish))
        {
            playerToPunish.PlusOneCard();
        }
        else
        {
#if Log
            LogManager.LogError($"Failed Doubt Over Logic Current Player! Cant Find  Player with ID:=> {playerToPunishID}");
#endif
            yield break;
        }
        yield return null;
    }
    #endregion Doubt Over Logic

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
        byte[] sortedBet = _diffusedBet.ToByteArray();
        int sortedBetLength = sortedBet.Length;
        //cleaning Network Array
        for (int index = 0; index < _liveBet.Length; index++)
        {
            _liveBet.Set(index, 0);
        }
        //adding Bet
        for (int index = 0; (index < sortedBetLength); index++)
        {
            _liveBet.Set(index, sortedBet[index]);
        }
        //setting live bet player id
        _liveBetPlayerID = playerID;
        //passing Turn here 
        PassTurn();
        //generating a Max Bet  
        byte[] MaxBet = BetGenerator.GenerateMaxBet(_dealtCardsNumber);
        //cheking if the Played Bet is a Max Bet 
        if (MaxBet.AreEqual(sortedBet))
        {
            //Directing the Game To an Auto Doubt State
            _dealtCards.ToByteList(_dealtCardsList);
            DoubtStateArguments stateArguments = new DoubtStateArguments(_dealtCardsList, sortedBet);
            ChangeState(_doubt, stateArguments);
#if Log
            LogManager.Log($"Auto Doubt is Launched!, Current Player {_currentPlayerID} Live Bet Player ID {_liveBetPlayerID}",Color.blue,LogManager.GameModeLogs);
#endif
            return;
        }
        //checking if the next Current Player Have to Play a Max Bet 
        byte[] roundedUpBet;
        if (BetGenerator.TryRoundUpBet(sortedBet, out roundedUpBet, DealtCardsNumber))
        {
            //cheking if the rounded up bet is a max Bet 
            if(MaxBet.AreEqual(roundedUpBet))
            {
                //directing Game State to a Last Player Game State 
                _gameState = GameState.LastPlayerTrun;
                return;
            }
        }
        //abbording everythink if a bet cannot be Rounded Up 
        else
        {
#if Log
            LogManager.LogError($"Failed Confirm Rpc ! Failed Rounding Up Current Bet! CurrentPlayerID is :=>{_currentPlayerID}");
#endif
            return;
        }

        //Directing Game State  to a normal Player turn 
        _gameState = GameState.PlayerTurn;
    }
    

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Doubt(string playerID)
    {
        //blocking invalid args
        if (string.IsNullOrEmpty(playerID))
        {
#if Log
            LogManager.LogError($"Blocking Doubt Rpc ! Invalid Args Found! CurrentPlayerID is :=>{_currentPlayerID}");
#endif
            return;
        }

        if (_doubt == null)
        {
#if Log
            LogManager.LogError($"Blocking Doubt Rpc ! Doubt State is not Initialized! CurrentPlayerID is :=>{_currentPlayerID}");
#endif
            return;
        }

        //only current player can Doubt
        if (_currentPlayerID != playerID)
        {
#if Log
            LogManager.Log($"Blocking Doubt Rpc ! player with ID:= {playerID} is not the Current Player!,Current Player ID:={_currentPlayerID}", Color.red, LogManager.GameModeLogs);
#endif
            return;
        }
        //player cant doubt himself
        if (_currentPlayerID == _liveBetPlayerID)
        {
#if Log
            LogManager.LogError($"Blocking Doubt Rpc ! player Cant Doubt himself ,Current Player ID:={_currentPlayerID} Live Bet Player ID {_liveBetPlayerID}");
#endif
            return;
        }


        byte[] liveBet = _liveBet.ToByteArray();
        _dealtCards.ToByteList(_dealtCardsList);
        //invoking Doubt State
        DoubtStateArguments stateArguments = new DoubtStateArguments(_dealtCardsList, liveBet);
        ChangeState(_doubt, stateArguments);
    }

    #endregion Player Commands Methods
    #region Doubting State 
    private void CaluCulateDoubtSceneTimer()
    {
        //TODO: Calculate Doubt Scene Timer Based On UI Needs 
    }
    #endregion

    #region state contol methods

    public void ChangeState<T>(State newState, T newStateArgs) where T : struct
    {
        // froce end current state
        _currentState?.ForceEnd();

        // starting the new state
        newState.Start(newStateArgs);
        _currentState = newState;
    }

    #endregion state contol methods

    #region Passing Turn

    private void NextPlayerIndex()
    {
        _playerIndex = _playerIndex + 1;
        if (_playerIndex >= Players.Length)
            _playerIndex = 0;
    }

    private bool GameIsStillRunning()
    {
        if (Players == null || Players.Length == 0) return false;
        int counter = 0;
        for (int index = 0; index < Players.Length; index++)
        {
            if (!Players[index].IsOut)
                counter++;
        }
        if (counter >= 2) return true;
        return false;
    }

    private void PassTurn()
    {
        //Server Only Bitch
        if (IsClient) return;

        if (Players.IsNullOrHaveNullElements())
        {
#if Log
            LogManager.LogError("Failed Passing Turn ! Players Array is Null or Have Null Elements");
#endif
            return;
        }
        if (!GameIsStillRunning())
        {
#if Log
            LogManager.Log("Failed Passing Turn !There only player left Game should be Over !", Color.red, LogManager.GameModeLogs);
#endif
            return;
        }

        int currentPlayerIndex = _playerIndex;
        IPlayer player = null;
        do
        {
            //Moving player Indexer
            NextPlayerIndex();

            try
            {
                player = Players[_playerIndex];
            }
            catch (Exception ex)
            {
#if Log
                LogManager.LogError("Failed Passing Turn!" + ex.Message);
#endif
                return;
            }
        } while (NeedToLookForPlayers(ref currentPlayerIndex, player));
        //final check for player
        bool loopedArray = currentPlayerIndex == _playerIndex;
        if (loopedArray || player.IsOut)
        {
#if Log
            LogManager.LogError($"Failed Passing Turn! looped array{loopedArray}/  Player :{player}");
#endif
            return;
        }

        //setting current player
        CurrentPlayer = player;
        _currentPlayerID = player.ID;
        //if singlePlayer invoke shit here 
        if (GameMode == GameMode.Single)
        {
            //Idk Ui shit or smth
        }

    }

    private bool TryFindPlayer(string playerID, out IPlayer player)
    {
        player = null;
        if (string.IsNullOrEmpty(playerID)) return false;
        if (Players.IsNullOrHaveNullElements())
        {
#if Log
            LogManager.LogError("Failed Finding Player! Players Array is Null or Have Null Elements");
#endif
            return false;
        }
        foreach (var item in Players)
        {
            if (item.ID == playerID)
            {
                player = item;
                return true;
            }
        }
        return false;
    }

    private void OnCurrentPlayerIDChanged()
    {
        if (_currentPlayerID == string.Empty) return;

        //if host is updated return
        if (IsHost)
            if (CurrentPlayer != null && CurrentPlayer.ID == _currentPlayerID) return;
        //looking for desired payer
        IPlayer newCurrentPlayer = null;
        if (TryFindPlayer(_currentPlayerID, out newCurrentPlayer))
        {
            CurrentPlayer = newCurrentPlayer;
            //should invoke corresponding UI or something
        }
        else
        {
#if Log
            LogManager.LogError($"Failed updating Current Player! Cant Find  Player with ID:=> {_currentPlayerID}");
#endif
            return;
        }
    }

    private bool NeedToLookForPlayers(ref int CurrentPlayerIndex, IPlayer player)
    {
        //detecting if I already looped the array
        if (CurrentPlayerIndex == _playerIndex) return false;
        // first player is still playing Halt !
        if (!player.IsOut) return false;
        return true;
    }

    #endregion Passing Turn
}