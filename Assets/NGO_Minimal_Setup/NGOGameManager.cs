using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NGOGameManager : MonoBehaviour
{
    [SerializeField]
    Button StartHost;
    [SerializeField]
    Button StartClient;
    [SerializeField]
    TMP_InputField LobbyCode;

    [SerializeField]
    GameNetworkManager NetworkManager;
    private void Awake()
    {
        StartHost.onClick.RemoveAllListeners();
        StartHost.onClick.AddListener(() => 
        {
            NetworkManager.StartHost(4);
        });

        NetworkManager.OnHostLobbyCreated += SetLobbyCode;
        StartClient.onClick.RemoveAllListeners();
        StartClient.onClick.AddListener(() => 
        {
            if (ulong.TryParse(LobbyCode.text,out ulong code ))
            {
                NetworkManager.StartClient(code);
            }
        });
    }

    private void SetLobbyCode(ulong lobbycode)
    {
        LobbyCode.text = string.Empty;
        LobbyCode.text = lobbycode.ToString();
    }

}
