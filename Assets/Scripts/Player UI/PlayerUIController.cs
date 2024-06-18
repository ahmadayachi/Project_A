using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIController : IPlayerUIControler
{
    private PlayerUI _playerUI;
    private Player _player;
    private Coroutine _loadingHandCoroutine;

    public PlayerUIController(PlayerUI playerUI, Player player )
    {
        _playerUI = playerUI;
        _player = player;
    }
    public void SetUpCardPositionerCardPool(CardPool cardPool)=> _playerUI.CardPositioner.Init(cardPool);
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
        if (PlayerCanLoadCards())
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
    public void ShowNormalPlayerUI()
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
        yield return new WaitUntil(PlayerCanLoadCards);
        _playerUI.CardPositioner.LoadCards(_player.Hand);
    }
    private bool PlayerCanLoadCards()
    {
        return _playerUI.CardPositioner.CardPool != null && _player.IsHandFull;
    }
}