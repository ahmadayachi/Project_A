using Steamworks;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;
    public const string IsReady = "IsReady";
    public const string NotReady = "Not Ready";
    public const string Start = "Start";

    [SerializeField]
    private LobbyUIRefs _lobbyUIRefs;
    private NetworkVariable<FixedString64Bytes> LobbyName = new NetworkVariable<FixedString64Bytes>();
    private NetworkVariable<FixedString64Bytes> LobbyID = new NetworkVariable<FixedString64Bytes>();

    private List<NetworkObject> lobbyPlayersNetObjects = new List<NetworkObject>();
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SetUpSingleton();
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

    }

    #region Logic
    public void StartGame()
    {

    }
    private void SpawnLobbyPlayer(ulong ClientID)
    {
        var lobbbyPlayerGO = Instantiate(AssetLoader.PrefabContainer.LobbyPlayerPrefab);
        var lobbyPlayerNetObject = lobbbyPlayerGO.GetComponent<NetworkObject>();
        lobbyPlayerNetObject.SpawnAsPlayerObject(ClientID, true);
        lobbyPlayerNetObject.transform.SetParent(_lobbyUIRefs.LobbyPlayersHolder, false);
        lobbyPlayersNetObjects.Add(lobbyPlayerNetObject);
    }
    private void DespawnLobbyPlayer(ulong ClientID)
    {
        var lobbyPlayer = lobbyPlayersNetObjects.Find(x => x.OwnerClientId == ClientID);
        lobbyPlayer.Despawn();
    }
    private void SetUpSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Lobby UI  
    public void InitStartButtonUI()
    {
        if (IsHost)
        {
            _lobbyUIRefs.StartButtonText.text = Start;
            _lobbyUIRefs.StartButton.enabled = false;
        }
        else
        {
            _lobbyUIRefs.StartButtonText.text = NotReady;
            _lobbyUIRefs.StartButton.enabled = true;
        }
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
        LogManager.Log($"[{nameof(MainMenuLogicManager)}] - is a Host=> {arg1.IsHost}!", UnityEngine.Color.green);
        LogManager.Log($"[{nameof(MainMenuLogicManager)}] - Event Type: {arg2.EventType}", UnityEngine.Color.green);
        LogManager.Log($"[{nameof(MainMenuLogicManager)}] - Client ID: {arg2.ClientId}", UnityEngine.Color.green);

        if (arg2.PeerClientIds.IsCreated && arg2.PeerClientIds.Length > 0)
        {
            string peerClientIds = string.Join(", ", arg2.PeerClientIds);
            LogManager.Log($"[{nameof(MainMenuLogicManager)}] - Peer Client IDs: {peerClientIds}", UnityEngine.Color.green);
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
