using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor;
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
        LookForLobby();
        Init();
    }

    private void LookForLobby()
    {
        if (IsLocalPlayer)
        {
            var lobby = FindFirstObjectByType<LobbyManager>();
            if (lobby == null)
            {
#if Log
                LogManager.LogError($"[{nameof(LobbyPlayer)}] - Failed to Find A Lobby Manager Class !");
#endif
                return;
            }

            lobby.SetUpLocalPlayer(this);
        }
    }
    private void OnIsReadyValueChanged(bool previousValue, bool newValue)
    {
        if (IsLocalPlayer && IsClient)
        {
            OnIsReady?.Invoke(newValue);
#if Log
            LogManager.Log($"[{nameof(LobbyPlayer)}] - {Name.Value.ToString()}, ID=>({ID.Value.ToString()}) Is Ready =>[{newValue}]", UnityEngine.Color.green, LogManager.ValueInformationLog);
#endif
        }
    }

    private void OnIDValueChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {

#if Log
        LogManager.Log($"[{nameof(LobbyPlayer)}] - {Name.Value.ToString()} ID is Set !, ID=>({ID.Value.ToString()})", UnityEngine.Color.green, LogManager.ValueInformationLog);
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
        //setting the isready deaulfting to false at first 
        //if it is the hosts player setting it to true
        IsReady.Value = IsLocalPlayer ? true : false;
#if Log
        LogManager.Log($"[{nameof(LobbyPlayer)}] - setting Player Info, playerName=>{playerName.ToString()} / playerID{playerID.ToString()}/iconID=>{iconID}", UnityEngine.Color.green, LogManager.ValueInformationLog);
#endif
    }
    [Rpc(SendTo.Server)]
    public void IsReadyRpc()
    {
        //toggling the is ready status 
        IsReady.Value = !IsReady.Value;
    }
    private void Init()
    {
        //if it is the owner , he sends his info 
        if (IsLocalPlayer)
        {
            FixedString32Bytes playerName = AssetLoader.RunTimeDataHolder.LocalPlayerInfo.Name;
            FixedString64Bytes playerID = AssetLoader.RunTimeDataHolder.LocalPlayerInfo.ID;
            byte IconD = (byte)AssetLoader.RunTimeDataHolder.LocalPlayerInfo.IconID;

            InitPlayerRpc(playerName, playerID, (byte)UnityEngine.Random.Range(1, 9));
        }
        //else syncing the first tick if it exists 
        else
        {
            if (!Name.Value.Equals(default(FixedString32Bytes)))
                _uiRefs.Name.text = Name.Value.ToString();

            if (IconID.Value != 0)
                _uiRefs.Icon.sprite = AssetLoader.AllIcons[IconID.Value];
        }
    }
    public override string ToString() { return $"LobbyPlayer:\n" + $"Name: {Name.Value}\n" + $"ID: {ID.Value}\n" + $"IconID: {IconID.Value}\n" + $"IsReady: {IsReady.Value}"; }

}
[System.Serializable]
public struct LobbyPlayerUI
{
    public Image Icon;
    public TextMeshProUGUI Name;
}

#if UNITY_EDITOR
[CustomEditor(typeof(LobbyPlayer))]
public class LobbyPlayerCustomInspector : Editor
{
    private GUIStyle headerStyle;
    private GUIStyle labelStyle;

    private void OnEnable()
    {
        headerStyle = new GUIStyle()
        {
            richText = true,
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState() { textColor = Color.cyan }
        };

        labelStyle = new GUIStyle()
        {
            richText = true,
            fontSize = 14,
            normal = new GUIStyleState() { textColor = Color.white }
        };
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        LobbyPlayer _this = (LobbyPlayer)target;
        EditorGUILayout.LabelField("<color=cyan>Lobby Player Info</color>", headerStyle);
        try
        {
            GUILayout.Label($"<color=white> Name: {_this.Name.Value}</color>", labelStyle);
            GUILayout.Label($"<color=white> ID: {_this.ID.Value}</color>", labelStyle);
            GUILayout.Label($"<color=white> Icon ID: {_this.IconID.Value}</color>", labelStyle);
            GUILayout.Label($"<color=white> Is Ready: {_this.IsReady.Value}</color>", labelStyle);
        }
        catch
        {
            // Handle exceptions if necessary
        }
    }
}
#endif
