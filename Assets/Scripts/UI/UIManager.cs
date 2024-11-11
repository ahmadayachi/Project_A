using Fusion;
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

    private UIEvents _uiEvents;
    public UIEvents ActiveUIEvents {get => _uiEvents;}

    /// <summary>
    /// All UI Refs/UI Panels that Concerns the PlayerTurn,Doubt,GoverOver Game States 
    /// </summary>
    [SerializeField] private PlayerTurnUI _playerTurnUI;
    public PlayerTurnUI PlayerTurnUI { get => _playerTurnUI; }
    public void Init( GameMode mode)
    {
        var args = new UIEventsArgs();
        args.GameMode = mode;
        args.UIManager = this;
        _uiEvents = new UIEvents(args);
    }

    public void InjectGameManager(GameManager gameManager) => _gameManager = gameManager;
}