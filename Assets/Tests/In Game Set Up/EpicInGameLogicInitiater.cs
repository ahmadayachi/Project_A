using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    #endregion

    [Header("Panel Indipendant Shit")]
    [SerializeField] private Button _StartGame;
    [SerializeField] private RunTimeDataHolder _dataHolder;


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
        for(int index=0; index<playersNumber; index++)
        {
            var player = Instantiate(_inGameSetUpPlayerPrefab,_playersPanel.transform);
            player.PlayerIcon.sprite = AssetLoader.AllIcons[Random.Range(0, AssetLoader.AllIcons.Count)];
            _playersUI.Add(player);
        }
    }
    #endregion
}
public enum InGameLogicModes
{
    Offline,
    Online,
    Multipeer
}