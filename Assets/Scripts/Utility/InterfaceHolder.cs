
using Fusion;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#region Interfaces
#region Player stuff 
public interface IPlayerBehaviour
{

}
public interface IPlayer:ICardReceiver
{
    string Name {get;}
    void SetPlayerName(string playerName);
    string ID {get;}
    void SetPlayerID(string playerID);
    void SetCardCounter(byte cardCounter);
    CardInfo[] Hand { get;}
    void ClearHand();
    string ToString();
}
public interface ICardReceiver
{
    /// <summary>
    /// if true player cant/wont have cards or play 
    /// </summary>
    NetworkBool IsOut {get;}
    /// <summary>
    /// how much Cards to deal 
    /// </summary>
    byte CardsToDealCounter { get;}
    bool AddCard(CardInfo card);
}
public interface IPlayerUIControler
{
    void SetPlayerIcon();
    void LoadPlayerCards();
}
#endregion

#region Card stuff  
public interface ICard : ICardInfo
{
    CardUI CardUI { get; }
    ICardUIControler CardUIControl { get; }
    //void SetRank(byte rank);
    //void SetID(byte id);
    //void SetSuite(byte suite);
    void Enable(CardInfo card);
    void Disable();
    CardInfo ToCardInfo();
    string ToString();
}
public interface ICardInfo
{
    byte Rank { get; }
    byte ID { get; }
    CardSuit Suit { get; }
}
public interface ICardUIControler
{
    void SetCardRankSprite();
    void ResetCardRankSprite();
}
public interface ICardBehaviour
{

}
/// <summary>
/// network ID only 
/// </summary>
public struct CardInfo : INetworkStruct
{
    public byte Rank;
    public byte ID;
    public CardSuit Suit;
    public NetworkBool IsValid;
    public override string ToString()
    {
        return $"Ivalid {IsValid} ID: {ID}, Rank: {Rank}, Suit: {Suit}";
    }

}
public enum CardSuit : byte
{
    NoSuit = 0,
    Spades = 1,
    Diamond = 2,
    Hearts = 3,
    Clover = 4
}
#endregion
public interface IDealerBehaviour
{
    List<ICard> DeckOfCards { get; set; }
}

public interface IGameModeBehaviour
{

}
public interface IState<T> where T : struct
{
    void Start(T arg);
    void ForceEnd();
}
public interface IValidator
{
    // Property for the next validator in the chain of responsibility
    public IValidator Next { get; set; }

    // Method to validate Bet
    public bool Validate(ValidatorArguments args);
}
#endregion




#region State Struct Arguments
public struct DealerStateArguments
{
    public CardInfo[] DeckToDeal;
    public ICardReceiver[] Players;
    public Action OnDealerStateEnds;

}
public struct PlayerStateArguments
{
    public GameState GameState;
}
public struct DoubtStateArguments
{
    public List<byte> DealtCards;
    public byte[] Livebet;

    public DoubtStateArguments(List<byte> dealtCards, byte[] livebet)
    {
        DealtCards = dealtCards;
        Livebet = livebet;
    }
}
#endregion
#region Struct Arguments
public struct DoubtOverUIArguments
{
    public List<byte> CorrectBetRanks;
    public List<byte> WrongBetRanks;

    public DoubtOverUIArguments(List<byte> correctBetRanks, List<byte> wrongBetRanks)
    {
        CorrectBetRanks = correctBetRanks;
        WrongBetRanks = wrongBetRanks;
    }
}
public struct CardPoolArguments
{
    public GameObject CardPrefab;
    public byte MaxPlayerCards;
    public byte ActivePlayerCount;
}
public struct PlayerArguments
{
    public string Name;
    public string ID;
    public byte IconID;
    public byte CardCounter;
    public GameManager GameManager;
    public NetworkBool isplayerOut;
}
public struct ValidatorArguments
{
    //public bool Chain;
    public byte[] CurrentBet;
    public byte[] PreviousBet;
    public byte DealtCardsNumber;

    public ValidatorArguments(byte[] currentBet, byte[] previousBet, byte dealtCardsNumber)
    {
        CurrentBet = currentBet;
        PreviousBet = previousBet;
        DealtCardsNumber = dealtCardsNumber;
    }
}
#endregion
#region Structs
public struct DeckInfo
{
    public DeckType DeckType;
    public byte SuitsNumber;
    public byte[] CustomSuitRanks;
}
[Serializable]
public struct CardUI
{
    [Header("Card Counter Fields")]
    public RectTransform CardCounterRect;
    public Image CardCounterImage;
    public TextMeshProUGUI CardCounterText;
    public GameObject CardCounterImageGO;
    [Header("Core Card Fields")]
    public Button CardButton;
    public GameObject CardRankImageGO;
    public Image CardRank;
    public Image CardPlate;
}
[Serializable]
public struct PlayerUI
{
    public CardPositioner CardPositioner;
    public SpriteRenderer PlayerIcon;
}

public struct DiffusedRankInfo
{
    public byte Rank;
    public int RankBruteValue;
    public byte CardsCount;

    public DiffusedRankInfo(byte rank, int rankBruteValue, byte cardsCount)
    {
        Rank = rank;
        RankBruteValue = rankBruteValue;
        CardsCount = cardsCount;
    }

    public override string ToString()
    {
        return $"Rank: {Rank}, RankBruteValue: {RankBruteValue}, CardsCount: {CardsCount}";
    }
}
#endregion


#region enums
public enum GameState:byte
{
    /// <summary>
    /// just chilling, maybe waiting for clients, a host migration happening 
    /// </summary>
    Idle,
    GameStarted,
    Dealing,
    FirstPlayerTurn,
    PlayerTurn,
    LastPlayerTrun,
    Doubting,
    RoudOver,
    GameOver,
    HostMigration
}
public enum DoubtState
{
    NoDoubting,
    WinDoubt,
    LooseDoubt
}
public enum DeckType
{
    /// <summary>
    /// a standart Deck will contain all Ranks from Ace to king 
    /// </summary>
    Standard=2,
    /// <summary>
    /// A Belote Deck will Contain Ranks from 7 to Ace excluding these ranks (2,3,4,5,6)
    /// </summary>
    Belote = 7,
    /// <summary>
    /// A Custom Deck that Can Have Any Ranks the players Choose with min of 8 Ranks
    /// </summary>
    Custom = 69
}
#endregion