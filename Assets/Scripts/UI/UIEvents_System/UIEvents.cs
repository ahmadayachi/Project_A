using System.Collections;

public class UIEvents
{
    private IUIEvents _uiEvents;
    public UIEvents(UIEventsArgs args)
    {
        //since starting off with two modes then i can just do this 
        _uiEvents = args.GameMode == GameMode.Single ? new SinglePlayerUIEvents(args.UIManager) : new MultiPlaterUIEvents(args.UIManager);
    }
    /// <summary>
    /// something to let players know what are they waiting
    /// </summary>
    public void OnSetUpStarted()
    {
        _uiEvents.OnSetUpStarted();
    }

    public IEnumerator SetUpUI()
    {
        yield return _uiEvents.SetUpUI();
    }

    /// <summary>
    /// some UI animation only when a game starts or smthing
    /// </summary>
    public void OnGameStarted()
    {
        _uiEvents.OnGameStarted();
    }

    public void OnDealingCards()
    {
        _uiEvents.OnDealingCards();
    }

    public void OnDoubting()
    {
        _uiEvents.OnDoubting();
    }

    public void OnRoundOver()
    {
        _uiEvents.OnRoundOver();
    }

    public void OnGameOver()
    {
        _uiEvents.OnGameOver();
    }

    public void OnFirstPlayerTurn()
    {
        _uiEvents.OnFirstPlayerTurn();
    }

    public void OnPlayerTurn()
    {
        _uiEvents.OnPlayerTurn();
    }

    public void OnLastPlayerTurn()
    {
        _uiEvents.OnLastPlayerTurn();
    }
    public void PlayerTurnUIOff()
    {
        _uiEvents.PlayerTurnUIOff();
    }
}
