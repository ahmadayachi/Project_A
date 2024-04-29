using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : State
{
    private PlayerUIController _uiControler;
    public PlayerBehaviour(PlayerUIController uIController)
    {
        _uiControler = uIController;
    }
    public override void Start<T>(T arg)
    {
        if (Extention.TryCastToStruct(arg, out PlayerStateArguments PlayerStateArgs))
        {
            //starting timer
            _uiControler.StartTimer();
            //showing Player UI Commands 
            UnlockPlayerUI(PlayerStateArgs.GameState);
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
        _uiControler.StopTimer();
        _uiControler.HidePlayerUICommands();
    }
    private void UnlockPlayerUI(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.FirstPlayerTurn: _uiControler.ShowFirstPlayerUI(); break;
            case GameState.PlayerTurn: _uiControler.ShowNormalPlayerUICommands(); break;
            case GameState.LastPlayerTrun: _uiControler.ShowLastPlayerUI(); break;
        }
    }

}
