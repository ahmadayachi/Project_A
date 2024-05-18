using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BetRoundingUpAndValidationTest : SinglePeerBase
{


    // A Test behaves as an ordinary method
    [Test]
    public void BetRoundingUpAndValidationTestSimplePasses()
    {
        _previousBet = new byte[] {};
        LoopAndValidateRoundedUpBet();
    }
    #region private shitters
    private void LoopAndValidateRoundedUpBet()
    {
        bool isRounded;
        byte[] maxBet = BetGenerator.GenerateMaxBet(_maxDealtCards);
        int dealtCardsIndex = 2;
        do
        {
            //generating Bet
            isRounded = BetGenerator.TryRoundUpBet(_previousBet, out _currentBet, dealtCardsIndex);
            //making sure it is not null 
            if (isRounded)
                Assert.IsNotNull(_currentBet);
            //setting up validator args
            _validatorArgs = new ValidatorArguments(_currentBet, _previousBet, (byte)dealtCardsIndex);
            //Since it is a Max Only Bet we validating only when Bet Changed 
            if (isRounded)
            {
                bool isBetValid = _betHandler.ChainValidateBet(_validatorArgs);
                Assert.IsTrue(isBetValid, $"Current bet {string.Join(",", _currentBet)} Previous Bet {string.Join(",", _previousBet)}");

                //chaining the previous with the current Bet
                _previousBet = _currentBet;
            }
            dealtCardsIndex++;
        } while (!Extention.AreEqual(_currentBet, maxBet));
        //Assert.IsTrue(, $"Current bet {string.Join(",", _currentBet)} Max Bet {string.Join(",", maxBet)}");
    }
    #endregion
}
