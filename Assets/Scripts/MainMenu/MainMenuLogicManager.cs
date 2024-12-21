using Unity.Netcode;
using UnityEngine;

public class MainMenuLogicManager : MonoBehaviour
{
    public void StartHost()
    {
        if (AuthenticationManager.Instance.SteamAuthentication)
        {
            SteamNetworkManager steamManager = NetworkManager.Singleton.GetComponent<SteamNetworkManager>();
            if (steamManager != null) 
            {
                steamManager.StartHost();
            }
            else
            {
#if Log
                LogManager.LogError("NetworkManager does not have a SteamNetworkManager component!");
#endif
            }
        }
        else
        {

        }
    }
}
public struct StartHostARGS
{
    public string SessionName;
    public bool IsPrivate;
}