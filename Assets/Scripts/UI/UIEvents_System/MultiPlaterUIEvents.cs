using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiPlaterUIEvents : UIEventsBase
{
    private Coroutine _gameStartAnimationRoutine;
    public MultiPlaterUIEvents(UIManager manager) 
    {
        _uiManager = manager;
    }


    #region Base GameStates CallBacks

    public override IEnumerator SetUpUI()
    {
        yield return PlacingPlayersUI();
        yield return null;
        _uiManager.GameManagerUI.SimulationState = SimulationSetUpState.UISetUp;
#if Log
        LogManager.Log($" UI is Set Up Runner Player Ref => {_uiManager.GameManagerUI.Runner.LocalPlayer}", Color.green, LogManager.ValueInformationLog);
#endif
    }
    public override void OnGameStarted()
    {
        if (_gameStartAnimationRoutine != null)
            _uiManager.StopCoroutine(_gameStartAnimationRoutine);
        _gameStartAnimationRoutine = _uiManager.StartCoroutine(GameStartAnimation());
    }

    public override void OnDealingCards()
    {
       // throw new System.NotImplementedException();
    }

    public override void OnDoubting()
    {

        //after the animation inoking logic
        if (_uiManager.GameManagerUI.IsHost)
        {
            _uiManager.GameManagerUI.GameModeManager.DoubtOverLogic();
        }
    }

    public override void OnGameOver()
    {
       // throw new System.NotImplementedException();
    }

    public override void OnRoundOver()
    {
       // throw new System.NotImplementedException();
    }

    public override void OnSetUpStarted()
    {
       // throw new System.NotImplementedException();
    }
    public override void OnFirstPlayerTurn()
    {
        FirstPlayerTurnLayOutSetUp();
    }

    public override void OnPlayerTurn()
    {
        throw new System.NotImplementedException();
    }

    public override void OnLastPlayerTurn()
    {
        throw new System.NotImplementedException();
    }
    #endregion Base GameStates CallBacks


    #region OnGameStarted tools
    private IEnumerator GameStartAnimation()
    {
        yield return null;
        //informing server player animation finished
        _uiManager.GameManagerUI.PlayerIsReady();
    }


    #endregion
}