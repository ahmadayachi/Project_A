using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;
using WebSocketSharp;

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
    public CardPool CardPool { get => _cardsPool; }

    #endregion Card Pool System

    #region Doubt

    private Doubt _doubt;

    #endregion Doubt

    #region UI Manager
    [SerializeField] UIManager _uiManager;
    #endregion

    #region Run Time Data
    [SerializeField] private RunTimeDataHolder _runTimeDataHolder;
    #endregion

    #region Change Detector
    private ChangeDetector _changeDetector;
    #endregion

    #region Deck properties

    private const int DoubleStandartDeckSize = 104;
    [Networked] private byte _maxPlayerCards { get; set;}

    ///// <summary>
    ///// The max amount of cards that can be dealt to a player, a player should be out if he carry more than this amount
    ///// </summary>
    public byte MaxPlayerCards { get => _maxPlayerCards; }
    [Networked] private DeckType _deckType { get; set; }
    [Networked] byte _suitsNumber { get; set;}
    private const byte MaxCardsInSuitNumber = 13;
    [Networked, Capacity(MaxCardsInSuitNumber)]
    private NetworkArray<byte> _customSuitRanks {  get;}
    #endregion Deck properties
  
    #region Player Propertys

    private const byte MaxPlayersNumber = 8;

    /// <summary>
    /// Array of  Active Players
    /// </summary>
    [Networked, Capacity(MaxPlayersNumber)]
    private NetworkArray<NetworkObject> _cloudplayersData { get; }

    [Networked, Capacity(MaxPlayersNumber - 1)]
    private NetworkArray<string> _loosersIDs { get; }

    [Networked] private string _winnerID { get; set; }
    [Networked] private string _currentPlayerID { get; set; }
    [Networked] private PlayerTimerState _playerTimerState { get; set;}
    public IPlayer LocalPlayer;
    public IPlayer CurrentPlayer;
    private int _playersNumber;
    public int PlayersNumber { get => _playersNumber; }

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

    [Networked] private byte _doubtSceneTimer { get; set; }
    public byte DoubtSceneTimer { get => _doubtSceneTimer; }

    #endregion Live Bet Props

    #region State Props

    private State _currentState;

    #endregion State Props

    #region Simulation Props

    public NetworkRunner GameRunner { get => Runner; }
    public bool IsHost { get => GameRunner.IsServer; }
    public bool IsClient { get => GameRunner.IsClient; }
    public GameMode GameMode { get => GameRunner.GameMode;}
    public SimulationSetUpState SimulationState;

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
        if (_playersNumber == 0 || _maxPlayerCards == 0)
        {
#if Log
            LogManager.LogError($" Failed Creating CardPool! active player number {_playersNumber} max player cards {_maxPlayerCards}");
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

    public override void Spawned()
    {
        //injecting UI dependancy 
        _uiManager.InjectGameManager(this);
        //setting UI 
        _uiManager.InitUI();


        if (HostNeedSetUpPlayersProperties())
        {
            //uploading Deck Info 
            UploadDeckInfo();
            //Initialising players 
            InitPlayers();
            //uploading max cards a player can get 
            SetMaxPlayerCards();

            //simulation Prep
            _gameState = GameState.SimulationSetUp;
        }
    }
    public override void FixedUpdateNetwork()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(_gameState): OnGameStateChanged(); break;
            }
        }
    }

    #region methods to link with UI

    public void SetNumberOfPlayers(int numberOfPlayers)
    {
        if (numberOfPlayers <= 8 && numberOfPlayers > 0)
        {
            _playersNumber = (byte)numberOfPlayers;
#if Log
            LogManager.Log($"Player number is Set !, Player Number = {_playersNumber}", Color.green, LogManager.ValueInformationLog);
#endif
        }
    }

    #endregion methods to link with UI

    #region General Logic
    private bool HostNeedSetUpPlayersProperties()=>(IsHost && (_gameState == GameState.NoGameState));
    
    public bool IsMyTurn()
    {
        if (_currentPlayerID.IsNullOrEmpty() || LocalPlayer == null) return false;
        return LocalPlayer.ID == _currentPlayerID;
    }
    private void StartGame()
    {
        
    }
    private void InitPlayers()
    {
        //canceling if not enough run time data
        int dataCount = _runTimeDataHolder.RunTimePlayersData.Count;
        if (dataCount < 2)
        {
#if Log
            LogManager.LogError($"Players Init Is Canceled ! not enough Run Time Data , Run Time Data Count = {dataCount}");
#endif
            return;
        }

        int playerIndex = 0;
        Players = new IPlayer[dataCount];
        _playersNumber = dataCount;
        List<RunTimePlayerData> newRunTimeData = new List<RunTimePlayerData>();
        foreach (RunTimePlayerData playerData in _runTimeDataHolder.RunTimePlayersData)
        {
            //spawping player 
            NetworkObject playerObject = GameRunner.Spawn(AssetLoader.PrefabContainer.PlayerPrefab, default, default, playerData.PlayerRef);
            playerObject.name = playerData.PlayerName;
            Player player = playerObject.GetComponent<Player>();
         
            //player prep 
            PlayerArguments playerArgs = new PlayerArguments();
            playerArgs.PlayerRef = playerData.PlayerRef;
            playerArgs.Name = playerData.PlayerName;
            playerArgs.ID = playerData.PlayerID;
            playerArgs.IconID = (byte)playerData.IconIndex;
            playerArgs.GameManager = this;
            playerArgs.isplayerOut = false;
            player.InitPlayer(playerArgs);

            //RunTime Data Adjust 
            RunTimePlayerData newData = new RunTimePlayerData();
            newData.PlayerRef =playerData.PlayerRef;
            newData.PlayerName = playerData.PlayerName;
            newData.PlayerID = playerData.PlayerID;
            newData.IconIndex = playerData.IconIndex;
            newData.PlayerNetObject = playerObject;
            newData.AuthorityAssigned = true;
            newRunTimeData.Add(newData);

            // setting Local Player 
            SetLocalPlayer(player);

            //uploading player netobject on cloud
            _cloudplayersData.Set(playerIndex, playerObject);
            //stroing player on local simulation
            Players[playerIndex] = player;
            playerIndex++;
        }
        //resetting RunTime Data
        _runTimeDataHolder.RunTimePlayersData.Clear();
        _runTimeDataHolder.RunTimePlayersData.AddRange(newRunTimeData);
    }
    private void SetUpRunTimeData()
    {
        if (Players == null || Players.Count() == 0)
        {
#if Log
            LogManager.LogError("Run Time Data Set Up Is Canceled!");
#endif
            return;
        }
        //just making sure 
        _runTimeDataHolder.RunTimePlayersData.Clear();

        foreach (IPlayer player in Players)
        {
            RunTimePlayerData playerData = new RunTimePlayerData();
            playerData.PlayerRef = player.playerRef;
            playerData.PlayerName =player.Name;
            playerData.PlayerID = player.ID;
            playerData.IconIndex = player.IconID;
            playerData.PlayerNetObject = player.NetworkObject;
            //since this only happens after spawning players im assuming it should be true
            playerData.AuthorityAssigned = true;

            _runTimeDataHolder.RunTimePlayersData.Add(playerData);
        }
    }
    /// <summary>
    /// Initializes The Players Array Data
    /// </summary>
    private void LoadPlayers()
    {
        if (_cloudplayersData.IsEmpty())
        {
#if Log
            LogManager.Log($"No Data In Cloud Found! Loading Player for this Player {LocalPlayer} is Canceled", Color.cyan, LogManager.ValueInformationLog);
#endif
            return;
        }
        Players = new IPlayer[_cloudplayersData.Count()];
        int playerIndex = 0;
        //players need to be spawned before Fetching 
        foreach (NetworkObject playerNetObject in _cloudplayersData)
        {
            if (Extention.IsObjectUsable(playerNetObject))
            {
                Player player = playerNetObject.GetComponent<Player>();
                if (player == null)
                {
#if Log
                    LogManager.LogError($"Player Loading Is Canceled! Cloud Player Data does not Contain a Player Component ! Local Player {LocalPlayer}");
#endif
                    return;
                }
                //setting local player
                SetLocalPlayer(player);

                //storing player on local simulation
                Players[playerIndex++] = player;
            }
        }
    }
    private void SetLocalPlayer(Player player)
    {
        if (player.HasInputAuthority)
        {
            LocalPlayer = player;
#if Log
            LogManager.Log($"{player} Local Player Is Set", Color.cyan, LogManager.ValueInformationLog);
#endif

        }
    }
    private void LoadDeckInfo()
    {
        _runTimeDataHolder.DeckInfo = new DeckInfo();
        _runTimeDataHolder.DeckInfo.DeckType = _deckType;
        _runTimeDataHolder.DeckInfo.SuitsNumber = _suitsNumber;
        if (_deckType == DeckType.Custom)
        {
            if (_customSuitRanks.IsEmpty())
            {
#if Log
                LogManager.LogError("Loading Deck Info is Canceled ! Custom Deck Suit Ranks is Empty ");
#endif
                return;
            }
            _runTimeDataHolder.DeckInfo.CustomSuitRanks = new byte[_customSuitRanks.ValidCardsCount()];
            int index = 0;
            foreach (byte card in _customSuitRanks)
            {
                if (card != 0)
                    _runTimeDataHolder.DeckInfo.CustomSuitRanks[index++] = card;
            }
        }
    }
    private void UploadDeckInfo()
    {
        _deckType = _runTimeDataHolder.DeckInfo.DeckType;
        _suitsNumber = _runTimeDataHolder.DeckInfo.SuitsNumber;
        if (_deckType == DeckType.Custom)
        {

            if (_runTimeDataHolder.DeckInfo.CustomSuitRanks.IsEmpty())
            {
#if Log
                LogManager.LogError("Uploading Deck Info is Canceled ! Custom Deck Suit Ranks is Empty ");
#endif
                return;
            }
            _customSuitRanks.ClearByteArray();
            int index = 0;
            foreach (byte card in _runTimeDataHolder.DeckInfo.CustomSuitRanks)
            {
                if (card != 0)
                    _customSuitRanks.Set(index++, card);
            }

        }
    }
    private void SetUpCardManager()=>CardManager.Init(_runTimeDataHolder.DeckInfo);
    #endregion

    #region private Logic methods

    private void SetMaxPlayerCards()
    {
        if (_playersNumber == 0)
        {
#if Log
            LogManager.LogError("player number need to be > 0 before setting the max player cards ");
#endif
            return;
        }
        byte playerCards = 1;
        int currentDeckSize = CardManager.Deck.Length;
        while ((currentDeckSize - (playerCards * PlayersNumber) > 0))
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
        //Player Control
        if (Players.IsNullOrHaveNullElements())
        {
#if Log
            LogManager.LogError("Failed Finding Player! Players Array is Null or Have Null Elements");
#endif
            yield break;
        }
        //if a player cards to deal counter > max cards Count he should be out
        foreach (var player in Players)
        {
            if (player.CardsToDealCounter > _maxPlayerCards)
            {
                if (!player.IsOut)
                {
                    player.ClearHand();
                    player.ClearCardsCounter();
                    player.SetIsplayerOut(true);
                    _loosersIDs.AddPlayerID(player.ID);
                }
            }
        }
        yield return null;
        //setting the Current Player
        _currentPlayerID = playerToPunishID;
        CurrentPlayer = playerToPunish;
        //Directing Game State
        _gameState = GameState.RoudOver;
    }

    #endregion Doubt Over Logic

    #region Round Over Logic

    private void OnRoundIsOver()
    {
        //host only stuff 
        if (IsHost)
        {
            //checking if the game is over
            if (IsGameOver())
            {
                //fetch winner
                IPlayer Winner = Players.First(player => (!player.IsOut));
                if (Winner == null)
                {
#if Log
                    LogManager.LogError("Failed Fetching Winner!");
#endif
                    return;
                }
                //setting the Winner ID
                _winnerID = Winner.ID;
                //directing Game State and updating clients
                _gameState = GameState.GameOver;
                //Maybe game Over stuff here
            }
            else
            {
                //clearing
                _liveBetPlayerID = string.Empty;
                _liveBet.ClearByteArray();
                _diffusedBet.Clear();
                _doubtSceneTimer = 0;
                _dealtCards.ClearByteArray();
                _dealtCardsList.Clear();
                _dealtCardsNumber = 0;
                //clearing Players Hand
                foreach (var player in Players)
                {
                    if (!player.IsOut)
                    {
                        player.ClearHand();
                    }
                }
                //directing game state
                _gameState = GameState.Dealing;
            }
        }
        //regular UI Cleaning Stuff
    }

    #endregion Round Over Logic

    #region Player Commands  RPC Methods

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
            LogManager.Log($"Auto Doubt is Launched!, Current Player {_currentPlayerID} Live Bet Player ID {_liveBetPlayerID}", Color.blue, LogManager.GameModeLogs);
#endif
            return;
        }
        //checking if the next Current Player Have to Play a Max Bet
        byte[] roundedUpBet;
        if (BetGenerator.TryRoundUpBet(sortedBet, out roundedUpBet, DealtCardsNumber))
        {
            //cheking if the rounded up bet is a max Bet
            if (MaxBet.AreEqual(roundedUpBet))
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

    #endregion Doubting State

    #region state contol methods

    private void ChangeState<T>(State newState, T newStateArgs) where T : struct
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

    private bool IsGameOver()
    {
        if (Players == null || Players.Length == 0) return false;
        int counter = 0;
        for (int index = 0; index < Players.Length; index++)
        {
            if (!Players[index].IsOut)
                counter++;
        }
        if (counter == 1) return true;
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
        if (IsGameOver())
        {
#if Log
            LogManager.Log("Failed Passing Turn ! Game should be Over !", Color.red, LogManager.GameModeLogs);
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

    #region Player Timer State Callback
    private void OnPlayerTimerStateChanged()
    {
        //blocking resets
        if(_playerTimerState == PlayerTimerState.NoTimer) return;
        //at this time each simulation should Have a Current Player 
        if (CurrentPlayer == null)
        {
#if Log
            LogManager.LogError($"Failed Player Turn CallBack! {LocalPlayer} Current Player is null");
#endif
            return;
        }

        if(_playerTimerState == PlayerTimerState.StopTimer) 
        {
            _currentState?.ForceEnd();
#if Log
            LogManager.Log($"Player Timer Stoped!Simulation=> {LocalPlayer}",Color.green,LogManager.ValueInformationLog);
#endif
            return;
        }
        PlayerStateArguments PlayerStateArgs = new PlayerStateArguments(_gameState,IsMyTurn());
        ChangeState(CurrentPlayer.PlayerState, PlayerStateArgs);
    }
    #endregion

    #region GameState Call Backs
    private void OnGameStateChanged()
    {
        switch (_gameState)
        {
            case GameState.SimulationSetUp:SimulationPrepGameState();break;
        }
    }
    private void SimulationPrepGameState()
    {
        if (IsHost)
        {
            if (Players == null)
            {
                LoadDeckInfo();
                SetUpCardManager();
                LoadPlayers();
                SetUpRunTimeData();
            }
            CreateDealer();
            CreateDoubt();
          
        }
        else
        {
            LoadDeckInfo();
            SetUpCardManager();
            LoadPlayers();
            SetUpRunTimeData();
        }
        CreateCardPool();
        CreateBetHandler();
    }

    #endregion
}