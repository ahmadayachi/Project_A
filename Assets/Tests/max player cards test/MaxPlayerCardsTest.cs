using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MaxPlayerCardsTest:SinglePeerBase
{

    private const byte _beloteDeckSize = 32;
    private byte _maxPlayerCards;
    [Test]
    public void MaxPlayerCardsTestSimplePasses()
    {
        byte playerNumber = 4;
        _maxPlayerCards = SetMaxPlayerCards(playerNumber);
        Assert.AreEqual(7, _maxPlayerCards);
    }

}
