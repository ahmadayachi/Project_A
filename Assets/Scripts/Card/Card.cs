using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : NetworkBehaviour
{
    private ICardBehaviour _activeCardBehaviour;
    private NetworkRunner _runner;
    public override void Spawned()
    {
        _runner = Runner;
        SetUpCardBehaviour();
    }
    private void SetUpCardBehaviour()
    {
        if(_runner==null) return;
        if(_runner.GameMode==GameMode.Single)
            _activeCardBehaviour = new OfflineCardBehaviour();
        else 
            _activeCardBehaviour = new OnlineCardBehaviour();
    }
}
