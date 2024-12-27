using Fusion;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "PrefabContainer")]
public class PrefabContainer : ScriptableObject
{
    #region Network prefabs
    public NetworkPrefabRef PlayerPrefab;
    public GameObject CardPrefab;
    public GameObject RunnerPrefab;
    #endregion
    #region Log Manager Prefabs
    public GameObject LogPrefab;
    #endregion

    public DisplayCard DisplayCardPrefab;

    public NetworkManager SteamNetworkManager;
    public NetworkManager LocalNetworkManager;
    public GameObject PublicSteamLobbyPrefab;
    public GameObject LobbyPlayerPrefab;
}
