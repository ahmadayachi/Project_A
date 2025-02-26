using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class BetValidationTest : SinglePeerBase
{


    [Test]
    public void SimpleBetValidationsTest()
    {
        //creating a bet args 
        _validatorArgs = new ValidatorArguments();

        //_currentBet = BetGenerator.GenerateMaxBet(9);
        _currentBet = new byte[] { 1,1,1 };
        _previousBet = new byte[] {};

        //bool isrounded = BetGenerator.TryRoundUpBet(_previousBet, out _currentBet,4);

        _validatorArgs.CurrentBet = _currentBet;
        _validatorArgs.PreviousBet = _previousBet;
        _validatorArgs.DealtCardsNumber = 4;

        bool isBetValid = _betHandler.ChainValidateBet(_validatorArgs);
        Assert.IsTrue(isBetValid);
    }
    [Test]
    public void BetSortingTest()
    {
        _currentBet = new byte[] {7,7, 7,1, 1, 1, 1 };
        Dictionary<byte,byte> diffusedBet = new Dictionary<byte,byte>();
        Extention.BetDiffuserAlpha(_currentBet, diffusedBet,0);
        diffusedBet.SortBet();
        Debug.Log(string.Join(",", diffusedBet));
        _currentBet = diffusedBet.ToByteArray();
        Debug.Log(string.Join(",", _currentBet));

    }
    #region private methods

    //private void SetUpCardManager

    #endregion private methods
}