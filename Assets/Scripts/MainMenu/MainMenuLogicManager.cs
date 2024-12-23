using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MainMenuLogicManager : MonoBehaviour
{




    #region Netcode Wrapping 
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
            if (NetworkManager.Singleton.StartHost())
            {
#if Log
                LogManager.Log("Host is starting a Session!", UnityEngine.Color.green);
#endif
            }
            else
            {
#if Log
                LogManager.LogError("Host Failed Creating a Session!");
#endif
            }
        }
    }
    public List<Lobby>  LoadSteamPublicLobbys()
    {
        if (!AuthenticationManager.Instance.SteamAuthentication)
        {
#if Log
            LogManager.Log("Loading Steam Lobbys Canceled! not using steam to authenticate players !", UnityEngine.Color.red);
#endif
            return null;
        }

        SteamNetworkManager steamManager = NetworkManager.Singleton.GetComponent<SteamNetworkManager>();
        if (steamManager == null)
        {
#if Log
            LogManager.LogError("NetworkManager does not have a SteamNetworkManager component!");
#endif
            return null;
        }


        var result = steamManager.RefreshLobbies();
        if (result.IsCompleted)
        {
#if Log
            LogManager.Log("Loading Steam Lobbys Completed!", UnityEngine.Color.green);
#endif
            return steamManager.Lobbies;
        }
        else
        {
#if Log
            LogManager.LogError("Failed Loading Steam Lobbys!");
#endif            
        }
        return null;
    }

    public void StartSteamClient(SteamId steamID)
    {
        if (!AuthenticationManager.Instance.SteamAuthentication)
        {
#if Log
            LogManager.Log("Loading Steam Lobbys Canceled! not using steam to authenticate players !", UnityEngine.Color.red);
#endif
            return;
        }

        SteamNetworkManager steamManager = NetworkManager.Singleton.GetComponent<SteamNetworkManager>();
        if (steamManager == null)
        {
#if Log
            LogManager.LogError("NetworkManager does not have a SteamNetworkManager component!");
#endif
            return;
        }
        steamManager.StartClient(steamID);
    }
    #endregion
}
