#define AUTOSTARTGAMECONTROL
//using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    //<=========================(Fields and Props)============================================>

    #region dealer

    private Dealer _dealer;
    public Dealer Dealer { get => _dealer; }

    #endregion dealer

    #region betHandler

    private BetHandler _betHandler;
    public BetHandler BetHandler { get => _betHandler; }
    #endregion betHandler

    #region Card Pool System

    private CardPool _cardPool;
    public CardPool CardPool { get => _cardPool; }

    #endregion Card Pool System

    #region Doubt

    private Doubt _doubt;
    public Doubt Doubt { get => _doubt; }
    #endregion Doubt

    #region UI Manager

    [SerializeField] private UIManager _uiManager;
    public UIManager UIManager { get => _uiManager; }

    #endregion UI Manager

    #region Call Back Manager

    private CallBackManager _callBackManager;
    public CallBackManager CallBackManager { get => _callBackManager; }

    #endregion Call Back Manager

    #region Game Mode Manager
    private GameModeManager _gameModeManager;
    public GameModeManager GameModeManager { get => _gameModeManager; }
    #endregion

    #region Run Time Data

    [SerializeField] public RunTimeDataHolder RunTimeDataHolder;

    #endregion Run Time Data

    //#region Change Detector

    //private ChangeDetector _changeDetector;

    //#endregion Change Detector

    #region Deck properties

    public const int DoubleStandartDeckSize = 104;
    /// <summary>
    /// The max amount of cards that can be dealt to a player, a player should be out if he carry more than this amount
    /// </summary>
    //[Networked] public byte MaxPlayerCards { get; set; }
    public NetworkVariable<byte> MaxPlayerCards = new NetworkVariable<byte>();

    //[Networked] public DeckType DeckType { get; set; }
    public NetworkVariable<DeckType> DeckType = new NetworkVariable<DeckType>();
    //[Networked] public byte SuitsNumber { get; set; }
    public NetworkVariable<byte> SuitsNumber = new NetworkVariable<byte>();
    public const byte MaxCardsInSuitNumber = 13;

    //[Networked, Capacity(MaxCardsInSuitNumber)]
    //public NetworkArray<byte> CustomSuitRanks { get; }

    public NetworkList<byte> CustomSuitRanks = new NetworkList<byte>();

    #endregion Deck properties

    #region Player Propertys

    public const byte MaxPlayersNumber = 8;

    /// <summary>
    /// List of the players that are connected to the cloud
    /// </summary>
    public NetworkList<NetworkObjectReference> CloudplayersData = new NetworkList<NetworkObjectReference>();

    //[Networked, Capacity(MaxPlayersNumber)]
    //public NetworkArray<NetworkObject> CloudplayersData { get; }

    //[Networked, Capacity(MaxPlayersNumber - 1)]
    //public NetworkArray<string> LoosersIDs { get; }

    public NetworkList<FixedString64Bytes> LoosersIDs = new NetworkList<FixedString64Bytes>();
    // [Networked] public string WinnerID { get; set; }
    [SerializeField]
    public NetworkVariable<FixedString64Bytes> WinnerID = new NetworkVariable<FixedString64Bytes>();
    //[Networked] public string CurrentPlayerID { get; set; }
    public NetworkVariable<FixedString64Bytes> CurrentPlayerID = new NetworkVariable<FixedString64Bytes>();
    // [Networked] public PlayerTimerStates PlayerTimerState { get; set; }
    //public NetworkVariable<PlayerTimerStates> PlayerTimerState = new NetworkVariable<PlayerTimerStates>();
    public IPlayer LocalPlayer;
    public IPlayer CurrentPlayer;
    private int _playersNumber;
    public int PlayersNumber { get => _playersNumber; set => _playersNumber = value; }
    public IPlayer[] Players;
    public int PlayerIndex;
    public List<ulong> PlayerReadyList = new List<ulong>();
    public NetworkVariable<PlayerUIState> PlayerUIStates = new NetworkVariable<PlayerUIState>();

    #endregion Player Propertys

    #region Cards Networked Properties

    /// <summary>
    /// Array of the total cards that are dealt to players per Round
    /// </summary>
    //[Networked, Capacity(DoubleStandartDeckSize)]
    //public NetworkArray<byte> DealtCards { get; }
    public NetworkList<byte> DealtCards = new NetworkList<byte>();

    public List<byte> DealtCardsList = new List<byte>();

    #endregion Cards Networked Properties

    #region GameState properties

    //[Networked]
    //public GameState State { get; set; }
    public NetworkVariable<GameState> State = new NetworkVariable<GameState>();
    //[Networked] public byte DealtCardsNumber { get; set; }
    public NetworkVariable<byte> DealtCardsNumber = new NetworkVariable<byte>();
    //[Networked] public DoubtState DoubtState { get; set; }
    public NetworkVariable<DoubtState> DoubtState = new NetworkVariable<DoubtState>();

    #endregion GameState properties

    #region Live Bet Props

    /// <summary>
    /// Array of card ranks that the previous player bet on
    /// </summary>
    //[Networked, Capacity(DoubleStandartDeckSize)]
    //public NetworkArray<byte> LiveBet { get; }
    public NetworkList<byte> LiveBet = new NetworkList<byte>();

    public List<DiffusedRankInfo> DiffusedBet = new List<DiffusedRankInfo>();

    /// <summary>
    /// Id of the Player who set the Live Bet
    /// </summary>
    //[Networked] public string LiveBetPlayerID { get; set; }
    public NetworkVariable<FixedString64Bytes> LiveBetPlayerID = new NetworkVariable<FixedString64Bytes>();

    //[Networked] public byte DoubtSceneTimer { get; set; }
    public NetworkVariable<byte> DoubtSceneTimer = new NetworkVariable<byte>();

    #endregion Live Bet Props

    #region State Props

    private State _currentState;
    public State CurrentState { get => _currentState; }

    #endregion State Props

    #region Simulation Props

    //only set after spawning
    //public NetworkRunner GameRunner;
    //public bool IsHost { get => GameRunner.IsServer; }
    //public bool IsHost { get => IsHost; }
    //public bool IsClient { get => GameRunner.IsClient; }
    public GameMode CurrentGameMode { get; private set; }
    public SimulationSetUpState SimulationState;
    public const int MaxSetUpWaitTime = 15;
    public bool SimulationSetUpSuccessfull { get; set; }

#if AUTOSTARTGAMECONTROL
    public bool AutoStartGame;
#endif

    #endregion Simulation Props

    #region Routins

    public Coroutine SimulationSetUpRoutine;
    public Coroutine WaitSetUpThenWaitPlayersRoutine;
    public Coroutine WaitingGameStartedAnimationRoutine;

    #endregion Routins

    //<==================================================================================================>

    //    public override void Spawned()
    //    {
    //        //grabing Runner 
    //        GameRunner = Runner;
    //        //injecting UI dependancy
    //        _uiManager.InjectGameManager(this);

    //        //setting UI
    //        _uiManager.Init(GameMode);

    //        //setting up game mode Manager
    //        GameModeManagerSetUp();

    //        //setting CallBackManager
    //        SetUpCallBackManager();

    //        //cheking if the host need to start the Game
    //        if (CanStartGame())
    //        {
    //#if AUTOSTARTGAMECONTROL
    //            if (AutoStartGame)
    //#endif
    //                HostStartGame();
    //        }
    //        else
    //        //cheking if immidiate simulation set up is needed
    //        if (NeedSimuationSetUp())
    //        {
    //            _gameModeManager.StartSimulationSetUp();
    //        }
    //#if Log
    //        LogManager.Log($"{Runner.LocalPlayer} Game Manager spawned", Color.gray, LogManager.ValueInformationLog);
    //#endif
    //    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //setting up callbacks 
        State.OnValueChanged += OnGameStateChanged;
        //PlayerTimerState.OnValueChanged += OnPlayerTimerStateChanged;
        CurrentPlayerID.OnValueChanged += OnCurrentPlayerIDChanged;
        PlayerUIStates.OnValueChanged += OnPlayerUIStateChanged;
        //injecting UI dependancy
        _uiManager.InjectGameManager(this);

        //setting up the Game Mode 
        CurrentGameMode = IsHost ? GameMode.Host : GameMode.Client;

        //setting UI
        _uiManager.Init(CurrentGameMode);

        //setting up game mode Manager
        GameModeManagerSetUp();

        //setting CallBackManager
        SetUpCallBackManager();

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
            _gameModeManager.StartSimulationSetUp();
        }

#if Log
        LogManager.Log($"{NetworkManager.Singleton.LocalClientId} Game Manager spawned", Color.gray, LogManager.ValueInformationLog);
#endif
    }





    //public override void Render()
    //{

    //    //#if Log
    //    //        LogManager.Log($"{Runner.LocalPlayer} Fixed Update Network from Game Manager   !", Color.gray, LogManager.ValueInformationLog);
    //    //#endif
    //    foreach (var change in _changeDetector.DetectChanges(this))
    //    {
    //        switch (change)
    //        {
    //            //case nameof(State): OnGameStateChanged(); break;
    //            //case nameof(PlayerTimerState): _callBackManager.EnqueueOrExecute(OnPlayerTimerStateChanged); break;
    //            case nameof(CurrentPlayerID): _callBackManager.EnqueueOrExecute(OnCurrentPlayerIDChanged); break;
    //        }
    //    }
    //}

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
    #region Mono Method Wrappers

    //    /// <summary>
    //    /// wrapper method for runnner.spawn(objectToSPawn)
    //    /// </summary>
    //    /// <param name="objectRef"></param>
    //    /// <returns></returns>
    //    private NetworkObject SpawnObject(NetworkPrefabRef objectRef)
    //    {
    //        if (GameRunner == null)
    //        {
    //#if Log
    //            LogManager.LogError("spawning object failed! runner is null!");
    //#endif
    //            return null;
    //        }
    //        if (objectRef == null)
    //        {
    //#if Log
    //            LogManager.LogError("spawning object failed! object is null!");
    //#endif
    //            return null;
    //        }

    //        return GameRunner.Spawn(objectRef);
    //    }

    public T Insttantiate<T>(T objectToInstantiate, Transform Parent = null) where T : UnityEngine.Object
    {
        return Instantiate(objectToInstantiate, Parent);
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

    #region General Logic Swamp
    //public void SetPlayerTimerState(PlayerTimerStates state)
    //{
    //    PlayerTimerState.Value = state;
    //}
    /// <summary>
    /// allocates callbackManager and grabs the change detector
    /// </summary>
    private void SetUpCallBackManager()
    {
        if (!IsModeSingle())
        {
            //#if Log
            //            LogManager.Log($"{IsLocalPlayer} Callback Manager and Changer detector is Set Up  !", Color.gray, LogManager.ValueInformationLog);
            //#endif
            _callBackManager = new CallBackManager();

            //GameRunner.SetIsSimulated(Object, true);
            //change dectector Set up
            //_changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }
    }
    private void GameModeManagerSetUp()
    {
        var gameModeArgs = new GameModeARGS();
        gameModeArgs.GameManager = this;
        gameModeArgs.GameMode = this.CurrentGameMode;

        _gameModeManager = new GameModeManager(gameModeArgs);
    }
    public void CreateCardPool()
    {
        var cardPrefab = AssetLoader.PrefabContainer.CardPrefab;
        if (cardPrefab == null)
        {
#if Log
            LogManager.LogError(" Failed Fetching CardPrefab from Prefab Container !");
#endif
            return;
        }
        if (_playersNumber == 0 || MaxPlayerCards.Value == 0)
        {
#if Log
            LogManager.LogError($" Failed Creating CardPool! active player number {_playersNumber} max player cards {MaxPlayerCards}");
#endif
            return;
        }
        CardPoolArguments poolArgs = new CardPoolArguments();
        poolArgs.CardPrefab = cardPrefab;
        poolArgs.MaxPlayerCards = MaxPlayerCards.Value;
        poolArgs.ActivePlayerCount = (byte)_playersNumber;
        poolArgs.CardsHolder = _uiManager.CardsHolder;
        _cardPool = new CardPool(poolArgs);
    }
    public void CreateBetHandler() => _betHandler = new BetHandler();
    public void CreateDoubt() => _doubt = new Doubt(OnDoubtLogic, StartRoutine, StopRoutine);
    public void CreateDealer() => _dealer = new Dealer(StartRoutine, StopRoutine);
    public bool IsModeSingle() => CurrentGameMode == GameMode.Single;
    private bool NeedSimuationSetUp()
    {
        return State.Value != GameState.NoGameState /*&& State.Value != GameState.SimulationSetUp*/;
    }
    public bool CanStartGame()
    {
        return IsHost && (State.Value == GameState.NoGameState);
    }
    public void HostStartGame()
    {
        _gameModeManager.StartGame();
    }
    public  bool IsMyTurn()
    {
        if (CurrentPlayerID.Value.IsEmpty || LocalPlayer == null) return false;
       
        if (LocalPlayer.ID == CurrentPlayerID.Value)
        {
            return true;
        }
#if Log
        LogManager.Log($"it is not my turn!my ID=>({LocalPlayer.ID}) vs Current Player ID=>({CurrentPlayerID.Value})", Color.yellow, LogManager.ValueInformationLog);
#endif
        return false;
    }
    public  bool IsMyTurn(string playerID)
    {
        if (CurrentPlayerID.Value.IsEmpty || LocalPlayer == null) return false;
        return playerID == LocalPlayer.ID && LocalPlayer.ID == CurrentPlayerID.Value;
    }
    public bool AllPlayersReady()
    {
        return PlayerReadyList.Count == _playersNumber;
    }
    public void SetLocalPlayer(Player player)
    {
        if (player.IsTheLocalPlayer)
        {
            LocalPlayer = player;
#if Log
            LogManager.Log($"{player} Local Player Is Set", Color.cyan, LogManager.ValueInformationLog);
#endif
        }
    }
    public void SetUpCardManager() => CardManager.Init(RunTimeDataHolder.DeckInfo);

    public IEnumerator DelayAction(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
    #endregion General Logic Swamp

    #region Player Commands  RPC Methods

    //    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    //    public void RPC_PlayerReady(int playerRefID)
    //    {
    //        PlayerReadyList.Add(playerRefID);
    //#if Log
    //        LogManager.Log($" playerRefID:={playerRefID} Added to PlayerReadyList ", Color.green, LogManager.ValueInformationLog);
    //#endif
    //    }
    //    [ServerRpc]
    //    public void PlayerReadyServerRpc(ulong playerRefID)
    //    {
    //        PlayerReadyList.Add(playerRefID);
    //#if Log
    //        LogManager.Log($" playerRefID:={playerRefID} Added to PlayerReadyList ", Color.green, LogManager.ValueInformationLog);
    //#endif
    //    }


    //    public void PlayerIsReady()
    //    {
    //        if (LocalPlayer == null)
    //        {
    //#if Log
    //            LogManager.LogError("Playuer Is Ready Rpc is Canceled !, Local Player Is Null! ");
    //#endif
    //            return;
    //        }
    //        PlayerReadyServerRpc(LocalPlayer.ClientID);
    //    }

    #endregion Player Commands  RPC Methods

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

    #region Call-backs
    private void OnDoubtLogic(DoubtState doubtState)
    {
        _gameModeManager.DoubtLogic(doubtState);
    }
    private void OnCurrentPlayerIDChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
#if Log
        LogManager.Log($"{NetworkManager.Singleton.LocalClientId} Current Player ID Changed ! Current Player ID={CurrentPlayerID.Value}", Color.gray, LogManager.ValueInformationLog);
#endif
        _callBackManager.EnqueueOrExecute(_gameModeManager.LoadCurrentPlayer, nameof(_gameModeManager.LoadCurrentPlayer));
    }
    private void OnPlayerUIStateChanged(PlayerUIState previousValue, PlayerUIState newValue)
    {
#if Log
        LogManager.Log($"{NetworkManager.Singleton.LocalClientId} PlayerUIState Changed ! TurnState{newValue.PlayerTurnState}//// TimerState {newValue.PlayerTimerState}", Color.gray, LogManager.ValueInformationLog);
#endif
        switch (newValue.PlayerTurnState)
        {
            //case PlayerTurnStates.NoState:
            //    {

            //    }
            //    break;
            case PlayerTurnStates.FirstPlayerTurn:
               
            case PlayerTurnStates.PlayerTurn:
                
            case PlayerTurnStates.LastPlayerTurn:
                {
                    _callBackManager.EnqueueOrExecute(GameModeManager.StartPlayerState, nameof(GameModeManager.StartPlayerState)); 
                }
                break;

        }
    }
    public void SyncFirstTick()
    {
        //syncing GameState the first Tick
        if (State.Value != default(GameState))
            OnGameStateChanged(GameState.NoGameState, State.Value);
        // Syncing PlayerTimer First Tick
        if(PlayerUIStates.Value.PlayerTurnState != PlayerTurnStates.NoState || PlayerUIStates.Value.PlayerTimerState!= PlayerTimerStates.NoTimer)
            OnPlayerUIStateChanged(new PlayerUIState(), PlayerUIStates.Value);
        // syncing CurrentPlayerID First Tick
        if (CurrentPlayerID.Value != default(FixedString64Bytes))
            OnCurrentPlayerIDChanged(new FixedString64Bytes(), CurrentPlayerID.Value);
    }
//    private void OnPlayerTimerStateChanged(PlayerTimerStates previousValue, PlayerTimerStates newValue)
//    {
//#if Log
//        LogManager.Log($"{NetworkManager.Singleton.LocalClientId} Player Timer State Changed ! PlayerTimerState={PlayerTimerState.Value}", Color.gray, LogManager.ValueInformationLog);
//#endif
//        _callBackManager.EnqueueOrExecute(_gameModeManager.StartPlayerState, nameof(_gameModeManager.StartPlayerState));
//    }

    private void OnGameStateChanged(GameState previousValue, GameState newValue)
    {
#if Log
        LogManager.Log($"Client ID {NetworkManager.Singleton.LocalClientId} Game State Changed ! gameState={State.Value}", Color.gray, LogManager.ValueInformationLog);
#endif
        _gameModeManager.SetGameState(newValue);
    }
    //    {
    //        //#if Log
    //        //        LogManager.Log($"{Runner.LocalPlayer} Current Player ID Changed ! Current Player ID={CurrentPlayerID}", Color.gray, LogManager.ValueInformationLog);
    //        //#endif
    //#if Log
    //        LogManager.Log($"{NetworkManager.Singleton.LocalClientId} Current Player ID Changed ! Current Player ID={CurrentPlayerID}", Color.gray, LogManager.ValueInformationLog);
    //#endif
    //        _gameModeManager.LoadCurrentPlayer();
    //    }
    //    private void OnPlayerTimerStateChanged()
    //    {
    //#if Log
    //        LogManager.Log($"{NetworkManager.Singleton.LocalClientId} Player Timer State Changed ! PlayerTimerState={PlayerTimerState}", Color.gray, LogManager.ValueInformationLog);
    //#endif
    //        _gameModeManager.StartPlayerState();
    //    }
    //    private void OnGameStateChanged()
    //    {
    //#if Log
    //        LogManager.Log($"{NetworkManager.Singleton.LocalClientId} Game State Changed ! gameState={State}", Color.gray, LogManager.ValueInformationLog);
    //#endif
    //        _gameModeManager.SetGameState(State.Value);
    //    }
    #endregion
}