using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogManagerUIPanel : MonoBehaviour
{
    [SerializeField] RectTransform _logsHolder;
    public RectTransform LogsHolder { get => _logsHolder;}
    [SerializeField] Button _clearLogs;
    public Button ClearLogs { get => _clearLogs;}
}
