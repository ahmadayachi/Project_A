//#define PRODUCTIONBUILD
using Unity.Netcode;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance;
   
    [Header("Use steam to Authenticate")]
    [SerializeField]
    private bool _steam;
    public bool Steam { get => _steam; }

    private void Awake()
    {
        SetUpSingleton();

#if PRODUCTIONBUILD
        SetupSteamAuthentication();
#else
        if (_steam)
            SetupSteamAuthentication();
        else
            SetUpLocalAuthentication();
#endif
    }
    private void SetupSteamAuthentication()
    {
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
