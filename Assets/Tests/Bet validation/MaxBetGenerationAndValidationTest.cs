using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.tvOS;

public class MaxBetGenerationAndValidationTest : SinglePeerBase
{
    private bool _correctWayOfGenerating;
    [Test]
    public void CorrectMaxBetGenerationAndValidationTest()
    {
        _previousBet = new byte[] { };
         _correctWayOfGenerating = true;
        LoopAndValidateMaxBet();
    }
    [Test]
    public void WrongMaxBetGenerationAndValidationTest()
    {
        _previousBet = BetGenerator.GenerateMaxBet(2);
        _correctWayOfGenerating = false;
        LoopAndValidateMaxBet();
    }
   
    #region private shitters
    private void LoopAndValidateMaxBet()
    {
        for (int dealtCardsIndex = 2; dealtCardsIndex <= _maxDealtCards; dealtCardsIndex++)
        {
            //generating Bet
            _currentBet = BetGenerator.GenerateMaxBet(dealtCardsIndex);
            //making sure it is not null 
            Assert.IsNotNull(_currentBet);
            //setting up validator args
            _validatorArgs = new ValidatorArguments(_currentBet, _previousBet, (byte)dealtCardsIndex);
            //Since it is a Max Only Bet we validating only when Bet Changed 
            if (!Extention.AreEqual(_previousBet, _currentBet))
            {
                bool isBetValid = _betHandler.ChainValidateBet(_validatorArgs);               
                Assert.IsTrue(_correctWayOfGenerating?isBetValid:!isBetValid);
            }
            //chaining the previous with the current Bet
            _previousBet = _correctWayOfGenerating ? _currentBet : BetGenerator.GenerateMaxBet(dealtCardsIndex+2);
        }
    }
    #endregion
}
