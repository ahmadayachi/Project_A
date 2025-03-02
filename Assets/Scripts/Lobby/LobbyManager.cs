using Steamworks;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LobbyManager : NetworkBehaviour
{
    public const string IsReady = "IsReady";
    public const string NotReady = "Not Ready";
    public const string Start = "Start";

    [SerializeField]
    private LobbyUIRefs _lobbyUIRefs;
    private NetworkVariable<FixedString64Bytes> LobbyName = new NetworkVariable<FixedString64Bytes>();
    private NetworkVariable<FixedString64Bytes> LobbyID = new NetworkVariable<FixedString64Bytes>();

    private List<NetworkObject> _lobbyPlayersNetObjects = new List<NetworkObject>();
    private LobbyPlayer _localLobbyPlayer;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        LobbyName.OnValueChanged += OnLobbyNameChanged;
        LobbyID.OnValueChanged += OnLobbyIDChanged;

        NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
        if (IsHost)
        {
            //grabing session name 
            LobbyName.Value = AssetLoader.RunTimeDataHolder.LobbySettings.LobbyName;
            //grabing session ID
            if (AuthenticationManager.Instance.SteamAuthentication)
                LobbyID.Value = SteamClient.SteamId.ToString();

            //spawning host lobby player 
            SpawnLobbyPlayer(NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            //sync first tick for client 
            if (AuthenticationManager.Instance.SteamAuthentication)
                _lobbyUIRefs.LobbyID.text = LobbyID.Value.ToString();
            _lobbyUIRefs.LobbyName.text = LobbyName.Value.ToString();
        }
        InitStartButtonUI();
        _lobbyUIRefs.CoppyButton.onClick.AddListener(CopyToClipboard);
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;

        if (_localLobbyPlayer != null)
            _localLobbyPlayer.OnIsReady -= OnIsReadyChangedCallBack;

    }

    #region Logic
    public void SetUpLocalPlayer(LobbyPlayer player)
    {
        _localLobbyPlayer = player;
        _localLobbyPlayer.OnIsReady += OnIsReadyChangedCallBack;

        _lobbyUIRefs.StartButton.onClick.RemoveAllListeners();

        if (IsHost)
            _lobbyUIRefs.StartButton.onClick.AddListener(StartGame);
        else
            _lobbyUIRefs.StartButton.onClick.AddListener(_localLobbyPlayer.IsReadyRpc);
    }
    public void StartGame()
    {
        if (SetUpPlayersData())
        {
            string sceneName = "InGameScene";
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
#if Log
            LogManager.Log($"[{nameof(LobbyManager)}] - Host is starting a Game!", UnityEngine.Color.green);
#endif
        }
    }
    private void OnIsReadyChangedCallBack(bool isReady)
    {
        if (isReady)
            _lobbyUIRefs.StartButtonText.text = IsReady;
        else
            _lobbyUIRefs.StartButtonText.text = NotReady;
    }
    private bool SetUpPlayersData()
    {
        if (_lobbyPlayersNetObjects.Count < 2)
        {
#if Log
            LogManager.Log($"[{nameof(LobbyManager)}] - Need More Players To start the Game! ", UnityEngine.Color.yellow, LogManager.ValueInformationLog);
#endif
            return false;
        }

        foreach (var item in _lobbyPlayersNetObjects)
        {
            var player = item.GetComponent<LobbyPlayer>();

            if (!PlayerIsValid(player))
            {
                return false;
            }

            //player data set up
            var runtimeData = new RunTimePlayerData();
            runtimeData.PlayerName = player.Name.Value.ToString();
            runtimeData.PlayerID = player.ID.Value.ToString();
            runtimeData.ClientID = item.OwnerClientId;
            runtimeData.IconIndex = player.IconID.Value;
            AssetLoader.RunTimeDataHolder.RunTimePlayersData.Add(runtimeData);

        }

        return true;
    }
    private bool PlayerIsValid(LobbyPlayer player)
    {
        if (player == null)
        {
#if Log
            LogManager.LogError($"[{nameof(LobbyManager)}] - Failed Setting up Player Data!,Player object does not have a LobbyPlayer Component!");
#endif
            return false;
        }

        if (player.IsReady.Value != true)
        {
#if Log
            LogManager.Log($"[{nameof(LobbyManager)}] - Player=>{player} is not ready!", UnityEngine.Color.yellow, LogManager.ValueInformationLog);
#endif
            return false;
        }

        if (player.IconID.Value == 0)
        {
#if Log
            LogManager.LogError($"[{nameof(LobbyManager)}] -  Failed Setting up Player Data!,Lobby Player Icon ID is 0!Player=>{player}");
#endif
            return false;
        }

        if (player.Name.Value.Equals(default(FixedString32Bytes)))
        {
#if Log
            LogManager.LogError($"[{nameof(LobbyManager)}] -  Player=>{player} Failed Setting up Player Data!,Lobby Player Name is empty! Player=>{player}");
#endif
            return false;
        }

        if (player.ID.Value.Equals(default(FixedString64Bytes)))
        {
#if Log
            LogManager.LogError($"[{nameof(LobbyManager)}] -  Player=>{player} Failed Setting up Player Data!,Lobby Player ID is empty!");
#endif
            return false;
        }
        return true;
    }
    private void SpawnLobbyPlayer(ulong ClientID)
    {
        var lobbbyPlayerGO = Instantiate(AssetLoader.PrefabContainer.LobbyPlayerPrefab);
        var lobbyPlayerNetObject = lobbbyPlayerGO.GetComponent<NetworkObject>();
        lobbyPlayerNetObject.SpawnAsPlayerObject(ClientID, true);
        lobbyPlayerNetObject.transform.SetParent(_lobbyUIRefs.LobbyPlayersHolder, false);
        _lobbyPlayersNetObjects.Add(lobbyPlayerNetObject);
    }
    private void DespawnLobbyPlayer(ulong ClientID)
    {
        var lobbyPlayer = _lobbyPlayersNetObjects.Find(x => x.OwnerClientId == ClientID);
        lobbyPlayer.Despawn();
    }
    #endregion

    #region Lobby UI  
    public void InitStartButtonUI()
    {
        if (IsHost)
            _lobbyUIRefs.StartButtonText.text = Start;
        else
            _lobbyUIRefs.StartButtonText.text = NotReady;
    }
    public void SetUpClientIsReadyButton(UnityAction rpc)
    {
        _lobbyUIRefs.StartButton.onClick.RemoveAllListeners();
        _lobbyUIRefs.StartButton.onClick.AddListener(rpc);
    }
    private void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = _lobbyUIRefs.LobbyID.text;
    }
    #endregion

    #region netcode Call Backs
    private void OnConnectionEvent(NetworkManager arg1, ConnectionEventData arg2)
    {
#if Log
        LogManager.Log($"[{nameof(LobbyManager)}] - is a Host=> {arg1.IsHost}!", UnityEngine.Color.green);
        LogManager.Log($"[{nameof(LobbyManager)}] - Event Type: {arg2.EventType}", UnityEngine.Color.green);
        LogManager.Log($"[{nameof(LobbyManager)}] - Client ID: {arg2.ClientId}", UnityEngine.Color.green);

        if (arg2.PeerClientIds.IsCreated && arg2.PeerClientIds.Length > 0)
        {
            string peerClientIds = string.Join(", ", arg2.PeerClientIds);
            LogManager.Log($"[{nameof(LobbyManager)}] - Peer Client IDs: {peerClientIds}", UnityEngine.Color.green);
        }
#endif

        if (!IsHost) return;
        switch (arg2.EventType)
        {
            case ConnectionEvent.ClientConnected: SpawnLobbyPlayer(arg2.ClientId); break;
            case ConnectionEvent.ClientDisconnected: DespawnLobbyPlayer(arg2.ClientId); break;
        }
    }
    private void OnLobbyNameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        _lobbyUIRefs.LobbyName.text = newValue.ToString();
    }
    private void OnLobbyIDChanged(FixedString64Bytes oldID, FixedString64Bytes newID)
    {
        _lobbyUIRefs.LobbyID.text = newID.ToString();
    }
    #endregion
}

[System.Serializable]
public struct LobbyUIRefs
{
    public TextMeshProUGUI LobbyName;
    public TextMeshProUGUI LobbyID;
    public Button CoppyButton;
    public TextMeshProUGUI StartButtonText;
    public Button StartButton;
    public Transform LobbyPlayersHolder;
}
