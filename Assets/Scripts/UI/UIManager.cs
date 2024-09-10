using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PlayerUIPlacementSceneRefs _playerUIPlacementSceneRefs;
    public PlayerUIPlacementSceneRefs PlayerUIPlacementSceneRefs { get => _playerUIPlacementSceneRefs; }
    [SerializeField] public Transform CardsHolder;
    private GameManager _gameManager;
    //later itll be an Interface
    public GameManager GameManagerUI { get => _gameManager; }

    private IUIEvents _uiEvents;
    public IUIEvents UIEvents {get => _uiEvents;}

    [SerializeField] private PlayerTurnUI _playerTurnUI;
    public PlayerTurnUI PlayerTurnUI { get => _playerTurnUI; }
    public void Init()
    {
        _uiEvents = new UIEvents(this);
    }

    public void InjectGameManager(GameManager gameManager) => _gameManager = gameManager;
}