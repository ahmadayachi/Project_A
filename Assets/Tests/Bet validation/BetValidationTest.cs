using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.tvOS;

public class BetValidationTest : SinglePeerBase
{
    private BetHandler _betHandler;
    private ValidatorArguments _validatorArgs;

    private byte[] _currentBet;
    private byte[] _previousBet;
    [Test]
    public void BetValidationsTest()
    {
        // setting up CardManager Deck
        StandardSizeDeckCheck(DeckType.Belote,
                              StandardSuitsNumber,
                              Belote,
                              CardManager.BELOTE_DECK_SUIT_SIZE);

        //creating a BetHandler 
        _betHandler = new BetHandler();

        //creating a bet args 
        _validatorArgs = new ValidatorArguments();
        _currentBet = new byte[] {7,7,7,8,8,8};
        _previousBet = new byte[] {1,1,1,8,8 };

        _validatorArgs.CurrentBet = _currentBet;
        _validatorArgs.PreviousBet = _previousBet;
        _validatorArgs.dealtCardsNumber = 6;

        bool isBetValid = _betHandler.ChainValidateBet(_validatorArgs);
        Assert.IsTrue(isBetValid);
    }

    #region private methods

    //private void SetUpCardManager

    #endregion private methods
}