using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "RunTimeDataHolder ")]
public class RunTimeDataHolder : ScriptableObject
{
    public List<RunTimePlayerData> RunTimePlayersData = new List<RunTimePlayerData>();
    public DeckInfo DeckInfo;
    public LobbyData LobbySettings;
    public PlayerData LocalPlayerInfo;
}
[System.Serializable]
public struct PlayerData
{
    public FixedString32Bytes Name { get; set; }
    public FixedString64Bytes ID { get; set; }
    public int IconID;
}