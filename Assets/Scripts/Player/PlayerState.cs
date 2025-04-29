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
                UnlockPlayerUI(PlayerStateArgs.TurnState);

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
    private void UnlockPlayerUI(PlayerTurnStates TurnState)
    {
        switch (TurnState)
        {
            case PlayerTurnStates.FirstPlayerTurn: _uiControler.ShowFirstPlayerUI(); break;
            case PlayerTurnStates.PlayerTurn: _uiControler.ShowNormalPlayerUI(); break;
            case PlayerTurnStates.LastPlayerTurn: _uiControler.ShowLastPlayerUI(); break;
        }
    }

}
