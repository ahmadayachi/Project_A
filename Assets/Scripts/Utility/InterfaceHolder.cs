//using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;

#region Interfaces

#region Player stuff

public interface IplayerState
{
}

public interface IPlayer : ICardReceiver
{
    //public PlayerRef playerRef { get; }
    ulong ClientID { get; }
    string Name { get; }
    PlayerUIController PlayerUIControler
    {
        get;
    }
    void SetPlayerName(string playerName);

    string ID { get; }

    void SetPlayerID(string playerID);

    void PlusOneCard();

    void ClearCardsCounter();

    CardInfo[] Hand { get; }

    void ClearHand();

    void SetIsplayerOut(bool isPlayerOut);
    void SetIsPlayerWinner(bool isPlayerWinner);

    State PlayerState { get; }
    byte IconID { get; }
    bool IsTheLocalPlayer { get; }
    public NetworkObject PlayerNetworkObject { get; }
    public PlayerUI PlayerUI{ get; }
    Transform Transform { get; }

    //player commands 
    void ConfirmBet();
    void DoubtBet();
    void PlayerIsReady();
    string ToString();
}

public interface ICardReceiver
{
    /// <summary>
    /// if true player cant/wont have cards or play
    /// </summary>
    bool IsOut { get; }

    /// <summary>
    /// how much Cards to deal
    /// </summary>
    byte CardsToDealCounter { get; }

    bool AddCard(CardInfo card);
}

//public interface IPlayerUIControler
//{
//    void SetPlayerIcon();

//    void LoadPlayerCards();
//}

[Serializable]
public struct RunTimePlayerData
{
    //public PlayerRef PlayerRef;
    public string PlayerName;
    public string PlayerID;
    public int IconIndex;
    public ulong ClientID;
    public NetworkObjectReference PlayerNetObjectRef;
    public bool AuthorityAssigned;
}

#endregion Player stuff

#region Card stuff

public interface ICard : ICardInfo
{
    CardUI CardUI { get; }
    CardUIControler CardUIControl { get; }

    //void SetRank(byte rank);
    //void SetID(byte id);
    //void SetSuite(byte suite);
    void Enable(CardInfo card);

    void Disable();

    CardInfo ToCardInfo();

    Transform Transform { get; }

    void EnableCardCounter(int counter);
    void DisableCardCounter();

    string ToString();
}

public interface ICardInfo
{
    byte Rank { get; }
    byte ID { get; }
    CardSuit Suit { get; }
}

[System.Serializable]
public struct DisplayCardUIRefs
{
    public GameObject DisplayCardGameObject;
    public Image CardPlate;
    public Image CardPlateHighlight;
    public GameObject CardCounterGameObject;
    public Image CardCounter;
    public Image CardCounterHighlight;
    public TextMeshProUGUI CardCounterText;
    public Image CardSuit;
    public Button CardButton;
}
//TODO: check why the fuck this still networked struct
/// <summary>
/// network ID only
/// </summary>
//public struct CardInfo : INetworkStruct
//{
//    public byte Rank;
//    public byte ID;
//    public CardSuit Suit;
//    public NetworkBool IsValid;

//    public override string ToString()
//    {
//        return $"Isvalid {IsValid} ID: {ID}, Rank: {Rank}, Suit: {Suit}";
//    }
//}
public struct CardInfo 
{
    public byte Rank;
    public byte ID;
    public CardSuit Suit;
    public bool IsValid;

    public override string ToString()
    {
        return $"Isvalid {IsValid} ID: {ID}, Rank: {Rank}, Suit: {Suit}";
    }
}

[Serializable]
public enum CardSuit : byte
{
    NoSuit = 0,
    Spades = 1,
    Diamond = 2,
    Hearts = 3,
    Clover = 4
}

#endregion Card stuff

public interface IDealerBehaviour
{
    List<ICard> DeckOfCards { get; set; }
}

public interface IGameMode
{
    void ConfirmBet(byte[] bet, FixedString64Bytes playerID);
    void DoubtBet(FixedString64Bytes playerID);
    void PassTurn();
    void StartGame();
    void SetGameState(GameState state);
    void StartSimulationSetUp();
    bool IsGameOver();
    void StartPlayerState();
    void LoadCurrentPlayer();
    void DoubtLogic(DoubtState doubtState);
    void DoubtOverLogic();
    List<DiffusedRankInfo> RoundUpCurrentBet();
    bool TryFindPlayer(FixedString64Bytes playerID, out IPlayer player);
}
public struct GameModeARGS
{
    public GameMode GameMode;
    public GameManager GameManager;
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
    void OnFirstPlayerTurn();
    void OnPlayerTurn();
    void OnLastPlayerTurn();
    void PlayerTurnUIOff();
    void UpdateLosersScreen();
    void LoosersScreenLayoutSetUp();
    void AddWinnerEndGameDisplay();
}

#endregion Interfaces

#region State Struct Arguments

public struct DealerStateArguments
{
    public CardInfo[] DeckToDeal;
    public ICardReceiver[] Players;
    public Action OnDealerStateEnds;
}

public struct PlayerStateArguments
{
    public PlayerTurnStates TurnState;
    public bool IsMyTurn;

    public PlayerStateArguments(PlayerTurnStates turnState, bool isMyTurn)
    {
        TurnState = turnState;
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

#endregion State Struct Arguments

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
    public Transform CardsHolder;
}

public struct PlayerArguments
{
    //public PlayerRef PlayerRef;
    public string Name;
    public string ID;
    public byte IconID;
    public ulong ClientID;
    //public byte CardCounter;
    //public GameManager GameManager;
    public bool isplayerOut;
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

#endregion Struct Arguments

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
    public MeshRenderer CardRank;
    public TextMeshPro CardCounterText;
    //public SpriteRenderer CardPlate;
    //public SpriteRenderer CardBack;
}

[Serializable]
public struct PlayerUI
{
    public CardPositioner CardPositioner;
    public SpriteRenderer PlayerIcon;
    public TextMeshPro PlayerName;
}

[Serializable]
public struct PlayerTurnUI
{
    [Header("Parent")]
    public GameObject PlayerTurnUIManager;
    [Header("BackGround")]
    public GameObject BackGround;
    [Header("Ultimatum Screen")]
    public UltimatumScreenUI UltimatumScreenUI;
    [Header("Betting Screen")]
    public BettingScreenUI BettingScreenUI;
    [Header("Doubt Screen")]
    public DoubtScreenUI DoubtScreenUI;
    [Header("Looser Screen")]
    public LooserScreenUI LooserScreen;
}
[Serializable]
public struct BettingScreenUI
{
    public GameObject BettingScreen;
    public GameObject FirstBetLauncherText;
    public GameObject TimerHolder;
    public Image TimerImage;
    public Button BackButton;
    public TextMeshProUGUI PreviousBetSuitScore;
    public Transform PreviousBetSuitHolder;
    public TextMeshProUGUI MyBetSuitScore;
    public Transform MyBetSuitHolder;
    public Button SuggestBet;
    public Button MaxBet;
    public Button ClearBet;
    public Button Confirm;
}
[Serializable]
public struct UltimatumScreenUI
{
    public GameObject UltimatumScreen;
    public GameObject TimerHolder;
    public Image TimerImage;
    public GameObject PreviousBetPlayerDisplay;
    public Image PreviousBetPlayerIcon;
    public TextMeshProUGUI PreviousBetPlayerName;
    public GameObject PreviousBetSuit;
    public TextMeshProUGUI PreviousBetSuitScore;
    public Transform PreviousBetSuitHolder;
    public GameObject ButtonsContainer;
    public Button BetButton;
    public Button DoubtButton;
}
[Serializable]
public struct DoubtScreenUI
{
    public GameObject DoubtScreen;
    public Transform LeftPlayerDisplayHolder;
    public Transform RightPlayerDisplayHolder;
    public DoubtScreenPlayerDisplay LeftPlayerDisplay;
    public DoubtScreenPlayerDisplay RightPlayerDisplay;
}
[Serializable]
public struct LooserScreenUI
{
    public GameObject LooserPanel;
    public Transform LoosersHolder;
    public GameObject OutletTextHolder;
    public TextMeshProUGUI OutletText;
    public GameObject ButtonsHolder;
    public Button SpectateButton;
    public Button ExitButton;
}
[Serializable]
public struct DoubtScreenPlayerDisplay
{
    public GameObject DoubtPlayerDisplay;
    public Image PlayerIcon;
    public TextMeshProUGUI PlayerName;
    public GameObject DoubtStateHolder;
    public TextMeshProUGUI DoubtStateText;
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

#endregion Structs

#region enums

public enum SimulationSetUpState
{
    NoSetUp,
    LogicSetUp,
    UISetUp,
    SetUpComplete,
    SetUpCanceled
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
    Standard = 2,

    /// <summary>
    /// A Belote Deck will Contain Ranks from 7 to Ace excluding these ranks (2,3,4,5,6)
    /// </summary>
    Belote = 7,

    /// <summary>
    /// A Custom Deck that Can Have Any Ranks the players Choose with min of 8 Ranks
    /// </summary>
    Custom = 69
}
public enum GameMode
{
    //
    // Summary:
    //     Single Player Mode: it works very similar to Fusion.GameMode.Host Mode, but don't
    //     accept any connections.
    Single = 1,
    //
    // Summary:
    //     Shared Mode: starts a Game Client, which will connect to a Game Server running
    //     in the Photon Cloud using the Fusion Plugin.
    Shared,
    //
    // Summary:
    //     Server Mode: starts a Dedicated Game Server with no local player.
    Server,
    //
    // Summary:
    //     Host Mode: starts a Game Server and allows a local player.
    Host,
    //
    // Summary:
    //     Client Mode: starts a Game Client, which will connect to a peer in either Fusion.GameMode.Server
    //     or Fusion.GameMode.Host Modes.
    Client,
    //
    // Summary:
    //     Automatically start as Host or Client. The first peer to connect to a room will
    //     be started as a Host, all others will connect as clients.
    AutoHostOrClient
}
#endregion enums

public struct UILogsData
{
    public Color LogColor;
    public string Log;
}
public struct UIEventsArgs
{
    public UIManager UIManager;
    public GameMode GameMode;
}

[Serializable]
public struct MainPanelsUIRefs
{
    /// <summary>
    /// Parent to All Main Panels
    /// </summary>
    public GameObject MainPanelsGO;
    public GameObject ButtonsHolder;
    public Button CreateLobbyButton;
    public Button JoinLobbyButton;
    public Button ProfileButton;
   
    [Header("Create Lobby Panel")]
    public CreateLobbyUIRefs CreateLobbyUIRefs;

    [Header("Join Lobby Panel")]
    public JoinLobbyPanelUIRefs JoinLobbyPanelUIRefs;

    [Header("Join Private Lobby Panel")]
    public JoinPrivateLobbyPanelUIRefs JoinPrivateLobbyPanelUIRefs;

    [Header(" Join Public Lobby Panel")]
    public PublicLobbysUIRefs PublicLobbysUIRefs;

    [Header("Profile Panel")]
    public ProfilePanelUIRefs ProfilePanelUIRefs;
}

[Serializable]
public struct CreateLobbyUIRefs
{
    public GameObject CreateLobbyPanel;
    public TMP_InputField LobbyName;
    public TMP_Dropdown LobbyPrivacy;
    public Button CreateButton;
    public Button BackButton;
}

[Serializable]
public struct JoinLobbyPanelUIRefs
{
    public GameObject JoinLobbyPanel;
    public Button JoinPrivateLobbyButton;
    public Button PublicLobbysButton;
    public Button BackButton;
}

[Serializable]
public struct PublicLobbysUIRefs
{
    public GameObject PublicLobbysPanel;
    public GameObject PublicLobbysScrollGO;
    public Transform PublicLobbysHolders;
    public Button RefreshLobbysButton;
    public Button BackButton;
}
[Serializable]
public struct ProfilePanelUIRefs
{
    public GameObject ProfilePanel;
    public Button ProfileCloseButton;
}

[Serializable]
public struct JoinPrivateLobbyPanelUIRefs
{
    public GameObject JoinPrivateLobbyPanel;
    public TMP_InputField LobbyCode;
    public Button JoinButton;
    public Button BackButton;
}

public struct LobbyData
{
    public string LobbyName;
    public bool IsPrivate;
    public bool IsValid;

    public LobbyData(string lobbyName, bool isPrivate)
    {
        LobbyName = lobbyName;
        IsPrivate = isPrivate;
        IsValid = true;
    }
}
public enum GameState : byte
{
    NoGameState,

    /// <summary>
    /// just chilling, maybe waiting for clients, a host migration happening
    /// </summary>
    SimulationSetUp,

    GameStarted,
    Dealing,
    //FirstPlayerTurn,
    //PlayerTurn,
    //LastPlayerTurn,
    Doubting,
    RoudOver,
    GameOver,
    //HostMigration
}
public enum PlayerTurnStates
{
    NoState = 0,
    FirstPlayerTurn = 1,
    PlayerTurn = 2,
    LastPlayerTurn = 3,
}
public enum PlayerTimerStates
{
    NoTimer = 0,
    StartTimer = 1,
    StopTimer = -1
}

public struct PlayerUIState : INetworkSerializable
{
    public PlayerTurnStates PlayerTurnState;
    public PlayerTimerStates PlayerTimerState;

    public PlayerUIState(PlayerTurnStates turnState = PlayerTurnStates.NoState, PlayerTimerStates timerState = PlayerTimerStates.NoTimer)
    {
        PlayerTurnState = turnState;
        PlayerTimerState = timerState;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref PlayerTurnState);
        serializer.SerializeValue(ref PlayerTimerState);
    }
}

[System.Serializable]
public struct EndGamePlayerDisplayUIRefs
{
    public TextMeshProUGUI PlayerName;
    public Image PlayerIcon;
    public TextMeshProUGUI playerRank;
}

public struct EndGamePlayerDisplayData
{
    public FixedString64Bytes PlayerID;
    public FixedString32Bytes PlayerName;
    public int PlayerRank;
    public Sprite PlayerIcon;
}