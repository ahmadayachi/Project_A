using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class OnlineMode : GameModeBase
{
    public OnlineMode(GameModeARGS args)
    {
        _gameManager = args.GameManager;
    }

    public override void ConfirmBet(byte[] bet, string playerID)
    {
        //blocking invalid args
        if (string.IsNullOrEmpty(playerID) || bet == null)
        {
#if Log
            LogManager.LogError($"Blocking Confirm Rpc ! Invalid Args Found! CurrentPlayerID is :=>{_gameManager.CurrentPlayerID}");
#endif
            return;
        }
        //only current player can confirm bet
        if (_gameManager.CurrentPlayerID != playerID)
        {
#if Log
            LogManager.Log($"Blocking Confirm Rpc ! player with ID:= {playerID} is not the Current PLayer!,Current PLayer ID:={_gameManager.CurrentPlayerID}", Color.red, LogManager.GameModeLogs);
#endif
            return;
        }
        //the previous bet should always be sorted
        byte[] liveBet = _gameManager.LiveBet.ToByteArray();
        ValidatorArguments betArgs = new ValidatorArguments(bet, liveBet, _gameManager.DealtCardsNumber);
        bool isValid = _gameManager.BetHandler.ChainValidateBet(betArgs);
        //bet has to be valid
        if (!isValid)
        {
#if Log
            LogManager.Log($"Blocking Confirm Rpc ! player with ID:= {playerID} Sent an Invalid Bet!, Bet=:{string.Join(",", liveBet)}", Color.red, LogManager.GameModeLogs);
#endif
            return;
        }

        //stoping timer
        _gameManager.PlayerTimerState = PlayerTimerState.StopTimer;

        //making sure the array is sorted before confirming
        Extention.BetDiffuser(bet, _gameManager.DiffusedBet);
        byte[] sortedBet = _gameManager.DiffusedBet.ToByteArray();
        int sortedBetLength = sortedBet.Length;

        //cleaning Network Array      
        _gameManager.LiveBet.ClearByteArray();

        //adding Bet
        for (int index = 0; (index < sortedBetLength); index++)
        {
            _gameManager.LiveBet.Set(index, sortedBet[index]);
        }

        //setting live bet player id
        _gameManager.LiveBetPlayerID = playerID;

        //passing Turn here
        PassTurn();

        //generating a Max Bet
        byte[] MaxBet = BetGenerator.GenerateMaxBet(_gameManager.DealtCardsNumber);

        //cheking if the Played Bet is a Max Bet
        if (MaxBet.AreEqual(sortedBet))
        {
            //Directing the Game To an Auto Doubt State
            _gameManager.DealtCards.ToByteList(_gameManager.DealtCardsList);
            DoubtStateArguments stateArguments = new DoubtStateArguments(_gameManager.DealtCardsList, sortedBet);

            _gameManager.ChangeState(_gameManager.Doubt, stateArguments);
#if Log
            LogManager.Log($"Auto Doubt is Launched!, Current Player {_gameManager.CurrentPlayerID} Live Bet Player ID {_gameManager.LiveBetPlayerID}", Color.blue, LogManager.GameModeLogs);
#endif
            return;
        }

        //checking if the next Current Player Have to Play a Max Bet
        byte[] roundedUpBet;
        if (BetGenerator.TryRoundUpBet(sortedBet, out roundedUpBet, _gameManager.DealtCardsNumber))
        {
            //cheking if the rounded up bet is a max Bet
            if (MaxBet.AreEqual(roundedUpBet))
            {
                //directing Game State to a Last Player Game State
                _gameManager.GameState = GameState.LastPlayerTurn;
                return;
            }
        }
        //abbording everythink if a bet cannot be Rounded Up
        else
        {
#if Log
            LogManager.LogError($"Failed Confirm Rpc ! Failed Rounding Up Current Bet! CurrentPlayerID is :=>{_gameManager.CurrentPlayerID}");
#endif
            return;
        }

        //Directing Game State  to a normal Player turn
        _gameManager.GameState = GameState.PlayerTurn;
    }

    public override void DoubtBet(string playerID)
    {
        //blocking invalid args
        if (string.IsNullOrEmpty(playerID))
        {
#if Log
            LogManager.LogError($"Blocking Doubt Rpc ! Invalid Args Found! CurrentPlayerID is :=>{_gameManager.CurrentPlayerID}");
#endif
            return;
        }

        if (_gameManager.Doubt == null)
        {
#if Log
            LogManager.LogError($"Blocking Doubt Rpc ! Doubt State is not Initialized! CurrentPlayerID is :=>{_gameManager.CurrentPlayerID}");
#endif
            return;
        }

        //only current player can Doubt
        if (_gameManager.CurrentPlayerID != playerID)
        {
#if Log
            LogManager.Log($"Blocking Doubt Rpc ! player with ID:= {playerID} is not the Current Player!,Current Player ID:={_gameManager.CurrentPlayerID}", Color.red, LogManager.GameModeLogs);
#endif
            return;
        }

        //player cant doubt himself
        if (_gameManager.CurrentPlayerID == _gameManager.LiveBetPlayerID)
        {
#if Log
            LogManager.LogError($"Blocking Doubt Rpc ! player Cant Doubt himself ,Current Player ID:={_gameManager.CurrentPlayerID} Live Bet Player ID {_gameManager.LiveBetPlayerID}");
#endif
            return;
        }

        //stoping timer
        _gameManager.PlayerTimerState = PlayerTimerState.StopTimer;
      
        //invoking Doubt State
        byte[] liveBet = _gameManager.LiveBet.ToByteArray();
        _gameManager.DealtCards.ToByteList(_gameManager.DealtCardsList);
        DoubtStateArguments stateArguments = new DoubtStateArguments(_gameManager.DealtCardsList, liveBet);
        _gameManager.ChangeState(_gameManager.Doubt, stateArguments);
    }

    public override void PassTurn()
    {
        //Server Only Bitch
        if (_gameManager.IsClient) return;

        if (_gameManager.Players.IsNullOrHaveNullElements())
        {
#if Log
            LogManager.LogError("Failed Passing Turn ! Players Array is Null or Have Null Elements");
#endif
            return;
        }
        if (_gameManager.IsGameOver())
        {
#if Log
            LogManager.Log("Failed Passing Turn ! Game should be Over !", Color.red, LogManager.GameModeLogs);
#endif
            return;
        }

        int currentPlayerIndex = _gameManager.PlayerIndex;
        IPlayer player = null;
        do
        {
            //Moving player Indexer
            _gameManager.NextPlayerIndex();

            try
            {
                player = _gameManager.Players[_gameManager.PlayerIndex];
            }
            catch (Exception ex)
            {
#if Log
                LogManager.LogError("Failed Passing Turn!" + ex.Message);
#endif
                return;
            }
        } while (_gameManager.NeedToLookForPlayers(ref currentPlayerIndex, player));

        //final check for player
        bool loopedArray = (currentPlayerIndex == _gameManager.PlayerIndex);

        if (loopedArray || player.IsOut)
        {
#if Log
            LogManager.LogError($"Failed Passing Turn! looped array{loopedArray}/  Player :{player}");
#endif
            return;
        }

        //setting current player
        _gameManager.CurrentPlayer = player;
        _gameManager.CurrentPlayerID = player.ID;

        //if singlePlayer invoke shit here
        //if (_gameManager.GameMode == GameMode.Single)
        //{
        //    //Idk Ui shit or smth
        //}
    }


    #region private Swamp
   

    #endregion
}
