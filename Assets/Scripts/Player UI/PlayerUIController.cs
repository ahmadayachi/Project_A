using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;

public class PlayerUIController :MonoBehaviour
{
    [SerializeField] private Player _player;
    private Coroutine _loadingHandCoroutine;
    public List<byte> SelectedBet = new List<byte>();
    private void Awake()
    {
        if (_player == null)
        {
#if Log
            LogManager.LogError("Assign the Player Script!");
#endif
        }
    }


    #region Player UI
    public void SetUpCardPositionerCardPool(CardPool cardPool) => _player.PlayerUI.CardPositioner.Init(cardPool, _player.IsLocalPlayer);

    public void SetPlayerName()
    {
        //maybe bypass empty one for resting
        if (_player.Name == string.Empty)
        {
#if Log
            LogManager.LogError($"Failed to Load Icon for Player=>{_player}");
#endif
            return;
        }
        _player.PlayerUI.PlayerName.text = _player.Name;
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
#if Log
        LogManager.Log($"{_player.Runner.LocalPlayer} Icon Is Set !", Color.gray, LogManager.ValueInformationLog);
#endif
        _player.PlayerUI.PlayerIcon.sprite = sprite;
    }

    public void LoadPlayerCards()
    {
        if (_player.HandCount == 0)
        {
            return;
        }
        if (PlayerCanLoadCards())
        {
            _player.PlayerUI.CardPositioner.LoadCards(_player.Hand);
            return;
        }
        if (_loadingHandCoroutine != null)
            StopCoroutine(_loadingHandCoroutine);
        _loadingHandCoroutine = StartCoroutine(WaitHandThenUpdate());
    }
    private IEnumerator WaitHandThenUpdate()
    {
        yield return new WaitUntil(PlayerCanLoadCards);
        _player.PlayerUI.CardPositioner.LoadCards(_player.Hand);
    }

    private bool PlayerCanLoadCards()
    {
        return _player.PlayerUI.CardPositioner.CardPool != null && _player.IsHandFull;
    }
    #endregion


    /// <summary>
    /// starts the player turn timer
    /// </summary>
    public void StartTimers()
    {
    }

    /// <summary>
    /// stops the player turn timer
    /// </summary>
    public void StopTimers()
    {
    }

    public void ShowBetButton()
    {
    }

    public void ShowDoubtButton()
    {
    }

    public void HideBetButton()
    {
    }

    public void HideDoubtButton()
    {
    }

  
    #region Player Turn UI Panels Management
    /// <summary>
    /// Resets and Hides all PlayerPanels
    /// </summary>
    private void ResetPlayerTurnUI()
    {

    }
    /// <summary>
    /// force to just betting
    /// </summary>
    public void ShowFirstPlayerUI()
    {
        _player.PlayerGameManager.UIManager.ActiveUIEvents.OnFirstPlayerTurn();
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

    /// <summary>
    /// hides every possible UI command with its effects
    /// </summary>
    public void HidePlayerUI()
    {

    }
    #endregion
    #region Player Commands Wraping

    private void ConfirmBet()
    {
        //if it is this player turn and he is the local player
        if (_player.PlayerGameManager.IsMyTurn(_player.ID))
        {
            //send confirm RPC
            _player.PlayerGameManager.RPC_ConfirmBet(ProcessSelectedCards(), _player.ID);
        }
        //turn off the UI Panel

    }

    private byte[] ProcessSelectedCards()
    {
        //checking if the list is Empty
        if (SelectedBet.IsEmpty())
        {
#if Log
            LogManager.Log(" Confirm Bet Failed! No SelectedBet Found!", Color.red, LogManager.PlayerLog);
#endif
            //UI to inform Player here that he Cant Confirm
            return null;
        }

        //check for any blanks/Invalid ranks in the selected bet
        for (int index = 0; index < SelectedBet.Count; index++)
        {
            byte bet = SelectedBet[index];
            if (!Extention.IsAValidCardRank(bet))
                SelectedBet.Remove(bet);
        }

        //creating array
        byte[] ProseceesedBet = SelectedBet.ToByteArray();
        return ProseceesedBet;
    }

    #endregion Player Commands Wraping
}