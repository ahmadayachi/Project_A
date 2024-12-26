using Steamworks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class LobbyManager : NetworkBehaviour
{
    [SerializeField]
    private LobbyUIRefs _lobbyUIRefs;

    private NetworkVariable<FixedString64Bytes> LobbyName = new NetworkVariable<FixedString64Bytes>();
    private NetworkVariable<FixedString64Bytes> LobbyID = new NetworkVariable<FixedString64Bytes>();

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
        }
        else
        {
            //sync first tick for client 
            if (AuthenticationManager.Instance.SteamAuthentication)
                _lobbyUIRefs.LobbyID.text = LobbyID.Value.ToString();
            _lobbyUIRefs.LobbyName.text = LobbyName.Value.ToString();
        }

        _lobbyUIRefs.CoppyButton.onClick.AddListener(CopyToClipboard);
    }
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
    }



    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;

    }

    private void OnLobbyNameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        _lobbyUIRefs.LobbyName.text = newValue.ToString();
    }

    private void OnLobbyIDChanged(FixedString64Bytes oldID, FixedString64Bytes newID)
    {
        _lobbyUIRefs.LobbyID.text = newID.ToString();
    }

    private void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = _lobbyUIRefs.LobbyID.text;
    }
    private void TestButton()
    {
        Debug.Log("CHanging Lobby Name ");
        LobbyName.Value = "wawa" + Random.Range(0, 9);
    }
}

[System.Serializable]
public struct LobbyUIRefs
{
    public TextMeshProUGUI LobbyName;
    public TextMeshProUGUI LobbyID;
    public Button CoppyButton;
}
