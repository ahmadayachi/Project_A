using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private GameManager _gameManager;
    public IUIEvents UIEvents;
    public void Init()
    {

    }
    public void InjectGameManager(GameManager gameManager)=>_gameManager = gameManager;
}
