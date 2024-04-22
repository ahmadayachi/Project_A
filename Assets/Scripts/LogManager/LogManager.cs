#define LOGUI
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class LogManager 
{

#if Log
#if LOGUI
    private static GameObject _logPrefab = AssetLoader.PrefabContainer.LogPrefab;
    private static LogManagerUIPanel _logsHolder;
    private static Button _logButton;
    private const float logHeight = 26;
    private const float maxChar = 53;
    private static List<GameObject> _logsGO = new List<GameObject>();
    private static Dictionary<bool, string> _logs = new Dictionary<bool, string>();
    private static Coroutine _waitCanvas;
    private static GameObject _logPanelGO;
#endif
#endif
    //private void Awake()
    //{
    //    MakeSingleton();
    //    _logsHolder.ClearLogs.onClick.AddListener(ClearLogs);
    //}

    #region Log to Ui Methods

    public static void SetUpLogToUI()
    {
       
    }
    private static void SetUpLogsToUICoroutine()
    {
        if (_waitCanvas == null)

        _waitCanvas = MonoBehaviour.StartCoroutine(WaitCanvasThenInvoke(() =>
        {
            if (_logsHolder == null)
            {
                _logPanelGO = MonoBehaviour.Instantiate(AssetLoader.PrefabContainer.LogManagerPanel);
                _logsHolder = _logPanelGO.GetComponent<LogManagerUIPanel>();
            }
            if (_logButton == null && _logPanelGO != null)
            {
                _logButton = MonoBehaviour.Instantiate(AssetLoader.PrefabContainer.LogButton).GetComponent<Button>();

            _logButton.onClick.AddListener(
                () =>
                {
                    _logPanelGO.SetActive(false);
                }
                );
            }
        }));
    }
    private static IEnumerator WaitCanvasThenInvoke(Action action)
    {
        WaitUntil waitforcanvas = new WaitUntil(() => IsCanvasFound());
        yield return waitforcanvas;
        action?.Invoke();
    }
    private static  bool IsCanvasFound()
    {
       Canvas  _canvas = MonoBehaviour.FindObjectOfType<Canvas>();
        if (_canvas == null) return false;
        if (!_canvas.isActiveAndEnabled) return false;
        return true;
    }
    public void DebugLog(string message,bool isError)
    {
        GameObject _logGO = Instantiate(_logPrefab, _logsHolder.LogsHolder);
        _logsGO.Add(_logGO);
        TextMeshProUGUI logtext = _logGO.GetComponentInChildren<TextMeshProUGUI>();
        logtext.text = message;
        RectTransform logrect = _logGO.GetComponent<RectTransform>();

        ResizeLog(message.Length, logrect);
        ResizeLogHolder(logrect.transform.localPosition.y);
        Image logImage = _logGO.GetComponent<Image>();

        if (isError)
            logImage.sprite = AssetLoader.PrefabContainer.LogErrorSprite;
        else
            logImage.gameObject.SetActive(false);
    }
    private void ResizeLog(int charCount, RectTransform logRect)
    {
        float height = logHeight;
        if (charCount > maxChar)
            height = Mathf.Floor((charCount / maxChar) * logHeight) + logHeight;

        logRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }
    private void ResizeLogHolder(float logypos)
    {
        float absY = Mathf.Abs(logypos);
        if (absY >= _logsHolder.LogsHolder.rect.height)
        {
            _logsHolder.LogsHolder.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _logsHolder.LogsHolder.rect.height + logHeight);
        }
    }
    private void ClearLogs()
    {
        GameObject log;
        for (int index = 0; index < _logsGO.Count; index++)
        {
            log = _logsGO[index];
            _logsGO.Remove(log);
            Destroy(log);
        }
    }
    #endregion
    private void MakeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
    }
}
