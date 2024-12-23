using Steamworks.Data;
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
        JoinPublicLobbyPanel();
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
            //crashing Lobby data 
            int privateIndex = 1;
            bool isPrivate = _mainPanelsUIRefs.CreateLobbyUIRefs.LobbyPrivacy.value == privateIndex;
            var lobbyData = new LobbyData(lobbyName, isPrivate);

            //starting a session 
            _mainLogicManager.StartHost();
        }
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

        _mainPanelsUIRefs.JoinLobbyPanelUIRefs.PublicLobbysButton.onClick.RemoveAllListeners();
        _mainPanelsUIRefs.JoinLobbyPanelUIRefs.PublicLobbysButton.onClick.AddListener(JoinPublicLobbyButton);

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


    private void RefreshLobbys()
    {
        var lobbys = _mainLogicManager.LoadSteamPublicLobbys();

        if (lobbys == null)
        {
            return;
        }
        else if (lobbys.Count <= 0)
        {
#if Log
            LogManager.Log("No Lobbys Found!", UnityEngine.Color.yellow);
#endif
        }
        ResetLobbysGO();
        foreach (var lobby in lobbys)
        {
            GameObject lobbyGO = null;
            if (_emptyLobbysGO.Count > 0)
                lobbyGO = _emptyLobbysGO.Dequeue();
            else
                lobbyGO = Instantiate(AssetLoader.PrefabContainer.PublicSteamLobbyPrefab, _mainPanelsUIRefs.PublicLobbysUIRefs.PublicLobbysHolders);
            var lobbyName = lobbyGO.GetComponent<TextMeshProUGUI>();
            lobbyName.text = lobby.GetData("name");
            var lobbyButton = lobbyGO.GetComponent<Button>();
            lobbyButton.onClick.RemoveAllListeners();
            lobbyButton.onClick.AddListener(() =>
            {
                _mainLogicManager.StartSteamClient(lobby.Owner.Id);
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
    private void JoinPublicLobbyPanel()
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
}

