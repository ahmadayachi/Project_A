using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public abstract class UIEventsBase : IUIEvents
{
    protected UIManager _uiManager;
    private const string Zeros = "0000";

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
        var bettingScreen = _uiManager.PlayerTurnUI.BettingScreenUI;

        //lets not show shet for now 
        _uiManager.PlayerTurnUI.PlayerTurnUIManager.SetActive(false);
        bettingScreen.BettingScreen.gameObject.SetActive(false);

        // reseting the score 
        bettingScreen.MyBetSuitScore.text = Zeros;

        //hiding the previous player bet 
        bettingScreen.PreviousBetSuitScore.text = Zeros;
        bettingScreen.PreviousBetSuitHolder.gameObject.SetActive(false);

        //show Bet Launcher Outlet 
        bettingScreen.FirstBetLauncherText.SetActive(true);

        //panel on 
        bettingScreen.BettingScreen.gameObject.SetActive(true);
        _uiManager.PlayerTurnUI.PlayerTurnUIManager.SetActive(true);
    }
    #endregion

    #region Linking PlayerTurn Panles UI with Logic
    protected void LinkPlayerTurnUIWithLogic()
    {
        //cant grabshet if there is no local Player yet 
        if (_uiManager.GameManagerUI.LocalPlayer == null)
        {
#if Log
            LogManager.LogError("Linking Player Turn UI Panels with Logic Failed!, Local Player is Null! ");
#endif
            return;
        }

        var localPlayer = _uiManager.GameManagerUI.LocalPlayer;
        
        //setting up comfirm Bet Buttton 
        var confirmButton = _uiManager.PlayerTurnUI.BettingScreenUI.Confirm;
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(localPlayer.ConfirmBet);

        //setting up Doubt it Button
        var doubtItButton = _uiManager.PlayerTurnUI.UltimatumScreenUI.DoubtButton;
        doubtItButton.onClick.RemoveAllListeners();
        doubtItButton.onClick.AddListener(localPlayer.DoubtBet);

        //Level Up Bet Button Set Up 
        var levelUpBetButton = _uiManager.PlayerTurnUI.BettingScreenUI.SuggestBet;
        levelUpBetButton.onClick.RemoveAllListeners();
        levelUpBetButton.onClick.AddListener(LevelUpCurrentBet);
    }
    private void LevelUpCurrentBet()
    {

    }
    #endregion
}