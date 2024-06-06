using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private GameManager _gameManager;
    public void InitUI()
    {

    }
    public void InjectGameManager(GameManager gameManager)=>_gameManager = gameManager;
}
