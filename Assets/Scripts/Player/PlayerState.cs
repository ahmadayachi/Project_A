using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : State
{
    private PlayerUIController _uiControler;
    public PlayerState(PlayerUIController uIController)
    {
        _uiControler = uIController;
    }
    public override void Start<T>(T arg)
    {
        if (Extention.TryCastToStruct(arg, out PlayerStateArguments PlayerStateArgs))
        {
            //showing Player UI Commands/panels 
            if (PlayerStateArgs.IsMyTurn)
                UnlockPlayerUI(PlayerStateArgs.GameState);

            //starting timers
            _uiControler.StartTimers();
        }
        else
        {
#if Log
            LogManager.LogError("  wrong Player State args passed !");
#endif
        }
    }
    public override void ForceEnd()
    {
        _uiControler.StopTimers();
        _uiControler.HidePlayerUI();
    }
    private void UnlockPlayerUI(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.FirstPlayerTurn: _uiControler.ShowFirstPlayerUI(); break;
            case GameState.PlayerTurn: _uiControler.ShowNormalPlayerUI(); break;
            case GameState.LastPlayerTurn: _uiControler.ShowLastPlayerUI(); break;
        }
    }

}
