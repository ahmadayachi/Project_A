using Steamworks;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LobbyManager : NetworkBehaviour
{
    public const string IsReady = "IsReady";
    public const string NotReady = "Not Ready";
    public const string Start = "Start";
    public const string Next = "Next";

    [SerializeField]
    private LobbyUIRefs _lobbyUIRefs;
    private NetworkVariable<FixedString64Bytes> LobbyName = new NetworkVariable<FixedString64Bytes>();
    private NetworkVariable<FixedString64Bytes> LobbyID = new NetworkVariable<FixedString64Bytes>();

    private List<NetworkObject> _lobbyPlayersNetObjects = new List<NetworkObject>();
    private LobbyPlayer _localLobbyPlayer;



    private DeckType _selectedDeckType;
    private List<byte> _customDeckCards = new List<byte>();
    private List<byte> _selectedCustomDeckCards = new List<byte>();
    private List<CustomCombinationCard> _customCombinationCards = new List<CustomCombinationCard>();
    private CardSuit _customDeckSuit = CardSuit.Spades;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        LobbyName.OnValueChanged += OnLobbyNameChanged;
        LobbyID.OnValueChanged += OnLobbyIDChanged;

        NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
        if (IsHost)
        {
            //grabing session name 
            LobbyName.Value = AssetLoader.RunTimeDataHolder.LobbySettings.LobbyName;
            //grabing session ID
            if (AuthenticationManager.Instance.SteamAuthentication)
                LobbyID.Value = SteamClient.SteamId.ToString();

            //spawning host lobby player 
            SpawnLobbyPlayer(NetworkManager.Singleton.LocalClientId);

            //setting InGame SetUp UI Logic
            InGameLogicPanelSetUp();
        }
        else
        {
            //sync first tick for client 
            if (AuthenticationManager.Instance.SteamAuthentication)
                _lobbyUIRefs.LobbyID.text = LobbyID.Value.ToString();
            _lobbyUIRefs.LobbyName.text = LobbyName.Value.ToString();
        }
        InitStartButtonUI();
        _lobbyUIRefs.CoppyButton.onClick.AddListener(CopyToClipboard);
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;

        if (_localLobbyPlayer != null)
            _localLobbyPlayer.OnIsReady -= OnIsReadyChangedCallBack;

    }

    #region Logic
    public void SetUpLocalPlayer(LobbyPlayer player)
    {
        _localLobbyPlayer = player;
        _localLobbyPlayer.OnIsReady += OnIsReadyChangedCallBack;

        _lobbyUIRefs.StartButton.onClick.RemoveAllListeners();

        if (IsHost)
            _lobbyUIRefs.StartButton.onClick.AddListener(StartGame);
        else
            _lobbyUIRefs.StartButton.onClick.AddListener(_localLobbyPlayer.IsReadyRpc);
    }
    public void StartGame()
    {
        if (SetUpPlayersData())
        {
            SetUpDeckInfo();
            string sceneName = "InGameScene";
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
#if Log
            LogManager.Log($"[{nameof(LobbyManager)}] - Host is starting a Game!", UnityEngine.Color.green);
#endif
        }
    }
    private void OnIsReadyChangedCallBack(bool isReady)
    {
        if (isReady)
            _lobbyUIRefs.StartButtonText.text = IsReady;
        else
            _lobbyUIRefs.StartButtonText.text = NotReady;
    }
    private bool SetUpPlayersData()
    {
        if (_lobbyPlayersNetObjects.Count < 2)
        {
#if Log
            LogManager.Log($"[{nameof(LobbyManager)}] - Need More Players To start the Game! ", UnityEngine.Color.yellow, LogManager.ValueInformationLog);
#endif
            return false;
        }
        //reseting data 
        AssetLoader.RunTimeDataHolder.ResetRuntimePLayerData();

        foreach (var item in _lobbyPlayersNetObjects)
        {
            var player = item.GetComponent<LobbyPlayer>();

            if (!PlayerIsValid(player))
            {
                return false;
            }


            //player data set up
            var runtimeData = new RunTimePlayerData();
            runtimeData.PlayerName = player.Name.Value.ToString();
            runtimeData.PlayerID = player.ID.Value.ToString();
            runtimeData.ClientID = item.OwnerClientId;
            runtimeData.IconIndex = player.IconID.Value;
            AssetLoader.RunTimeDataHolder.RunTimePlayersData.Add(runtimeData);

        }

        return true;
    }
    private void SetUpDeckInfo()
    {
        //reseting deck info
        AssetLoader.RunTimeDataHolder.ResetDeckInfo();

        //injecting a Standard Deck info 
        DeckInfo deckInfo = new DeckInfo();
        deckInfo.DeckType = DeckType.Belote;
        deckInfo.SuitsNumber = (byte)_lobbyPlayersNetObjects.Count;
        deckInfo.CustomSuitRanks = null;
        AssetLoader.RunTimeDataHolder.DeckInfo = deckInfo;
    }
    private bool PlayerIsValid(LobbyPlayer player)
    {
        if (player == null)
        {
#if Log
            LogManager.LogError($"[{nameof(LobbyManager)}] - Failed Setting up Player Data!,Player object does not have a LobbyPlayer Component!");
#endif
            return false;
        }

        if (player.IsReady.Value != true)
        {
#if Log
            LogManager.Log($"[{nameof(LobbyManager)}] - Player=>{player} is not ready!", UnityEngine.Color.yellow, LogManager.ValueInformationLog);
#endif
            return false;
        }

        if (player.IconID.Value == 0)
        {
#if Log
            LogManager.LogError($"[{nameof(LobbyManager)}] -  Failed Setting up Player Data!,Lobby Player Icon ID is 0!Player=>{player}");
#endif
            return false;
        }

        if (player.Name.Value.Equals(default(FixedString32Bytes)))
        {
#if Log
            LogManager.LogError($"[{nameof(LobbyManager)}] -  Player=>{player} Failed Setting up Player Data!,Lobby Player Name is empty! Player=>{player}");
#endif
            return false;
        }

        if (player.ID.Value.Equals(default(FixedString64Bytes)))
        {
#if Log
            LogManager.LogError($"[{nameof(LobbyManager)}] -  Player=>{player} Failed Setting up Player Data!,Lobby Player ID is empty!");
#endif
            return false;
        }
        return true;
    }
    private void SpawnLobbyPlayer(ulong ClientID)
    {
        var lobbbyPlayerGO = Instantiate(AssetLoader.PrefabContainer.LobbyPlayerPrefab);
        var lobbyPlayerNetObject = lobbbyPlayerGO.GetComponent<NetworkObject>();
        lobbyPlayerNetObject.SpawnAsPlayerObject(ClientID, true);
        lobbyPlayerNetObject.transform.SetParent(_lobbyUIRefs.LobbyPlayersHolder, false);
        _lobbyPlayersNetObjects.Add(lobbyPlayerNetObject);
    }
    private void DespawnLobbyPlayer(ulong ClientID)
    {
        var lobbyPlayer = _lobbyPlayersNetObjects.Find(x => x.OwnerClientId == ClientID);
        lobbyPlayer.Despawn();
    }
    #endregion

    #region Lobby UI  
    public void InitStartButtonUI()
    {
        if (IsHost)
            _lobbyUIRefs.StartButtonText.text = Start;
        else
            _lobbyUIRefs.StartButtonText.text = NotReady;
    }
    public void SetUpClientIsReadyButton(UnityAction rpc)
    {
        _lobbyUIRefs.StartButton.onClick.RemoveAllListeners();
        _lobbyUIRefs.StartButton.onClick.AddListener(rpc);
    }
    private void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = _lobbyUIRefs.LobbyID.text;
    }
    #endregion

    #region netcode Call Backs
    private void OnConnectionEvent(NetworkManager arg1, ConnectionEventData arg2)
    {
#if Log
        LogManager.Log($"[{nameof(LobbyManager)}] - is a Host=> {arg1.IsHost}!", UnityEngine.Color.green);
        LogManager.Log($"[{nameof(LobbyManager)}] - Event Type: {arg2.EventType}", UnityEngine.Color.green);
        LogManager.Log($"[{nameof(LobbyManager)}] - Client ID: {arg2.ClientId}", UnityEngine.Color.green);

        if (arg2.PeerClientIds.IsCreated && arg2.PeerClientIds.Length > 0)
        {
            string peerClientIds = string.Join(", ", arg2.PeerClientIds);
            LogManager.Log($"[{nameof(LobbyManager)}] - Peer Client IDs: {peerClientIds}", UnityEngine.Color.green);
        }
#endif

        if (!IsHost) return;
        switch (arg2.EventType)
        {
            case ConnectionEvent.ClientConnected: SpawnLobbyPlayer(arg2.ClientId); break;
            case ConnectionEvent.ClientDisconnected: DespawnLobbyPlayer(arg2.ClientId); break;
        }
    }
    private void OnLobbyNameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        _lobbyUIRefs.LobbyName.text = newValue.ToString();
    }
    private void OnLobbyIDChanged(FixedString64Bytes oldID, FixedString64Bytes newID)
    {
        _lobbyUIRefs.LobbyID.text = newID.ToString();
    }
    #endregion

    #region InGame SetUp UI Logic

    public void ToggleInGameSetUpUIPanel(bool Toggle)
    {
        _lobbyUIRefs.InGameSetUpUIRefs.InGameSetUpPanel.SetActive(Toggle);
    }

    private void SuitNumber()
    {
        string CurrentValue = _lobbyUIRefs.InGameSetUpUIRefs.DeckSuitsNumberText.text;
        _lobbyUIRefs.InGameSetUpUIRefs.DeckSuitsNumberText.text = NumberBumber(CurrentValue);
        CalculateMaxCardsInHand();
    }
    private string NumberBumber(string numberText)
    {
        int number = int.Parse(numberText);
        return (number == 8 ? 1 : ++number).ToString();
    }
    private void CalculateMaxCardsInHand()
    {
        int playersNumber = _lobbyPlayersNetObjects.Count;
        int totalSuitsNumber = int.Parse(_lobbyUIRefs.InGameSetUpUIRefs.DeckSuitsNumberText.text);
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
        _lobbyUIRefs.InGameSetUpUIRefs.MixMaxCardsInHandsText.text = mixMaxCardsInHand.ToString();
        _lobbyUIRefs.InGameSetUpUIRefs.MaxCardsInHandSlider.maxValue = mixMaxCardsInHand;
        _lobbyUIRefs.InGameSetUpUIRefs.MaxCardsInHandSlider.wholeNumbers = true;
    }


    private void MaxCardsInHandSlider(float value)
    {
        _lobbyUIRefs.InGameSetUpUIRefs.MaxCardsInHandsText.text = ((int)value).ToString();
    }
    private void Belote(bool isOn)
    {
        if (isOn)
        {
            _selectedDeckType = DeckType.Belote;
            _lobbyUIRefs.InGameSetUpUIRefs.StandartDeckTypeToggle.SetIsOnWithoutNotify(false);
            _lobbyUIRefs.InGameSetUpUIRefs.CustomDeckTypeToggle.SetIsOnWithoutNotify(false);
            _lobbyUIRefs.InGameSetUpUIRefs.StartButtonText.text = Start;
        }
    }

    private void Standart(bool isOn)
    {
        if (isOn)
        {
            _selectedDeckType = DeckType.Standard;
            _lobbyUIRefs.InGameSetUpUIRefs.BeloteDeckTypeToglle.SetIsOnWithoutNotify(false);
            _lobbyUIRefs.InGameSetUpUIRefs.CustomDeckTypeToggle.SetIsOnWithoutNotify(false);
            _lobbyUIRefs.InGameSetUpUIRefs.StartButtonText.text = Start;
        }
    }

    private void Custom(bool isOn)
    {
        if (isOn)
        {
            _selectedDeckType = DeckType.Custom;
            _lobbyUIRefs.InGameSetUpUIRefs.BeloteDeckTypeToglle.SetIsOnWithoutNotify(false);
            _lobbyUIRefs.InGameSetUpUIRefs.StandartDeckTypeToggle.SetIsOnWithoutNotify(false);
            _lobbyUIRefs.InGameSetUpUIRefs.StartButtonText.text = Next;
        }
    }
    private void ResetCustomComnbination()
    {
        _selectedCustomDeckCards.Clear();
        _customDeckCards.Clear();
        foreach (var item in _customCombinationCards)
        {
            item.CardButton.onClick.RemoveAllListeners();
            item.CardButton.onClick.AddListener(() => { OnCustomCardClicked(item); });
            item.gameObject.transform.SetParent(_lobbyUIRefs.InGameSetUpUIRefs.FirstCombinationHolder);
            _customDeckCards.Add(item.CardRank);
        }
        _lobbyUIRefs.InGameSetUpUIRefs.CustomCombinationIndicator.sprite = _lobbyUIRefs.InGameSetUpUIRefs.RedIndicator;
    }
    private void OnCustomCardClicked(CustomCombinationCard card)
    {
        if (_customDeckCards.Contains(card.CardRank))
        {
            _selectedCustomDeckCards.Add(card.CardRank);
            _customDeckCards.Remove(card.CardRank);
            card.gameObject.transform.SetParent(_lobbyUIRefs.InGameSetUpUIRefs.FinalCombinationHolder);
        }
        else
        {
            _selectedCustomDeckCards.Remove(card.CardRank);
            _customDeckCards.Add(card.CardRank);
            card.gameObject.transform.SetParent(_lobbyUIRefs.InGameSetUpUIRefs.FirstCombinationHolder);
        }
        _lobbyUIRefs.InGameSetUpUIRefs.CustomCombinationIndicator.sprite = _selectedCustomDeckCards.Count >= 8 ? _lobbyUIRefs.InGameSetUpUIRefs.GreenIndicator : _lobbyUIRefs.InGameSetUpUIRefs.RedIndicator;
    }
    private void ConfirmCustomCombination()
    {
        if (_selectedCustomDeckCards.Count < 8) return;
        //maybe a popup letting em know we starting the game 
        StartGame();
    }
    private void InitCustomCombinationCards()
    {
        _customDeckCards.Clear();
        _selectedCustomDeckCards.Clear();
        _customCombinationCards.Clear();
        for (int index = 1; index <= 13; index++)
        {
            CustomCombinationCard customCard = Instantiate(_lobbyUIRefs.InGameSetUpUIRefs.CustomDeckCardPrefab, _lobbyUIRefs.InGameSetUpUIRefs.FirstCombinationHolder);
            customCard.CardRank = (byte)index;
            customCard.CardImage.sprite = AssetLoader.DeckContainerInstance.GetSuitSprite(customCard.CardRank, _customDeckSuit);
            customCard.CardButton.onClick.RemoveAllListeners();
            customCard.CardButton.onClick.AddListener(() => OnCustomCardClicked(customCard));
            _customCombinationCards.Add(customCard);
            _customDeckCards.Add(customCard.CardRank);
        }
        _lobbyUIRefs.InGameSetUpUIRefs.CustomCombinationIndicator.sprite = _lobbyUIRefs.InGameSetUpUIRefs.RedIndicator;
    }
    private void InitCustomCombinationPanel()
    {
        _lobbyUIRefs.InGameSetUpUIRefs.ConfirmButton.onClick.RemoveAllListeners();
        _lobbyUIRefs.InGameSetUpUIRefs.ConfirmButton.onClick.AddListener(ConfirmCustomCombination);
        _lobbyUIRefs.InGameSetUpUIRefs.ResetButton.onClick.RemoveAllListeners();
        _lobbyUIRefs.InGameSetUpUIRefs.ResetButton.onClick.AddListener(ResetCustomComnbination);
        InitCustomCombinationCards();
    }
    private void NextButton()
    {
        if (_selectedDeckType == DeckType.Custom)
        {
            _lobbyUIRefs.InGameSetUpUIRefs.FazeOneOptions.SetActive(false);
            _lobbyUIRefs.InGameSetUpUIRefs.CustomCombinationPanel.SetActive(true);
        }
        else
            StartGame();
    }
    public void CloseButton()
    {
        if (_selectedDeckType == DeckType.Custom && _lobbyUIRefs.InGameSetUpUIRefs.CustomCombinationPanel.activeSelf)
        {
            ResetCustomComnbination();
            _lobbyUIRefs.InGameSetUpUIRefs.CustomCombinationPanel.SetActive(false);
            _lobbyUIRefs.InGameSetUpUIRefs.FazeOneOptions.SetActive(true);
        }
        else
        {
            _selectedDeckType = DeckType.Standard;
            _lobbyUIRefs.InGameSetUpUIRefs.InGameSetUpPanel.SetActive(false);
        }
    }
    private void InGameLogicPanelSetUp()
    {
        _lobbyUIRefs.InGameSetUpUIRefs.InGameSetUpPanel.SetActive(false);
        _lobbyUIRefs.InGameSetUpUIRefs.FazeOneOptions.SetActive(true);
        _lobbyUIRefs.InGameSetUpUIRefs.CustomCombinationPanel.SetActive(false);

        _lobbyUIRefs.InGameSetUpUIRefs.DeckSuitsNumberText.text = 2.ToString();
        _lobbyUIRefs.InGameSetUpUIRefs.DeckSuitsNumberButton.onClick.AddListener(SuitNumber);

        _selectedDeckType = DeckType.Belote;
        _lobbyUIRefs.InGameSetUpUIRefs.BeloteDeckTypeToglle.isOn = true;
        _lobbyUIRefs.InGameSetUpUIRefs.StandartDeckTypeToggle.isOn = false;
        _lobbyUIRefs.InGameSetUpUIRefs.CustomDeckTypeToggle.isOn = false;

        _lobbyUIRefs.InGameSetUpUIRefs.BeloteDeckTypeToglle.onValueChanged.AddListener(Belote);
        _lobbyUIRefs.InGameSetUpUIRefs.StandartDeckTypeToggle.onValueChanged.AddListener(Standart);
        _lobbyUIRefs.InGameSetUpUIRefs.CustomDeckTypeToggle.onValueChanged.AddListener(Custom);

        _lobbyUIRefs.InGameSetUpUIRefs.MaxCardsInHandSlider.minValue = 2;
        _lobbyUIRefs.InGameSetUpUIRefs.MaxCardsInHandSlider.wholeNumbers = true;
        _lobbyUIRefs.InGameSetUpUIRefs.MaxCardsInHandSlider.onValueChanged.RemoveAllListeners();
        _lobbyUIRefs.InGameSetUpUIRefs.MaxCardsInHandSlider.onValueChanged.AddListener(MaxCardsInHandSlider);
        CalculateMaxCardsInHand();

        InitCustomCombinationPanel();

        _lobbyUIRefs.InGameSetUpUIRefs.StartGameButton.onClick.RemoveAllListeners();
        _lobbyUIRefs.InGameSetUpUIRefs.StartGameButton.onClick.AddListener(NextButton);

        _lobbyUIRefs.InGameSetUpUIRefs.CloseButton.onClick.RemoveAllListeners();
        _lobbyUIRefs.InGameSetUpUIRefs.CloseButton.onClick.AddListener(CloseButton);
    }



    #endregion

}

[System.Serializable]
public struct LobbyUIRefs
{
    public TextMeshProUGUI LobbyName;
    public TextMeshProUGUI LobbyID;
    public Button CoppyButton;
    public TextMeshProUGUI StartButtonText;
    public Button StartButton;
    public Transform LobbyPlayersHolder;
    public InGameSetUpUIRefs InGameSetUpUIRefs;
}
[System.Serializable]
public struct InGameSetUpUIRefs
{
    public GameObject InGameSetUpPanel;
    public GameObject FazeOneOptions;
    // First Faze Panel 
    public Button CloseButton;
    public TextMeshProUGUI DeckSuitsNumberText;
    public Button DeckSuitsNumberButton;
    public Toggle StandartDeckTypeToggle;
    public Toggle CustomDeckTypeToggle;
    public Toggle BeloteDeckTypeToglle;
    public TextMeshProUGUI MixMaxCardsInHandsText;
    public TextMeshProUGUI MaxCardsInHandsText;
    public Slider MaxCardsInHandSlider;
    public Button StartGameButton;
    public TextMeshProUGUI StartButtonText;
    // Second Faze Panel
    public GameObject CustomCombinationPanel;
    public Transform FirstCombinationHolder;
    public Transform FinalCombinationHolder;
    public CustomCombinationCard CustomDeckCardPrefab;
    public Image CustomCombinationIndicator;
    public Sprite GreenIndicator;
    public Sprite RedIndicator;
    public Button ResetButton;
    public Button ConfirmButton;
}
