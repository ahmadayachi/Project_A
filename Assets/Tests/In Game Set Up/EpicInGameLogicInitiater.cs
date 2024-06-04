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
    [SerializeField] private Button _onlineMode;
    private InGameLogicModes _mode;
    #endregion
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
    [SerializeField] private Slider _maxCardsInHand;
    [SerializeField] private TextMeshProUGUI _mixMaxCardsInHandsText;
    [SerializeField] private TextMeshProUGUI _maxCardsInHandText;
    [SerializeField] private Button _nextTwo;
    private const int MinCardsInHand = 2;
    private int _mixMaxCardsInHand;
    private int _maxCardsInHands;
    #endregion
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
    List<CustomCombinationCard> _customCombinationCards = new List<CustomCombinationCard>();
    List<byte> _customDeckCards = new List<byte>();
    List<byte> _selectedCustomDeckCards = new List<byte>();
    CardSuit _customDeckSuit = CardSuit.Spades;
    #endregion
    #region Players Info Panel
    [Header("Players Panel Refs")]
    [SerializeField] private GameObject _playersPanel;
    [SerializeField] private RunTimePlayerUI _inGameSetUpPlayerPrefab;
    private List<RunTimePlayerUI> _playersUI = new List<RunTimePlayerUI>();
    private List<RunTimePlayerData> _playersData = new List<RunTimePlayerData>();
    private const string Player = "Player";
    #endregion

    [Header("Panel Indipendant Shit")]
    [SerializeField] private Button _StartGame;
    [SerializeField] private RunTimeDataHolder _dataHolder;
    private bool _generateID;
    private GameObject _runnerPrefab;
    private GameObject RunnerPrefab;
    private StartGameArgs StartGameArgs;
    private Coroutine StartOnlineCoroutine;
    private List<GameManager> GameManagerlist = new List<GameManager>();
    private List<Task> RunnerTasksList = new List<Task>();
    public bool AllSet { get; set; }
    public List<PeerOnlineInfo> PeersInfoList = new List<PeerOnlineInfo>();
    private PeerOnlineInfo NewPeerData;
    private void Awake()
    {
        InitFirstPanelButtons();

        InitGameLogicFirstPhaze();

        InitGameLogicSecondPhaze();

        InitCustomCombinationPanel();
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
        _mode = InGameLogicModes.Online;
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
        _StartGame.gameObject.SetActive(false);

        _offlineMode.onClick.RemoveAllListeners();
        _offlineMode.onClick.AddListener(OfflineButton);
        //_onlineMode.onClick.RemoveAllListeners();
        //_onlineMode.onClick.AddListener(OnlineButton);
        _multiPeerMode.onClick.RemoveAllListeners();
        _multiPeerMode.onClick.AddListener(MultiPeerButton);
    }
    #endregion

    #region Game Logic Panel First phaze Methods
    private string NumberBumber(string numberText)
    {
        int number = int.Parse(numberText);
        return (number == 8? 1 : ++number).ToString();
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
    #endregion

    #region  Game Logic Panel Second phaze Methods
    private void CalculateMaxCardsInHand()
    {
        int playersNumber =int.Parse(_playerNumberText.text);
        int totalSuitsNumber = int.Parse(_totalSuitsNumberText.text);
        int CardsInSuitNumber = 0;
        switch (_selectedDeckType)
        {
            case DeckType.Standard: CardsInSuitNumber = 13;break;
            case DeckType.Belote: CardsInSuitNumber = 8;break;
            case DeckType.Custom: CardsInSuitNumber = _customDeckCards.Count;break;
        }
        int DeckCount = CardsInSuitNumber * totalSuitsNumber;
        int playerCards = 1;
        while ((DeckCount - (playerCards * playersNumber) > 0))
        {
            playerCards++;
        }
        _mixMaxCardsInHand = playerCards - 1;
        _mixMaxCardsInHandsText.text = _mixMaxCardsInHand.ToString();
    }
    private void SuitNumber()
    {
        _totalSuitsNumberText.text = NumberBumber(_playerNumberText.text);
        CalculateMaxCardsInHand();
    }
    private void MaxCardsInHandSlider(float value)
    {
        _maxCardsInHands =(int)value;
        _maxCardsInHandText.text = _maxCardsInHand.ToString();
    }
    private void SecondPhazeNext()
    {
        _gameLogicModPanelSecondPhaze.SetActive(false);
        _playersPanel.SetActive(true);
        InitPlayersUI();
    }
    private void InitGameLogicSecondPhaze()
    {
        _totalSuitsNumberText.text = 2.ToString();
        CalculateMaxCardsInHand();
        _totalSuitNumber.onClick.RemoveAllListeners();
        _totalSuitNumber.onClick.AddListener(SuitNumber);
        _maxCardsInHand.minValue = MinCardsInHand;
        _maxCardsInHand.maxValue = _mixMaxCardsInHand;
        _maxCardsInHand.wholeNumbers = true;
        _maxCardsInHand.onValueChanged.RemoveAllListeners();
        _maxCardsInHand.onValueChanged.AddListener(MaxCardsInHandSlider);
        _nextTwo.onClick.RemoveAllListeners();
        _nextTwo.onClick.AddListener(SecondPhazeNext);
    }
    #endregion

    #region Custom Deck Mod Methods
    private void InitCustomCombinationCards()
    {
        _customDeckCards.Clear();
        _selectedCustomDeckCards.Clear();
        _customCombinationCards.Clear();
        for (int index = 1; index <= 13; index++)
        {
            CustomCombinationCard customCard = Instantiate(_CustomDeckCardPrefab,_scrollCardsHolder);
            customCard.CardRank = (byte)index;
            customCard.CardImage.sprite = AssetLoader.DeckContainerInstance.GetSuitSprite(customCard.CardRank, _customDeckSuit);
            customCard.CardButton.onClick.RemoveAllListeners();
            customCard.CardButton.onClick.AddListener(() =>OnCustomCardClicked(customCard));
            _customCombinationCards.Add(customCard);
            _customDeckCards.Add(customCard.CardRank);
        }
        _customCombinationIndicator.sprite = _redIndicator;
    }
    private void OnCustomCardClicked(CustomCombinationCard card)
    {
        if(_customDeckCards.Contains(card.CardRank))
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
        _playersPanel.SetActive(true);
    }
    private void InitCustomCombinationPanel()
    {
        _confirmCustomCombination.onClick.RemoveAllListeners();
        _confirmCustomCombination.onClick.AddListener(ConfirmCustomCombination);
        _resetCustomCombination.onClick.RemoveAllListeners();
        _resetCustomCombination.onClick.AddListener(ResetCustomComnbination);
        InitCustomCombinationCards();
    }
    #endregion

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
            RunTimePlayerData playerData = new RunTimePlayerData(playerName, GenerateUniqueID(), playerUi.IconIndex);
            playerIndex++;
            _playersData.Add(playerData);
        }
    }
    #endregion

    #region Start Game SetUp
    private void SingletonRunner()
    {
        NetworkRunner existingrunner = FindObjectOfType<NetworkRunner>();
        if (existingrunner != null)
            Destroy(gameObject);
        _runnerPrefab = AssetLoader.PrefabContainer.RunnerPrefab;
    }
    private Task StartRunner(StartGameArgs startarg)
    {
        GameObject runnerobj = Instantiate(RunnerPrefab);
        DontDestroyOnLoad(runnerobj);
        runnerobj.name = startarg.GameMode.ToString() + " Runner";
        NetworkRunner runner = runnerobj.GetComponent<NetworkRunner>();

        if (startarg.GameMode != GameMode.Single)
        {
            Debug.Log($"Starting {runnerobj.name}!");
            PeerOnlineInfo peerOnlineInfo = new PeerOnlineInfo();
            peerOnlineInfo.PeerRunner = runner;
            PeersInfoList.Add(peerOnlineInfo);
        }
        return runner.StartGame(startarg);
    }
    public async void OfflineRunner()
    {
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        StartGameArgs = new StartGameArgs()
        {
            SessionName = "offlinetestroom",
            GameMode = GameMode.Single,
            Scene = scene
        };
        await StartRunner(StartGameArgs);
    }
    private IEnumerator WaitForTask(Task task)
    {
        RunnerTasksList.Add(task);

        while (task.IsCompleted == false)
        {
            yield return new WaitForSeconds(1);
        }
        if (task.IsFaulted)
        {
            Debug.LogWarning(task.Exception);
            StopCoroutine(StartOnlineCoroutine);
            yield break;
        }
    }
    private IEnumerator SetUpClients()
    {
        StartGameArgs.GameMode = GameMode.Client;

        for (int i = 0; i < _playersData.Count-1; i++)
        {
            yield return WaitForTask(StartRunner(StartGameArgs));
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
            StopCoroutine(StartOnlineCoroutine);
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
            NewPeerData = new PeerOnlineInfo();
            foreach (PeerOnlineInfo peerdata in PeersInfoList.ToList())
            {
                if (peerdata.PeerRunner.LocalPlayer == manager.GameRunner.LocalPlayer)
                {
                    //resetting data and readding it to peerdatalist
                    NewPeerData.PeerRunner = peerdata.PeerRunner;
                    //sending peertoken to gamemanger
                    NewPeerData.PeerManager = manager;
                    PeersInfoList.Remove(peerdata);
                    PeersInfoList.Add(NewPeerData);
                    //setting peer connection token on peer gamemanager
                    Debug.Log($"PeerData Added to list => {NewPeerData}");
                }
            }
            yield return null;
        }
    }
    public IEnumerator StartOnline()
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
        //starting host runner 
        Debug.Log("Starting Host");
        yield return WaitForTask(StartRunner(StartGameArgs));
        //starting clients
        Debug.Log("Staring Clients!");
        yield return SetUpClients();
        //just making sure that peers gameManager spawned
        yield return new WaitForSeconds(1);
        // setting up peer data 
        yield return SetPeersInfo();
        AllSet = true;
    }
    #endregion
}
public enum InGameLogicModes
{
    Offline,
    Online,
    Multipeer
}
public struct PeerOnlineInfo
{
    public NetworkRunner PeerRunner;
    public GameManager PeerManager;
    public override string ToString()
    {
        return $"PeerPlayerRef = {PeerRunner.LocalPlayer}";
    }
}