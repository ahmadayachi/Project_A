using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class LogManagerUIPanel : MonoBehaviour
{
    [SerializeField] private RectTransform _inScrollLogsHolder;
    [SerializeField] private RectTransform _offScrollLogsHolder;

    //[SerializeField] private Sprite _errorSprite;
    //[SerializeField] private Sprite _normalLogSprite;

    [SerializeField] private Button _clearLogsUI;
    [SerializeField] private Button _syncLogs;
    [SerializeField] private Button _clearLogsData;

    [SerializeField] private Button _closePanel;

    [Header("Button to Open this Panel Must Not Be a Child ")]
    [SerializeField] private Button _openPanel;

    private GameObject _logPrefab;
    private const float logHeight = 26;
    private const float maxChar = 53;

    /// <summary>
    /// Log thatt are in view
    /// </summary>
    private Dictionary<TextMeshProUGUI, RectTransform> _logsPair = new Dictionary<TextMeshProUGUI, RectTransform>();

    /// <summary>
    /// Logs that are empty and not Used
    /// </summary>
    private Dictionary<TextMeshProUGUI, RectTransform> _emptyLogsPair = new Dictionary<TextMeshProUGUI, RectTransform>();

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        SyncLogs();
    }

    private void Init()
    {
        LogManager.InitUILogData();

        if (_clearLogsUI != null)
        {
            _clearLogsUI.onClick.RemoveAllListeners();
            _clearLogsUI.onClick.AddListener(ClearLogsUI);
        }
        if (_clearLogsData != null)
        {
            _clearLogsData.onClick.RemoveAllListeners();
            _clearLogsData.onClick.AddListener(ClearLogsData);
        }
        if (_syncLogs != null)
        {
            _syncLogs.onClick.RemoveAllListeners();
            _syncLogs.onClick.AddListener(SyncLogs);
        }
        if (_openPanel != null)
        {
            _openPanel.onClick.RemoveAllListeners();
            _openPanel.onClick.AddListener(() => gameObject.SetActive(true));
        }
        if (_closePanel != null)
        {
            _closePanel.onClick.RemoveAllListeners();
            _closePanel.onClick.AddListener(() => gameObject.SetActive(false));
        }

        if (_logPrefab == null)
            _logPrefab = AssetLoader.PrefabContainer.LogPrefab;
        gameObject.SetActive(false);
    }

    private void SyncLogs()
    {
        //cleaning
        ClearLogsUI();

        if (LogManager.UILogs.Count > 0)
        {
            
            for (int index = LogManager.UILogs.Count-1; index >=0 ; index--)
            {
                var log = LogManager.UILogs[index];
                DebugLog(log.Log,log.LogColor);
                //LogManager.UILogs.Remove(log);
            }
        }
    }

    private void DebugLog(string message,Color color)
    {
        //setting up log oustside first
        GameObject _logGO = null;
        RectTransform logRect = null;
        TextMeshProUGUI logText = null;
        if (_emptyLogsPair.Count > 0)
        {
            var emptyLogVassel = _emptyLogsPair.FirstOrDefault();
            logText = emptyLogVassel.Key;
            logRect = emptyLogVassel.Value;
            _logGO = logRect.gameObject;
            _emptyLogsPair.Remove(emptyLogVassel.Key);
        }

        //just incase emptyLogs Pair gets stolen
        if (_logGO == null || logRect == null || logText == null)
        {
            _logGO = Instantiate(_logPrefab, _offScrollLogsHolder);
            logText = _logGO.GetComponentInChildren<TextMeshProUGUI>();
            logRect = _logGO.GetComponent<RectTransform>();
        }

        //setting log image indicator
        //Image logImage = _logGO.GetComponentInChildren<Image>();
        //logImage.sprite = isError ? _errorSprite : _normalLogSprite;

        //setting the message
        logText.text = message;
        logText.color = color;
        // resizing the log
        ResizeLog(message.Length, logRect);

        //setting Log inside the Scroll
        Extention.SetParent(logRect, _inScrollLogsHolder);

        // resizing logHolder for scrollabilty
        //ResizeLogHolder(logRect.transform.localPosition.y);

        // cashing Log
        _logsPair.Add(logText, logRect);
    }

    private void ResizeLog(int charCount, RectTransform logRect)
    {
        float height = logHeight;
        if (charCount > maxChar)
            height = Mathf.Floor((charCount / maxChar) * logHeight) + logHeight;

        logRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    //private void ResizeLogHolder(float logYpos)
    //{
    //    float absY = Mathf.Abs(logYpos);
    //    if (absY >= _inScrollLogsHolder.rect.height)
    //    {
    //        _inScrollLogsHolder.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _inScrollLogsHolder.rect.height + logHeight);
    //    }
    //}

    private void ClearLogsData()
    {
        ////clearing logs data
        LogManager.ClearLogs();
       
    }

    private void ClearLogsUI()
    {
        if (_logsPair.Count == 0) return;
        //emptiying the scroll from logs
        for (int index = 0; index < _logsPair.Count; index++)
        {
            var pair = _logsPair.ElementAt(index);
            pair.Key.text = string.Empty;
            Extention.SetParent(pair.Value, _offScrollLogsHolder);
            pair.Value.transform.localPosition = Vector3.zero;
            _logsPair.Remove(pair.Key);

            //cashing empty log
            _emptyLogsPair.Add(pair.Key, pair.Value);
        }
    }
}