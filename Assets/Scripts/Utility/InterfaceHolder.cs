
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#region Interfaces
#region Player stuff 
public interface IplayerState
{

}
public interface IPlayer:ICardReceiver
{
    public PlayerRef playerRef {get;}
    string Name {get;}
    void SetPlayerName(string playerName);
    string ID {get;}
    void SetPlayerID(string playerID);
    void PlusOneCard();
    void ClearCardsCounter();
    CardInfo[] Hand { get;}
    void ClearHand();
    void SetIsplayerOut(NetworkBool isPlayerOut);
    State PlayerState { get;}
    byte IconID { get; }
    bool IsLocalPlayer { get; }
    public NetworkObject NetworkObject {get;}
    Transform Transform { get; }
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
[Serializable]
public struct RunTimePlayerData
{
    public PlayerRef PlayerRef;
    public string PlayerName;
    public string PlayerID;
    public int IconIndex;
    public NetworkObject PlayerNetObject;
    public bool AuthorityAssigned;
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
//TODO: check why the fuck this still networked struct
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
public interface IUIEvents
{
    /// <summary>
    /// something to let players know what are they waiting 
    /// </summary>
    void OnSetUpStarted();
    IEnumerator SetUpUI();
    /// <summary>
    /// some UI animation only when a game starts or smthing 
    /// </summary>
    void OnGameStarted();
    void OnDealingCards();
    void OnDoubting();
    void OnRoundOver();
    void OnGameOver();
    void OnHostMigration();
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
    public bool IsMyTurn;

    public PlayerStateArguments(GameState gameState, bool isMyTurn)
    {
        GameState = gameState;
        IsMyTurn = isMyTurn;
    }
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
    public PlayerRef PlayerRef;
    public string Name;
    public string ID;
    public byte IconID;

    //public byte CardCounter;
    //public GameManager GameManager;
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
[Serializable]
public struct DeckInfo 
{
    public DeckType DeckType;
    public byte SuitsNumber;
    public byte[] CustomSuitRanks;
}
[Serializable]
public struct CardUI
{
    public SpriteRenderer CardRank;
    public SpriteRenderer CardPlate;
    public SpriteRenderer CardBack;
}
[Serializable]
public struct PlayerUI
{
    public CardPositioner CardPositioner;
    public SpriteRenderer PlayerIcon;
    public TextMeshPro PlayerName;
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

[Serializable]
public struct PlayerUIPlacementSceneRefs 
{
    [Header("Player POV Placement ")]
    [SerializeField] public Transform PlayerPOV;

    [Header("Players On Left Placement ")]
    [SerializeField] public Transform PlayersOnLeft;

    [Header("Players On Right Placement Setting")]
    [SerializeField] public Transform PlayersOnRight;

    [Header("Players On Front Placement Setting")]
    [SerializeField] public Transform PlayersOnFront;
}
#endregion


#region enums
public enum SimulationSetUpState
{
    NoSetUp,
    LogicSetUp,
    UISetUp,
    SetUpComplete,
    SetUpCanceled
}
public enum PlayerTimerState
{
    NoTimer,
    StartTimer,
    StopTimer
}
public enum GameState:byte
{
    NoGameState,
    /// <summary>
    /// just chilling, maybe waiting for clients, a host migration happening 
    /// </summary>
    SimulationSetUp,
    GameStarted,
    Dealing,
    FirstPlayerTurn,
    PlayerTurn,
    LastPlayerTurn,
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