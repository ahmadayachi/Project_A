using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIController : IPlayerUIControler
{
    private PlayerUI _playerUI;
    private Player _player;
    private Coroutine _loadingHandCoroutine;

    public PlayerUIController(PlayerUI playerUI, Player player , CardPool cardPool)
    {
        _playerUI = playerUI;
        _player = player;
        _playerUI.CardPositioner.Init(cardPool);
    }

    public void SetPlayerIcon()
    {
        Sprite sprite = AssetLoader.AllIcons[_player.IconID];
        if (sprite == null)
        {
#if Log
            LogManager.LogError($"Failed to Load Icon for Player=>{_player}");
#endif
            return;
        }
        _playerUI.PlayerIcon.sprite = sprite;
    }

    public void LoadPlayerCards()
    {
        if (_player.HandCount == 0)
        {
            return;
        }
        if (_player.IsHandFull)
        {
            _playerUI.CardPositioner.LoadCards(_player.Hand);
            return;
        }
        if (_loadingHandCoroutine != null)
            _player.StopRoutine(_loadingHandCoroutine);
        _loadingHandCoroutine = _player.Startroutine(WaitHandThenUpdate());
    }

    /// <summary>
    /// starts the player turn timer 
    /// </summary>
    public void StartTimer()
    {

    }
    /// <summary>
    /// stops the player turn timer 
    /// </summary>
    public void StopTimer()
    {

    }
    public void ShowBetButton()
    {

    }
    public void ShowDoubtButton()
    {

    }
    /// <summary>
    /// force to just betting 
    /// </summary>
    public void ShowFirstPlayerUI()
    {

    }
    /// <summary>
    /// maybe a flame on the betting button, to show that it is the last bet!
    /// </summary>
    public void ShowLastPlayerUI()
    {

    }
    public void ShowNormalPlayerUICommands()
    {

    }
    public void HideBetButton()
    {

    }
    public void HideDoubtButton()
    {

    }
    /// <summary>
    /// hides every possible UI command with its effects 
    /// </summary>
    public void HidePlayerUICommands()
    {

    }
    private IEnumerator WaitHandThenUpdate()
    {
        yield return new WaitUntil(() => _player.IsHandFull);
        _playerUI.CardPositioner.LoadCards(_player.Hand);
    }
    
}