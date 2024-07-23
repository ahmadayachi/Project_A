#define AUTOSTARTGAMECONTROL
#define STARTWITH13CARDS

using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WebSocketSharp;

public class GameManager : NetworkBehaviour
{
    //<=======================================================================(Fields and Props)======================================================================>

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

    [SerializeField] private UIManager _uiManager;
    public UIManager UIManager { get => _uiManager; }

    #endregion UI Manager

    #region Call Back Manager

    private CallBackManager _callBackManager;
    public CallBackManager CallBackManager { get => _callBackManager; }

    #endregion Call Back Manager

    #region Run Time Data

    [SerializeField] private RunTimeDataHolder _runTimeDataHolder;

    #endregion Run Time Data

    #region Change Detector

    private ChangeDetector _changeDetector;

    #endregion Change Detector

    #region Deck properties

    private const int DoubleStandartDeckSize = 104;
    [Networked] private byte _maxPlayerCards { get; set; }

    ///// <summary>
    ///// The max amount of cards that can be dealt to a player, a player should be out if he carry more than this amount
    ///// </summary>
    public byte MaxPlayerCards { get => _maxPlayerCards; }

    [Networked] private DeckType _deckType { get; set; }
    [Networked] private byte _suitsNumber { get; set; }
    private const byte MaxCardsInSuitNumber = 13;

    [Networked, Capacity(MaxCardsInSuitNumber)]
    private NetworkArray<byte> _customSuitRanks { get; }

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
    [Networked] private PlayerTimerState _playerTimerState { get; set; }
    public IPlayer LocalPlayer;
    public IPlayer CurrentPlayer;
    private int _playersNumber;
    public int PlayersNumber { get => _playersNumber; }
    public IPlayer[] Players;
    private int _playerIndex;
    private List<int> _playerReadyList = new List<int>();

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

    [Networked]
    public GameState _gameState { get; set; }

    [Networked] private byte _dealtCardsNumber { get; set; }
    public byte DealtCardsNumber { get => _dealtCardsNumber; }
    [Networked] private DoubtState _doubtState { get; set; }
    public DoubtState DoubtState { get; set; }

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
    public GameMode GameMode { get => GameRunner.GameMode; }
    public SimulationSetUpState SimulationState;
    public const int MaxSetUpWaitTime = 15;
    private bool _simulationSetUpSuccessfull;
#if AUTOSTARTGAMECONTROL
    public bool AutoStartGame;
#endif

    #endregion Simulation Props

    #region Routins

    private Coroutine _simulationSetUpRoutine;
    private Coroutine _waitSetUpThenWaitPlayersRoutine;
    private Coroutine _waitingGameStartedAnimationRoutine;

    #endregion Routins

    //<=======================================================================(Methods)======================================================================>

    #region Dealer Setup

    private void CreateDealer() => _dealer = new Dealer(StartRoutine, StopRoutine);

    #endregion Dealer Setup

    #region Doubt Setup

    private void CreateDoubt() => _doubt = new Doubt(OnDoubtLogic, StartRoutine, StopRoutine);

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
        poolArgs.ActivePlayerCount = (byte)_playersNumber;
        poolArgs.CardsHolder = _uiManager.CardsHolder;
        _cardsPool = new CardPool(poolArgs);
    }

    #endregion Cards Pool Setup

    public override void Spawned()
    {
        //injecting UI dependancy
        _uiManager.InjectGameManager(this);
        //setting UI
        _uiManager.Init();

        //setting CallBackManager
        if (GameMode != GameMode.Single)
        {
#if Log
            LogManager.Log($"{Runner.LocalPlayer} Callback Manager and Changer detector is Set Up  !", Color.gray, LogManager.ValueInformationLog);
#endif
            _callBackManager = new CallBackManager();

            Runner.SetIsSimulated(Object, true);
            //change dectector Set up
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }

        //cheking if the host need to start the Game
        if (CanStartGame())
        {
#if AUTOSTARTGAMECONTROL
            if (AutoStartGame)
#endif
                HostStartGame();
        }
        else
        //cheking if immidiate simulation set up is needed
        if (NeedSimuationSetUp())
        {
            StartSimulationSetUp();
        }
#if Log
        LogManager.Log($"{Runner.LocalPlayer} Game Manager spawned", Color.gray, LogManager.ValueInformationLog);
#endif
    }

    public override void FixedUpdateNetwork()
    {
#if Log
        LogManager.Log($"{Runner.LocalPlayer} Fixed Update Network from Game Manager   !", Color.gray, LogManager.ValueInformationLog);
#endif
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(_gameState): OnGameStateChanged(); break;
                case nameof(_playerTimerState): _callBackManager.EnqueueOrExecute(OnPlayerTimerStateChanged); break;
                case nameof(_currentPlayerID): _callBackManager.EnqueueOrExecute(OnCurrentPlayerIDChanged); break;
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

    #region General Logic Swamp

    private bool NeedSimuationSetUp()
    {
        return _gameState != GameState.NoGameState && _gameState != GameState.SimulationSetUp;
    }

    public bool CanStartGame()
    {
        return IsHost && (_gameState == GameState.NoGameState);
    }

    public void HostStartGame()
    {
        //uploading Deck Info
        UploadDeckInfo();
        // Create CardManager
        SetUpCardManager();
        //Initialising players
        InitPlayers();
        //uploading max cards a player can get
        SetMaxPlayerCards();

        //simulation Prep
        _gameState = GameState.SimulationSetUp;
    }

    private IEnumerator WaitSetUp()
    {
        _simulationSetUpSuccessfull = false;
        bool SetUpCanceled;
        do
        {
            yield return new WaitForSeconds(1);
            _simulationSetUpSuccessfull = SimulationState == SimulationSetUpState.SetUpComplete;
            SetUpCanceled = SimulationState == SimulationSetUpState.SetUpCanceled;
        } while (!_simulationSetUpSuccessfull && !SetUpCanceled);
    }

    /// <summary>
    /// wait players then clears the players List
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitPlayers()
    {
        //waiting players RPC
        yield return new WaitUntil(AllPlayersReady);
        //reseting
        _playerReadyList.Clear();
    }

    private IEnumerator WaitForPlayersGameStartedAnimation()
    {
        yield return WaitPlayers();
#if Log
        LogManager.Log("All Players GameStarted Animation Exectuted", Color.green, LogManager.ValueInformationLog);
#endif
        //moving to dealing state
        _gameState = GameState.Dealing;
    }

    private IEnumerator WaitSetUpThenWaitPlayers()
    {
        yield return WaitSetUp();

        //if Host Set Up Complete then Wait for players
        if (_simulationSetUpSuccessfull)
        {
#if Log
            LogManager.Log("Host Waiting For Players Simulation Set Up", Color.yellow, LogManager.ValueInformationLog);
#endif

            yield return WaitPlayers();
#if Log
            LogManager.Log("All Players Simulation is Set Up", Color.green, LogManager.ValueInformationLog);
#endif
            //Moving tto Game Started Game State
            _gameState = GameState.GameStarted;
        }
        else
        {
            //still dont know maybe disconnect? reload scene?
        }
        _waitSetUpThenWaitPlayersRoutine = null;
    }

    private void StartSimulationSetUp()
    {
        if (_simulationSetUpRoutine != null)
            StopCoroutine(_simulationSetUpRoutine);
        _simulationSetUpRoutine = StartCoroutine(SetUp());
    }

    private IEnumerator SetUp()
    {
        //some panel that tracks simulation states as a loading screen
        _uiManager.UIEvents?.OnSetUpStarted();

        //Logic Set up
        yield return SimulationLogicSetUp();

        //just being a protective trans mother
        yield return new WaitUntil(() => SimulationState == SimulationSetUpState.LogicSetUp);

        //UI Set Up
        yield return _uiManager.UIEvents?.SetUpUI();

        yield return new WaitUntil(() => SimulationState == SimulationSetUpState.UISetUp);

        SimulationState = SimulationSetUpState.SetUpComplete;

        _callBackManager.SetReady(true);

        _simulationSetUpRoutine = null;

        if (_gameState == GameState.SimulationSetUp)
            RPC_PlayerReady(LocalPlayer.playerRef.PlayerId);
    }

    private IEnumerator SimulationLogicSetUp()
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

        yield return null;

        //forcing waiting minimum one sec
#if Log
        LogManager.Log($"Waiting for Logic Set Up Runner Player Ref => {Runner.LocalPlayer}", Color.yellow, LogManager.ValueInformationLog);
#endif
        int timer = 0;
        do
        {
            yield return new WaitForSeconds(1);
            timer++;
        } while ((!CheckLogicSetUp()) && timer <= MaxSetUpWaitTime);

        // one more time !
        if (CheckLogicSetUp())
        {
            SimulationState = SimulationSetUpState.LogicSetUp;
#if Log
            LogManager.Log($" Logic is Set Up Local Player => {LocalPlayer}", Color.green, LogManager.ValueInformationLog);
#endif
        }
        else
        {
            //stop the whole process
            StopCoroutine(_simulationSetUpRoutine);
            SimulationState = SimulationSetUpState.SetUpCanceled;
#if Log
            LogManager.LogError($"Simulation Set Up is Canceled! Logic Set Up Failed! player Ref=> {Runner.LocalPlayer}");
#endif
        }
    }

    private bool CheckLogicSetUp()
    {
        bool checkState = true;
        if (CardManager.Deck == null)
        {
#if Log
            LogManager.Log($"Deck is null !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }
        if (Players == null)
        {
#if Log
            LogManager.Log($"Players array is null  !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }

        if (Players.Count() == 0)
        {
#if Log
            LogManager.Log($"Players array is Empty  !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }

        if (_playersNumber == 0)
        {
#if Log
            LogManager.Log($"Player Number Is Invalid !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }

        if (LocalPlayer == null)
        {
#if Log
            LogManager.Log($"Local Player Is null !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }
        if (_runTimeDataHolder.RunTimePlayersData == null)
        {
#if Log
            LogManager.Log($"RunTimePlayersData Is null !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }
        if (_runTimeDataHolder.RunTimePlayersData.Count != _playersNumber)
        {
#if Log
            LogManager.Log($"RunTimePlayersData Count is Invalid !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }
        if (_cardsPool == null)
        {
#if Log
            LogManager.Log($"cardsPool Is null !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }
        if (_betHandler == null)
        {
#if Log
            LogManager.Log($"betHandler Is null !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }

        if (IsHost)
        {
            if (_dealer == null)
            {
#if Log
                LogManager.Log($"dealer Is null !", Color.red, LogManager.ValueInformationLog);
#endif
                checkState = false;
            }
            if (_doubt == null)
            {
#if Log
                LogManager.Log($"doubt Is null !", Color.red, LogManager.ValueInformationLog);
#endif
                checkState = false;
            }
        }

        return checkState;
    }

    public bool IsMyTurn()
    {
        if (_currentPlayerID.IsNullOrEmpty() || LocalPlayer == null) return false;
        return LocalPlayer.ID == _currentPlayerID;
    }

    private bool AllPlayersReady()
    {
        return _playerReadyList.Count == _playersNumber;
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

            //hooking player with simulation
            player.BondPlayerSimulation(this);

            //player prep
            PlayerArguments playerArgs = new PlayerArguments();
            playerArgs.PlayerRef = playerData.PlayerRef;
            playerArgs.Name = playerData.PlayerName;
            playerArgs.ID = playerData.PlayerID;
            playerArgs.IconID = (byte)playerData.IconIndex;
            //playerArgs.GameManager = this;
            playerArgs.isplayerOut = false;
            player.InitPlayer(playerArgs);

#if STARTWITH13CARDS
            for (int index = 0; index < 2; index++)
            {
                player.PlusOneCard();
            }
#endif
            //RunTime Data Adjust
            RunTimePlayerData newData = new RunTimePlayerData();
            newData.PlayerRef = playerData.PlayerRef;
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
        //setting the first player
        _currentPlayerID = Players[0].ID;
        CurrentPlayer = Players[0];
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
            playerData.PlayerName = player.Name;
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
            LogManager.Log($"No Data In Cloud Found! Loading Player for this Player {Runner.LocalPlayer} is Canceled", Color.cyan, LogManager.ValueInformationLog);
#endif
            return;
        }
        int playersCount = _cloudplayersData.Count();
        Players = new IPlayer[playersCount];
        _playersNumber = playersCount;
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
                player.BondPlayerSimulation(this);
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

    private void SetUpCardManager() => CardManager.Init(_runTimeDataHolder.DeckInfo);

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

    //TODO : Link with UI(when Doubt scene finsih on Host )
    /// <summary>
    /// invked by host after Doubt scene
    /// </summary>
    private void DoubtOverLogic()
    {
        //Player Control
        PlayerControl();

        //Directing Game State
        _gameState = GameState.RoudOver;
    }

    private void CaluCulateDoubtSceneTimer()
    {
        //TODO: Calculate Doubt Scene Timer Based On UI Needs
    }

    private void PlayerControl()
    {
        if (Players.IsNullOrHaveNullElements())
        {
#if Log
            LogManager.LogError("Failed Finding Player! Players Array is Null or Have Null Elements");
#endif
            return;
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
    }

    private void PunishingDoubtLooser(out string playerToPunishID, out IPlayer playerToPunish)
    {
        playerToPunishID = (_doubtState == DoubtState.WinDoubt) ? _liveBetPlayerID : _currentPlayerID;
        if (TryFindPlayer(playerToPunishID, out playerToPunish))
        {
            playerToPunish.PlusOneCard();
        }
        else
        {
#if Log
            LogManager.LogError($"Failed Doubt Over Logic Current Player! Cant Find  Player with ID:=> {playerToPunishID}");
#endif
            return;
        }
    }

    private void RoundOverVariablesCleaning()
    {
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
            player.ClearHand();
        }
        _doubtState = DoubtState.NoDoubting;
    }

#endregion General Logic Swamp

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

    #region Player Commands  RPC Methods

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ConfirmBet(byte[] bet, string playerID)
    {
        ConfirmingBet(bet, playerID);
    }

    private void ConfirmingBet(byte[] bet, string playerID)
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
        //stoping timer
        _playerTimerState = PlayerTimerState.StopTimer;
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
                _gameState = GameState.LastPlayerTurn;
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

        //stoping timer
        _playerTimerState = PlayerTimerState.StopTimer;

        byte[] liveBet = _liveBet.ToByteArray();
        _dealtCards.ToByteList(_dealtCardsList);
        //invoking Doubt State
        DoubtStateArguments stateArguments = new DoubtStateArguments(_dealtCardsList, liveBet);
        ChangeState(_doubt, stateArguments);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayerReady(int playerRefID)
    {
        _playerReadyList.Add(playerRefID);
#if Log
        LogManager.Log($" playerRefID:={playerRefID} Added to PlayerReadyList ", Color.green, LogManager.ValueInformationLog);
#endif
    }

    public void PlayerIsReady()
    {
        if (LocalPlayer == null)
        {
#if Log
            LogManager.LogError("Playuer Is Ready Rpc is Canceled !, Local Player Is Null! ");
#endif
            return;
        }
        RPC_PlayerReady(LocalPlayer.playerRef.PlayerId);
    }

    #endregion Player Commands  RPC Methods

    #region Doubting State

    private void OnDoubtLogic(DoubtState doubtState)
    {
        //updating the DoubtState , removeable maybe needed for ui
        _doubtState = doubtState;
        // Calculate and Set Doubt Scene Time
        CaluCulateDoubtSceneTimer();

        //punishing Doubt looser
        string playerToPunishID;
        IPlayer playerToPunish;
        PunishingDoubtLooser(out playerToPunishID, out playerToPunish);

        //setting the Current Player
        _currentPlayerID = playerToPunishID;
        CurrentPlayer = playerToPunish;

        // Updating Clients and Host UI
        _gameState = GameState.Doubting;
    }

    #endregion Doubting State

    #region Round Over Logic

    private void OnRoundIsOverLogic()
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
            RoundOverVariablesCleaning();
            //directing game state
            _gameState = GameState.Dealing;
        }
    }

    #endregion Round Over Logic

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

            //starting Player State
            if (_gameState == GameState.PlayerTurn)
                StartPlayerTimer();
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
        if (_playerTimerState == PlayerTimerState.NoTimer) return;
        //at this time each simulation should Have a Current Player
        if (CurrentPlayer == null)
        {
#if Log
            LogManager.LogError($"Failed Player Turn CallBack! {LocalPlayer} Current Player is null");
#endif
            return;
        }
        //game state should a player turn states
        if (_playerTimerState == PlayerTimerState.StopTimer)
        {
            _currentState?.ForceEnd();
#if Log
            LogManager.Log($"Player Timer Stoped!Simulation=> {LocalPlayer}", Color.green, LogManager.ValueInformationLog);
#endif
            return;
        }
        PlayerStateArguments PlayerStateArgs = new PlayerStateArguments(_gameState, IsMyTurn());
        ChangeState(CurrentPlayer.PlayerState, PlayerStateArgs);
    }

    #endregion Player Timer State Callback

    #region GameState Call Backs

    private void OnGameStateChanged()
    {
#if Log
        LogManager.Log($"{Runner.LocalPlayer} Game State Changed ! gameState={_gameState}", Color.gray, LogManager.ValueInformationLog);
#endif
        switch (_gameState)
        {
            case GameState.SimulationSetUp: SimulationPrepGameState(); break;
            case GameState.GameStarted: _callBackManager.EnqueueOrExecute(GameStarted); break;
            case GameState.Dealing: _callBackManager.EnqueueOrExecute(Dealing); break;
            case GameState.Doubting: _callBackManager.EnqueueOrExecute(Doubting); break;
            case GameState.RoudOver: _callBackManager.EnqueueOrExecute(RoundOver); break;
            case GameState.GameOver: _callBackManager.EnqueueOrExecute(GameOver); break;
            case GameState.HostMigration: _callBackManager.EnqueueOrExecute(HostMigration); break;
            case GameState.FirstPlayerTurn:
            case GameState.LastPlayerTurn: _callBackManager.EnqueueOrExecute(StartPlayerTimer); break;
        }
    }

    private void HostMigration()
    {
        _uiManager.UIEvents.OnHostMigration();
    }

    private void GameOver()
    {
        _uiManager.UIEvents.OnGameOver();
        //cleaing Cards
        _cardsPool.DestroyAll();
        if (IsHost)
        {
            RoundOverVariablesCleaning();
        }
    }

    private void RoundOver()
    {
        //regular UI Cleaning Stuff
        _uiManager.UIEvents.OnRoundOver();
        //TODO : Link with UI
        //cleaing Cards , link to On ROund Over
        _cardsPool.DestroyAll();
        if (IsHost)
            OnRoundIsOverLogic();
    }

    private void Doubting()
    {
        //Doubting Scene Or Something
        _uiManager.UIEvents.OnDoubting();
    }

    private void StartPlayerTimer()
    {
        if (IsHost)
            _playerTimerState = PlayerTimerState.StartTimer;
    }

    private void Dealing()
    {
        _uiManager.UIEvents.OnDealingCards();
        if (IsHost)
        {
            DealerStateArguments args = new DealerStateArguments();
            args.DeckToDeal = CardManager.Deck;
            args.Players = Players;
            args.OnDealerStateEnds = OnDealingOver;
            ChangeState(_dealer, args);
        }
    }

    private void OnDealingOver()
    {
        //maybe some other UI Shit here
        _gameState = GameState.FirstPlayerTurn;
    }

    private void SimulationPrepGameState()
    {
        StartSimulationSetUp();

        if (IsHost)
        {
            if (_waitSetUpThenWaitPlayersRoutine != null)
                StopCoroutine(_waitSetUpThenWaitPlayersRoutine);
            _waitSetUpThenWaitPlayersRoutine = StartCoroutine(WaitSetUpThenWaitPlayers());
        }
    }

    private void GameStarted()
    {
        _uiManager.UIEvents.OnGameStarted();
        if (IsHost)
        {
            if (_waitingGameStartedAnimationRoutine != null)
                StopCoroutine(_waitingGameStartedAnimationRoutine);
            _waitingGameStartedAnimationRoutine = StartCoroutine(WaitForPlayersGameStartedAnimation());
        }
    }

    #endregion GameState Call Backs
}