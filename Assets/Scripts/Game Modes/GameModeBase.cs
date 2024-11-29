using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameModeBase : IGameMode
{
    protected GameManager _gameManager;

    public abstract void ConfirmBet(byte[] bet, string playerID);

    public abstract void DoubtBet(string playerID);

    public abstract void PassTurn();

    public void StartGame()
    {
        throw new System.NotImplementedException();
    }
}
