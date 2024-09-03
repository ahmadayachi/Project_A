//#define SUPPRESS_DEALER_LOG
//#define SUPPRESS_CARDMANAGER_LOG
//#define SUPPRESS_INFORMATIONVALUE_LOG
//#define SUPPRESS_CARDPOOL_LOG
//#define SUPPRESS_PLAYER_LOG
//#define SUPPRESS_GAMEMODE_LOG
//#define SUPPRESS_VALIDATORS_LOG
#define LOGTOUI
using System.Collections.Generic;
using UnityEngine;

public static class LogManager
{
    #region Debug Keys

    public static string DealerLog = "Dealer";
    public static string CardManagerLog = "CardManager";
    public static string CardPoolLog = "CardPool";
    
    /// <summary>
    /// any loging of a value just to see the value
    /// </summary>
    public static string ValueInformationLog = "Value";
    public static string PlayerLog = "Player";
    public static string Validators = "Validators";
    public static string GameModeLogs = "GameMode";

    #endregion Debug Keys

    #region Log to Ui Methods and fields

    public static List<UILogsData> UILogs;
    //public static List<UILogsData> UILogErrors;

    public static void InitUILogData()
    {
        if (UILogs == null)
            UILogs = new List<UILogsData>();
        //if (UILogErrors == null)
        //    UILogErrors = new List<UILogsData>();
    }

    public static void ClearLogs()
    {
        UILogs.Clear();
        //UILogErrors.Clear();
    }

    #endregion Log to Ui Methods and fields

    public static void LogError(string message)
    {
        Debug.LogError(message);
#if LOGTOUI
        InitUILogData();
        var logData = new UILogsData();
        logData.LogColor = Color.red;
        logData.Log = message;
        UILogs.Add(logData);
        //UILogErrors.Add(logData);
#endif
    }

    public static void Log(string message, Color color, string DebugKey = "")
    {
#if SUPPRESS_DEALER_LOG
        if (DebugKey == CardManagerLog) return;
#endif
#if SUPPRESS_CARDMANAGER_LOG
        if (DebugKey == DealerLog) return;
#endif
#if SUPPRESS_INFORMATIONVALUE_LOG
        if (DebugKey == ValueInformationLog) return;
#endif
#if SUPPRESS_PLAYER_LOG
        if (DebugKey == PlayerLog) return;
#endif
#if SUPPRESS_CARDPOOL_LOG
        if (DebugKey == CardPoolLog) return;
#endif
#if SUPPRESS_VALIDATORS_LOG
        if (DebugKey == Validators) return;
#endif
#if SUPPRESS_GAMEMODE_LOG
        if (DebugKey == GameModeLogs) return;
#endif
        string ColorString = ColorUtility.ToHtmlStringRGB(color);
        Debug.Log($"<color=#{ColorString}>" + message + "</color>");
#if LOGTOUI
        InitUILogData();
        var logData = new UILogsData();
        logData.LogColor = color;
        logData.Log = message;
        UILogs.Add(logData);
#endif
    }
}