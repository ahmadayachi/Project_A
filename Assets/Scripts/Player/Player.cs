using UnityEngine;
using Fusion;
public class Player : NetworkBehaviour
{
    private IPlayerBehaviour _playerState;
    public IPlayerBehaviour PlayerState { get => _playerState;}
    private NetworkRunner _runner;
    public override void Spawned()
    {
        _runner = Runner; 
        SetUpPlayerBehaviour();
    }
    private void SetUpPlayerBehaviour()
    {
        if (_runner.GameMode == GameMode.Single)
        {
            _playerState = new OfflinePlayerBehaviour();
        }
        else
        {
            _playerState = new OnlinePlayerBehaviour();
        }
    }
}
