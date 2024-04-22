using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DealerTest : SinglePeerBase
{
    private const int PlayersNumber = 4;
    private State _dealerState;
    private Dealer _dealer;

    [Test]
    public void FakeDeckCreationTest()
    {
        FillFakeDeck();
        Assert.IsTrue(IsAValidBeloteDeckWithAStandartSize(FakeDeck), "Deck is not a valid Belote Deck !");
    }

    [Test]
    public void DealingTest()
    {
        FillFakeDeck();
        Assert.IsTrue(IsAValidBeloteDeckWithAStandartSize(FakeDeck), "Deck is not a valid Belote Deck !");
        CreateFakePlayers(PlayersNumber);
        Assert.IsTrue(ArePlayersValid(FakePlayers), "Players Ares Not Valid!");
        _dealer = new Dealer();
        CardInfo[] FakeDeckClone = new CardInfo[FakeDeck.Length];
        Array.Copy(FakeDeck, FakeDeckClone, FakeDeck.Length);
        DealerArguments args = new DealerArguments();
        args.DeckToDeal = FakeDeckClone;
        args.Players = FakePlayers;
        Assert.IsFalse(IsDeckShuffled(FakeDeck, FakeDeckClone), "Deck should not be Shuffled");
        _dealer.Start(args);
        Assert.IsTrue(IsDealingValid(FakePlayers), "Dealing Is Not Valid!");
        Assert.False(IsDeckShuffled(FakeDeck, FakeDeckClone), "Deck should not be Shuffled");
    }

    [Test]
    public void DeckShufflingTest()
    {
        FillFakeDeck();
        Assert.IsTrue(IsAValidBeloteDeckWithAStandartSize(FakeDeck), "Deck is not a valid Belote Deck !");
        CardInfo[] FakeDeckClone = new CardInfo[FakeDeck.Length];
        Array.Copy(FakeDeck, FakeDeckClone, FakeDeck.Length);
        Assert.False(IsDeckShuffled(FakeDeck, FakeDeckClone), "Deck should not be Shuffled");
        LogDeck(FakeDeckClone, "FakeDeck Card Pre Shuffle =>>");
        FakeDeckClone.Shuffle();
        Assert.IsTrue(IsDeckShuffled(FakeDeck, FakeDeckClone), "Deck Have Been Shuffled !");
        LogDeck(FakeDeckClone, "FakeDeck Card after Fisher-Yites Shuffle =>>");
        CardInfo[] SecondFakeDeckClone = new CardInfo[FakeDeck.Length];
        Array.Copy(FakeDeckClone, SecondFakeDeckClone, FakeDeckClone.Length);
        _dealer = new Dealer();
        _dealer.RiffleShuffle(SecondFakeDeckClone);
        Assert.IsTrue(IsDeckShuffled(FakeDeckClone, SecondFakeDeckClone), "Deck Have Been Shuffled !");
        LogDeck(SecondFakeDeckClone, "FakeDeck Card after Riffle Shuffle =>>");
        Assert.IsTrue(IsDeckShuffled(FakeDeck, SecondFakeDeckClone), "Deck Have Been Shuffled !");
    }
}