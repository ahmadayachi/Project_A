using UnityEngine;
using Fusion;
using System;
using System.Collections;
using UnityEditor;

public class Player : NetworkBehaviour, IPlayer, IAfterSpawned
{
    #region Player fields

    private NetworkRunner _playerRunner;
    private ChangeDetector _changeDetector;
    private State _playerState;
    private PlayerUIController _playerUIControler;

    [SerializeField]
    private PlayerUI _playerUI;

    private GameManager _playerGameManager;

    #endregion Player fields

    #region Player Networked Properties

    [Networked] public PlayerRef playerRef { get; set;}
    [Networked] private string _playerName { get; set; }
    [Networked] private string _id { get; set; }

    /// <summary>
    /// how many cards should the player Get
    /// </summary>
    [Networked] private byte _cardToDealCounter { get; set; }

    [Networked] private NetworkBool _isOut { get; set; }
    [Networked] private byte _iconID { get; set; }
    private const int MaxCardsInHand = 52;
    /// <summary>
    /// an array of player Card ID's
    /// </summary>
    [Networked, Capacity(MaxCardsInHand)]
    private NetworkArray<byte> _hand { get; }

    #endregion Player Networked Properties

    #region Player Properties

    public State PlayerState { get => _playerState; }
    public PlayerUIController PlayerUIControler { get => _playerUIControler; }
    public string Name { get => _playerName; }
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
    public GameManager PlayerGameManager { get => _playerGameManager;}
    public PlayerUI PlayerUI { get => _playerUI; }
    #endregion Player Properties

    private CallBackManager _callBackManager;
    private Coroutine _waitSimulationInit;
    private Coroutine _simulationBondingRoutine;
    private Coroutine _firstTickSyncRoutine;

    public override void Spawned()
    {
        _playerRunner = Runner;
        _callBackManager = new CallBackManager();
        SetUpPlayerUIControler();
        SetUpPlayerState();

        Runner.SetIsSimulated(Object, true);
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        if (_playerName != string.Empty)
            gameObject.name = _playerName + ":" + _id;
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
                case nameof(_playerName): _callBackManager.EnqueueOrExecute(_playerUIControler.SetPlayerName); break;
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
        if (_playerGameManager == null) return false;
        if (_playerGameManager.CardPool == null) return false;
        if (_playerGameManager.SimulationState != SimulationSetUpState.SetUpComplete) return false;
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
        _playerName = playerName;
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
            _playerUIControler = new PlayerUIController( this);
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
        return $"Name:{_playerName}/ ID:{_id}/ CardCounter{_cardToDealCounter}/ IsOut{_isOut}";
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
                GUILayout.Label($"<color=white> Player Ref: {_this.playerRef}</color>", labelStyle);
                GUILayout.Label($"<color=white> ID: {_this.ID}</color>", labelStyle);
                GUILayout.Label($"<color=white> Is Local Player: {_this.IsLocalPlayer}</color>", labelStyle);
                GUILayout.Label($"<color=white> Card To Deal Counter: {_this._cardToDealCounter}</color>", labelStyle);
                GUILayout.Label($"<color=white> Networked Hand Count: {_this._hand.ValidCardsCount()}</color>", labelStyle);
                GUILayout.Label($"<color=white> Networked Cards IDs in Hand: {_this._hand.ArrayOfBytesToString()}</color>", labelStyle);
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