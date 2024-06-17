using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlasticGui.PlasticTableColumn;

public class UIEvents : IUIEvents
{
    private UIManager _uiManager;
    public UIEvents (UIManager manager)
    {
        _uiManager = manager;
    }
    public void OnDealingCards()
    {
        //throw new System.NotImplementedException();
    }

    public void OnDoubting()
    {
        //throw new System.NotImplementedException();
    }

    public void OnGameOver()
    {
        //throw new System.NotImplementedException();
    }

    public void OnGameStarted()
    {
        //throw new System.NotImplementedException();
    }

    public void OnHostMigration()
    {
        //throw new System.NotImplementedException();
    }

    public void OnRoundOver()
    {
        //throw new System.NotImplementedException();
    }

    public void OnSetUpStarted()
    {
        //throw new System.NotImplementedException();
    }

    public IEnumerator SetUpUI()
    {
        //throw new System.NotImplementedException();
        yield return null;
        _uiManager.GameManagerUI.SimulationState = SimulationSetUpState.UISetUp;
#if Log
        LogManager.Log($" UI is Set Up Runner Player Ref => {_uiManager.GameManagerUI.Runner.LocalPlayer}", Color.green, LogManager.ValueInformationLog);
#endif
    }

}
