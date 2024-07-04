using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEvents : IUIEvents
{
    private UIManager _uiManager;
    private Coroutine _gameStartAnimationRoutine;
    public UIEvents (UIManager manager)
    {
        _uiManager = manager;
    }
    #region UIEvent
    public IEnumerator SetUpUI()
    {
        //throw new System.NotImplementedException();
        yield return null;
        _uiManager.GameManagerUI.SimulationState = SimulationSetUpState.UISetUp;
#if Log
        LogManager.Log($" UI is Set Up Runner Player Ref => {_uiManager.GameManagerUI.Runner.LocalPlayer}", Color.green, LogManager.ValueInformationLog);
#endif
    }

    public void OnGameStarted()
    {
        if (_gameStartAnimationRoutine != null)
            _uiManager.StopCoroutine(_gameStartAnimationRoutine);
        _gameStartAnimationRoutine = _uiManager.StartCoroutine(GameStartAnimation());
    }
    private IEnumerator GameStartAnimation()
    {
        yield  return null;
        //informing server player animation finished 
        _uiManager.GameManagerUI.PlayerIsReady();
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

    #endregion

}
