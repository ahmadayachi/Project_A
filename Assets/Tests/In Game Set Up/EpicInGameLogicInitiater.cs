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
    #region First Panel Refs 
    [Header("Game Logic Mod Panel Refs")]
    [SerializeField] private GameObject _gameLogicModPanel;
    [SerializeField] private Button _playerNumber;
    [SerializeField] private TextMeshProUGUI _playerNumberText;
    [SerializeField] private TMP_Dropdown _deckDropThese;
    [SerializeField] private Button _totalSuitsNumber;
    [SerializeField] private TextMeshProUGUI _totalSuitsNumberText;
    [SerializeField] private Slider _maxCardsInHand;
    [SerializeField] private TextMeshProUGUI _mixMaxCardsInHandsText;
    [SerializeField] private TextMeshProUGUI _maxCardsInHandText;
    [SerializeField] private Button _next;
    #endregion
    #region Custom Deck Panel Refs
    [Header("Custom Deck Mod Panel Refs")]
    [SerializeField] private GameObject _CustomDeckModPanel;
    [SerializeField] private Transform _scrollCardsHolder;
    [SerializeField] private Transform _finalCardsCombinationHolder;
    [SerializeField] private Button _confirm;
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
    #region Game Logic Panel Methods
    private int NumberBumber(string numberText)
    {
        int number = int.Parse(numberText);
        return number == 8? 1 : ++number;
    }
    #endregion
}
public enum InGameLogicModes
{
    Offline,
    Online,
    Multipeer
}