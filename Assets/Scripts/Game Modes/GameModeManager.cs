using System.Collections.Generic;

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
    public void SetGameState(GameState state)
    {
        _activeGameModeBehaviour.SetGameState(state);
    }
    public void StartSimulationSetUp()
    {
        _activeGameModeBehaviour.StartSimulationSetUp();
    }
    public bool IsGameOver()
    {
        return _activeGameModeBehaviour.IsGameOver();
    }
    public void StartPlayerState()
    {
        _activeGameModeBehaviour.StartPlayerState();
    }
    public void LoadCurrentPlayer()
    {
        _activeGameModeBehaviour.LoadCurrentPlayer();
    }
    public void DoubtLogic(DoubtState doubtState)
    {
        _activeGameModeBehaviour.DoubtLogic(doubtState);
    }
    public void DoubtOverLogic()
    {
        _activeGameModeBehaviour.DoubtOverLogic();
    }

    public List<DiffusedRankInfo> RoundUpCurrentBet()
    {
        return _activeGameModeBehaviour.RoundUpCurrentBet();
    }

    public bool TryFindPlayer(string playerID, out IPlayer player)
    {
        return _activeGameModeBehaviour.TryFindPlayer(playerID, out player);
    }
}

