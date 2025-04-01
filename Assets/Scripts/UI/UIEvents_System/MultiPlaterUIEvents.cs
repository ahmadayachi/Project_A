using System.Collections;
using Unity.Netcode;
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
        if(_uiManager.GameManagerUI.IsHost)
        yield return PlacingPlayersUI();
        yield return null;
        yield return SetUpMyDisplayCards();
        yield return SetUpPreviousPlayerDisplayCards();
        yield return null;
        yield return LinkPlayerTurnUIWithLogic();

        _uiManager.GameManagerUI.SimulationState = SimulationSetUpState.UISetUp;
#if Log
        LogManager.Log($" UI is Set Up!, Local Client ID => {NetworkManager.Singleton.LocalClientId}", Color.green, LogManager.ValueInformationLog);
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
        if (_doubtSceneRoutine != null)
            _uiManager.StopCoroutine(_doubtSceneRoutine);
        _doubtSceneRoutine = _uiManager.StartCoroutine(DoubtScene());
    }

   

    public override void OnGameOver()
    {
        // throw new System.NotImplementedException();
    }

    public override void OnRoundOver()
    {
        //some indicator that round ended and a new round is startting 
        
        PlayerTurnUIOff();
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
        PlayerTurnLayOutSetUp();
    }

    public override void OnLastPlayerTurn()
    {
        LastPlayerUIPanelsLayOutSetUp();
    }
    #endregion Base GameStates CallBacks


    #region OnGameStarted tools
    private IEnumerator GameStartAnimation()
    {
        yield return null;
        //informing server player animation finished
        _uiManager.GameManagerUI.LocalPlayer.PlayerIsReady();
    }



    #endregion
}