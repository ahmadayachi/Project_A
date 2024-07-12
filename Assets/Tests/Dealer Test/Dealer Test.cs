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

    [UnityTest]
    public IEnumerator DealingTest()
    {
        FillFakeDeck();
        Assert.IsTrue(IsAValidBeloteDeckWithAStandartSize(FakeDeck), "Deck is not a valid Belote Deck !");
        CreateFakePlayers(PlayersNumber);
        Assert.IsTrue(ArePlayersValid(FakePlayers), "Players Ares Not Valid!");
        _dealer = new Dealer(StartRoutine, StopRoutine);
        yield return null;
        CardInfo[] FakeDeckClone = new CardInfo[FakeDeck.Length];
        Array.Copy(FakeDeck, FakeDeckClone, FakeDeck.Length);
        DealerStateArguments args = new DealerStateArguments();
        args.DeckToDeal = FakeDeckClone;
        args.Players = FakePlayers;
        Assert.IsFalse(IsDeckShuffled(FakeDeck, FakeDeckClone), "Deck should not be Shuffled");
        _dealer.Start(args);
        yield return new WaitUntil(() => _dealer.DealingRoutine == null);
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
        _dealer = new Dealer(StartRoutine, StopRoutine);
        _dealer.RiffleShuffle(SecondFakeDeckClone);
        Assert.IsTrue(IsDeckShuffled(FakeDeckClone, SecondFakeDeckClone), "Deck Have Been Shuffled !");
        LogDeck(SecondFakeDeckClone, "FakeDeck Card after Riffle Shuffle =>>");
        Assert.IsTrue(IsDeckShuffled(FakeDeck, SecondFakeDeckClone), "Deck Have Been Shuffled !");
    }

    [UnityTest]
    public IEnumerator CustomDeckShuflingTest()
    {
        Assert.IsNull(CardManager.Deck);
        //standart deck variables
        DeckInfo standartDeckInfo = new DeckInfo();
        standartDeckInfo.DeckType = DeckType.Standard;

        standartDeckInfo.SuitsNumber = 4;

        CardManager.Init(standartDeckInfo);
        yield return null;
        Assert.NotNull(CardManager.Deck);
        Assert.IsFalse(CardManager.Deck.IsEmpty());
        yield return null;

        //check air pockets before shuffling
        Assert.False(DeckAirPocketsCheck(CardManager.Deck));
        //chekcing lenght
        Assert.AreEqual(CardManager.Deck.Length, CardManager.Deck.ValidCardsCount());

        _dealer = new Dealer(StartRoutine, StopRoutine);
        CardManager.Deck.Shuffle();
        Assert.AreEqual(CardManager.Deck.Length, CardManager.Deck.ValidCardsCount());
        Assert.IsFalse(DeckAirPocketsCheck(CardManager.Deck));

        _dealer.RiffleShuffle(CardManager.Deck);
        Assert.AreEqual(CardManager.Deck.Length, CardManager.Deck.ValidCardsCount());
        Assert.IsFalse(DeckAirPocketsCheck(CardManager.Deck));

    }
}