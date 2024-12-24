using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SteamNetworkManager : MonoBehaviour
{
    public Lobby? CurrentLobby { get; private set; }
    public List<Lobby> Lobbies = new List<Lobby>();

    private FacepunchTransport _facePunchTransport;
    /// <summary>
    /// default lobby max members
    /// </summary>
    private int _lobbyMaxMenbers = 8;
    public int LobbyMaxMembers { get => _lobbyMaxMenbers; set { _lobbyMaxMenbers = value; } }
    public const string SteamLobbyMap = "BathaM9atra";

    private void Start()
    {
        _facePunchTransport = GetComponent<FacepunchTransport>();

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }


    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
    }

    private void OnApplicationQuit() => Disconnect();



    #region Network Callbacks
    public async void StartHost()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            CurrentLobby = await SteamMatchmaking.CreateLobbyAsync(LobbyMaxMembers);
            string sceneName = "Lobby";
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
#if Log
            LogManager.Log($"[{nameof(MainMenuLogicManager)}] - Host is starting a Session!", UnityEngine.Color.green);
#endif
        }
        else
        {
#if Log
            LogManager.LogError($"[{nameof(MainMenuLogicManager)}] - Host Failed Creating a Session!");
#endif
        }
    }

    public void StartClient(SteamId id)
    {
        _facePunchTransport.targetSteamId = id;
#if Log
        LogManager.Log($"[{nameof(SteamNetworkManager)}] - Attempting to joing Game ,Host ID=> {_facePunchTransport.targetSteamId}", UnityEngine.Color.grey);
#endif

        if (NetworkManager.Singleton.StartClient())
        {

#if Log
            LogManager.Log($"[{nameof(SteamNetworkManager)}] - Client started succesfully", UnityEngine.Color.green);
#endif
        }
        else
        {
#if Log
            LogManager.LogError($"[{nameof(SteamNetworkManager)}] - Client failed starting !");
#endif
        }
    }

    public void Disconnect()
    {
        CurrentLobby?.Leave();

        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.Shutdown();
    }
    #endregion


    public async Task<bool> RefreshLobbies(int maxResults = 20)
    {
        try
        {
            Lobbies.Clear();

            var lobbies = await SteamMatchmaking.LobbyList
                    .FilterDistanceClose().WithKeyValue("map", SteamLobbyMap)
            .WithMaxResults(maxResults)
            .RequestAsync();

            if (lobbies != null)
            {
                for (int i = 0; i < lobbies.Length; i++)
                    Lobbies.Add(lobbies[i]);
            }

            return true;
        }
        catch (System.Exception ex)
        {
#if Log
            LogManager.LogError($"Error fetching lobbies ! ,=>{ex.Message}");
#endif
            return false;
        }
    }
    #region Steam Callbacks

    private void OnGameLobbyJoinRequested(Lobby lobby, SteamId id)
    {
        StartClient(id);
    }

    private void OnLobbyInvite(Friend friend, Lobby lobby)
    {
#if Log
        LogManager.Log($"[{nameof(SteamNetworkManager)}] - You got a invite from {friend.Name}", UnityEngine.Color.magenta);
#endif
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
    {
#if Log
        LogManager.Log($"[{nameof(SteamNetworkManager)}] - {friend.Name} Left Lobby", UnityEngine.Color.magenta);
#endif
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
#if Log
        LogManager.Log($"[{nameof(SteamNetworkManager)}] - {friend.Name} Joined Lobby", UnityEngine.Color.magenta);
#endif
    }

    private void OnLobbyEntered(Lobby lobby)
    {
#if Log
        Debug.Log($"[{nameof(SteamNetworkManager)}] - You have entered in lobby, clientId={NetworkManager.Singleton.LocalClientId}", this);
#endif
        //StartClient(lobby.Owner.Id);
    }

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            Debug.LogError($"[{nameof(SteamNetworkManager)}] - Lobby couldn't be created!, {result}", this);
            return;
        }

        var lobbyData = AssetLoader.RunTimeDataHolder.LobbySettings;
        if (lobbyData.IsValid)
        {
            lobby.SetData("name", lobbyData.LobbyName);
            lobby.SetData("map", SteamLobbyMap);

            if (lobbyData.IsPrivate)
                lobby.SetPrivate();
            else
                lobby.SetPublic();

            lobby.SetJoinable(true);

#if Log
            LogManager.Log($"[{nameof(SteamNetworkManager)}] - Lobby has been created!", UnityEngine.Color.green, LogManager.ValueInformationLog);
#endif
        }
        else
        {
#if Log
            LogManager.LogError($"[{nameof(SteamNetworkManager)}] - Failed Setting Up Lobby! LobbySettings is Not Valid !");
#endif
        }
    }

    //public void SetLobbyData(Lobby lobby)
    //{
    //    lobby.SetData("name", "Random Cool Lobby");
    //    lobby.SetData("maxPlayers", "10"); // Maximum number of players
    //    lobby.SetData("gameMode", "Deathmatch"); // Game mode
    //    lobby.SetData("map", "Arena"); // Map name
    //    lobby.SetData("region", "Europe"); // Region
    //    lobby.SetData("passwordProtected", "false"); 
    //}
    #endregion


}
