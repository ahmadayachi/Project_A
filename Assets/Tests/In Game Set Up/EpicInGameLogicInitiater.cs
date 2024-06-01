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
    [SerializeField] private Button _confirm;
    [SerializeField] private CustomCombinationCard _CustomDeckCardPrefab;
    List<CustomCombinationCard> _customCombinationCards = new List<CustomCombinationCard>();
    List<byte> _customDeckCards = new List<byte>();
    List<byte> _selectedCustomDeckCards = new List<byte>();
    CardSuit _customDeckSuit = CardSuit.Spades;
    #endregion
    #region Players Info Panel
    [Header("Players Panel Refs")]
    [SerializeField] private GameObject _playersPanel;
    #endregion

    [Header("Panel Indipendant Shit")]
    [SerializeField] private Button _StartGame;



    private void Awake()
    {
        InitFirstPanelButtons();

        InitGameLogicFirstPhaze();

        InitGameLogicSecondPhaze();
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
    private void Belote()
    {
        if (_belote.isOn)
        {
            _selectedDeckType = DeckType.Belote;
            _standart.SetIsOnWithoutNotify(false);
            _custom.SetIsOnWithoutNotify(false);
        }
    }
    private void Standart()
    {
        if (_standart.isOn)
        {

            _selectedDeckType = DeckType.Standard;
            _belote.SetIsOnWithoutNotify(false);
            _custom.SetIsOnWithoutNotify(false);
        }
    }
    private void Custom()
    {
        if (_custom.isOn)
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
        CustomCombinationCard CustomCard;

        _customDeckCards.Clear();
        _customCombinationCards.Clear();
        for(int index =1;index<=13;index++)
        {
            CustomCard = Instantiate(_CustomDeckCardPrefab);
            CustomCard.CardRank = (byte)index;
            CustomCard.CardImage.sprite = AssetLoader.DeckContainerInstance.GetSuitSprite(CustomCard.CardRank, _customDeckSuit);
            _customCombinationCards.Add(CustomCard);
            _customDeckCards.Add(CustomCard.CardRank);
        }
    }
    private void OnCustomCardClicked(CustomCombinationCard card)
    {
        if(_customDeckCards.Contains(card.CardRank))
        {
            _customDeckCards.Remove(card.CardRank);

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