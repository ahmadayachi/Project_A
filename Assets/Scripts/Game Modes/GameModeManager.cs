using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeManager 
{
    private GameMode _gameMode;
    private IGameModeBehaviour _activeGameModeBehaviour;
    public GameModeManager(GameMode gameMode)
    {
        _gameMode = gameMode;
        if (gameMode == GameMode.Single)
            _activeGameModeBehaviour = new OfflineMode();
        else
            _activeGameModeBehaviour = new OnlineMode();
    }
    
}
