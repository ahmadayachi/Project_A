//using Fusion;
//using Fusion;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class Player : NetworkBehaviour, IPlayer
{
    #region Player fields

    //private NetworkRunner _playerRunner;
    //private ChangeDetector _changeDetector;
    private State _playerState;
    [SerializeField] private PlayerUIController _playerUIControler;

    [SerializeField]
    private PlayerUI _playerUI;

    private GameManager _playerGameManager;

    #endregion Player fields

    #region Player Networked Properties

    //[Networked] public PlayerRef playerRef { get; set; }
    private NetworkVariable<ulong> _clientID  = new NetworkVariable<ulong>();
    //[Networked] private string _playerName { get; set; }
    private NetworkVariable<FixedString32Bytes> _playerName = new NetworkVariable<FixedString32Bytes>();
    //[Networked] private string _id { get; set; }
    private NetworkVariable<FixedString64Bytes> _id = new NetworkVariable<FixedString64Bytes>();

    /// <summary>
    /// how many cards should the player Get
    /// </summary>
    //[Networked] private byte _cardToDealCounter { get; set; }
    private NetworkVariable<byte> _cardToDealCounter = new NetworkVariable<byte>();
    //[Networked] private NetworkBool _isOut { get; set; }
    private NetworkVariable<bool> _isOut = new NetworkVariable<bool>();
    //[Networked] private byte _iconID { get; set; }
    private NetworkVariable<byte> _iconID = new NetworkVariable<byte>();
    private const int MaxCardsInHand = 52;
    /// <summary>
    /// an array of player Card ID's
    /// </summary>
    //[Networked, Capacity(MaxCardsInHand)]
    //private NetworkArray<byte> _hand { get; }
    private NetworkList<byte> _hand = new NetworkList<byte>();
    private NetworkObject _netowrkObject { get; set; }

    #endregion Player Networked Properties

    #region Player Properties

    public State PlayerState { get => _playerState; }
    public PlayerUIController PlayerUIControler { get => _playerUIControler; }
    public string Name { get => _playerName.Value.ToString(); }
    public string ID { get => _id.Value.ToString(); }
    public byte IconID { get => _iconID.Value; }
    public bool IsTheLocalPlayer { get => IsLocalPlayer; }
    public NetworkObject PlayerNetworkObject { get => _netowrkObject; }
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
    public bool IsOut { get => _isOut.Value; }
    public byte CardsToDealCounter { get => _cardToDealCounter.Value; }
    public int HandCount { get => _hand.ValidCardsCount(); }
    public bool IsHandFull { get => (HandCount == CardsToDealCounter); }
    public GameManager PlayerGameManager { get => _playerGameManager; }
    public PlayerUI PlayerUI { get => _playerUI; }

    public ulong ClientID => _clientID.Value;

    #endregion Player Properties

    private CallBackManager _callBackManager;
    private Coroutine _waitSimulationInit;
    private Coroutine _simulationBondingRoutine;
    private Coroutine _firstTickSyncRoutine;


    //public override void Spawned()
    //{
    //    _playerRunner = Runner;
    //    _callBackManager = new CallBackManager();
    //    SetUpPlayerState();

    //    Runner.SetIsSimulated(Object, true);
    //    _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    //    if (_playerName != string.Empty)
    //        gameObject.name = _playerName + ":" + _id;
    //    if (_waitSimulationInit != null)
    //        StopCoroutine(_waitSimulationInit);
    //    _waitSimulationInit = StartCoroutine(WaitSimulation());
    //}

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _callBackManager = new CallBackManager();
        _playerName.OnValueChanged += (previousValue, newValue) => _callBackManager.EnqueueOrExecute(_playerUIControler.SetPlayerName);
        _iconID.OnValueChanged += (previousValue, newValue) => _callBackManager.EnqueueOrExecute(_playerUIControler.SetPlayerIcon);
        _hand.OnListChanged += (newValue) => _callBackManager.EnqueueOrExecute(_playerUIControler.LoadPlayerCards);

        SetUpPlayerState();

        if (_playerName.Value != string.Empty)
            gameObject.name = _playerName + ":" + _id;
        if (_waitSimulationInit != null)
            StopCoroutine(_waitSimulationInit);
        _waitSimulationInit = StartCoroutine(WaitSimulation());
    }

    public void AfterSpawned()
    {
        if (_firstTickSyncRoutine != null)
            StopCoroutine(_firstTickSyncRoutine);
        _firstTickSyncRoutine = StartCoroutine(WaitSimulationAndSyncFirstTick());
    }
    //public override void FixedUpdateNetwork()
    //{
    //    foreach (var change in _changeDetector.DetectChanges(this))
    //    {
    //        switch (change)
    //        {
    //            case nameof(_playerName): _callBackManager.EnqueueOrExecute(_playerUIControler.SetPlayerName); break;
    //            case nameof(_iconID): _callBackManager.EnqueueOrExecute(_playerUIControler.SetPlayerIcon); break;
    //            case nameof(_hand): _callBackManager.EnqueueOrExecute(_playerUIControler.LoadPlayerCards); break;
    //        }
    //    }
    //}

    #region Player Set Up Methods

    private void SetUpPlayerState()
    {
        _playerState = new PlayerState(_playerUIControler);
    }

    private IEnumerator WaitSimulation()
    {
        yield return new WaitUntil(PlayerReadyForCallBacks);
        _callBackManager.SetReady(true);
        AfterSpawned();
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
        if (_playerGameManager == null) return false;
        if (_playerGameManager.CardPool == null) return false;
        if (_playerGameManager.SimulationState != SimulationSetUpState.SetUpComplete) return false;
        return true;
    }

    public void InitPlayer(PlayerArguments playerArgs)
    {
        //SetPlayerRef(playerArgs.PlayerRef);
        SetPlayerName(playerArgs.Name);
        SetPlayerID(playerArgs.ID);
        SetPlayerClientID(playerArgs.ClientID);
        //SetCardCounter(playerArgs.CardCounter);
        SetPlayerIcon(playerArgs.IconID);
        SetIsplayerOut(playerArgs.isplayerOut);
        //BondPlayerSimulation(playerArgs.GameManager);
        PlusOneCard();
    }

//    public void SetPlayerRef(PlayerRef playerRef)
//    {
//        if (playerRef == null || playerRef == PlayerRef.None)
//        {
//#if Log
//            LogManager.LogError($"Invalid Player Player Ref =>{playerRef} player =>{this}");
//#endif
//            return;
//        }
//        this.playerRef = playerRef;
//    }
    public void SetPlayerClientID(ulong playerCLientID)
    {
       _clientID.Value = playerCLientID;
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
        _id.Value = playerID;
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
        _playerName.Value = playerName;
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
        _cardToDealCounter.Value = cardCounter;
    }

    /// <summary>
    /// adds one to the totall Cards Counter
    /// </summary>
    public void PlusOneCard()
    {
        _cardToDealCounter.Value++;
    }

    public void ClearCardsCounter()
    {
        _cardToDealCounter.Value = 0;
    }

    public void SetPlayerIcon(byte IconID)
    {
        _iconID.Value = IconID;
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
        _playerGameManager = gameManager;
        if (_simulationBondingRoutine != null)
            StopCoroutine(_simulationBondingRoutine);
        _simulationBondingRoutine = StartCoroutine(BondPlayerSimulationRoutine());
    }
    private IEnumerator BondPlayerSimulationRoutine()
    {
#if Log
        LogManager.Log($"{this} is waiting for Simulation Set Up Complete", Color.yellow, LogManager.ValueInformationLog);
#endif
        yield return new WaitUntil(() => _playerGameManager.CardPool != null);
        _playerUIControler.SetUpCardPositionerCardPool(_playerGameManager.CardPool);

#if Log
        LogManager.Log($"{this} simulation Bonding is Complete", Color.green, LogManager.ValueInformationLog);
#endif
    }

    public void SetIsplayerOut(bool isPlayerOut)
    {
        if (isPlayerOut)
        {
#if Log
            LogManager.Log($"{this} is Out !", Color.yellow, LogManager.ValueInformationLog);
#endif
        }
        _isOut.Value = isPlayerOut;
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
            LogManager.LogError($"Adding Failed!=>>> {card} to player={this.ToString()} ");
#endif
            return false;
        }
        return true;
    }

    public override string ToString()
    {
        return $"Name:{_playerName.Value}/ ID:{_id.Value}/ CardCounter{_cardToDealCounter.Value}/ IsOut{_isOut.Value}";
    }

    #region Player Commands
    public void ConfirmBet()
    {
        //if it is this player turn and he is the local player
        if (_playerGameManager.IsMyTurn(_id.Value.ToString()))
        {
#if Log
            LogManager.Log($"Sending Confirm RPC !, Player=>{this}", Color.grey, LogManager.ValueInformationLog);
#endif
            //send confirm RPC
            ConfirmBetServerRpc(_playerUIControler.ProcessSelectedCards(), _id.Value);
            //turn off the UI Panel
        }
        else
        {
#if Log
            LogManager.Log($"Confirm Ignored !, it is not this player's Turn ! Player=>{this}", Color.yellow);
#endif
        }

    }
    public void DoubtBet()
    {
        if (_playerGameManager.IsMyTurn(_id.Value.ToString()))
        {

            //send confirm RPC
            DoubtServerRpc(_id.Value);
            //turn off the UI Panel
        }
    }

    //[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    //public void RPC_ConfirmBet(byte[] bet, string playerID)
    //{
    //    _playerGameManager.GameModeManager.ConfirmBet(bet, playerID);
    //}

    [Rpc(SendTo.Server)]
    public void ConfirmBetServerRpc(byte[] bet, FixedString128Bytes playerID)
    {
        _playerGameManager.GameModeManager.ConfirmBet(bet, playerID.ToString());
    }


    //[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    //public void RPC_Doubt(string playerID)
    //{
    //    _playerGameManager.GameModeManager.DoubtBet(playerID);
    //}
    [Rpc(SendTo.Server)]
    public void DoubtServerRpc(FixedString64Bytes playerID)
    {
        _playerGameManager.GameModeManager.DoubtBet(playerID.ToString());
    }

    //public void SetIsplayerOut(bool isPlayerOut)
    //{
    //    throw new System.NotImplementedException();
    //}
    public void PlayerIsReady()
    {
        if (_playerGameManager == null)
        {
#if Log
            LogManager.LogError("Playuer Is Ready Rpc is Canceled !, PlayerGameManager Is Null! ");
#endif
            return;
        }
        PlayerReadyServerRpc(_clientID.Value);
    }
    [ServerRpc]
    public void PlayerReadyServerRpc(ulong playerRefID)
    {
        _playerGameManager.PlayerReadyList.Add(playerRefID);
#if Log
        LogManager.Log($" playerRefID:={playerRefID} Added to PlayerReadyList ", Color.green, LogManager.ValueInformationLog);
#endif
    }
    #endregion




#if UNITY_EDITOR
    [CustomEditor(typeof(Player))]
    public class NetworkPlayerCustomInspector : Editor
    {
        private GUIStyle headerStyle;
        private GUIStyle labelStyle;

        private void OnEnable()
        {
            headerStyle = new GUIStyle()
            {
                richText = true,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState() { textColor = Color.cyan }
            };

            labelStyle = new GUIStyle()
            {
                richText = true,
                fontSize = 14,
                normal = new GUIStyleState() { textColor = Color.white }
            };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Player _this = (Player)target;
            EditorGUILayout.LabelField("<color=cyan>Player Info</color>", headerStyle);
            try
            {
                GUILayout.Label($"<color=white> Name: {_this.Name}</color>", labelStyle);
                GUILayout.Label($"<color=white> Client ID: {_this.ClientID}</color>", labelStyle);
                GUILayout.Label($"<color=white> ID: {_this.ID}</color>", labelStyle);
                GUILayout.Label($"<color=white> Is Local Player: {_this.IsTheLocalPlayer}</color>", labelStyle);
                GUILayout.Label($"<color=white> Card To Deal Counter: {_this._cardToDealCounter}</color>", labelStyle);
                GUILayout.Label($"<color=white> Networked Hand Count: {_this._hand.ValidCardsCount()}</color>", labelStyle);
                GUILayout.Label($"<color=white> Networked Cards IDs in Hand: {_this._hand.ToString()}</color>", labelStyle);
                GUILayout.Label($"<color=white> Cards in Hand: {_this.Hand.ArrayOfCardInfoToString()}</color>", labelStyle);
                GUILayout.Label($"<color=white> Is Out: {_this.IsOut}</color>", labelStyle);
                GUILayout.Label($"<color=white> Icon ID: {_this.IconID}</color>", labelStyle);
            }
            catch
            {
                // Handle exceptions if necessary
            }
        }
    }

#endif
}