
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


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Name.OnValueChanged += OnPlayerNameValueChanged;
        IconID.OnValueChanged += OnIconIDValueChanged;
        ID.OnValueChanged += OnIDValueChanged;

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
}
[System.Serializable]
public struct LobbyPlayerUI
{
    public Image Icon;
    public TextMeshProUGUI Name;
}
