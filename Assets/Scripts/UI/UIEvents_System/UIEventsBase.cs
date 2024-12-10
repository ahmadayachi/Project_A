using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

public abstract class UIEventsBase : IUIEvents
{
    protected UIManager _uiManager;
    public const string Zeros = "0000";
    public const string Winner = "Winner";
    public const string Looser = "Looser";
    protected List<DisplayCard> _myDisplayCards= new List<DisplayCard>();
    protected List<DisplayCard> _previousPlayerDisplayCards = new List<DisplayCard>();
    protected Coroutine _doubtSceneRoutine;
    #region Const
    private const string PlayerUIPlacementSettingAddr = "PlayerUIPlacementSetting";
    #endregion

    #region Base GameStates CallBacks
    /// <summary>
    /// Sets Up the simulation UI
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerator SetUpUI();
    public abstract void OnGameStarted();
    public abstract void OnDealingCards();
    public abstract void OnDoubting();
    public abstract void OnGameOver();
    public abstract void OnRoundOver();
    public abstract void OnSetUpStarted();
    public abstract void OnFirstPlayerTurn();
    public abstract void OnPlayerTurn();
    public abstract void OnLastPlayerTurn();

    #endregion UIEvent


    #region Player Placing
    protected IEnumerator PlacingPlayersUI()
    {
        //loading player UI Settings
        PlayerLayoutScriptable playerUISettings = null;
        var op = Addressables.LoadAssetAsync<PlayerLayoutScriptable>(PlayerUIPlacementSettingAddr);
        yield return op.WaitForCompletion();
        playerUISettings = op.Result;
        //copyin setting
        int PlayersOnLeftPlayersNumber = 0;
        int PlayersOnRightPlayersNumber = 0;
        int PlayersOnFrontPlayersNumber = 0;
        foreach (var setting in playerUISettings.Settings)
        {
            if (setting.TotalPlayerNumber == _uiManager.GameManagerUI.PlayersNumber)
            {
                PlayersOnLeftPlayersNumber = setting.PlayersOnLeftPlayersNumber;
                PlayersOnRightPlayersNumber = setting.PlayersOnRightPlayersNumber;
                PlayersOnFrontPlayersNumber = setting.PlayersOnFrontPlayersNumber;
            }
        }
#if Log
        LogManager.Log($"Select Player Placement UI Setting is left{PlayersOnLeftPlayersNumber}/right{PlayersOnRightPlayersNumber}/front{PlayersOnFrontPlayersNumber}", Color.gray, LogManager.ValueInformationLog);
#endif
        yield return null;
        List<string> placedIDs = new List<string>();
        //placing local player on POV
        _uiManager.GameManagerUI.LocalPlayer.Transform.SetParent(_uiManager.PlayerUIPlacementSceneRefs.PlayerPOV);
        ResetTransform(_uiManager.GameManagerUI.LocalPlayer.Transform);
        placedIDs.Add(_uiManager.GameManagerUI.LocalPlayer.ID);
        //placing players on front
        switch (PlayersOnFrontPlayersNumber)
        {
            //one in middle
            case 1:
                {
                    PlacePlayer(placedIDs, _uiManager.PlayerUIPlacementSceneRefs.PlayersOnFront);
                }
                break;
            //two seperated
            case 2:
                {
                    PlaceTwoPlayers(playerUISettings, PlayersOnFrontPlayersNumber, placedIDs);
                }
                break;
            // three players layed out
            case 3:
                {
                    PlacePlayer(placedIDs, _uiManager.PlayerUIPlacementSceneRefs.PlayersOnFront);
                    yield return null;
                    PlaceTwoPlayers(playerUISettings, PlayersOnFrontPlayersNumber - 1, placedIDs);
                }
                break;
        }
        yield return null;
        //placing players on left
        switch (PlayersOnLeftPlayersNumber)
        {
            case 1:
                {
                    PlacePlayer(placedIDs, _uiManager.PlayerUIPlacementSceneRefs.PlayersOnLeft);
                }
                break;

            case 2:
                {
                    PlacePlayer(placedIDs, _uiManager.PlayerUIPlacementSceneRefs.PlayersOnLeft);
                    yield return null;
                    PlaceExtraPlayer(placedIDs, _uiManager.PlayerUIPlacementSceneRefs.PlayersOnLeft, true, playerUISettings.PlayerUIExtraPosition);
                }
                break;
        }
        yield return null;
        //placing players on right 
        switch (PlayersOnRightPlayersNumber)
        {
            case 1:
                {
                    PlacePlayer(placedIDs, _uiManager.PlayerUIPlacementSceneRefs.PlayersOnRight);
                }
                break;

            case 2:
                {
                    PlacePlayer(placedIDs, _uiManager.PlayerUIPlacementSceneRefs.PlayersOnRight);
                    yield return null;
                    PlaceExtraPlayer(placedIDs, _uiManager.PlayerUIPlacementSceneRefs.PlayersOnRight, false, playerUISettings.PlayerUIExtraPosition);
                }
                break;
        }
    }
    private void PlaceExtraPlayer(List<string> placedIDs, Transform parent, bool Left, Vector3 extraPos)
    {
        foreach (var player in _uiManager.GameManagerUI.Players)
        {
            if (!placedIDs.Contains(player.ID))
            {
                player.Transform.SetParent(parent);
                ResetTransform(player.Transform);
                placedIDs.Add(player.ID);
                if (!Left)
                    extraPos = new Vector3(extraPos.x * -1, extraPos.y, extraPos.z);
                player.Transform.localPosition = extraPos;
                break;
            }
        }
    }
    private void PlaceTwoPlayers(PlayerLayoutScriptable playerUISettings, int PlayersOnLeftPlayersNumber, List<string> placedIDs)
    {
        int sign = 1;
        for (int index = 0; index < PlayersOnLeftPlayersNumber; index++)
        {
            foreach (var player in _uiManager.GameManagerUI.Players)
            {
                if (!placedIDs.Contains(player.ID))
                {
                    player.Transform.SetParent(_uiManager.PlayerUIPlacementSceneRefs.PlayersOnFront);
                    ResetTransform(player.Transform);
                    placedIDs.Add(player.ID);
                    player.Transform.localPosition = new Vector3(playerUISettings.XOffset * sign, 0, 0);
                    sign = -sign;
                    break;
                }
            }
        }
    }
    private void PlacePlayer(List<string> placedIDs, Transform parent)
    {
        foreach (var player in _uiManager.GameManagerUI.Players)
        {
            if (!placedIDs.Contains(player.ID))
            {
                player.Transform.SetParent(parent);
                ResetTransform(player.Transform);
                placedIDs.Add(player.ID);
                break;
            }
        }
    }
    private void ResetTransform(Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
    #endregion

    #region PlayerTurn
    protected virtual void FirstPlayerTurnLayOutSetUp()
    {
        //force reseting ui panels 
        ResetPlayerTurnUIPanels(true);

        var bettingScreen = _uiManager.PlayerTurnUI.BettingScreenUI;

        //lets not show shet for now 
        _uiManager.PlayerTurnUI.PlayerTurnUIManager.SetActive(false);
        bettingScreen.BettingScreen.gameObject.SetActive(false);

        // reseting the score 
        bettingScreen.MyBetSuitScore.text = Zeros;

        //hiding the previous player bet 
        bettingScreen.PreviousBetSuitScore.text = Zeros;
        bettingScreen.PreviousBetSuitScore.gameObject.SetActive(false);
        bettingScreen.PreviousBetSuitHolder.gameObject.SetActive(false);

        //hiding Back Button 
        bettingScreen.BackButton.gameObject.SetActive(false);

        //show Bet Launcher Outlet 
        bettingScreen.FirstBetLauncherText.SetActive(true);

        //making sure that the level up button is on 
        ToggleLevelUpButton(true);

        //panel on 
        bettingScreen.BettingScreen.gameObject.SetActive(true);
        _uiManager.PlayerTurnUI.PlayerTurnUIManager.SetActive(true);
    }

    protected virtual void PlayerTurnLayOutSetUp()
    {
        //force reseting ui panels 
        ResetPlayerTurnUIPanels(true);

        //setting up ultimatum screen 
        var ultimatumScreenUI = _uiManager.PlayerTurnUI.UltimatumScreenUI;

        //setting previous player Icon 
        var previousPlayerIconID = _uiManager.GameManagerUI.CurrentPlayer.IconID;
        ultimatumScreenUI.PreviousBetPlayerIcon.sprite = AssetLoader.AllIcons[previousPlayerIconID];

        //setting previous player name 
        var previousPlayerName = _uiManager.GameManagerUI.CurrentPlayer.Name;
        ultimatumScreenUI.PreviousBetPlayerName.text = previousPlayerName;

        //moving previous player display cards
        foreach (var displayCard in _previousPlayerDisplayCards)
        {
            displayCard.transform.SetParent(ultimatumScreenUI.PreviousBetSuitHolder);
        }

        //setting up previous bet as display cards 
        var previousBet = _uiManager.GameManagerUI.LiveBet.ToByteArray();
        var diffusedBet = new List<DiffusedRankInfo>();
        Extention.BetDiffuser(previousBet, diffusedBet);
        UpdatePreviousPlayerDisplayCards(diffusedBet, ultimatumScreenUI.PreviousBetSuitScore);

        //ultimatum screen on 
        ultimatumScreenUI.UltimatumScreen.gameObject.SetActive(true);
        _uiManager.PlayerTurnUI.PlayerTurnUIManager.SetActive(true);

        //making sure that the level up button is on 
        ToggleLevelUpButton(true);
    }
    protected virtual void LastPlayerUIPanelsLayOutSetUp()
    {
        //it is basicly an inhansed player turn 
        PlayerTurnLayOutSetUp();

        //turing off few Button from Betting screen 
        ToggleLevelUpButton(false);

        //maybe in future add some effects here 

    }

    protected void ToggleLevelUpButton(bool toggle) => _uiManager.PlayerTurnUI.BettingScreenUI.SuggestBet.gameObject.SetActive(toggle);
    public virtual void ResetPlayerTurnUIPanels(bool isNotALooser)
    {
        //betting screen off
        _uiManager.PlayerTurnUI.BettingScreenUI.BettingScreen.SetActive(false);
        //ultimatum screen off
        _uiManager.PlayerTurnUI.UltimatumScreenUI.UltimatumScreen.SetActive(false);
        //doubt screen off 
        _uiManager.PlayerTurnUI.DoubtScreenUI.DoubtScreen.SetActive(false);
        //only player who did not loose, looser screen off 
        if (isNotALooser)
            _uiManager.PlayerTurnUI.LooserScreen.LooserPanel.SetActive(false);
    }
    public virtual void PlayerTurnUIOff()
    {
        _uiManager.PlayerTurnUI.PlayerTurnUIManager.SetActive(false);
        //forcing everything off
        ResetPlayerTurnUIPanels(true);
    }
    #endregion

    #region Linking PlayerTurn Panles UI with Logic
    protected IEnumerator LinkPlayerTurnUIWithLogic()
    {
        //cant grabshet if there is no local Player yet 
        if (_uiManager.GameManagerUI.LocalPlayer == null)
        {
#if Log
            LogManager.LogError("Linking Player Turn UI Panels with Logic Failed!, Local Player is Null! ");
#endif
            yield break;
        }

        var localPlayer = _uiManager.GameManagerUI.LocalPlayer;

        //setting up ultimatum screen Doubt it Button
        var doubtItButton = _uiManager.PlayerTurnUI.UltimatumScreenUI.DoubtButton;
        doubtItButton.onClick.RemoveAllListeners();
        doubtItButton.onClick.AddListener(localPlayer.DoubtBet);

        //setting up ultimatum screen Bet Button 
        var betButton = _uiManager.PlayerTurnUI.UltimatumScreenUI.BetButton;
        betButton.onClick.RemoveAllListeners();
        betButton.onClick.AddListener(BettingScreenOn);

        //setting up  betting screen comfirm Bet Buttton 
        var confirmButton = _uiManager.PlayerTurnUI.BettingScreenUI.Confirm;
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(localPlayer.ConfirmBet);

        // setting betting screen Clear Bet Button 
        var clearBetButton = _uiManager.PlayerTurnUI.BettingScreenUI.ClearBet;
        clearBetButton.onClick.RemoveAllListeners();
        clearBetButton.onClick.AddListener(ClearMySelectedBet);

        // betting screen Level Up Bet Button Set Up 
        var levelUpBetButton = _uiManager.PlayerTurnUI.BettingScreenUI.SuggestBet;
        levelUpBetButton.onClick.RemoveAllListeners();
        levelUpBetButton.onClick.AddListener(LevelUpCurrentBet);

        // betting screen MaxBet Button Set Up 
        var MaxBetButton = _uiManager.PlayerTurnUI.BettingScreenUI.MaxBet;
        MaxBetButton.onClick.RemoveAllListeners();
        MaxBetButton.onClick.AddListener(MaxBet);

        // betting screen back button set up 
        var backButton = _uiManager.PlayerTurnUI.BettingScreenUI.BackButton;
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(BettingScreenBackButton);

        // looser screen spectate button set up 
        var spectateButton = _uiManager.PlayerTurnUI.LooserScreen.SpectateButton;
        spectateButton.onClick.RemoveAllListeners();
        spectateButton.onClick.AddListener(Spectate);
    }
    protected virtual void MaxBet()
    {
        var maxBet = BetGenerator.GenerateMaxBet(_uiManager.GameManagerUI.DealtCardsNumber);
        if (maxBet != null)
        {
            var diffusedMaxBet = new List<DiffusedRankInfo>();
            Extention.BetDiffuser(maxBet, diffusedMaxBet);
            UpdateDisplayCards(diffusedMaxBet);
        }
    }
    protected virtual void LevelUpCurrentBet()
    {
        var roundedCurrentBet = _uiManager.GameManagerUI.GameModeManager.RoundUpCurrentBet();
        if (roundedCurrentBet != null)
        {
            UpdateDisplayCards(roundedCurrentBet);
        }
    }
    protected void UpdateBetScore(TextMeshProUGUI score, List<DiffusedRankInfo> bet)
    {
        score.text = bet.DifusedBetToBruteValue().ToString();
    }
    protected void ClearMySelectedBet()
    {
        var localPlayer = _uiManager.GameManagerUI.LocalPlayer;
        if (localPlayer == null)
        {
#if Log
            LogManager.LogError("Clearing Bet Failed!, Local Player Is null");
#endif
            return;
        }
        // clearing selected bet data
        localPlayer.PlayerUIControler.SelectedBet.Clear();

        //reseting display cards 
        foreach (var displayCard in _myDisplayCards)
        {
            displayCard.SetIdleState();
        }

        //reseting bet score 
        _uiManager.PlayerTurnUI.BettingScreenUI.MyBetSuitScore.text = Zeros;
    }
    protected void BettingScreenBackButton()
    {
        //restting and turning off betting screen 
        BettingScreenOff();


        var ultimatumScreenUI = _uiManager.PlayerTurnUI.UltimatumScreenUI;
        //moving dsplay cards 
        foreach (var displayCard in _previousPlayerDisplayCards)
        {
            displayCard.transform.SetParent(ultimatumScreenUI.PreviousBetSuitHolder);
        }

        //setting up previous bet as display cards 
        var previousBet = _uiManager.GameManagerUI.LiveBet.ToByteArray();
        var diffusedBet = new List<DiffusedRankInfo>();
        Extention.BetDiffuser(previousBet, diffusedBet);
        UpdatePreviousPlayerDisplayCards(diffusedBet, ultimatumScreenUI.PreviousBetSuitScore);

        //ultimatum screen on 
        ultimatumScreenUI.UltimatumScreen.gameObject.SetActive(true);
    }
    protected void Spectate()
    {
        //togling loosers holder
        var loosersholder = _uiManager.PlayerTurnUI.LooserScreen.LoosersHolder.gameObject;
        loosersholder.SetActive(!loosersholder.activeSelf);

        //togling back ground 
        var backGround = _uiManager.PlayerTurnUI.BackGround;
        backGround.SetActive(!backGround.activeSelf);
    }

    protected void BettingScreenOn()
    {
        ClearMySelectedBet();
        //reseting the score 
        var bettingScreen = _uiManager.PlayerTurnUI.BettingScreenUI;
        bettingScreen.MyBetSuitScore.text = Zeros;

        //setting previous player bet 
        bettingScreen.PreviousBetSuitScore.text = Zeros;
        bettingScreen.PreviousBetSuitHolder.gameObject.SetActive(true);

        //hiding Bet Launcher Outlet 
        bettingScreen.FirstBetLauncherText.SetActive(false);

        //setting back button on 
        bettingScreen.BackButton.gameObject.SetActive(true);

        //moving previous player display cards 
        foreach (var displayCard in _previousPlayerDisplayCards)
        {
            displayCard.transform.SetParent(bettingScreen.PreviousBetSuitHolder);
        }

        //setting up previous player display cards 
        var previousBet = _uiManager.GameManagerUI.LiveBet.ToByteArray();
        var diffusedBet = new List<DiffusedRankInfo>();
        Extention.BetDiffuser(previousBet, diffusedBet);
        UpdatePreviousPlayerDisplayCards(diffusedBet, bettingScreen.PreviousBetSuitScore);

        //turning on the betting screen
        bettingScreen.BettingScreen.SetActive(true);
    }
    protected void BettingScreenOff()
    {
        ClearMySelectedBet();
        _uiManager.PlayerTurnUI.BettingScreenUI.BettingScreen.SetActive(false);

    }
    #endregion

    #region Prevous Display Cards
    protected IEnumerator SetUpPreviousPlayerDisplayCards()
    {
        var localPlayer = _uiManager.GameManagerUI.LocalPlayer;
        if (localPlayer == null)
        {
#if Log
            LogManager.LogError("Setting up Display Cards Failed!, Local Player Is null");
#endif
            yield break;
        }
        var displayCardSuit = CardSuit.Spades;
        var displayCardsParent = _uiManager.PlayerTurnUI.UltimatumScreenUI.PreviousBetSuitHolder;

        foreach (var rank in CardManager.SortedRanks)
        {
            var displayCard = _uiManager.GameManagerUI.Insttantiate(AssetLoader.PrefabContainer.DisplayCardPrefab, displayCardsParent);
            displayCard.SetRank(rank);
            displayCard.SetSuit(displayCardSuit);
            displayCard.SetHighlighColor(Color.blue);
            displayCard.SetIdleState();
            displayCard.DisbaleButton();
            _previousPlayerDisplayCards.Add(displayCard);
            yield return null;
        }
    }
    private void UpdatePreviousPlayerDisplayCards(List<DiffusedRankInfo> bet, TextMeshProUGUI score)
    {
        //reseting display cards 
        foreach (var displayCard in _previousPlayerDisplayCards)
        {
            displayCard.SetIdleState();
        }

        //presenting rounded bet with display cards 
        foreach (var rank in bet)
        {
            foreach (var displayCard in _previousPlayerDisplayCards)
            {
                if (rank.Rank == displayCard.Rank)
                {
                    displayCard.CustomSelectState(rank.CardsCount);
                    break;
                }
            }
        }

        //updating betting score 
        UpdateBetScore(score, bet);
    }
    #endregion

    #region  My Display Cards
    private void UpdateDisplayCards(List<DiffusedRankInfo> bet)
    {
        //making sure there is no selected cards 
        ClearMySelectedBet();

        //presenting rounded bet with display cards 
        foreach (var rank in bet)
        {
            foreach (var displayCard in _myDisplayCards)
            {
                if (rank.Rank == displayCard.Rank)
                {
                    displayCard.CustomSelectState(rank.CardsCount);
                    break;
                }
            }
        }

        //updating betting score 
        UpdateBetScore(_uiManager.PlayerTurnUI.BettingScreenUI.MyBetSuitScore, bet);
    }
    protected IEnumerator SetUpMyDisplayCards()
    {
        var localPlayer = _uiManager.GameManagerUI.LocalPlayer;
        if (localPlayer == null)
        {
#if Log
            LogManager.LogError("Setting up Display Cards Failed!, Local Player Is null");
#endif
            yield break;
        }
        var displayCardSuit = CardSuit.Spades;
        var displayCardsParent = _uiManager.PlayerTurnUI.BettingScreenUI.MyBetSuitHolder;

        foreach (var rank in CardManager.SortedRanks)
        {
            var displayCard = _uiManager.GameManagerUI.Insttantiate(AssetLoader.PrefabContainer.DisplayCardPrefab, displayCardsParent);
            displayCard.SetRank(rank);
            displayCard.SetSuit(displayCardSuit);
            displayCard.SetIdleState();
            displayCard.OnCardSelected = OnCardSelected;
            displayCard.OnCardDeSelected = OnCardDeSelected;
            _myDisplayCards.Add(displayCard);
            yield return null;
        }
    }

    protected virtual void OnCardSelected(byte rank)
    {
        UpdateCardSelection(rank, true);
    }

    protected virtual void OnCardDeSelected(byte rank)
    {
        UpdateCardSelection(rank, false);
    }

    protected void UpdateCardSelection(byte rank, bool isSelected)
    {
        var localPlayer = _uiManager.GameManagerUI.LocalPlayer;
        if (localPlayer == null)
        {
#if Log
            LogManager.LogError("Setting up Display Cards Failed!, Local Player Is null");
#endif
            return;
        }
        if (isSelected)
        {
            localPlayer.PlayerUIControler.AddSelectedRank(rank);
        }
        else
        {
            localPlayer.PlayerUIControler.RemoveSelectedRank(rank);
        }

        var betArray = localPlayer.PlayerUIControler.SelectedBet.ToByteArray();
        var diffusedBet = new List<DiffusedRankInfo>();
        Extention.BetDiffuser(betArray, diffusedBet);
        UpdateBetScore(_uiManager.PlayerTurnUI.BettingScreenUI.MyBetSuitScore, diffusedBet);
    }
    #endregion

    #region Doubting

    protected virtual void DoubtScreenSetUp()
    {
        var doubtScreen = _uiManager.PlayerTurnUI.DoubtScreenUI;
        var localPlayer = _uiManager.GameManagerUI.LocalPlayer;
        if (localPlayer == null)
        {
#if Log
            LogManager.LogError("Setting up Doubt Screen Failed!, Local Player Is null");
#endif
            return;
        }
        ResetPlayerTurnUIPanels(!localPlayer.IsOut);

        //setting player icons and names  
        var currentPlayerIconID = _uiManager.GameManagerUI.CurrentPlayer.IconID;
        doubtScreen.RightPlayerDisplay.PlayerIcon.sprite = AssetLoader.AllIcons[currentPlayerIconID];
        doubtScreen.RightPlayerDisplay.PlayerName.text = _uiManager.GameManagerUI.CurrentPlayer.Name;
        doubtScreen.RightPlayerDisplay.DoubtStateText.text = _uiManager.GameManagerUI.DoubtState == DoubtState.WinDoubt ? Winner : Looser;
        //grabing previous player 
        IPlayer previousPlayer;
        if (_uiManager.GameManagerUI.GameModeManager.TryFindPlayer(_uiManager.GameManagerUI.LiveBetPlayerID, out previousPlayer))
        {
            var previousPlayerIconID = previousPlayer.IconID;
            doubtScreen.LeftPlayerDisplay.PlayerIcon.sprite = AssetLoader.AllIcons[previousPlayerIconID];
            doubtScreen.LeftPlayerDisplay.PlayerName.text = previousPlayer.Name;
            doubtScreen.LeftPlayerDisplay.DoubtStateText.text = _uiManager.GameManagerUI.DoubtState == DoubtState.WinDoubt ? Looser : Winner;
        }
        else
        {
#if Log
            LogManager.LogError("Setting up Doubt Screen Failed !, Failed Finding Previous Player");
#endif
            return;
        }
        //panel on 
        _uiManager.PlayerTurnUI.PlayerTurnUIManager.SetActive(true);

    }
    protected virtual IEnumerator DoubtScene()
    {
        DoubtScreenSetUp();
        //waiting animation
        yield return new WaitForSeconds(_uiManager.GameManagerUI.DoubtSceneTimer);
      
        //after the animation inoking logic
        if (_uiManager.GameManagerUI.IsHost)
        {
            _uiManager.GameManagerUI.GameModeManager.DoubtOverLogic();
        }
    }
    #endregion

}