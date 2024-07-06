using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UIEvents : IUIEvents
{
    private UIManager _uiManager;
    private Coroutine _gameStartAnimationRoutine;
    private const string PlayerUIPlacementSettingAddr = "PlayerUIPlacementSetting";

    public UIEvents(UIManager manager)
    {
        _uiManager = manager;
    }

    #region UIEvent

    public IEnumerator SetUpUI()
    {
        yield return PlacingPlayersUI();
        yield return null;
        _uiManager.GameManagerUI.SimulationState = SimulationSetUpState.UISetUp;
#if Log
        LogManager.Log($" UI is Set Up Runner Player Ref => {_uiManager.GameManagerUI.Runner.LocalPlayer}", Color.green, LogManager.ValueInformationLog);
#endif
    }

    public void OnGameStarted()
    {
        if (_gameStartAnimationRoutine != null)
            _uiManager.StopCoroutine(_gameStartAnimationRoutine);
        _gameStartAnimationRoutine = _uiManager.StartCoroutine(GameStartAnimation());
    }

    public void OnDealingCards()
    {
        //throw new System.NotImplementedException();
    }

    public void OnDoubting()
    {
        //throw new System.NotImplementedException();
    }

    public void OnGameOver()
    {
        //throw new System.NotImplementedException();
    }

    public void OnHostMigration()
    {
        //throw new System.NotImplementedException();
    }

    public void OnRoundOver()
    {
        //throw new System.NotImplementedException();
    }

    public void OnSetUpStarted()
    {
        //throw new System.NotImplementedException();
    }

    #endregion UIEvent

    private IEnumerator PlacingPlayersUI()
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
        LogManager.Log($"Select Player Placement UI Setting is left{PlayersOnLeftPlayersNumber}/right{PlayersOnRightPlayersNumber}/front{PlayersOnFrontPlayersNumber}",Color.gray,LogManager.ValueInformationLog);
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
                    PlaceTwoPlayers(playerUISettings, PlayersOnLeftPlayersNumber, placedIDs);
                }
                break;
            // three players layed out
            case 3:
                {
                    PlacePlayer(placedIDs, _uiManager.PlayerUIPlacementSceneRefs.PlayersOnFront);
                    yield return null;
                    PlaceTwoPlayers(playerUISettings, PlayersOnLeftPlayersNumber, placedIDs);
                }
                break;
        }
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
                player.Transform.position = extraPos;
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

    private IEnumerator GameStartAnimation()
    {
        yield return null;
        //informing server player animation finished
        _uiManager.GameManagerUI.PlayerIsReady();
    }
}