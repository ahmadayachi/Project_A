using Fusion;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EpicInGameLogicInitiater : MonoBehaviour
{
    #region First Panel Refs

    [Header("Initial Panel Refs")]
    [SerializeField] private GameObject _initialPanel;

    [SerializeField] private Button _offlineMode;
    [SerializeField] private Button _multiPeerMode;
    [SerializeField] private Button _singlePeerMode;
    private InGameLogicModes _mode;

    #endregion First Panel Refs

    #region GameLogic Panels Refs

    [Header("Game Logic Mod Panel First Phase Refs")]
    [SerializeField] private GameObject _gameLogicModPanel;

    [SerializeField] private Button _playerNumber;
    [SerializeField] private TextMeshProUGUI _playerNumberText;
    [SerializeField] private Toggle _belote;
    [SerializeField] private Toggle _standart;
    [SerializeField] private Toggle _custom;
    [SerializeField] private Button _next;
    private DeckType _selectedDeckType;

    [Header("Game Logic Mod Panel Second Phase Refs")]
    [SerializeField] private GameObject _gameLogicModPanelSecondPhaze;

    [SerializeField] private TextMeshProUGUI _totalSuitsNumberText;
    [SerializeField] private Button _totalSuitNumber;
    [SerializeField] private Slider _maxCardsInHandSlider;
    [SerializeField] private TextMeshProUGUI _mixMaxCardsInHandsText;
    [SerializeField] private TextMeshProUGUI _maxCardsInHandText;
    [SerializeField] private Button _nextTwo;
    private const int MinCardsInHand = 2;

    #endregion GameLogic Panels Refs

    #region Custom Deck Panel Refs

    [Header("Custom Deck Mod Panel Refs")]
    [SerializeField] private GameObject _CustomDeckModPanel;

    [SerializeField] private Transform _scrollCardsHolder;
    [SerializeField] private Transform _finalCardsCombinationHolder;
    [SerializeField] private Button _confirmCustomCombination;
    [SerializeField] private Button _resetCustomCombination;
    [SerializeField] private CustomCombinationCard _CustomDeckCardPrefab;
    [SerializeField] private Image _customCombinationIndicator;
    [SerializeField] private Sprite _greenIndicator;
    [SerializeField] private Sprite _redIndicator;
    private List<CustomCombinationCard> _customCombinationCards = new List<CustomCombinationCard>();
    private List<byte> _customDeckCards = new List<byte>();
    private List<byte> _selectedCustomDeckCards = new List<byte>();
    private CardSuit _customDeckSuit = CardSuit.Spades;

    #endregion Custom Deck Panel Refs

    #region Players Info Panel

    [Header("Players Panel Refs")]
    [SerializeField] private GameObject _playersPanel;

    [SerializeField] private RunTimePlayerUI _inGameSetUpPlayerPrefab;
    private List<RunTimePlayerUI> _playersUI = new List<RunTimePlayerUI>();
    private List<RunTimePlayerData> _playersData = new List<RunTimePlayerData>();
    private const string Player = "Player";

    #endregion Players Info Panel

    [Header("Panel Indipendant Shit")]
    [SerializeField] private Button _startRunners;

    [SerializeField] private RunTimeDataHolder _dataHolder;
    #region SinglePeer
    [Header("Single Peer Refs")]
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private Button _hostStartGameButton;
    [SerializeField] private GameObject _singlePeerButtonsHolder;
    [SerializeField] private Button _startHostButton;
    [SerializeField] private Button _startClientButton;
    private NetworkRunner _singlePeerHostRunner;
    #endregion
    #region MultiPeer
    private bool _generateID;
    private GameObject _runnerPrefab;
    private StartGameArgs StartGameArgs;
    private Coroutine _startOnlineCoroutine;
    private List<GameManager> GameManagerlist = new List<GameManager>();
    private List<Task> RunnerTasksList = new List<Task>();
    public bool AllSet { get; set; }
    public List<PeerOnlineInfo> PeersInfoList = new List<PeerOnlineInfo>();
    private PeerOnlineInfo NewPeerData;
    private GameManager _hostGameManager;
    #endregion
    private void Awake()
    {
        SingletonRunner();

        InitFirstPanelButtons();

        InitGameLogicFirstPhaze();

        InitGameLogicSecondPhaze();

        InitCustomCombinationPanel();

        InitSinglePeerButtons();
    }

    #region First Panel Methods

    private void OfflineButton()
    {
        _mode = InGameLogicModes.Offline;
        OpenGameLogicPanel();
        _generateID = true;
    }

    private void OnlineButton()
    {
        _mode = InGameLogicModes.SinglePeer;
        OpenGameLogicPanel();
    }

    private void MultiPeerButton()
    {
        _mode = InGameLogicModes.Multipeer;
        OpenGameLogicPanel();
        _generateID = true;
    }

    private void OpenGameLogicPanel()
    {
        _initialPanel.SetActive(false);
        _gameLogicModPanel.SetActive(true);
    }

    private void InitFirstPanelButtons()
    {
        _initialPanel.SetActive(true);
        _gameLogicModPanel.SetActive(false);
        _CustomDeckModPanel.SetActive(false);
        _playersPanel.SetActive(false);
        _startRunners.gameObject.SetActive(false);

        _offlineMode.onClick.RemoveAllListeners();
        _offlineMode.onClick.AddListener(OfflineButton);
        //_onlineMode.onClick.RemoveAllListeners();
        //_onlineMode.onClick.AddListener(OnlineButton);
        _multiPeerMode.onClick.RemoveAllListeners();
        _multiPeerMode.onClick.AddListener(MultiPeerButton);
        _startRunners.onClick.RemoveAllListeners();
        _startRunners.onClick.AddListener(StartGame);
    }

    #endregion First Panel Methods

    #region Game Logic Panel First phaze Methods

    private string NumberBumber(string numberText)
    {
        int number = int.Parse(numberText);
        return (number == 8 ? 1 : ++number).ToString();
    }

    private void PlayerNumber()
    {
        _playerNumberText.text = NumberBumber(_playerNumberText.text);
    }

    private void Belote(bool isOn)
    {
        if (isOn)
        {
            _selectedDeckType = DeckType.Belote;
            _standart.SetIsOnWithoutNotify(false);
            _custom.SetIsOnWithoutNotify(false);
        }
    }

    private void Standart(bool isOn)
    {
        if (isOn)
        {
            _selectedDeckType = DeckType.Standard;
            _belote.SetIsOnWithoutNotify(false);
            _custom.SetIsOnWithoutNotify(false);
        }
    }

    private void Custom(bool isOn)
    {
        if (isOn)
        {
            _selectedDeckType = DeckType.Custom;
            _belote.SetIsOnWithoutNotify(false);
            _standart.SetIsOnWithoutNotify(false);
        }
    }

    private void GameLogicNextFirstphaze()
    {
        _gameLogicModPanel.SetActive(false);

        if (_selectedDeckType == DeckType.Custom)
            _CustomDeckModPanel.SetActive(true);
        else
            _gameLogicModPanelSecondPhaze.SetActive(true);
    }

    private void InitGameLogicFirstPhaze()
    {
        _playerNumberText.text = 2.ToString();
        _playerNumber.onClick.RemoveAllListeners();
        _playerNumber.onClick.AddListener(PlayerNumber);

        _selectedDeckType = DeckType.Belote;
        _belote.isOn = true;
        _standart.isOn = false;
        _custom.isOn = false;
        _belote.onValueChanged.AddListener(Belote);
        _standart.onValueChanged.AddListener(Standart);
        _custom.onValueChanged.AddListener(Custom);
        _next.onClick.RemoveAllListeners();
        _next.onClick.AddListener(GameLogicNextFirstphaze);
    }

    #endregion Game Logic Panel First phaze Methods

    #region Game Logic Panel Second phaze Methods

    private void CalculateMaxCardsInHand()
    {
        int playersNumber = int.Parse(_playerNumberText.text);
        int totalSuitsNumber = int.Parse(_totalSuitsNumberText.text);
        int CardsInSuitNumber = 0;
        int mixMaxCardsInHand = 0;
        switch (_selectedDeckType)
        {
            case DeckType.Standard: CardsInSuitNumber = 13; break;
            case DeckType.Belote: CardsInSuitNumber = 8; break;
            case DeckType.Custom: CardsInSuitNumber = _customDeckCards.Count; break;
        }
        int DeckCount = CardsInSuitNumber * totalSuitsNumber;
        int playerCards = 1;
        while ((DeckCount - (playerCards * playersNumber) > 0))
        {
            playerCards++;
        }
        mixMaxCardsInHand = playerCards - 1;
        _mixMaxCardsInHandsText.text = mixMaxCardsInHand.ToString();
        _maxCardsInHandSlider.maxValue = mixMaxCardsInHand;
        _maxCardsInHandSlider.wholeNumbers = true;
    }

    private void SuitNumber()
    {
        _totalSuitsNumberText.text = NumberBumber(_totalSuitsNumberText.text);
        CalculateMaxCardsInHand();
    }

    private void MaxCardsInHandSlider(float value)
    {
        _maxCardsInHandText.text = ((int)value).ToString();
    }

    private void SecondPhazeNext()
    {
        _gameLogicModPanelSecondPhaze.SetActive(false);
        InitPlayersUI();
        _playersPanel.SetActive(true);
        _startRunners.gameObject.SetActive(true);
    }

    private void InitGameLogicSecondPhaze()
    {
        _totalSuitsNumberText.text = 2.ToString();
        CalculateMaxCardsInHand();
        _totalSuitNumber.onClick.RemoveAllListeners();
        _totalSuitNumber.onClick.AddListener(SuitNumber);
        _maxCardsInHandSlider.minValue = MinCardsInHand;
        _maxCardsInHandSlider.wholeNumbers = true;
        _maxCardsInHandSlider.onValueChanged.RemoveAllListeners();
        _maxCardsInHandSlider.onValueChanged.AddListener(MaxCardsInHandSlider);
        _nextTwo.onClick.RemoveAllListeners();
        _nextTwo.onClick.AddListener(SecondPhazeNext);
    }

    #endregion Game Logic Panel Second phaze Methods

    #region Custom Deck Mod Methods

    private void InitCustomCombinationCards()
    {
        _customDeckCards.Clear();
        _selectedCustomDeckCards.Clear();
        _customCombinationCards.Clear();
        for (int index = 1; index <= 13; index++)
        {
            CustomCombinationCard customCard = Instantiate(_CustomDeckCardPrefab, _scrollCardsHolder);
            customCard.CardRank = (byte)index;
            customCard.CardImage.sprite = AssetLoader.DeckContainerInstance.GetSuitSprite(customCard.CardRank, _customDeckSuit);
            customCard.CardButton.onClick.RemoveAllListeners();
            customCard.CardButton.onClick.AddListener(() => OnCustomCardClicked(customCard));
            _customCombinationCards.Add(customCard);
            _customDeckCards.Add(customCard.CardRank);
        }
        _customCombinationIndicator.sprite = _redIndicator;
    }

    private void OnCustomCardClicked(CustomCombinationCard card)
    {
        if (_customDeckCards.Contains(card.CardRank))
        {
            _selectedCustomDeckCards.Add(card.CardRank);
            _customDeckCards.Remove(card.CardRank);
            card.gameObject.transform.SetParent(_finalCardsCombinationHolder);
        }
        else
        {
            _selectedCustomDeckCards.Remove(card.CardRank);
            _customDeckCards.Add(card.CardRank);
            card.gameObject.transform.SetParent(_scrollCardsHolder);
        }
        _customCombinationIndicator.sprite = _selectedCustomDeckCards.Count >= 8 ? _greenIndicator : _redIndicator;
    }

    private void ResetCustomComnbination()
    {
        _selectedCustomDeckCards.Clear();
        _customDeckCards.Clear();
        foreach (var item in _customCombinationCards)
        {
            item.CardButton.onClick.RemoveAllListeners();
            item.CardButton.onClick.AddListener(() => { OnCustomCardClicked(item); });
            item.gameObject.transform.SetParent(_scrollCardsHolder);
            _customDeckCards.Add(item.CardRank);
        }
        _customCombinationIndicator.sprite = _redIndicator;
    }

    private void ConfirmCustomCombination()
    {
        if (_selectedCustomDeckCards.Count < 8) return;
        _CustomDeckModPanel.SetActive(false);
        _gameLogicModPanelSecondPhaze.SetActive(true);
        //InitPlayersUI();
        //_playersPanel.SetActive(true);
        //_startRunners.gameObject.SetActive(true);
    }

    private void InitCustomCombinationPanel()
    {
        _confirmCustomCombination.onClick.RemoveAllListeners();
        _confirmCustomCombination.onClick.AddListener(ConfirmCustomCombination);
        _resetCustomCombination.onClick.RemoveAllListeners();
        _resetCustomCombination.onClick.AddListener(ResetCustomComnbination);
        InitCustomCombinationCards();
    }

    #endregion Custom Deck Mod Methods

    #region Player Panel Set up

    private void InitPlayersUI()
    {
        _playersUI.Clear();
        int playersNumber = int.Parse(_playerNumberText.text);

        if (playersNumber < 2) return;
        for (int index = 0; index < playersNumber; index++)
        {
            var player = Instantiate(_inGameSetUpPlayerPrefab, _playersPanel.transform);
            int IconIndex = UnityEngine.Random.Range(0, AssetLoader.AllIcons.Count);
            player.PlayerIcon.sprite = AssetLoader.AllIcons[IconIndex];
            player.IconIndex = IconIndex;
            _playersUI.Add(player);
        }
    }

    private string GenerateUniqueID()
    {
        string ID = Guid.NewGuid().ToString();
        if (_playersData.Count == 0) return ID;
        int counter;
        do
        {
            counter = 0;
            foreach (var player in _playersData)
            {
                if (player.PlayerID == ID)
                {
                    counter++;
                    ID = Guid.NewGuid().ToString();
                    break;
                }
            }
        } while (counter > 0);
        return ID;
    }

    private void SetUpPlayersData()
    {
        _playersData.Clear();
        string playerName = string.Empty;
        int playerIndex = 1;
        foreach (var playerUi in _playersUI)
        {
            playerName = string.IsNullOrEmpty(playerUi.PlayerName) ? Player + playerIndex : playerUi.PlayerName;
            RunTimePlayerData playerData = new RunTimePlayerData();
            playerData.PlayerRef = PlayerRef.None;
            playerData.PlayerName = playerName;
            playerData.PlayerID = GenerateUniqueID();
            playerData.IconIndex = playerUi.IconIndex;
            playerData.PlayerNetObjectRef = null;
            playerData.AuthorityAssigned = false;
            playerIndex++;
            _playersData.Add(playerData);
        }
        _dataHolder.RunTimePlayersData.Clear();
        _dataHolder.RunTimePlayersData.AddRange(_playersData);
        SetUpDeckInfo();
    }

    private void SetUpDeckInfo()
    {
        _dataHolder.DeckInfo = new DeckInfo();
        _dataHolder.DeckInfo.DeckType = _selectedDeckType;
        _dataHolder.DeckInfo.SuitsNumber = byte.Parse(_totalSuitsNumberText.text);
        if (_selectedDeckType == DeckType.Custom)
        {
            int Length = _selectedCustomDeckCards.Count;
            if (Length >= 8)
            {
                _dataHolder.DeckInfo.CustomSuitRanks = new byte[Length];
                for (int index = 0; index < Length; index++)
                {
                    _dataHolder.DeckInfo.CustomSuitRanks[index] = _selectedCustomDeckCards[index];
                }
            }
            else
            {
#if Log
                LogManager.LogError($"Failed Setting Up Deck Info ! Custom Deck Length is Invalid Lenghth =>{Length} ");
#endif
            }
        }
    }

    #endregion Player Panel Set up

    #region Start Game SetUp

    private void SingletonRunner()
    {
        NetworkRunner existingrunner = FindObjectOfType<NetworkRunner>();
        if (existingrunner != null)
            Destroy(gameObject);
        _runnerPrefab = AssetLoader.PrefabContainer.RunnerPrefab;
    }

    private Task StartMultiPeerRunner(StartGameArgs startarg, string playerID)
    {
        GameObject runnerobj = Instantiate(_runnerPrefab);
        DontDestroyOnLoad(runnerobj);
        runnerobj.name = startarg.GameMode.ToString() + " Runner";
        NetworkRunner runner = runnerobj.GetComponent<NetworkRunner>();

        if (startarg.GameMode != GameMode.Single)
        {
#if Log
            LogManager.Log($"Starting {runnerobj.name}!", Color.blue, LogManager.ValueInformationLog);
#endif
            PeerOnlineInfo peerOnlineInfo = new PeerOnlineInfo();
            peerOnlineInfo.PeerRunner = runner;
            peerOnlineInfo.PeerID = playerID;
            PeersInfoList.Add(peerOnlineInfo);
        }
        return runner.StartGame(startarg);
    }

    private IEnumerator WaitForRunnerTask(Task task)
    {
        RunnerTasksList.Add(task);

        while (task.IsCompleted == false)
        {
            yield return new WaitForSeconds(1);
        }
        if (task.IsFaulted)
        {
            Debug.LogWarning(task.Exception);
            StopCoroutine(_startOnlineCoroutine);
            yield break;
        }
    }

    private IEnumerator SetUpClients()
    {
        StartGameArgs.GameMode = GameMode.Client;

        for (int index = 1; index < _playersData.Count; index++)
        {
            yield return WaitForRunnerTask(StartMultiPeerRunner(StartGameArgs, _playersData[index].PlayerID));
        }

        yield return null;
        Task clientTasks = Task.WhenAll(RunnerTasksList);
        while (clientTasks.IsCompleted == false)
        {
            yield return new WaitForSeconds(1);
        }
        if (clientTasks.IsFaulted)
        {
            Debug.LogWarning(clientTasks.Exception);
            StopCoroutine(_startOnlineCoroutine);
            yield break;
        }
    }

    private IEnumerator SetPeersInfo()
    {
        GameManagerlist.AddRange(FindObjectsOfType<GameManager>().ToList());
        yield return new WaitForSeconds(2);
        int gamemangercount = GameManagerlist.Count;
        int playernumber = _playersData.Count;

        Assert.AreEqual(
            playernumber,
            gamemangercount,
            $"GameManagerList Missing data! GameManagerlist count = {gamemangercount} PlayerNumber{playernumber}");

        foreach (GameManager manager in GameManagerlist)
        {
            if (manager.IsHost)
                _hostGameManager = manager;
            NewPeerData = new PeerOnlineInfo();
            foreach (PeerOnlineInfo peerdata in PeersInfoList.ToList())
            {
                if (peerdata.PeerRunner.LocalPlayer == manager.GameRunner.LocalPlayer)
                {
                    //resetting data and readding it to peerdatalist
                    NewPeerData.PeerRunner = peerdata.PeerRunner;
                    //sending peertoken to gamemanger
                    NewPeerData.PeerManager = manager;

                    NewPeerData.PeerID = peerdata.PeerID;
                    NewPeerData.PeerRef = manager.GameRunner.LocalPlayer;
                    PeersInfoList.Remove(peerdata);
                    PeersInfoList.Add(NewPeerData);

                    //setting peer connection token on peer gamemanager
#if Log

                    LogManager.Log($"PeerData Added to list => {NewPeerData}", Color.blue, LogManager.ValueInformationLog);
#endif
                }
            }
            yield return null;
        }

        foreach (PeerOnlineInfo peerdata in PeersInfoList)
        {
            for (int index = 0; index < _dataHolder.RunTimePlayersData.Count; index++)
            {
                if (peerdata.PeerID == _dataHolder.RunTimePlayersData[index].PlayerID)
                {
                    var oldData = _dataHolder.RunTimePlayersData[index];
                    var newData = new RunTimePlayerData();
                    newData.PlayerRef = peerdata.PeerRef;
                    newData.PlayerName = oldData.PlayerName;
                    newData.PlayerID = oldData.PlayerID;
                    newData.IconIndex = oldData.IconIndex;
                    newData.PlayerNetObjectRef = null;
                    newData.AuthorityAssigned = false;
                    _dataHolder.RunTimePlayersData[index] = newData;
                }
            }
        }
    }

    public async void OfflineRunner()
    {
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        gameObject.transform.parent = null;
        DontDestroyOnLoad(this);
        StartGameArgs = new StartGameArgs()
        {
            SessionName = "offlinetestroom",
            GameMode = GameMode.Single,
            Scene = scene
        };
        string hostID = _playersData.First().PlayerID;
        await StartMultiPeerRunner(StartGameArgs, hostID);
    }

    public IEnumerator StartMultipeerGame()
    {
        AllSet = false;
        //moving to dontdestroy on load
        gameObject.transform.parent = null;
        DontDestroyOnLoad(this);
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

        //setting args for host runner
        StartGameArgs = new StartGameArgs()
        {
            SessionName = "OnlineTestRoom",
            GameMode = GameMode.Host,
            Scene = scene,
        };
        string hostID = _playersData.First().PlayerID;
        //starting host runner
#if Log

        LogManager.Log("Starting Host", Color.blue, LogManager.ValueInformationLog);
#endif
        yield return WaitForRunnerTask(StartMultiPeerRunner(StartGameArgs, hostID));
        //starting clients
#if Log

        LogManager.Log("Staring Clients!", Color.blue, LogManager.ValueInformationLog);
#endif
        yield return SetUpClients();
        //just making sure that peers gameManager spawned
        yield return new WaitForSeconds(2);
        // setting up peer data
        yield return SetPeersInfo();
        //waiting for all simulation set up
        yield return new WaitForSeconds(1);
        //starting game
        _hostGameManager.HostStartGame();
        AllSet = true;
        _startOnlineCoroutine = null;
#if Log
        LogManager.Log("Players Runners Prep Is Done!", Color.blue, LogManager.ValueInformationLog);
#endif
    }

    private void StartGame()
    {
        SetUpPlayersData();
        if (_mode == InGameLogicModes.Offline)
            OfflineRunner();
        else
            _startOnlineCoroutine = StartCoroutine(StartMultipeerGame());
    }

    #endregion Start Game SetUp

    #region SinglePeerStart

    private void InitSinglePeerButtons()
    {
        _singlePeerMode.onClick.RemoveAllListeners();
        _singlePeerMode.onClick.AddListener(InitSinglePeerButton);

        _startHostButton.onClick.RemoveAllListeners();
        _startHostButton.onClick.AddListener(StartHost);

        _startClientButton.onClick.RemoveAllListeners();
        _startClientButton.onClick.AddListener(StartClient);

        _hostStartGameButton.onClick.RemoveAllListeners();
        _hostStartGameButton.onClick.AddListener(SinglePeerHostStartGame);
    }
    private void InitSinglePeerButton()
    {
        //disbale main buttons
        _singlePeerMode.gameObject.SetActive(false);
        _multiPeerMode.gameObject.SetActive(false);
        _offlineMode.gameObject.SetActive(false);

        //enable runner starters
        _singlePeerButtonsHolder.gameObject.SetActive(true);
    }

    private async void StartHost()
    {
        //creating runner object
        GameObject runnerobj = Instantiate(_runnerPrefab);
        DontDestroyOnLoad(runnerobj);
        runnerobj.name = "Host Runner";
        NetworkRunner runner = runnerobj.GetComponent<NetworkRunner>();
        //creating runner args
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var StartGameArgs = new StartGameArgs()
        {
            SessionName = "TestRoom",
            GameMode = GameMode.Host,
            Scene = scene
        };
        //starting srunner
        var task = await runner.StartGame(StartGameArgs);
        //if all gucci toggle host start game method and toggle other shit off
        if (task.Ok)
        {
            _singlePeerButtonsHolder.SetActive(false);
            _hostStartGameButton.gameObject.SetActive(true);
            _singlePeerHostRunner = runner;
        }
    }

    private async void StartClient()
    {
        //creating runner object
        GameObject runnerobj = Instantiate(_runnerPrefab);
        DontDestroyOnLoad(runnerobj);
        runnerobj.name = "Client Runner";
        NetworkRunner runner = runnerobj.GetComponent<NetworkRunner>();
        //creating runner args
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var StartGameArgs = new StartGameArgs()
        {
            SessionName = "TestRoom",
            GameMode = GameMode.Client,
            Scene = scene
        };
        //starting srunner
        var task = await runner.StartGame(StartGameArgs);
        //if all gucci toggle host start game method and toggle other shit off
        if (task.Ok)
        {
            gameObject.SetActive(false);
        }
    }
    private void SinglePeerHostStartGame()
    {
        gameObject.SetActive(false);
        SinglePeerPlayerInfoSetUp();
        _gameManager.HostStartGame();
    }
    private void SinglePeerPlayerInfoSetUp()
    {
        _dataHolder.RunTimePlayersData.Clear();
     //setting player shet 
        int playerIndex = 1;
        foreach (var item in _singlePeerHostRunner.ActivePlayers)
        {
            var playerData = new RunTimePlayerData();
            playerData.PlayerRef = item;
            playerData.PlayerName = Player+playerIndex.ToString();
            playerData.PlayerID = Guid.NewGuid().ToString();
            playerIndex++;
            playerData.AuthorityAssigned = false;
            playerData.PlayerNetObjectRef = null;
            playerData.IconIndex = 0;
            _dataHolder.RunTimePlayersData.Add(playerData);
        }
        //setting Deck shet 
        _dataHolder.DeckInfo = new DeckInfo();
        _dataHolder.DeckInfo.DeckType = DeckType.Belote;
        _dataHolder.DeckInfo.SuitsNumber = 4;

    }

   
    #endregion SinglePeerStart
}

public enum InGameLogicModes
{
    Offline,
    SinglePeer,
    Multipeer
}

public struct PeerOnlineInfo
{
    public NetworkRunner PeerRunner;
    public GameManager PeerManager;
    public PlayerRef PeerRef;
    public string PeerID;

    public override string ToString()
    {
        return $"PeerPlayerRef = {PeerRunner.LocalPlayer}";
    }
}