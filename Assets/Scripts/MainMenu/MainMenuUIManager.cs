using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField]
    private MainPanelsUIRefs _mainPanelsUIRefs;

    private void Awake()
    {
        SetUpMainPanelButtons();
        SetUpCreateLobby();
    }

    private void SetUpMainPanelButtons()
    {
        _mainPanelsUIRefs.CreateLobbyButton.onClick.RemoveAllListeners();
        _mainPanelsUIRefs.CreateLobbyButton.onClick.AddListener(() =>
        {
            //create lobby panel on 
            _mainPanelsUIRefs.CreateLobbyUIRefs.CreateLobbyPanel.SetActive(true);
        });

        _mainPanelsUIRefs.JoinLobbyButton.onClick.RemoveAllListeners();
        _mainPanelsUIRefs.JoinLobbyButton.onClick.AddListener(() =>
        {
            //joining lobby panel on 
            _mainPanelsUIRefs.JoinLobbyPanelUIRefs.JoinLobbyPanel.SetActive(true);
        });

    }
    private void SetUpCreateLobby()
    {

    }
}

