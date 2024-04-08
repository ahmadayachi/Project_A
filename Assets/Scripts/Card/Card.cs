using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : NetworkBehaviour, ICard
{

    private ICardBehaviour _activeCardBehaviour;
    private NetworkRunner _runner;

    #region Card Properties
    [Networked] private byte _rank { get; set;}
    [Networked] private byte _id { get; set;}
    [Networked] private string _suite { get; set;}
    public byte Rank { get =>_rank;}
    public byte ID { get=>_id;}
    public string Suite { get=>_suite;}
    #endregion
    public void SetRank(byte rank)
    {
        _rank = rank;
    }
    public void SetID(byte id)
    {
        _id = id;
    }
    public void SetSuite(string suite)
    {
        _suite = suite;
    }
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
