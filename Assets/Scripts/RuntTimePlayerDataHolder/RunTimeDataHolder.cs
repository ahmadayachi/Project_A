using System.Collections.Generic;
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
    public string Name;
    public string ID;
    public ulong ClientID;
    public int IconID;
}