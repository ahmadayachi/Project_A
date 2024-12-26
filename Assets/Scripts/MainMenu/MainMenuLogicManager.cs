using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                LogManager.LogError($"[{nameof(MainMenuLogicManager)}] - NetworkManager does not have a SteamNetworkManager component!");
#endif
            }
        }
        else
        {

            if (NetworkManager.Singleton.StartHost())
            {
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
    }
    public async Task<List<Lobby>> LoadSteamPublicLobbys()
    {
        if (!AuthenticationManager.Instance.SteamAuthentication)
        {
#if Log
            LogManager.Log($"[{nameof(MainMenuLogicManager)}] - Loading Steam Lobbys Canceled! not using steam to authenticate players !", UnityEngine.Color.red);
#endif
            return null;
        }

        SteamNetworkManager steamManager = NetworkManager.Singleton.GetComponent<SteamNetworkManager>();
        if (steamManager == null)
        {
#if Log
            LogManager.LogError($"[{nameof(MainMenuLogicManager)}] - NetworkManager does not have a SteamNetworkManager component!");
#endif
            return null;
        }


        var result = await steamManager.RefreshLobbies();
        if (result)
        {
#if Log
            LogManager.Log($"[{nameof(MainMenuLogicManager)}] - Loading Steam Lobbys Completed!", UnityEngine.Color.green);
#endif
            return steamManager.Lobbies;
        }
        else
        {
#if Log
            LogManager.LogError($"[{nameof(MainMenuLogicManager)}] - Failed Loading Steam Lobbys!");
#endif            
        }
        return null;
    }

    public void StartClient(SteamId steamID = default(SteamId))
    {
        if (AuthenticationManager.Instance.SteamAuthentication)
        {
            SteamNetworkManager steamManager = NetworkManager.Singleton.GetComponent<SteamNetworkManager>();
            if (steamManager == null)
            {
#if Log
                LogManager.LogError($"[{nameof(MainMenuLogicManager)}] - NetworkManager does not have a SteamNetworkManager component!");
#endif
                return;
            }
            steamManager.StartClient(steamID);
#if Log
            LogManager.Log($"[{nameof(MainMenuLogicManager)}] - Steam Client started succesfully", UnityEngine.Color.green);
#endif
        }
        else
        {
            if (NetworkManager.Singleton.StartClient())
            {

#if Log
                LogManager.Log($"[{nameof(MainMenuLogicManager)}] - Client started succesfully", UnityEngine.Color.green);
#endif
            }
            else
            {
#if Log
                LogManager.LogError($"[{nameof(MainMenuLogicManager)}] - Client failed starting !");
#endif
            }
        }

    }


    #endregion


}
