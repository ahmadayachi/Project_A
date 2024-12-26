
using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayer : NetworkBehaviour
{
    [SerializeField] LobbyPlayerUI _uiRefs;
    private NetworkVariable<FixedString64Bytes> _playerName = new NetworkVariable<FixedString64Bytes>();
    private NetworkVariable<byte> _iconID = new NetworkVariable<byte>();
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _playerName.OnValueChanged += OnPlayerNameValueChanged;
        _iconID.OnValueChanged += OnIconIDValueChanged;

    }

    private void OnIconIDValueChanged(byte previousValue, byte newValue)
    {
        _uiRefs.Icon.sprite = AssetLoader.AllIcons[newValue];
    }

    private void OnPlayerNameValueChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
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
