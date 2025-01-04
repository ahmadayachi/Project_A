using Steamworks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    [SerializeField]
    private ProfileManagerUIRefs _uiRefs;

    private List<ProfileIcon> _allIcons = new List<ProfileIcon>();
    public const string Player = "Player";
    private void Start()
    {
        SetUpIcons();
        LoadPlayerData();
        LoadIcon();
        _uiRefs.CloseButton.onClick.AddListener(OnCloseProfilePanelClicked);
    }

    private void SavePlayerName()
    {
        string playerName = string.Empty;
        playerName = _uiRefs.PlayerName.text;
        if (!string.IsNullOrEmpty(playerName))
            AssetLoader.RunTimeDataHolder.LocalPlayerInfo.Name = playerName;
    }

    private void LoadPlayerData()
    {
        //grabbing player Name and ID from steam
        string playerName = string.Empty;
        string playerID = string.Empty;
        if (AuthenticationManager.Instance.SteamAuthentication)
        {
            playerName = SteamClient.Name;
            playerID = SteamClient.SteamId.ToString();
        }
        else
        {
            playerName = Player;
            playerID = playerID = Guid.NewGuid().ToString();
        }
        AssetLoader.RunTimeDataHolder.LocalPlayerInfo.Name = playerName;
        AssetLoader.RunTimeDataHolder.LocalPlayerInfo.ID = playerID;
        //loading player name to UI
        _uiRefs.PlayerName.text = playerName;
    }
    private void LoadIcon()
    {
        foreach (var icon in _allIcons)
        {
            if (icon.ID == AssetLoader.RunTimeDataHolder.LocalPlayerInfo.IconID)
            {
                icon.ToggleIconHighlight(true);
                _uiRefs.SelectedProfileIcon.sprite = icon.UIRefs.SpriteHolder.sprite;
            }
        }
    }
    private void SetUpIcons()
    {
        _allIcons.Clear();
        for (int index = 0; index < AssetLoader.AllIcons.Count; index++)
        {
            var icon = AssetLoader.AllIcons[index];
            var profileIcon = Instantiate(AssetLoader.PrefabContainer.ProfileIconPrefab,_uiRefs.IconsHolder);
            profileIcon.ID = index;
            profileIcon.SetIcon(icon);
            profileIcon.OnIconClicked += OnIconClicked;
            profileIcon.ToggleIconHighlight(false);
            _allIcons.Add(profileIcon);
        }
    }
    private void OnIconClicked(int ID)
    {
        foreach (var icon in _allIcons)
        {
            if (icon.ID == ID)
            {
                icon.ToggleIconHighlight(true);
                _uiRefs.SelectedProfileIcon.sprite = icon.UIRefs.SpriteHolder.sprite;
                AssetLoader.RunTimeDataHolder.LocalPlayerInfo.IconID = ID;
                Debug.Log($"Icon Clicked id=>{ID}");

            }
            else
                icon.ToggleIconHighlight(false);
        }
    }
    private void OnCloseProfilePanelClicked()
    {
        SavePlayerName();
    }
}
[Serializable]
public struct ProfileManagerUIRefs
{
    public GameObject ProfilePanel;
    public Image SelectedProfileIcon;
    public TMP_InputField PlayerName;
    public Transform IconsHolder;
    public Button CloseButton;
}