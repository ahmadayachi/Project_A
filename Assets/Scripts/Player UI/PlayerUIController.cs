using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIController : IPlayerUIControler
{
    private PlayerUI _playerUI;
    private Player _player;

    public PlayerUIController(PlayerUI playerUI, Player player)
    {
        _playerUI = playerUI;
        _player = player;
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
        if (_player.PlayerHand == null)
        {
            return;
        }
        _playerUI.CardPositioner.LoadCards(_player.PlayerHand);
    }
}