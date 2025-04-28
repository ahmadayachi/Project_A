using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CallBackManager
{
    private Queue<Action> _callbacks = new Queue<Action>();
    private Queue<string> _callbackNames = new Queue<string>();
    private bool _isReady = false;

    public void EnqueueOrExecute(Action callback,string callBackName)
    {
        if (_isReady)
        {
            callback?.Invoke();
#if Log
            LogManager.Log($"CallBackManager is Invoking a CallBack named =>{callBackName}",Color.green,LogManager.ValueInformationLog);
#endif
        }
        else
        {
            _callbacks.Enqueue(callback);
            _callbackNames.Enqueue(callBackName);
#if Log
            LogManager.Log($"CallBackManager is queing a CallBack named =>{callBackName}", Color.yellow, LogManager.ValueInformationLog);
#endif
        }
    }

    public void SetReady(bool ready)
    {
        _isReady = ready;
        if (_isReady)
        {
            DequeueAll();
        }
    }

    private void DequeueAll()
    {
        while (_callbacks.Count > 0)
        {
            var callback = _callbacks.Dequeue();
            var callBackName = _callbackNames.Dequeue();
#if Log
            LogManager.Log($"CallBackManager is dequeing a CallBack named =>{callBackName}", Color.magenta, LogManager.ValueInformationLog);
#endif      
            callback?.Invoke();
        }
    }
}
