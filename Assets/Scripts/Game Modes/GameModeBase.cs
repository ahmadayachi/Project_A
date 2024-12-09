using System.Collections;
using System.Collections.Generic;


public abstract class GameModeBase : IGameMode
{
    protected GameManager _gameManager;

    public abstract void ConfirmBet(byte[] bet, string playerID);
    public abstract void DoubtBet(string playerID);
    public abstract void PassTurn();
    public abstract void SetGameState(GameState state);
    public abstract void StartGame();
    public abstract void StartSimulationSetUp();
    public abstract bool IsGameOver();
    public abstract void StartPlayerState();
    public abstract void LoadCurrentPlayer();
    public abstract void DoubtLogic(DoubtState doubtState);
    public abstract void DoubtOverLogic();
    public abstract List<DiffusedRankInfo> RoundUpCurrentBet();
    
    public virtual bool TryFindPlayer(string playerID, out IPlayer player)
    {
        player = null;
        if (string.IsNullOrEmpty(playerID)) return false;
        if (_gameManager.Players.IsNullOrHaveNullElements())
        {
#if Log
            LogManager.LogError("Failed Finding Player! Players Array is Null or Have Null Elements");
#endif
            return false;
        }
        foreach (var item in _gameManager.Players)
        {
            if (item.ID == playerID)
            {
                player = item;
                return true;
            }
        }
        return false;
    }

    protected abstract void SimulationPrepGameState();
    protected abstract void GameStarted();
    protected abstract void Dealing();
    protected abstract void Doubting();
    protected abstract void RoundOver();
    protected abstract void OnRoundIsOverLogic();
    protected abstract void RoundOverVariablesCleaning();
    protected abstract void OnDealingOver();
    protected abstract void GameOver();
    protected abstract void StartPlayerTimer();
    protected abstract void PlayerControl();

    protected virtual void NextPlayerIndex()
    {
        _gameManager.PlayerIndex = _gameManager.PlayerIndex + 1;
        if (_gameManager.PlayerIndex >= _gameManager.Players.Length)
            _gameManager.PlayerIndex = 0;
    }
    protected virtual bool NeedToLookForPlayers(ref int CurrentPlayerIndex, IPlayer player)
    {
        //detecting if I already looped the array
        if (CurrentPlayerIndex == _gameManager.PlayerIndex) return false;
        // first player is still playing Halt !
        if (!player.IsOut) return false;
        return true;
    }
    protected virtual void CaluCulateDoubtSceneTimer()
    {
        //TODO: Calculate Doubt Scene Timer Based On UI Needs
        //Currently its a fixed time 
        _gameManager.DoubtSceneTimer = 3;
    }
    protected virtual void PunishingDoubtLooser(out string playerToPunishID, out IPlayer playerToPunish)
    {
        playerToPunishID = (_gameManager.DoubtState == DoubtState.WinDoubt) ? _gameManager.LiveBetPlayerID : _gameManager.CurrentPlayerID;
        if (TryFindPlayer(playerToPunishID, out playerToPunish))
        {
            playerToPunish.PlusOneCard();
        }
        else
        {
#if Log
            LogManager.LogError($"Failed Doubt Over Logic Current Player! Cant Find  Player with ID:=> {playerToPunishID}");
#endif
            return;
        }
    }
    protected virtual byte DealtCardsCounter()
    {
        byte dealtCards = 0;
        foreach (var player in _gameManager.Players)
        {
            dealtCards += player.CardsToDealCounter;
        }
        return dealtCards;
    }
}
