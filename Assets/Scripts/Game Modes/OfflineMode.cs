using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class OfflineMode : GameModeBase
{
    public OfflineMode(GameModeARGS args)
    {
        _gameManager = args.GameManager;
    }

    public override void ConfirmBet(byte[] bet, FixedString64Bytes playerID)
    {
        throw new System.NotImplementedException();
    }

    public override void DoubtBet(FixedString64Bytes playerID)
    {
        throw new System.NotImplementedException();
    }

    public override void DoubtLogic(DoubtState doubtState)
    {
        throw new System.NotImplementedException();
    }

    public override void DoubtOverLogic()
    {
        throw new System.NotImplementedException();
    }

    public override bool IsGameOver()
    {
        throw new System.NotImplementedException();
    }

    public override void LoadCurrentPlayer()
    {
        throw new System.NotImplementedException();
    }

    public override void PassTurn()
    {
        throw new System.NotImplementedException();
    }

    public override List<DiffusedRankInfo> RoundUpCurrentBet()
    {
        throw new System.NotImplementedException();
    }

    public override void SetGameState(GameState state)
    {
        throw new System.NotImplementedException();
    }

    public override void StartGame()
    {
        throw new System.NotImplementedException();
    }

    public override void StartPlayerState()
    {
        throw new System.NotImplementedException();
    }

    public override void StartSimulationSetUp()
    {
        throw new System.NotImplementedException();
    }

    protected override void Dealing()
    {
        throw new System.NotImplementedException();
    }

    protected override void Doubting()
    {
        throw new System.NotImplementedException();
    }

    protected override void GameOver()
    {
        throw new System.NotImplementedException();
    }

    protected override void GameStarted()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnDealingOver()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnRoundIsOverLogic()
    {
        throw new System.NotImplementedException();
    }

    protected override void PlayerControl()
    {
        throw new System.NotImplementedException();
    }

    protected override void RoundOver()
    {
        throw new System.NotImplementedException();
    }

    protected override void RoundOverVariablesCleaning()
    {
        throw new System.NotImplementedException();
    }

    protected override void SimulationPrepGameState()
    {
        throw new System.NotImplementedException();
    }

    //protected override void StartPlayerTimer()
    //{
    //    throw new System.NotImplementedException();
    //}
}
