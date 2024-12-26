using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField]
    private MainPanelsUIRefs _mainPanelsUIRefs;
    [SerializeField]
    private MainMenuLogicManager _mainLogicManager;

    private List<GameObject> _lobbysGO = new List<GameObject>();
    private Queue<GameObject> _emptyLobbysGO = new Queue<GameObject>();
    public const string Public = "Public";
    public const string Private = "Private";

    private void Awake()
    {
        SetUpMainPanelButtons();
        SetUpCreateLobby();
        SetUpJoinLobbyPanel();
        if (AuthenticationManager.Instance.SteamAuthentication)
            SetUpJoinPublicLobbyPanel();
        SetUpJoinPrivateLobbyPanel();
    }

    #region MainPanel

    private void SetUpMainPanelButtons()
    {
        _mainPanelsUIRefs.CreateLobbyButton.onClick.RemoveAllListeners();
        _mainPanelsUIRefs.CreateLobbyButton.onClick.AddListener(() =>
        {
            _mainPanelsUIRefs.ButtonsHolder.gameObject.SetActive(false);
            //create lobby panel on 
            _mainPanelsUIRefs.CreateLobbyUIRefs.CreateLobbyPanel.SetActive(true);
        });

        _mainPanelsUIRefs.JoinLobbyButton.onClick.RemoveAllListeners();
        _mainPanelsUIRefs.JoinLobbyButton.onClick.AddListener(() =>
        {
            _mainPanelsUIRefs.ButtonsHolder.gameObject.SetActive(false);

            //joining lobby panel on 
            _mainPanelsUIRefs.JoinLobbyPanelUIRefs.JoinLobbyPanel.SetActive(true);
        });

    }
    #endregion

    #region Create Lobby Panel 

    private void SetUpCreateLobby()
    {
        SetUpDropDown();

        //setting up create button 
        _mainPanelsUIRefs.CreateLobbyUIRefs.CreateButton.onClick.RemoveAllListeners();
        _mainPanelsUIRefs.CreateLobbyUIRefs.CreateButton.onClick.AddListener(CreateButton);

        //setting Up Back Button 
        _mainPanelsUIRefs.CreateLobbyUIRefs.BackButton.onClick.RemoveAllListeners();
        _mainPanelsUIRefs.CreateLobbyUIRefs.BackButton.onClick.AddListener(CreateLobbyBackButton);
    }
    private void SetUpDropDown()
    {
        //clearing all options 
        _mainPanelsUIRefs.CreateLobbyUIRefs.LobbyPrivacy.ClearOptions();

        //setting up options 
        var options = new List<string> { "Public", "Private" };
        _mainPanelsUIRefs.CreateLobbyUIRefs.LobbyPrivacy.AddOptions(options);

        //setting public as default value;
        _mainPanelsUIRefs.CreateLobbyUIRefs.LobbyPrivacy.value = 0;
    }
    private void CreateButton()
    {
        //grabing lobbys name 
        var lobbyName = _mainPanelsUIRefs.CreateLobbyUIRefs.LobbyName.text;

        if (string.IsNullOrEmpty(lobbyName))
        {
#if Log
            LogManager.Log("Create Lobby failed !, Lobby Name is empty!", UnityEngine.Color.red, LogManager.ValueInformationLog);
#endif
            return;
        }
        else
        {
            //cashing Lobby data 
            int privateIndex = 1;
            bool isPrivate = _mainPanelsUIRefs.CreateLobbyUIRefs.LobbyPrivacy.value == privateIndex;
            var lobbyData = new LobbyData(lobbyName, isPrivate);
            AssetLoader.RunTimeDataHolder.LobbySettings = lobbyData;
        }
        //starting a session 
        _mainLogicManager.StartHost();

    }
    private void CreateLobbyBackButton()
    {
        _mainPanelsUIRefs.CreateLobbyUIRefs.CreateLobbyPanel.SetActive(false);
        _mainPanelsUIRefs.ButtonsHolder.SetActive(true);
    }
    #endregion

    #region Join Lobby Panel
    private void SetUpJoinLobbyPanel()
    {
        _mainPanelsUIRefs.JoinLobbyPanelUIRefs.JoinPrivateLobbyButton.onClick.RemoveAllListeners();
        _mainPanelsUIRefs.JoinLobbyPanelUIRefs.JoinPrivateLobbyButton.onClick.AddListener(JoinPrivateButton);
        if (AuthenticationManager.Instance.SteamAuthentication)
        {
            _mainPanelsUIRefs.JoinLobbyPanelUIRefs.PublicLobbysButton.onClick.RemoveAllListeners();
            _mainPanelsUIRefs.JoinLobbyPanelUIRefs.PublicLobbysButton.onClick.AddListener(JoinPublicLobbyButton);
        }
        else
        {
            _mainPanelsUIRefs.JoinLobbyPanelUIRefs.PublicLobbysButton.gameObject.SetActive(false);
        }

        _mainPanelsUIRefs.JoinLobbyPanelUIRefs.BackButton.onClick.RemoveAllListeners();
        _mainPanelsUIRefs.JoinLobbyPanelUIRefs.BackButton.onClick.AddListener(JoinLobbyBackButton);
    }
    private void JoinPrivateButton()
    {
        _mainPanelsUIRefs.JoinLobbyPanelUIRefs.JoinLobbyPanel.SetActive(false);
        _mainPanelsUIRefs.JoinPrivateLobbyPanelUIRefs.JoinPrivateLobbyPanel.SetActive(true);
    }

    private void JoinPublicLobbyButton()
    {
        _mainPanelsUIRefs.JoinLobbyPanelUIRefs.JoinLobbyPanel.SetActive(false);
        _mainPanelsUIRefs.PublicLobbysUIRefs.PublicLobbysPanel.SetActive(true);
        //refresh public lobbys here 
        RefreshLobbys();
    }


    private async void RefreshLobbys()
    {
        var lobbys = await _mainLogicManager.LoadSteamPublicLobbys();

        if (lobbys == null || lobbys.Count <= 0)
        {
#if Log
            LogManager.Log($"[{nameof(MainMenuUIManager)}] - No Lobbys Found!", UnityEngine.Color.yellow);
#endif
            return;
        }

#if Log
        LogManager.Log($"[{nameof(MainMenuUIManager)}] - Lobbys Loaded Count=> {lobbys.Count}", UnityEngine.Color.green);

#endif
        ResetLobbysGO();
        foreach (var lobby in lobbys)
        {
            GameObject lobbyGO = null;
            if (_emptyLobbysGO.Count > 0)
                lobbyGO = _emptyLobbysGO.Dequeue();
            else
                lobbyGO = Instantiate(AssetLoader.PrefabContainer.PublicSteamLobbyPrefab, _mainPanelsUIRefs.PublicLobbysUIRefs.PublicLobbysHolders);
            var lobbyName = lobbyGO.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

            lobbyName.text = lobby.GetData("name");
            var lobbyButton = lobbyGO.GetComponentInChildren<Button>();
            lobbyButton.onClick.RemoveAllListeners();
            lobbyButton.onClick.AddListener(() =>
            {
                _mainLogicManager.StartClient(lobby.Owner.Id);
            });
            _lobbysGO.Add(lobbyGO);
        }
    }

    private void JoinLobbyBackButton()
    {
        _mainPanelsUIRefs.JoinLobbyPanelUIRefs.JoinLobbyPanel.SetActive(false);
        _mainPanelsUIRefs.ButtonsHolder.SetActive(true);
    }

    private void ResetLobbysGO()
    {
        if (_lobbysGO.Count > 0)
        {
            foreach (var item in _lobbysGO)
            {
                var lobbyName = item.GetComponent<TextMeshProUGUI>();
                lobbyName.text = string.Empty;
                var lobbyButton = item.GetComponent<Button>();
                lobbyButton.onClick.RemoveAllListeners();
                _emptyLobbysGO.Enqueue(item);
            }
            _lobbysGO.Clear();
        }
    }
    #endregion

    #region Join Public Lobby
    private void SetUpJoinPublicLobbyPanel()
    {
        _mainPanelsUIRefs.PublicLobbysUIRefs.RefreshLobbysButton.onClick.RemoveAllListeners();
        _mainPanelsUIRefs.PublicLobbysUIRefs.RefreshLobbysButton.onClick.AddListener(RefreshLobbys);

        _mainPanelsUIRefs.PublicLobbysUIRefs.BackButton.onClick.RemoveAllListeners();
        _mainPanelsUIRefs.PublicLobbysUIRefs.BackButton.onClick.AddListener(PublicLobbysBackButton);
    }
    private void PublicLobbysBackButton()
    {
        _mainPanelsUIRefs.PublicLobbysUIRefs.PublicLobbysPanel.SetActive(false);
        _mainPanelsUIRefs.JoinLobbyPanelUIRefs.JoinLobbyPanel.SetActive(true);
    }
    #endregion

    #region Join Private Lobby Panel
    private void SetUpJoinPrivateLobbyPanel()
    {
        //setting up the joing Button 
        _mainPanelsUIRefs.JoinPrivateLobbyPanelUIRefs.JoinButton.onClick.RemoveAllListeners();
        _mainPanelsUIRefs.JoinPrivateLobbyPanelUIRefs.JoinButton.onClick.AddListener(PrivateJoinButton);

        //setting up Back Button 
        _mainPanelsUIRefs.JoinPrivateLobbyPanelUIRefs.BackButton.onClick.RemoveAllListeners();
        _mainPanelsUIRefs.JoinPrivateLobbyPanelUIRefs.BackButton.onClick.AddListener(JoinPrivateBackButton);
    }
    private void PrivateJoinButton()
    {
        if (AuthenticationManager.Instance.SteamAuthentication)
        {
            //if no code input return 
            if (string.IsNullOrEmpty(_mainPanelsUIRefs.JoinPrivateLobbyPanelUIRefs.LobbyCode.text))
            {
#if Log
                LogManager.Log($"[{nameof(MainMenuUIManager)}] - Lobby Code is Empty!", UnityEngine.Color.yellow);
#endif
                return;
            }
            //code must be a ulong 
            if (ulong.TryParse(_mainPanelsUIRefs.JoinPrivateLobbyPanelUIRefs.LobbyCode.text, out ulong code))
            {
                _mainLogicManager.StartClient(code);

#if Log
                LogManager.Log($"[{nameof(MainMenuUIManager)}] - Client is Starting", UnityEngine.Color.grey);

#endif
            }
            else
            {
#if Log
                LogManager.Log($"[{nameof(MainMenuUIManager)}] - Failed Parsing Lobby Code!", UnityEngine.Color.yellow);
#endif
            }
        }
        else
        {
            _mainLogicManager.StartClient();
#if Log
            LogManager.Log($"[{nameof(MainMenuUIManager)}] - Client is Starting", UnityEngine.Color.grey);

#endif
        }
    }
    private void JoinPrivateBackButton()
    {
        _mainPanelsUIRefs.JoinPrivateLobbyPanelUIRefs.JoinPrivateLobbyPanel.SetActive(false);
        _mainPanelsUIRefs.JoinLobbyPanelUIRefs.JoinLobbyPanel.SetActive(true);
    }
    #endregion
}

