using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SpicyHarissa/Player UI Placing Settings")]
public class PlayerLayoutScriptable : ScriptableObject
{
    public List<PlayerUIPlacmentSetting> Settings = new List<PlayerUIPlacmentSetting>();

    [Header("Extra Side Players Position and Scale")]
    public Vector3 PlayerUIExtraPosition = Vector3.zero;

    public Vector3 PlayerUIExtraScale = Vector3.zero;

    [Header("Extra Front PlayerUI X axis Offset")]
    public float XOffset = 0;
}

[System.Serializable]
public struct PlayerUIPlacmentSetting
{
    public int TotalPlayerNumber;

    //there will always be one on POV placement
    //public int PlayerPOVPlayersNumber;
    public int PlayersOnLeftPlayersNumber;

    public int PlayersOnRightPlayersNumber;
    public int PlayersOnFrontPlayersNumber;
}