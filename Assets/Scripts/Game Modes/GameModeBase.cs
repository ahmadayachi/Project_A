using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;


public abstract class GameModeBase : IGameMode
{
    protected GameManager _gameManager;

    public abstract void ConfirmBet(byte[] bet, FixedString64Bytes playerID);
    public abstract void DoubtBet(FixedString64Bytes playerID);
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
    
    public virtual bool TryFindPlayer(FixedString64Bytes playerID, out IPlayer player)
    {
        player = null;
        if (default == playerID) return false;
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
    //protected abstract void StartPlayerTimer();
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
        _gameManager.DoubtSceneTimer.Value = 3;
    }
    protected virtual void PunishingDoubtLooser(out FixedString64Bytes playerToPunishID, out IPlayer playerToPunish)
    {
        playerToPunishID = (_gameManager.DoubtState.Value == DoubtState.WinDoubt) ? _gameManager.LiveBetPlayerID.Value : _gameManager.CurrentPlayerID.Value;
        if (TryFindPlayer(playerToPunishID, out playerToPunish))
        {
            playerToPunish.PlusOneCard();
#if Log
            LogManager.Log($"Punishing Player! Player ID:=> {playerToPunishID}, DoubtState{_gameManager.DoubtState.Value}", Color.red, LogManager.ValueInformationLog);
#endif
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
    protected virtual void CollectDealtCards()
    {
        //clearing previous dealt Cards
        _gameManager.DealtCards.Clear();
        //adding dealt Cards 
        foreach (var player in _gameManager.Players)
        {
            if(player.IsOut) continue;
            foreach (var card in player.Hand)
            {
                _gameManager.DealtCards.Add(card.Rank);
#if Log
                LogManager.Log($"Collected Dealt Card!, Card=>{card}",Color.green,LogManager.ValueInformationLog);
#endif
            }
        }

    }
}
