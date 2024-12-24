//#define PRODUCTIONBUILD
using Netcode.Transports.Facepunch;
using Steamworks;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance;
   
    [Header("Use steam to Authenticate")]
    [SerializeField]
    private bool _steamAuthentication;
    public bool SteamAuthentication { get => _steamAuthentication; }

    public Action OnAuthenticationSuccess;
    public Action OnAuthenticationFailure;
   
    private const int MaxWaitTime = 5;

    private Coroutine _steamAuthRoutine;

    private void Awake()
    {
        SetUpSingleton();
        //setting loggin callbacks
        OnAuthenticationSuccess += AuthSuccessLogger;
        OnAuthenticationFailure += AuthFailedLogger;

#if PRODUCTIONBUILD
        SetupSteamAuthentication();
#else
        if (_steamAuthentication)
            SetupSteamAuthentication();
        else
            SetUpLocalAuthentication();
#endif
    }
    private void OnDisable()
    {
        //setting loggin callbacks
        OnAuthenticationSuccess -= AuthSuccessLogger;
        OnAuthenticationFailure -= AuthFailedLogger;
    }
    private void SetupSteamAuthentication()
    {
        //facepunch transport will log in auto`
        NetworkManager netManager = Instantiate(AssetLoader.PrefabContainer.SteamNetworkManager);
       
        if(_steamAuthRoutine != null)
            StopCoroutine(_steamAuthRoutine);
        _steamAuthRoutine = StartCoroutine(SteamAuthenticationRoutine());
    }

    private void SetUpLocalAuthentication()
    {
        NetworkManager netManager = Instantiate(AssetLoader.PrefabContainer.LocalNetworkManager);
        OnAuthenticationSuccess?.Invoke();
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

    private IEnumerator SteamAuthenticationRoutine()
    {
        int timer = 0;
        do
        {
            if (!SteamClient.IsValid)
                yield return new WaitForSeconds(1);
            else
            {
                OnAuthenticationSuccess?.Invoke();
                _steamAuthRoutine = null;
                yield break;
            }

        }while (timer < MaxWaitTime);
        OnAuthenticationFailure?.Invoke();
    }

    private void AuthSuccessLogger()
    {
#if Log
        LogManager.Log($"[{nameof(AuthenticationManager)}] - Authentication Success!", Color.green);
        if(SteamAuthentication)
            LogManager.Log($"[{nameof(AuthenticationManager)}] - Steam Name=>{SteamClient.Name}/ Steam ID =>{SteamClient.SteamId} /AppID=>{SteamClient.AppId} ", Color.green);

#endif
    }
    private void AuthFailedLogger()
    {
#if Log
        LogManager.LogError($"[{nameof(AuthenticationManager)}] - Authentication Failed!");
#endif
    }
}
