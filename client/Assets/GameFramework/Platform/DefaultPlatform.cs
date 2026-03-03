using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 默认平台实现
/// </summary>
public class DefaultPlatform : IPlatform
{
    public void Init()
    {
        Debug.Log("[DefaultPlatform] 初始化默认平台");
    }

    public void Update()
    {
        // 默认平台更新
    }

    public void Release()
    {
        Debug.Log("[DefaultPlatform] 释放默认平台");
    }

    public void Login(Action<string> success, Action<string> fail)
    {
        Debug.Log("[DefaultPlatform] 默认平台登录");
        
        // 模拟登录成功
        //var result = new LoginResult
        //{
        //    UserId = "default_user_id",
        //    UserName = "DefaultUser",
        //    Avatar = "",
        //    Token = "default_token",
        //    ExtraData = new Dictionary<string, object>()
        //};
        
        success?.Invoke("");
    }

    public void Share(ShareInfo shareInfo, Action success, Action<string> fail)
    {
        Debug.Log("[DefaultPlatform] 默认平台分享: " + shareInfo.Title);
        
        // 模拟分享成功
        success?.Invoke();
    }

    public void ShowAd(AdType adType, string adId, Action success, Action<AdResult> close, Action<string> fail)
    {
        Debug.Log($"[DefaultPlatform] 默认平台显示广告: {adType}, ID: {adId}");
        //
        // // 模拟广告完成
        // var result = new AdResult
        // {
        //     IsCompleted = true,
        //     AdId = adId
        // };
        
        success?.Invoke();
    }

    public void Pay(PayInfo payInfo, Action<PayResult> success, Action<string> fail)
    {
        Debug.Log($"[DefaultPlatform] 默认平台支付: {payInfo.Amount} {payInfo.Currency}");
        
        // 模拟支付成功
        var result = new PayResult
        {
            OrderId = payInfo.OrderId,
            IsSuccess = true,
            Receipt = "default_receipt"
        };
        
        success?.Invoke(result);
    }

    public void GetPhoneNumber(Action<string> success, Action<string> fail)
    {
        success?.Invoke("13800138000");
    }

    public SystemInfo GetSystemInfo()
    {
        Debug.Log("[DefaultPlatform] 获取系统信息");
        
        return new SystemInfo
        {
            Platform = Application.platform.ToString(),
            Version = Application.version,
            ScreenWidth = Screen.width,
            ScreenHeight = Screen.height,
            Language = Application.systemLanguage.ToString()
        };
    }

    public void Vibrate(bool isShort)
    {
        Debug.Log($"[DefaultPlatform] 振动: {(isShort ? "短" : "长")}");
        
        // 默认平台使用Unity自带的振动
        #if UNITY_ANDROID || UNITY_IOS
        if (isShort)
        {
            Handheld.Vibrate();
        }
        else
        {
            Handheld.Vibrate();
        }
        #endif
    }

    public void NavigateToMiniGame(string appId, string path, string extraData, Action success, Action<string> fail)
    {
        Debug.Log($"[DefaultPlatform] 跳转小游戏: {appId}, Path: {path}");
        
        // 默认平台不支持跳转小游戏
        fail?.Invoke("默认平台不支持跳转小游戏");
    }

	public void CheckUserAuthorization(Action<bool> success, Action<string> fail)
	{
        success?.Invoke(true);
        // throw new NotImplementedException();
    }

	public IUserInfoButton CreateUserInfoButton(int x, int y, int width, int height, Action<UserInfoResult> onTap)
	{
        onTap?.Invoke(default);
        // throw new NotImplementedException();
        return default;
    }

    public void GetUserInfo(Action<UserInfoResult> success, Action<string> fail)
    {
        success?.Invoke(default);
        // throw new NotImplementedException();
    }
} 