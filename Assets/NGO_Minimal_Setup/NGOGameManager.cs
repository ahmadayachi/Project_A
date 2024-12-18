using UnityEngine;
using UnityEngine.UI;

public class NGOGameManager : MonoBehaviour
{
    [SerializeField]
    Button StartHost;
    [SerializeField]
    GameNetworkManager NetworkManager;
    private void Awake()
    {
        StartHost.onClick.RemoveAllListeners();
        StartHost.onClick.AddListener(() => 
        {
            NetworkManager.StartHost(4);
        });
    }
}
