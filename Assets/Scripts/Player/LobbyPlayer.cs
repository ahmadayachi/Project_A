using Steamworks;
using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayer : NetworkBehaviour
{
    [SerializeField]
    private LobbyPlayerUI _uiRefs;

    public NetworkVariable<FixedString32Bytes> Name = new NetworkVariable<FixedString32Bytes>();
    public NetworkVariable<FixedString64Bytes> ID = new NetworkVariable<FixedString64Bytes>();
    public NetworkVariable<byte> IconID = new NetworkVariable<byte>();
    public NetworkVariable<bool> IsReady = new NetworkVariable<bool>();
    public Action<bool> OnIsReady;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Name.OnValueChanged += OnPlayerNameValueChanged;
        IconID.OnValueChanged += OnIconIDValueChanged;
        ID.OnValueChanged += OnIDValueChanged;
        IsReady.OnValueChanged += OnIsReadyValueChanged;
        Init();
    }

    private void OnIsReadyValueChanged(bool previousValue, bool newValue)
    {
        if (IsOwner)
        {
            OnIsReady?.Invoke(newValue);
#if Log
            LogManager.Log($"{Name.Value.ToString()}, ID=>({ID.Value.ToString()}) Is Ready =>[{newValue}]", UnityEngine.Color.green, LogManager.ValueInformationLog);
#endif
        }
    }

    private void OnIDValueChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {

#if Log
        LogManager.Log($"{Name.Value.ToString()} ID is Set !, ID=>({ID.Value.ToString()})", UnityEngine.Color.green, LogManager.ValueInformationLog);
#endif
    }

    private void OnIconIDValueChanged(byte previousValue, byte newValue)
    {
        _uiRefs.Icon.sprite = AssetLoader.AllIcons[newValue];
    }

    private void OnPlayerNameValueChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        _uiRefs.Name.text = newValue.ToString();
    }

    [Rpc(SendTo.Server)]
    private void InitPlayerRpc(FixedString32Bytes playerName, FixedString64Bytes playerID, byte iconID)
    {
        Name.Value = playerName;
        ID.Value = playerID;
        IconID.Value = iconID;
#if Log
        LogManager.Log($"setting Player Info, playerName=>{playerName.ToString()} / playerID{playerID.ToString()}/iconID=>{iconID}", UnityEngine.Color.green, LogManager.ValueInformationLog);
#endif
    }

    private void Init()
    {
        //if it is the owner , he sends his info 
        if (IsOwner)
        {
            FixedString32Bytes playerName = string.Empty;
            FixedString64Bytes playerID = string.Empty;
            //grabbing player Name and ID from steam
            if (AuthenticationManager.Instance.SteamAuthentication)
            {
                playerName = SteamClient.Name;
                playerID = SteamClient.SteamId.ToString();
            }
            else
            {
                //grabing from scriptable object
                playerName = AssetLoader.RunTimeDataHolder.LocalPlayerInfo.Name;
                playerID = Guid.NewGuid().ToString();
            }

            InitPlayerRpc(playerName, playerID, (byte)UnityEngine.Random.Range(1, 9));
        }
        //else syncing the first tick if it exists 
        else
        {
            if (!Name.Value.Equals(default(FixedString32Bytes)))
            {
                _uiRefs.Name.text = Name.Value.ToString();
            }

            if (IconID.Value != 0)
            {
                _uiRefs.Icon.sprite = AssetLoader.AllIcons[IconID.Value];
            }
        }
    }

}
[System.Serializable]
public struct LobbyPlayerUI
{
    public Image Icon;
    public TextMeshProUGUI Name;
}
