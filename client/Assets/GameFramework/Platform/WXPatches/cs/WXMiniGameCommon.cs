#if WEIXINMINIGAME
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// 微信小游戏 MiniGameCommon 插件封装
/// 仅封装插件独有功能：分享图片到游戏圈、推送小游戏动态
/// 游戏圈按钮(wx.createGameClubButton)和游戏圈数据(wx.getGameClubData)
/// 已由 WX-WASM-SDK-V2 原生支持，直接使用 WX.CreateGameClubButton / WX.GetGameClubData
/// </summary>
public class WXMiniGameCommon : MonoBehaviour
{
    private static WXMiniGameCommon _instance;
    public static WXMiniGameCommon Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("WXMiniGameCommon");
                _instance = go.AddComponent<WXMiniGameCommon>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool WXMiniGameCommonIsAvailable();

    [DllImport("__Internal")]
    private static extern bool WXMiniGameCommonCanIUse(string feature);

    // 分享到游戏圈
    [DllImport("__Internal")]
    private static extern void WXShareImageToGameClub(string conf);

    [DllImport("__Internal")]
    private static extern void WXShareCanvasToGameClub(string conf);

    // 推送动态
    [DllImport("__Internal")]
    private static extern void WXInitGameTimeLine();

    [DllImport("__Internal")]
    private static extern void WXGameTimeLineAuthorize();

    [DllImport("__Internal")]
    private static extern bool WXGameTimeLineIsAuthorized();

    [DllImport("__Internal")]
    private static extern void WXGameTimeLinePushFeed(string conf);
#endif

    // ==================== 回调 ====================
    private Dictionary<string, Action<bool, string>> _shareCallbacks = new Dictionary<string, Action<bool, string>>();
    private Dictionary<string, Action<bool, string>> _pushFeedCallbacks = new Dictionary<string, Action<bool, string>>();

    private Action _onTimeLineReady;
    private Action<string> _onTimeLineError;

    private int _callbackIdCounter = 0;
    private string GenerateCallbackId()
    {
        return $"mgc_{_callbackIdCounter++}";
    }

    // ==================== 插件检查 ====================

    /// <summary>
    /// 检查 MiniGameCommon 插件是否可用
    /// </summary>
    public bool IsAvailable()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return WXMiniGameCommonIsAvailable();
#else
        return false;
#endif
    }

    /// <summary>
    /// 检查某个功能是否可用
    /// </summary>
    public bool CanIUse(string feature)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return WXMiniGameCommonCanIUse(feature);
#else
        return false;
#endif
    }

    // ==================== 分享到游戏圈 ====================

    /// <summary>
    /// 分享CDN图片到游戏圈
    /// </summary>
    /// <param name="imageUrl">CDN图片地址（需在MP中配置downloadFile域名）</param>
    /// <param name="title">分享标题</param>
    /// <param name="content">分享文案</param>
    /// <param name="callback">回调 (success, errMsg)</param>
    public void ShareImageToGameClub(string imageUrl, string title, string content, Action<bool, string> callback = null)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string callbackId = GenerateCallbackId();
        if (callback != null)
        {
            _shareCallbacks[callbackId] = callback;
        }

        var config = new ShareImageConfig
        {
            url = imageUrl,
            title = title,
            content = content,
            callbackId = callbackId,
        };

        WXShareImageToGameClub(JsonUtility.ToJson(config));
#else
        Debug.Log($"[WXMiniGameCommon] ShareImageToGameClub - url:{imageUrl} title:{title}");
        callback?.Invoke(false, "Editor模式不支持");
#endif
    }

    /// <summary>
    /// 分享canvas截图到游戏圈
    /// </summary>
    /// <param name="title">分享标题</param>
    /// <param name="content">分享文案</param>
    /// <param name="callback">回调 (success, errMsg)</param>
    /// <param name="x">截图起始x</param>
    /// <param name="y">截图起始y</param>
    /// <param name="width">截图宽度（0=全屏）</param>
    /// <param name="height">截图高度（0=全屏）</param>
    public void ShareCanvasToGameClub(string title, string content, Action<bool, string> callback = null,
        int x = 0, int y = 0, int width = 0, int height = 0)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string callbackId = GenerateCallbackId();
        if (callback != null)
        {
            _shareCallbacks[callbackId] = callback;
        }

        var config = new ShareCanvasConfig
        {
            x = x,
            y = y,
            width = width,
            height = height,
            title = title,
            content = content,
            callbackId = callbackId,
        };

        WXShareCanvasToGameClub(JsonUtility.ToJson(config));
#else
        Debug.Log($"[WXMiniGameCommon] ShareCanvasToGameClub - title:{title}");
        callback?.Invoke(false, "Editor模式不支持");
#endif
    }

    // ==================== 推送小游戏动态 ====================

    /// <summary>
    /// 初始化 GameTimeLine（推送动态前必须调用）
    /// </summary>
    /// <param name="onReady">TimeLine就绪回调</param>
    /// <param name="onError">错误回调</param>
    public void InitGameTimeLine(Action onReady = null, Action<string> onError = null)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        _onTimeLineReady = onReady;
        _onTimeLineError = onError;
        WXInitGameTimeLine();
#else
        Debug.Log("[WXMiniGameCommon] InitGameTimeLine - Editor模式不支持");
#endif
    }

    /// <summary>
    /// 拉起授权弹窗
    /// </summary>
    public void TimeLineAuthorize()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WXGameTimeLineAuthorize();
#endif
    }

    /// <summary>
    /// 检查是否已授权
    /// </summary>
    public bool IsTimeLineAuthorized()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return WXGameTimeLineIsAuthorized();
#else
        return false;
#endif
    }

    /// <summary>
    /// 推送动态
    /// </summary>
    /// <param name="eventId">事件ID（在MP后台配置）</param>
    /// <param name="query">透传信息</param>
    /// <param name="callback">回调 (success, errMsg)</param>
    public void PushFeed(int eventId, string query = "", Action<bool, string> callback = null)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string callbackId = GenerateCallbackId();
        if (callback != null)
        {
            _pushFeedCallbacks[callbackId] = callback;
        }

        var config = new PushFeedConfig
        {
            eventId = eventId,
            query = query,
            callbackId = callbackId,
        };

        WXGameTimeLinePushFeed(JsonUtility.ToJson(config));
#else
        Debug.Log($"[WXMiniGameCommon] PushFeed - eventId:{eventId} query:{query}");
        callback?.Invoke(false, "Editor模式不支持");
#endif
    }

    // ==================== JS回调接收 ====================

    public void OnShareToGameClubCallback(string json)
    {
        Debug.Log($"[WXMiniGameCommon] OnShareToGameClubCallback: {json}");
        var result = JsonUtility.FromJson<CallbackResult>(json);
        if (_shareCallbacks.TryGetValue(result.callbackId, out var callback))
        {
            _shareCallbacks.Remove(result.callbackId);
            callback?.Invoke(result.errMsg == "ok", result.errMsg);
        }
    }

    public void OnGameTimeLineReady(string json)
    {
        Debug.Log("[WXMiniGameCommon] GameTimeLine Ready");
        _onTimeLineReady?.Invoke();
    }

    public void OnGameTimeLineError(string json)
    {
        Debug.Log($"[WXMiniGameCommon] GameTimeLine Error: {json}");
        var result = JsonUtility.FromJson<CallbackResult>(json);
        _onTimeLineError?.Invoke(result.errMsg);
    }

    public void OnGameTimeLinePushFeedCallback(string json)
    {
        Debug.Log($"[WXMiniGameCommon] PushFeedCallback: {json}");
        var result = JsonUtility.FromJson<CallbackResult>(json);
        if (_pushFeedCallbacks.TryGetValue(result.callbackId, out var callback))
        {
            _pushFeedCallbacks.Remove(result.callbackId);
            callback?.Invoke(result.errMsg == "ok", result.errMsg);
        }
    }

    // ==================== 数据结构 ====================

    [Serializable]
    private class CallbackResult
    {
        public string callbackId;
        public string errMsg;
    }

    [Serializable]
    private class ShareImageConfig
    {
        public string url;
        public string title;
        public string content;
        public string callbackId;
    }

    [Serializable]
    private class ShareCanvasConfig
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public string title;
        public string content;
        public string callbackId;
    }

    [Serializable]
    private class PushFeedConfig
    {
        public int eventId;
        public string query;
        public string callbackId;
    }
}
#endif
