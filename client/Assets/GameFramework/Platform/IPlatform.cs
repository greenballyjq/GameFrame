using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 平台接口 - 定义平台通用功能
/// </summary>
public interface IPlatform
{
    /// <summary>
    /// 初始化平台
    /// </summary>
    void Init();
    
    /// <summary>
    /// 更新平台
    /// </summary>
    void Update();
    
    /// <summary>
    /// 释放平台
    /// </summary>
    void Release();
    
    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="success">成功回调</param>
    /// <param name="fail">失败回调</param>
    void Login(Action<LoginResult> success, Action<string> fail);
    
    /// <summary>
    /// 分享
    /// </summary>
    /// <param name="shareInfo">分享信息</param>
    /// <param name="success">成功回调</param>
    /// <param name="fail">失败回调</param>
    void Share(ShareInfo shareInfo, Action success, Action<string> fail);
    
    /// <summary>
    /// 显示广告
    /// </summary>
    /// <param name="adType">广告类型</param>
    /// <param name="adId">广告ID</param>
    /// <param name="success">成功回调</param>
    /// <param name="fail">失败回调</param>
    void ShowAd(AdType adType, string adId, Action<AdResult> success, Action<string> fail);
    
    /// <summary>
    /// 支付
    /// </summary>
    /// <param name="payInfo">支付信息</param>
    /// <param name="success">成功回调</param>
    /// <param name="fail">失败回调</param>
    void Pay(PayInfo payInfo, Action<PayResult> success, Action<string> fail);
    
    /// <summary>
    /// 获取系统信息
    /// </summary>
    /// <returns>系统信息</returns>
    SystemInfo GetSystemInfo();
    
    /// <summary>
    /// 振动
    /// </summary>
    /// <param name="isShort">是否短振动</param>
    void Vibrate(bool isShort);
    
    /// <summary>
    /// 跳转其他小游戏
    /// </summary>
    /// <param name="appId">小游戏ID</param>
    /// <param name="path">路径</param>
    /// <param name="extraData">额外数据</param>
    /// <param name="success">成功回调</param>
    /// <param name="fail">失败回调</param>
    void NavigateToMiniGame(string appId, string path, string extraData, Action success, Action<string> fail);
}

/// <summary>
/// 登录结果
/// </summary>
public class LoginResult
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Avatar { get; set; }
    public string Token { get; set; }
    public Dictionary<string, object> ExtraData { get; set; }
}

/// <summary>
/// 分享信息
/// </summary>
public class ShareInfo
{
    public string Title { get; set; }
    public string Desc { get; set; }
    public string ImageUrl { get; set; }
    public string Link { get; set; }
}

/// <summary>
/// 广告类型
/// </summary>
public enum AdType
{
    Banner,     // 横幅广告
    Interstitial, // 插屏广告
    Reward,     // 激励视频
    Native      // 原生广告
}

/// <summary>
/// 广告结果
/// </summary>
public class AdResult
{
    public bool IsCompleted { get; set; }
    public string AdId { get; set; }
}

/// <summary>
/// 支付信息
/// </summary>
public class PayInfo
{
    public string OrderId { get; set; }
    public string ProductId { get; set; }
    public float Amount { get; set; }
    public string Currency { get; set; }
    public string Desc { get; set; }
}

/// <summary>
/// 支付结果
/// </summary>
public class PayResult
{
    public string OrderId { get; set; }
    public bool IsSuccess { get; set; }
    public string Receipt { get; set; }
}

/// <summary>
/// 系统信息
/// </summary>
public class SystemInfo
{
    public string Platform { get; set; }
    public string Version { get; set; }
    public float ScreenWidth { get; set; }
    public float ScreenHeight { get; set; }
    public string Language { get; set; }
} 