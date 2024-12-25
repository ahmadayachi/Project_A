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

    private NetworkVariable<FixedString64Bytes> sessionID = new NetworkVariable<FixedString64Bytes>();

    public override void OnNetworkSpawn()
    {
        sessionID.OnValueChanged += OnSessionIDChanged;
        if (IsHost)
        {
            if (AuthenticationManager.Instance.SteamAuthentication)
            {
                sessionID.Value = SteamClient.SteamId.ToString();
            }
        }
        else
        {
            //grabing first tick for client 
            _lobbyUIRefs.LobbyID.text = sessionID.ToString();
        }

        _lobbyUIRefs.CoppyButton.onClick.AddListener(CopyToClipboard);
    }

    private void OnSessionIDChanged(FixedString64Bytes oldID, FixedString64Bytes newID)
    {
        _lobbyUIRefs.LobbyID.text = newID.ToString();
    }

    private void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = _lobbyUIRefs.LobbyID.text;
    }
}

[System.Serializable]
public struct LobbyUIRefs
{
    public TextMeshProUGUI LobbyID;
    public Button CoppyButton;
}
