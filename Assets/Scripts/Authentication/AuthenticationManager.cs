//#define PRODUCTIONBUILD
using Unity.Netcode;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance;
   
    [Header("Use steam to Authenticate")]
    [SerializeField]
    private bool _steamAuthentication;
    public bool SteamAuthentication { get => _steamAuthentication; }

    private void Awake()
    {
        SetUpSingleton();

#if PRODUCTIONBUILD
        SetupSteamAuthentication();
#else
        if (_steamAuthentication)
            SetupSteamAuthentication();
        else
            SetUpLocalAuthentication();
#endif
    }
    private void SetupSteamAuthentication()
    {
        //facepunch transport will log in auto
        NetworkManager netManager = Instantiate(AssetLoader.PrefabContainer.SteamNetworkManager);

    }

    private void SetUpLocalAuthentication()
    {
        NetworkManager netManager = Instantiate(AssetLoader.PrefabContainer.LocalNetworkManager);

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
}
