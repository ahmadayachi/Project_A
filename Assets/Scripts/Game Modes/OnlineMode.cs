//#define STARTWITH13CARDS
//using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class OnlineMode : GameModeBase
{
    private Coroutine _confirmRoutine;
    public OnlineMode(GameModeARGS args)
    {
        _gameManager = args.GameManager;
    }

    public override void ConfirmBet(byte[] bet, FixedString64Bytes playerID)
    {
        _confirmRoutine = _gameManager.StartCoroutine(ConfirmRoutine(bet, playerID));

    }
    public override void DoubtBet(FixedString64Bytes playerID)
    {
        //blocking invalid args
        if (default == playerID)
        {
#if Log
            LogManager.LogError($"Blocking Doubt Rpc ! Invalid Args Found! CurrentPlayerID is :=>{_gameManager.CurrentPlayerID}");
#endif
            return;
        }

        if (_gameManager.Doubt == null)
        {
#if Log
            LogManager.LogError($"Blocking Doubt Rpc ! Doubt State is not Initialized! CurrentPlayerID is :=>{_gameManager.CurrentPlayerID}");
#endif
            return;
        }

        //only current player can Doubt
        if (_gameManager.CurrentPlayerID.Value != playerID)
        {
#if Log
            LogManager.Log($"Blocking Doubt Rpc ! player with ID:= {playerID} is not the Current Player!,Current Player ID:={_gameManager.CurrentPlayerID}", Color.red, LogManager.GameModeLogs);
#endif
            return;
        }

        //player cant doubt himself
        if (_gameManager.CurrentPlayerID == _gameManager.LiveBetPlayerID)
        {
#if Log
            LogManager.LogError($"Blocking Doubt Rpc ! player Cant Doubt himself ,Current Player ID:={_gameManager.CurrentPlayerID} Live Bet Player ID {_gameManager.LiveBetPlayerID}");
#endif
            return;
        }

        //stoping timer
        PlayerUIState playerUIState = new PlayerUIState();
        playerUIState.PlayerTurnState = PlayerTurnStates.NoState;
        playerUIState.PlayerTimerState = PlayerTimerStates.StopTimer;
        _gameManager.PlayerUIStates.Value = playerUIState;

        //invoking Doubt State
        byte[] liveBet = _gameManager.LiveBet.ToByteArray();
        _gameManager.DealtCardsList = _gameManager.DealtCards.ToByteList();
        DoubtStateArguments stateArguments = new DoubtStateArguments(_gameManager.DealtCardsList, liveBet);
        _gameManager.ChangeState(_gameManager.Doubt, stateArguments);
    }
    public override void PassTurn()
    {
        //Server Only Bitch
        if (!_gameManager.IsHost)
        {
#if Log
            LogManager.Log($"Ignoring Passing Turn ! this is a Client!{_gameManager.LocalPlayer}", Color.yellow, LogManager.GameModeLogs);
#endif
            return;
        }

        if (_gameManager.Players.IsNullOrHaveNullElements())
        {
#if Log
            LogManager.LogError("Failed Passing Turn ! Players Array is Null or Have Null Elements");
#endif
            return;
        }
        if (IsGameOver())
        {
#if Log
            LogManager.Log("Failed Passing Turn ! Game should be Over !", Color.red, LogManager.GameModeLogs);
#endif
            return;
        }

        int currentPlayerIndex = _gameManager.PlayerIndex;
        IPlayer player = null;
        do
        {
            //Moving player Indexer
            NextPlayerIndex();

            try
            {
                player = _gameManager.Players[_gameManager.PlayerIndex];
            }
            catch (Exception ex)
            {
#if Log
                LogManager.LogError("Failed Passing Turn!" + ex.Message);
#endif
                return;
            }
        } while (NeedToLookForPlayers(ref currentPlayerIndex, player));

        //final check for player
        bool loopedArray = (currentPlayerIndex == _gameManager.PlayerIndex);

        if (loopedArray || player.IsOut)
        {
#if Log
            LogManager.LogError($"Failed Passing Turn! looped array{loopedArray}/  Player :{player}");
#endif
            return;
        }

        //setting current player
#if Log
        LogManager.Log($"Turn Passed! current player is {player} ", Color.green, LogManager.GameModeLogs);
#endif
        _gameManager.CurrentPlayer = player;
        _gameManager.CurrentPlayerID.Value = player.ID;

        //if singlePlayer invoke shit here
        //if (_gameManager.GameMode == GameMode.Single)
        //{
        //    //Idk Ui shit or smth
        //}
    }
    public override void StartGame()
    {

        //Initialising players
        InitPlayers();

        UploadDeckInfo();

        //Create CardManager
        _gameManager.SetUpCardManager();

        //uploading max cards a player can get
        SetMaxPlayerCards();

        //simulation Prep
        _gameManager.State.Value = GameState.SimulationSetUp;
    }
    public override void StartSimulationSetUp()
    {
        if (_gameManager.SimulationSetUpRoutine != null)
            _gameManager.StopCoroutine(_gameManager.SimulationSetUpRoutine);
        _gameManager.SimulationSetUpRoutine = _gameManager.StartCoroutine(SetUp());
    }
    public override bool IsGameOver()
    {
        if (_gameManager.Players == null || _gameManager.Players.Length == 0) return false;
        int counter = 0;
        for (int index = 0; index < _gameManager.Players.Length; index++)
        {
            if (!_gameManager.Players[index].IsOut)
                counter++;
        }
        if (counter == 1) return true;
        return false;
    }
    public override void SetGameState(GameState state)
    {
        switch (state)
        {
            case GameState.SimulationSetUp: SimulationPrepGameState(); break;
            case GameState.GameStarted: _gameManager.CallBackManager.EnqueueOrExecute(GameStarted, nameof(GameStarted)); break;
            case GameState.Dealing: _gameManager.CallBackManager.EnqueueOrExecute(Dealing, nameof(Dealing)); break;
            case GameState.Doubting: _gameManager.CallBackManager.EnqueueOrExecute(Doubting, nameof(Doubting)); break;
            case GameState.RoudOver: _gameManager.CallBackManager.EnqueueOrExecute(RoundOver, nameof(RoundOver)); break;
            case GameState.GameOver: _gameManager.CallBackManager.EnqueueOrExecute(GameOver, nameof(GameOver)); break;          
        }
    }
    public override void StartPlayerState()
    {
        //blocking resets
        if (_gameManager.PlayerUIStates.Value.PlayerTimerState == PlayerTimerStates.NoTimer)
            return;
        //at this time each simulation should Have a Current Player
        if (_gameManager.CurrentPlayer == null)
        {
#if Log
            LogManager.LogError($"Failed Player Turn CallBack! {_gameManager.LocalPlayer} Current Player is null");
#endif
            return;
        }
       
        //game state should a player turn states
        if (_gameManager.PlayerUIStates.Value.PlayerTimerState == PlayerTimerStates.StopTimer)
        {
            //TODO : check if the current state is a player state 
            _gameManager.CurrentState?.ForceEnd();
#if Log
            LogManager.Log($"Player Timer Stoped!Simulation=> {_gameManager.LocalPlayer}", Color.green, LogManager.ValueInformationLog);
#endif
            return;
        }

#if Log
        LogManager.Log($"Starting Player State! Current player state! {_gameManager.CurrentPlayer}//Simulation=> {_gameManager.LocalPlayer}", Color.green, LogManager.ValueInformationLog);
#endif
        PlayerStateArguments PlayerStateArgs = new PlayerStateArguments(_gameManager.PlayerUIStates.Value.PlayerTurnState, _gameManager.IsMyTurn());
        _gameManager.ChangeState(_gameManager.CurrentPlayer.PlayerState, PlayerStateArgs);
    }
    public override void LoadCurrentPlayer()
    {
        if (_gameManager.CurrentPlayerID.Value == string.Empty) return;

        //if host is updated return
        if (_gameManager.IsHost)
        {
            if (_gameManager.CurrentPlayer != null && _gameManager.CurrentPlayer.ID == _gameManager.CurrentPlayerID.Value)
            {
#if Log
                LogManager.Log("Loading Current Player is Skipped !, Host is Already Updated", Color.grey, LogManager.ValueInformationLog);
#endif
                //starting Player State
                //if (_gameManager.State.Value == GameState.PlayerTurn)
                //    StartPlayerTimer();
                return;
            }
        }
        //looking for desired payer
        IPlayer newCurrentPlayer = null;
        if (TryFindPlayer(_gameManager.CurrentPlayerID.Value, out newCurrentPlayer))
        {
#if Log
            LogManager.Log($"Loading Current Player={newCurrentPlayer}!// Simulatio=>{_gameManager.LocalPlayer}", Color.green, LogManager.ValueInformationLog);
#endif
            _gameManager.CurrentPlayer = newCurrentPlayer;
            //should invoke corresponding UI or something

            //starting Player State
            //if (_gameManager.State.Value == GameState.PlayerTurn)
            //    StartPlayerTimer();
        }
        else
        {
#if Log
            LogManager.LogError($"Failed updating Current Player! Cant Find  Player with ID:=> {_gameManager.CurrentPlayerID}");
#endif
            return;
        }
    }
    public override void DoubtOverLogic()
    {
        //punishing Doubt looser
        FixedString64Bytes playerToPunishID;
        IPlayer playerToPunish;
        PunishingDoubtLooser(out playerToPunishID, out playerToPunish);

        //setting the Current Player
        _gameManager.CurrentPlayerID.Value = playerToPunishID;
        _gameManager.CurrentPlayer = playerToPunish;
        //setting the current player index 
        _gameManager.PlayerIndex = IndexOfPlayer(playerToPunish.ID);
        //Player Control
        PlayerControl();

        //Directing Game State
        _gameManager.State.Value = GameState.RoudOver;
    }
    /// <summary>
    /// Might return -1 if no such player is found 
    /// </summary>
    /// <param name="playerID"></param>
    /// <returns></returns>
    private int IndexOfPlayer(FixedString64Bytes playerID)
    {
        for (int index = 0; index < _gameManager.Players.Length; index++)
        {
            if (_gameManager.Players[index].ID == playerID)
                return index;
        }
        return -1;
    }
    public override void DoubtLogic(DoubtState doubtState)
    {
        //updating the DoubtState , removeable maybe needed for ui
        _gameManager.DoubtState.Value = doubtState;

        // Calculate and Set Doubt Scene Time
        CaluCulateDoubtSceneTimer();

        // Updating Clients and Host UI
        _gameManager.State.Value = GameState.Doubting;
    }
    public override List<DiffusedRankInfo> RoundUpCurrentBet()
    {
        var diffusedBet = new List<DiffusedRankInfo>();
        byte[] currentBet = _gameManager.LiveBet.ToByteArray();

        //making sure bet is sorted 
        if (!currentBet.IsEmpty())
        {
            Extention.BetDiffuser(currentBet, diffusedBet);
            currentBet = diffusedBet.ToByteArray();
        }

        byte[] roundedUpBet;

        if (BetGenerator.TryRoundUpBet(currentBet, out roundedUpBet, (byte)_gameManager.DealtCardsNumber.Value))
        {
            Extention.BetDiffuser(roundedUpBet, diffusedBet);
        }
        else
        {
#if Log
            LogManager.Log($"{_gameManager.LocalPlayer} Failed Rounding Up Bet=>{currentBet}!", Color.yellow, LogManager.GameModeLogs);
#endif
            return null;
        }
        return diffusedBet;
    }

    #region gameStates 
    protected override void SimulationPrepGameState()
    {
        StartSimulationSetUp();

        if (_gameManager.IsHost)
        {
            if (_gameManager.WaitSetUpThenWaitPlayersRoutine != null)
                _gameManager.StopCoroutine(_gameManager.WaitSetUpThenWaitPlayersRoutine);
            _gameManager.WaitSetUpThenWaitPlayersRoutine = _gameManager.StartCoroutine(WaitSetUpThenWaitPlayers());
        }
    }
    protected override void GameStarted()
    {
        _gameManager.UIManager.ActiveUIEvents.OnGameStarted();
        if (_gameManager.IsHost)
        {
            if (_gameManager.WaitingGameStartedAnimationRoutine != null)
                _gameManager.StopCoroutine(_gameManager.WaitingGameStartedAnimationRoutine);
            _gameManager.WaitingGameStartedAnimationRoutine = _gameManager.StartCoroutine(WaitForPlayersGameStartedAnimation());
        }
    }
    protected override void Dealing()
    {
        _gameManager.UIManager.ActiveUIEvents.OnDealingCards();
        if (_gameManager.IsHost)
        {
            DealerStateArguments args = new DealerStateArguments();
            args.DeckToDeal = CardManager.Deck;
            args.Players = _gameManager.Players;
            args.OnDealerStateEnds = OnDealingOver;
            _gameManager.ChangeState(_gameManager.Dealer, args);
        }
    }
    protected override void OnDealingOver()
    {
        //maybe some other UI Shit here
        _gameManager.DealtCardsNumber.Value = DealtCardsCounter();
        CollectDealtCards();

        PlayerUIState playerUIState = new PlayerUIState();
        playerUIState.PlayerTurnState = PlayerTurnStates.FirstPlayerTurn;
        playerUIState.PlayerTimerState = PlayerTimerStates.StartTimer;

        _gameManager.PlayerUIStates.Value = playerUIState;
    }
    protected override void Doubting()
    {
        //Doubting Scene Or Something
        _gameManager.UIManager.ActiveUIEvents.OnDoubting();
    }
    protected override void RoundOver()
    {
        //regular UI Cleaning Stuff
        _gameManager.UIManager.ActiveUIEvents.OnRoundOver();
        //TODO : Link with UI
        //cleaing Cards , link to On ROund Over
        //_gameManager.CardPool.DestroyAll();
       // foreach (var player in _gameManager.Players)
       //     player.PlayerUI.CardPositioner.ClearLoadedCards();
        if (_gameManager.IsHost)
            OnRoundIsOverLogic();
    }
    protected override void OnRoundIsOverLogic()
    {
        //checking if the game is over
        if (IsGameOver())
        {
            //fetch winner
            IPlayer Winner = _gameManager.Players.First(player => (!player.IsOut));
            if (Winner == null)
            {
#if Log
                LogManager.LogError("Failed Fetching Winner!");
#endif
                return;
            }
            //setting the Winner ID
            _gameManager.WinnerID.Value = new FixedString64Bytes(Winner.ID);
            Winner.SetIsPlayerWinner(true);
            //directing Game State and updating clients
            _gameManager.State.Value = GameState.GameOver;
            //Maybe game Over stuff here
#if Log
            LogManager.Log($"Game Over! Winner is {Winner.ID}!", Color.green, LogManager.ValueInformationLog);
#endif
        }
        else
        {
            //clearing
            RoundOverVariablesCleaning();
            //directing game state
            _gameManager.State.Value = GameState.Dealing;
        }
    }
    protected override void RoundOverVariablesCleaning()
    {
        _gameManager.LiveBetPlayerID.Value = string.Empty;
        _gameManager.LiveBet.Clear();
        _gameManager.DiffusedBet.Clear();
        _gameManager.DoubtSceneTimer.Value = 0;
        _gameManager.DealtCards.Clear();
        _gameManager.DealtCardsList.Clear();
        _gameManager.DealtCardsNumber.Value = 0;
        //clearing Players Hand
        foreach (var player in _gameManager.Players)
        {
            player.ClearHand();
        }
        _gameManager.DoubtState.Value = DoubtState.NoDoubting;
    }
    protected override void GameOver()
    {
        _gameManager.UIManager.ActiveUIEvents.OnGameOver();
        //cleaing Cards
       // _gameManager.CardPool.DestroyAll();

        if (_gameManager.IsHost)
        {
            RoundOverVariablesCleaning();
        }
    }
    //protected override void StartPlayerTimer()
    //{
    //    if (_gameManager.IsHost)
    //        _gameManager.SetPlayerTimerState(PlayerTimerStates.StartTimer);
    //}
    protected override void PlayerControl()
    {
        if (_gameManager.Players.IsNullOrHaveNullElements())
        {
#if Log
            LogManager.LogError("Failed Finding Player! Players Array is Null or Have Null Elements");
#endif
            return;
        }
        //if a player cards to deal counter > max cards Count he should be out
        foreach (var player in _gameManager.Players)
        {
            if (player.CardsToDealCounter > _gameManager.MaxPlayerCards.Value)
            {
                if (!player.IsOut)
                {
                    _gameManager.LoosersIDs.AddPlayerID(player.ID);
                    player.SetIsplayerOut(true);
                  //  player.ClearHand();
                   // player.ClearCardsCounter();
                }
            }
        }
    }
    #endregion

    #region private Swamp
    private IEnumerator ConfirmRoutine(byte[] bet, FixedString64Bytes playerID)
    {
        yield return BetValidation(bet, playerID);

        //stoping timer
        PlayerUIState playerUIState = new PlayerUIState();
        playerUIState.PlayerTurnState = PlayerTurnStates.NoState;
        playerUIState.PlayerTimerState = PlayerTimerStates.StopTimer;

        _gameManager.PlayerUIStates.Value = playerUIState;

        yield return new WaitForSeconds(0.2f);

        byte[] sortedBet = SetLiveBet(bet, playerID);

        yield return new WaitForSeconds(0.2f);
        //generating a Max Bet
        byte[] MaxBet = BetGenerator.GenerateMaxBet(_gameManager.DealtCardsNumber.Value);

        //cheking if the Played Bet is a Max Bet
        if (MaxBet.AreEqual(sortedBet))
        {
            //Directing the Game To an Auto Doubt State
            _gameManager.DealtCardsList = _gameManager.DealtCards.ToByteList();
            DoubtStateArguments stateArguments = new DoubtStateArguments(_gameManager.DealtCardsList, sortedBet);

            //passing Turn here
            PassTurn();

            _gameManager.ChangeState(_gameManager.Doubt, stateArguments);
#if Log
            LogManager.Log($"Auto Doubt is Launched!, Current Player {_gameManager.CurrentPlayerID} Live Bet Player ID {_gameManager.LiveBetPlayerID}", Color.blue, LogManager.GameModeLogs);
#endif
            yield break;
        }

        //checking if the next Current Player Have to Play a Max Bet
        byte[] roundedUpBet;
        if (BetGenerator.TryRoundUpBet(sortedBet, out roundedUpBet, _gameManager.DealtCardsNumber.Value))
        {
            //cheking if the rounded up bet is a max Bet
            if (MaxBet.AreEqual(roundedUpBet))
            {
#if Log
                LogManager.Log($" changing Game State to Last Player turn after confirming Bet!, Current Player {_gameManager.CurrentPlayerID} Live Bet Player ID {_gameManager.LiveBetPlayerID}", Color.green, LogManager.GameModeLogs);
#endif
                

                //passing Turn here
                PassTurn();

                yield return new WaitForSeconds(0.2f);

                //directing Game State to a Last Player Game State
                playerUIState = new PlayerUIState();
                playerUIState.PlayerTurnState = PlayerTurnStates.LastPlayerTurn;
                playerUIState.PlayerTimerState = PlayerTimerStates.StartTimer;

                _gameManager.PlayerUIStates.Value = playerUIState;

                yield break;
            }
        }
        //abbording everythink if a bet cannot be Rounded Up
        else
        {
#if Log
            LogManager.LogError($"Failed Confirm Rpc ! Failed Rounding Up Current Bet! CurrentPlayerID is :=>{_gameManager.CurrentPlayerID}");
#endif
            yield break;
        }
#if Log
        LogManager.Log($" changing Game State to Player turn after confirming Bet!, Current Player {_gameManager.CurrentPlayerID.Value} Live Bet Player ID {_gameManager.LiveBetPlayerID}", Color.green, LogManager.GameModeLogs);
#endif
       
        //passing Turn here
        PassTurn();

        //yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(0.2f);

        playerUIState = new PlayerUIState();
        playerUIState.PlayerTurnState = PlayerTurnStates.PlayerTurn;
        playerUIState.PlayerTimerState = PlayerTimerStates.StartTimer;

        _gameManager.PlayerUIStates.Value = playerUIState;
    }

    private byte[] SetLiveBet(byte[] bet, FixedString64Bytes playerID)
    {
        //making sure the array is sorted before confirming
        Extention.BetDiffuser(bet, _gameManager.DiffusedBet);
        byte[] sortedBet = _gameManager.DiffusedBet.ToByteArray();
        int sortedBetLength = sortedBet.Length;

        //cleaning Network Array      
        _gameManager.LiveBet.Clear();

        //adding Bet
        for (int index = 0; (index < sortedBetLength); index++)
        {
            _gameManager.LiveBet.Add(sortedBet[index]);
        }

        //setting live bet player id
        _gameManager.LiveBetPlayerID.Value = playerID;
        return sortedBet;
    }

//    private IEnumerator CheckMaxBet(byte[] sortedBet)
//    {
//        //generating a Max Bet
//        byte[] MaxBet = BetGenerator.GenerateMaxBet(_gameManager.DealtCardsNumber.Value);

//        //cheking if the Played Bet is a Max Bet
//        if (MaxBet.AreEqual(sortedBet))
//        {
//            //Directing the Game To an Auto Doubt State
//            _gameManager.DealtCardsList = _gameManager.DealtCards.ToByteList();
//            DoubtStateArguments stateArguments = new DoubtStateArguments(_gameManager.DealtCardsList, sortedBet);

//            _gameManager.ChangeState(_gameManager.Doubt, stateArguments);
//#if Log
//            LogManager.Log($"Auto Doubt is Launched!, Current Player {_gameManager.CurrentPlayerID} Live Bet Player ID {_gameManager.LiveBetPlayerID}", Color.blue, LogManager.GameModeLogs);
//#endif
//            yield break;
//        }

//        //checking if the next Current Player Have to Play a Max Bet
//        byte[] roundedUpBet;
//        if (BetGenerator.TryRoundUpBet(sortedBet, out roundedUpBet, _gameManager.DealtCardsNumber.Value))
//        {
//            //cheking if the rounded up bet is a max Bet
//            if (MaxBet.AreEqual(roundedUpBet))
//            {
//#if Log
//                LogManager.Log($" changing Game State to Last Player turn after confirming Bet!, Current Player {_gameManager.CurrentPlayerID} Live Bet Player ID {_gameManager.LiveBetPlayerID}", Color.green, LogManager.GameModeLogs);
//#endif
//                //directing Game State to a Last Player Game State
//                _gameManager.State.Value = GameState.LastPlayerTurn;
//                yield break;
//            }
//        }
//        //abbording everythink if a bet cannot be Rounded Up
//        else
//        {
//#if Log
//            LogManager.LogError($"Failed Confirm Rpc ! Failed Rounding Up Current Bet! CurrentPlayerID is :=>{_gameManager.CurrentPlayerID}");
//#endif
//            yield break;
//        }
//    }

    private IEnumerator BetValidation(byte[] bet, FixedString64Bytes playerID)
    {
        //blocking invalid args
        if (default == playerID || bet == null)
        {
#if Log
            LogManager.LogError($"Blocking Confirm Rpc ! Invalid Args Found! CurrentPlayerID is :=>{_gameManager.CurrentPlayerID}");
#endif
            yield break;
        }
        //only current player can confirm bet
        if (_gameManager.CurrentPlayerID.Value != playerID)
        {
#if Log
            LogManager.Log($"Blocking Confirm Rpc ! player with ID:= {playerID} is not the Current PLayer!,Current PLayer ID:={_gameManager.CurrentPlayerID}", Color.red, LogManager.GameModeLogs);
#endif
            yield break;
        }
        //the previous bet should always be sorted
        byte[] liveBet = _gameManager.LiveBet.ToByteArray();
        ValidatorArguments betArgs = new ValidatorArguments(bet, liveBet, _gameManager.DealtCardsNumber.Value);
        bool isValid = _gameManager.BetHandler.ChainValidateBet(betArgs);
        //bet has to be valid
        if (!isValid)
        {
#if Log
            LogManager.Log($"Blocking Confirm Rpc ! player with ID:= {playerID} Sent an Invalid Bet!, Bet=:{string.Join(",", liveBet)}", Color.red, LogManager.GameModeLogs);
#endif
            yield break;
        }
    }
    private IEnumerator WaitForPlayersGameStartedAnimation()
    {
        yield return WaitPlayers();
#if Log
        LogManager.Log("All Players GameStarted Animation Exectuted", Color.green, LogManager.ValueInformationLog);
#endif
        //moving to dealing state
        _gameManager.State.Value = GameState.Dealing;
    }
    private IEnumerator SetUp()
    {
        //some panel that tracks simulation states as a loading screen
        _gameManager.UIManager.ActiveUIEvents?.OnSetUpStarted();

        //Logic Set up
        yield return SimulationLogicSetUp();

        //just being a protective trans mother
        yield return new WaitUntil(() => _gameManager.SimulationState == SimulationSetUpState.LogicSetUp);

        //UI Set Up
        yield return _gameManager.UIManager.ActiveUIEvents?.SetUpUI();

        yield return new WaitUntil(() => _gameManager.SimulationState == SimulationSetUpState.UISetUp);

        _gameManager.SimulationState = SimulationSetUpState.SetUpComplete;

        _gameManager.CallBackManager.SetReady(true);
        if (!_gameManager.IsHost)
            _gameManager.SyncFirstTick();

        _gameManager.SimulationSetUpRoutine = null;

        if (_gameManager.State.Value == GameState.SimulationSetUp)
            _gameManager.LocalPlayer.PlayerIsReady();
    }
    private IEnumerator SimulationLogicSetUp()
    {
        if (_gameManager.IsHost)
        {
            if (_gameManager.Players == null)
            {
                LoadDeckInfo();
                _gameManager.SetUpCardManager();
                LoadPlayers();
                SetUpRunTimeData();
            }
            _gameManager.CreateDealer();
            _gameManager.CreateDoubt();
        }
        else
        {
            LoadDeckInfo();
            _gameManager.SetUpCardManager();
            LoadPlayers();
            SetUpRunTimeData();
        }
        _gameManager.CreateCardPool();
        _gameManager.CreateBetHandler();

        yield return null;

        //forcing waiting minimum one sec
#if Log
        LogManager.Log($"Waiting for Logic Set Up Runner Player Ref => {NetworkManager.Singleton.LocalClientId}", Color.yellow, LogManager.ValueInformationLog);
#endif
        int timer = 0;
        do
        {
            yield return new WaitForSeconds(1);
            timer++;
        } while ((!CheckLogicSetUp()) && timer <= GameManager.MaxSetUpWaitTime);

        // one more time !
        if (CheckLogicSetUp())
        {
            _gameManager.SimulationState = SimulationSetUpState.LogicSetUp;
#if Log
            LogManager.Log($" Logic is Set Up Local Player => {_gameManager.LocalPlayer}", Color.green, LogManager.ValueInformationLog);
#endif
        }
        else
        {
            //stop the whole process
            _gameManager.StopCoroutine(_gameManager.SimulationSetUpRoutine);
            _gameManager.SimulationState = SimulationSetUpState.SetUpCanceled;
#if Log
            LogManager.LogError($"Simulation Set Up is Canceled! Logic Set Up Failed! Ref => {NetworkManager.Singleton.LocalClientId}");
#endif
        }
    }
    private void SetUpRunTimeData()
    {
        if (_gameManager.Players == null || _gameManager.Players.Count() == 0)
        {
#if Log
            LogManager.LogError("Run Time Data Set Up Is Canceled!");
#endif
            return;
        }

        //just making sure
        _gameManager.RunTimeDataHolder.RunTimePlayersData.Clear();

        foreach (IPlayer player in _gameManager.Players)
        {
            RunTimePlayerData playerData = new RunTimePlayerData();
            //  playerData.PlayerRef = player.playerRef;
            playerData.ClientID = player.ClientID;
            playerData.PlayerName = player.Name;
            playerData.PlayerID = player.ID;
            playerData.IconIndex = player.IconID;
            playerData.PlayerNetObjectRef = new NetworkObjectReference(player.PlayerNetworkObject);
            //since this only happens after spawning players im assuming it should be true
            playerData.AuthorityAssigned = true;

            _gameManager.RunTimeDataHolder.RunTimePlayersData.Add(playerData);
        }
    }
    /// <summary>
    /// Initializes The Players Array Data
    /// </summary>
    private void LoadPlayers()
    {
        if (_gameManager.CloudplayersData.IsEmpty())
        {
#if Log
            LogManager.Log($"No Data In Cloud Found! Loading Player for this Client, Client ID=> {NetworkManager.Singleton.LocalClientId} is Canceled", Color.cyan, LogManager.ValueInformationLog);
#endif
            return;
        }
        int playersCount = _gameManager.CloudplayersData.Count;
        _gameManager.Players = new IPlayer[playersCount];
        _gameManager.PlayersNumber = playersCount;
        int playerIndex = 0;
        //players need to be spawned before Fetching
        foreach (NetworkObject playerNetObject in _gameManager.CloudplayersData)
        {
            if (Extention.IsObjectUsable(playerNetObject))
            {
                Player player = playerNetObject.GetComponent<Player>();
                if (player == null)
                {
#if Log
                    LogManager.LogError($"Player Loading Is Canceled! Cloud Player Data does not Contain a Player Component ! Local Player {_gameManager.LocalPlayer}");
#endif
                    return;
                }
                player.BondPlayerSimulation(_gameManager);
                //setting local player
                _gameManager.SetLocalPlayer(player);

                //storing player on local simulation
                _gameManager.Players[playerIndex++] = player;
            }
        }
    }
    private void LoadDeckInfo()
    {
        _gameManager.RunTimeDataHolder.DeckInfo = new DeckInfo();
        _gameManager.RunTimeDataHolder.DeckInfo.DeckType = _gameManager.DeckType.Value;
        _gameManager.RunTimeDataHolder.DeckInfo.SuitsNumber = _gameManager.SuitsNumber.Value;
        if (_gameManager.DeckType.Value == DeckType.Custom)
        {
            if (_gameManager.CustomSuitRanks.IsEmpty())
            {
#if Log
                LogManager.LogError("Loading Deck Info is Canceled ! Custom Deck Suit Ranks is Empty ");
#endif
                return;
            }
            _gameManager.RunTimeDataHolder.DeckInfo.CustomSuitRanks = new byte[_gameManager.CustomSuitRanks.ValidCardsCount()];
            int index = 0;
            foreach (byte card in _gameManager.CustomSuitRanks)
            {
                if (card != 0)
                    _gameManager.RunTimeDataHolder.DeckInfo.CustomSuitRanks[index++] = card;
            }
        }
    }
    private bool CheckLogicSetUp()
    {
        bool checkState = true;
        if (CardManager.Deck == null)
        {
#if Log
            LogManager.Log($"Deck is null !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }
        if (_gameManager.Players == null)
        {
#if Log
            LogManager.Log($"Players array is null  !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }

        if (_gameManager.Players.Count() == 0)
        {
#if Log
            LogManager.Log($"Players array is Empty  !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }

        if (_gameManager.PlayersNumber == 0)
        {
#if Log
            LogManager.Log($"Player Number Is Invalid !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }

        if (_gameManager.LocalPlayer == null)
        {
#if Log
            LogManager.Log($"Local Player Is null !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }
        if (_gameManager.RunTimeDataHolder.RunTimePlayersData == null)
        {
#if Log
            LogManager.Log($"RunTimePlayersData Is null !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }
        if (_gameManager.RunTimeDataHolder.RunTimePlayersData.Count != _gameManager.PlayersNumber)
        {
#if Log
            LogManager.Log($"RunTimePlayersData Count is Invalid !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }
        if (_gameManager.CardPool == null)
        {
#if Log
            LogManager.Log($"cardsPool Is null !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }
        if (_gameManager.BetHandler == null)
        {
#if Log
            LogManager.Log($"betHandler Is null !", Color.red, LogManager.ValueInformationLog);
#endif
            checkState = false;
        }

        if (_gameManager.IsHost)
        {
            if (_gameManager.Dealer == null)
            {
#if Log
                LogManager.Log($"dealer Is null !", Color.red, LogManager.ValueInformationLog);
#endif
                checkState = false;
            }
            if (_gameManager.Doubt == null)
            {
#if Log
                LogManager.Log($"doubt Is null !", Color.red, LogManager.ValueInformationLog);
#endif
                checkState = false;
            }
        }

        return checkState;
    }
    private IEnumerator WaitSetUpThenWaitPlayers()
    {
        yield return WaitSetUp();

        //if Host Set Up Complete then Wait for players
        if (_gameManager.SimulationSetUpSuccessfull)
        {
#if Log
            LogManager.Log("Host Waiting For Players Simulation Set Up", Color.yellow, LogManager.ValueInformationLog);
#endif

            yield return WaitPlayers();
#if Log
            LogManager.Log("All Players Simulation is Set Up", Color.green, LogManager.ValueInformationLog);
#endif
            //Moving tto Game Started Game State
            _gameManager.State.Value = GameState.GameStarted;
        }
        else
        {
            //still dont know maybe disconnect? reload scene?
        }
        _gameManager.WaitSetUpThenWaitPlayersRoutine = null;
    }
    /// <summary>
    /// wait players then clears the players List
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitPlayers()
    {
        //waiting players RPC
        yield return new WaitUntil(_gameManager.AllPlayersReady);
        //reseting
        _gameManager.PlayerReadyList.Clear();
    }
    private IEnumerator WaitSetUp()
    {
        _gameManager.SimulationSetUpSuccessfull = false;
        bool SetUpCanceled;
        do
        {
            yield return new WaitForSeconds(1);
            _gameManager.SimulationSetUpSuccessfull = _gameManager.SimulationState == SimulationSetUpState.SetUpComplete;
            SetUpCanceled = _gameManager.SimulationState == SimulationSetUpState.SetUpCanceled;
        } while (!_gameManager.SimulationSetUpSuccessfull && !SetUpCanceled);
    }
    private void SetMaxPlayerCards()
    {
        if (_gameManager.PlayersNumber == 0)
        {
#if Log
            LogManager.LogError("player number need to be > 0 before setting the max player cards ");
#endif
            return;
        }
        byte playerCards = 1;
        int currentDeckSize = CardManager.Deck.Length;
        while ((currentDeckSize - (playerCards * _gameManager.PlayersNumber) > 0))
        {
            playerCards++;
        }
        _gameManager.MaxPlayerCards.Value = (byte)(playerCards - 1);
    }
    private void UploadDeckInfo()
    {
        _gameManager.DeckType.Value = _gameManager.RunTimeDataHolder.DeckInfo.DeckType;
        _gameManager.SuitsNumber.Value = _gameManager.RunTimeDataHolder.DeckInfo.SuitsNumber;
        if (_gameManager.DeckType.Value == DeckType.Custom)
        {
            if (_gameManager.RunTimeDataHolder.DeckInfo.CustomSuitRanks.IsEmpty())
            {
#if Log
                LogManager.LogError("Uploading Deck Info is Canceled ! Custom Deck Suit Ranks is Empty ");
#endif
                return;
            }
            _gameManager.CustomSuitRanks.Clear();
            foreach (byte card in _gameManager.RunTimeDataHolder.DeckInfo.CustomSuitRanks)
            {
                if (card != 0)
                    _gameManager.CustomSuitRanks.Add(card);
            }
        }
    }
    private void InitPlayers()
    {
        //canceling if not enough run time data
        int dataCount = _gameManager.RunTimeDataHolder.RunTimePlayersData.Count;
        if (dataCount < 2)
        {
#if Log
            LogManager.LogError($"Players Init Is Canceled ! not enough Run Time Data , Run Time Data Count = {dataCount}");
#endif
            return;
        }

        int playerIndex = 0;
        _gameManager.Players = new IPlayer[dataCount];
        _gameManager.PlayersNumber = dataCount;
        List<RunTimePlayerData> newRunTimeData = new List<RunTimePlayerData>();
        foreach (RunTimePlayerData playerData in _gameManager.RunTimeDataHolder.RunTimePlayersData)
        {
            //spawping player
            var playerGameObject = _gameManager.Insttantiate(AssetLoader.PrefabContainer.PlayerPrefab);
            NetworkObject playerObject = playerGameObject.GetComponent<NetworkObject>();
            playerObject.SpawnAsPlayerObject(playerData.ClientID);
            playerObject.name = playerData.PlayerName;
            Player player = playerObject.GetComponent<Player>();

            //hooking player with simulation
            player.BondPlayerSimulation(_gameManager);

            //player prep
            PlayerArguments playerArgs = new PlayerArguments();
            //playerArgs.PlayerRef = playerData.PlayerRef;
            playerArgs.Name = playerData.PlayerName;
            playerArgs.ID = playerData.PlayerID;
            playerArgs.IconID = (byte)playerData.IconIndex;
            //playerArgs.GameManager = this;
            playerArgs.isplayerOut = false;
            player.InitPlayer(playerArgs);

#if STARTWITH13CARDS
            for (int index = 0; index < 2; index++)
            {
                player.PlusOneCard();
            }
#endif
            //RunTime Data Adjust
            RunTimePlayerData newData = new RunTimePlayerData();
            //newData.PlayerRef = playerData.PlayerRef;
            newData.PlayerName = playerData.PlayerName;
            newData.PlayerID = playerData.PlayerID;
            newData.IconIndex = playerData.IconIndex;
            newData.PlayerNetObjectRef = new NetworkObjectReference(playerObject);
            newData.AuthorityAssigned = true;
            newRunTimeData.Add(newData);

            // setting Local Player
            _gameManager.SetLocalPlayer(player);

            //uploading player netobject on cloud
            _gameManager.CloudplayersData.Add(newData.PlayerNetObjectRef);

            //stroing player on local simulation
            _gameManager.Players[playerIndex] = player;
            playerIndex++;
        }
        //resetting RunTime Data
        _gameManager.RunTimeDataHolder.RunTimePlayersData.Clear();
        _gameManager.RunTimeDataHolder.RunTimePlayersData.AddRange(newRunTimeData);
        //setting the first player
        _gameManager.CurrentPlayerID.Value = new FixedString64Bytes(_gameManager.Players[0].ID);
        _gameManager.CurrentPlayer = _gameManager.Players[0];
    }

    #endregion
}
