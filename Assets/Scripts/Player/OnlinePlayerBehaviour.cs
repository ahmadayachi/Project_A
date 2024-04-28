using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlinePlayerBehaviour : State
{
    private PlayerUIController _uiControler;
    public OnlinePlayerBehaviour(PlayerUIController uIController)
    {
        _uiControler = uIController;
    }
    public override void Start<T>(T arg)
    {
        //dont see any use for arguments right now 

        //starting timer
        _uiControler.StartTimer();
    }
    public override void ForceEnd()
    {
        _uiControler.StopTimer();
    }

}
