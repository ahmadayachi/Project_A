using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeManager 
{
    //private GameMode _gameMode;
    private IGameMode _activeGameModeBehaviour;
    public GameModeManager(GameModeARGS args)
    {
        //_gameMode = gameMode;
        if (args.GameMode == GameMode.Single)
            _activeGameModeBehaviour = new OfflineMode(args);
        else
            _activeGameModeBehaviour = new OnlineMode(args);
    }
    
    public void ConfirmBet(byte[] bet, string playerID)
    {
        _activeGameModeBehaviour.ConfirmBet(bet, playerID);
    }

    public void DoubtBet(string playerID)
    {
        _activeGameModeBehaviour.DoubtBet(playerID);
    }

    public void StartGame()
    {
        _activeGameModeBehaviour.StartGame();
    }
   
}

