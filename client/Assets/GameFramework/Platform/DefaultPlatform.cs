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

    public void CreatGameClubButton(int left, int top, int width, int height)
    {

    }

    public void ShareImageToGameClub(string imageUrl, string title, string content, Action<bool, string> callback = null)
    {
        Debug.Log("[DefaultPlatform] ShareImageToGameClub 不支持");
        callback?.Invoke(false, "默认平台不支持");
    }

    public void ShareCanvasToGameClub(string title, string content, Action<bool, string> callback = null)
    {
        Debug.Log("[DefaultPlatform] ShareCanvasToGameClub 不支持");
        callback?.Invoke(false, "默认平台不支持");
    }

    public void PushGameFeed(int eventId, string query, Action<bool, string> callback = null)
    {
        Debug.Log("[DefaultPlatform] PushGameFeed 不支持");
        callback?.Invoke(false, "默认平台不支持");
    }

    public void ShowToast(string title, string icon, double duration, bool mask, string image = "")
    {
        
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

    public void ShowRewardAd(string adId, string userId, Action success, Action<AdResult> close, Action<string> fail)
    {
        Debug.Log($"[DefaultPlatform] 默认平台显示激励广告(服务端验证): ID: {adId}, userId: {userId}");
        success?.Invoke();
        close?.Invoke(new AdResult { IsCompleted = true, AdId = adId });
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

    public void ShowToast(string title, string icon = "none", int duration = 1500, bool mask = false, Action success = null, Action<string> fail = null)
    {
        Debug.Log($"[DefaultPlatform] ShowToast: {title}");
        success?.Invoke();
    }

    public void HideToast(Action success = null, Action<string> fail = null)
    {
        Debug.Log("[DefaultPlatform] HideToast");
        success?.Invoke();
    }

    public void ShowModal(string title, string content, bool showCancel = true, string confirmText = "确定", string cancelText = "取消", Action<bool, string> success = null, Action<string> fail = null)
    {
        Debug.Log($"[DefaultPlatform] ShowModal: {title} - {content}");
        success?.Invoke(true, "");
    }

    public void ShowLoading(string title, bool mask = false, Action success = null, Action<string> fail = null)
    {
        Debug.Log($"[DefaultPlatform] ShowLoading: {title}");
        success?.Invoke();
    }

    public void HideLoading(Action success = null, Action<string> fail = null)
    {
        Debug.Log("[DefaultPlatform] HideLoading");
        success?.Invoke();
    }

    public void ShowActionSheet(string[] itemList, string itemColor = "#000000", string alertText = "", Action<int> success = null, Action<string> fail = null)
    {
        Debug.Log($"[DefaultPlatform] ShowActionSheet: itemCount:{itemList?.Length}");
        success?.Invoke(0);
    }
}