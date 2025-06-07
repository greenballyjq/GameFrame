using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 抖音平台实现
/// </summary>
public class TikTokPlatform : IPlatform
{
    // 是否已初始化
    private bool _isInitialized = false;
    
    public void Init()
    {
        Debug.Log("[TikTokPlatform] 初始化抖音平台");
        
        // 调用抖音小游戏的初始化API
        #if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // 这里调用抖音小游戏的JS接口
            // 实际项目中需要通过JSLib来实现
            TTInit();
            _isInitialized = true;
            Debug.Log("[TikTokPlatform] 抖音平台初始化成功");
        }
        catch (Exception e)
        {
            Debug.LogError($"[TikTokPlatform] 抖音平台初始化失败: {e.Message}");
        }
        #else
        // 编辑器模式下模拟初始化成功
        _isInitialized = true;
        Debug.Log("[TikTokPlatform] 编辑器模式下模拟抖音平台初始化");
        #endif
    }

    public void Update()
    {
        // 抖音平台更新
    }

    public void Release()
    {
        Debug.Log("[TikTokPlatform] 释放抖音平台");
        _isInitialized = false;
    }

    public void Login(Action<LoginResult> success, Action<string> fail)
    {
        Debug.Log("[TikTokPlatform] 抖音平台登录");
        
        if (!_isInitialized)
        {
            fail?.Invoke("抖音平台未初始化");
            return;
        }
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // 调用抖音小游戏的登录API
            // 实际项目中需要通过JSLib来实现
            TTLogin(
                (code) => {
                    // 使用code调用游戏服务器换取用户信息
                    // 这里模拟获取用户信息
                    var result = new LoginResult
                    {
                        UserId = "tt_user_" + UnityEngine.Random.Range(10000, 99999),
                        UserName = "TikTokUser",
                        Avatar = "",
                        Token = code,
                        ExtraData = new Dictionary<string, object>()
                    };
                    
                    success?.Invoke(result);
                },
                (error) => {
                    fail?.Invoke(error);
                }
            );
        }
        catch (Exception e)
        {
            fail?.Invoke($"抖音登录失败: {e.Message}");
        }
        #else
        // 编辑器模式下模拟登录成功
        var result = new LoginResult
        {
            UserId = "tt_user_12345",
            UserName = "TikTokUser",
            Avatar = "",
            Token = "tt_token_12345",
            ExtraData = new Dictionary<string, object>()
        };
        
        success?.Invoke(result);
        #endif
    }

    public void Share(ShareInfo shareInfo, Action success, Action<string> fail)
    {
        Debug.Log("[TikTokPlatform] 抖音平台分享: " + shareInfo.Title);
        
        if (!_isInitialized)
        {
            fail?.Invoke("抖音平台未初始化");
            return;
        }
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // 调用抖音小游戏的分享API
            // 实际项目中需要通过JSLib来实现
            TTShare(
                shareInfo.Title,
                shareInfo.Desc,
                shareInfo.ImageUrl,
                shareInfo.Link,
                () => {
                    success?.Invoke();
                },
                (error) => {
                    fail?.Invoke(error);
                }
            );
        }
        catch (Exception e)
        {
            fail?.Invoke($"抖音分享失败: {e.Message}");
        }
        #else
        // 编辑器模式下模拟分享成功
        success?.Invoke();
        #endif
    }

    public void ShowAd(AdType adType, string adId, Action<AdResult> success, Action<string> fail)
    {
        Debug.Log($"[TikTokPlatform] 抖音平台显示广告: {adType}, ID: {adId}");
        
        if (!_isInitialized)
        {
            fail?.Invoke("抖音平台未初始化");
            return;
        }
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // 根据广告类型调用不同的API
            switch (adType)
            {
                case AdType.Banner:
                    // 显示Banner广告
                    TTShowBannerAd(
                        adId,
                        () => {
                            var result = new AdResult { IsCompleted = true, AdId = adId };
                            success?.Invoke(result);
                        },
                        (error) => {
                            fail?.Invoke(error);
                        }
                    );
                    break;
                
                case AdType.Interstitial:
                    // 显示插屏广告
                    TTShowInterstitialAd(
                        adId,
                        () => {
                            var result = new AdResult { IsCompleted = true, AdId = adId };
                            success?.Invoke(result);
                        },
                        (error) => {
                            fail?.Invoke(error);
                        }
                    );
                    break;
                
                case AdType.Reward:
                    // 显示激励视频
                    TTShowRewardedAd(
                        adId,
                        () => {
                            var result = new AdResult { IsCompleted = true, AdId = adId };
                            success?.Invoke(result);
                        },
                        (error) => {
                            fail?.Invoke(error);
                        }
                    );
                    break;
                
                default:
                    fail?.Invoke($"抖音平台不支持 {adType} 类型的广告");
                    break;
            }
        }
        catch (Exception e)
        {
            fail?.Invoke($"抖音显示广告失败: {e.Message}");
        }
        #else
        // 编辑器模式下模拟广告完成
        var result = new AdResult { IsCompleted = true, AdId = adId };
        success?.Invoke(result);
        #endif
    }

    public void Pay(PayInfo payInfo, Action<PayResult> success, Action<string> fail)
    {
        Debug.Log($"[TikTokPlatform] 抖音平台支付: {payInfo.Amount} {payInfo.Currency}");
        
        if (!_isInitialized)
        {
            fail?.Invoke("抖音平台未初始化");
            return;
        }
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // 调用抖音小游戏的支付API
            // 实际项目中需要通过JSLib来实现
            TTPay(
                payInfo.OrderId,
                payInfo.Amount.ToString(),
                payInfo.Desc,
                (receipt) => {
                    var result = new PayResult
                    {
                        OrderId = payInfo.OrderId,
                        IsSuccess = true,
                        Receipt = receipt
                    };
                    
                    success?.Invoke(result);
                },
                (error) => {
                    fail?.Invoke(error);
                }
            );
        }
        catch (Exception e)
        {
            fail?.Invoke($"抖音支付失败: {e.Message}");
        }
        #else
        // 编辑器模式下模拟支付成功
        var result = new PayResult
        {
            OrderId = payInfo.OrderId,
            IsSuccess = true,
            Receipt = "tt_receipt_12345"
        };
        
        success?.Invoke(result);
        #endif
    }

    public SystemInfo GetSystemInfo()
    {
        Debug.Log("[TikTokPlatform] 获取抖音系统信息");
        
        if (!_isInitialized)
        {
            Debug.LogWarning("[TikTokPlatform] 抖音平台未初始化，返回默认系统信息");
            return new SystemInfo
            {
                Platform = "TikTok",
                Version = "Unknown",
                ScreenWidth = Screen.width,
                ScreenHeight = Screen.height,
                Language = "zh_CN"
            };
        }
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // 调用抖音小游戏的获取系统信息API
            // 实际项目中需要通过JSLib来实现
            // 这里模拟获取系统信息
            var systemInfo = TTGetSystemInfo();
            return systemInfo;
        }
        catch (Exception e)
        {
            Debug.LogError($"[TikTokPlatform] 获取抖音系统信息失败: {e.Message}");
            return new SystemInfo
            {
                Platform = "TikTok",
                Version = "Unknown",
                ScreenWidth = Screen.width,
                ScreenHeight = Screen.height,
                Language = "zh_CN"
            };
        }
        #else
        // 编辑器模式下模拟系统信息
        return new SystemInfo
        {
            Platform = "TikTok",
            Version = "1.0.0",
            ScreenWidth = Screen.width,
            ScreenHeight = Screen.height,
            Language = "zh_CN"
        };
        #endif
    }

    public void Vibrate(bool isShort)
    {
        Debug.Log($"[TikTokPlatform] 抖音平台振动: {(isShort ? "短" : "长")}");
        
        if (!_isInitialized)
        {
            Debug.LogWarning("[TikTokPlatform] 抖音平台未初始化，无法振动");
            return;
        }
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // 调用抖音小游戏的振动API
            // 实际项目中需要通过JSLib来实现
            if (isShort)
            {
                TTVibrateShort();
            }
            else
            {
                TTVibrateLong();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[TikTokPlatform] 抖音平台振动失败: {e.Message}");
        }
        #else
        // 编辑器模式下模拟振动
        Debug.Log($"[TikTokPlatform] 编辑器模式下模拟抖音振动: {(isShort ? "短" : "长")}");
        #endif
    }

    public void NavigateToMiniGame(string appId, string path, string extraData, Action success, Action<string> fail)
    {
        Debug.Log($"[TikTokPlatform] 抖音平台跳转小游戏: {appId}, Path: {path}");
        
        if (!_isInitialized)
        {
            fail?.Invoke("抖音平台未初始化");
            return;
        }
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // 调用抖音小游戏的跳转API
            // 实际项目中需要通过JSLib来实现
            TTNavigateToMiniGame(
                appId,
                path,
                extraData,
                () => {
                    success?.Invoke();
                },
                (error) => {
                    fail?.Invoke(error);
                }
            );
        }
        catch (Exception e)
        {
            fail?.Invoke($"抖音跳转小游戏失败: {e.Message}");
        }
        #else
        // 编辑器模式下模拟跳转成功
        success?.Invoke();
        #endif
    }
    
    // 以下是模拟的抖音API调用，实际项目中需要通过JSLib实现
    #if UNITY_WEBGL && !UNITY_EDITOR
    private void TTInit()
    {
        // 实际项目中通过JSLib调用抖音小游戏的初始化API
    }
    
    private void TTLogin(Action<string> success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用抖音小游戏的登录API
    }
    
    private void TTShare(string title, string desc, string imageUrl, string link, Action success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用抖音小游戏的分享API
    }
    
    private void TTShowBannerAd(string adId, Action success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用抖音小游戏的Banner广告API
    }
    
    private void TTShowInterstitialAd(string adId, Action success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用抖音小游戏的插屏广告API
    }
    
    private void TTShowRewardedAd(string adId, Action success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用抖音小游戏的激励视频API
    }
    
    private void TTPay(string orderId, string amount, string desc, Action<string> success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用抖音小游戏的支付API
    }
    
    private SystemInfo TTGetSystemInfo()
    {
        // 实际项目中通过JSLib调用抖音小游戏的获取系统信息API
        return new SystemInfo();
    }
    
    private void TTVibrateShort()
    {
        // 实际项目中通过JSLib调用抖音小游戏的短振动API
    }
    
    private void TTVibrateLong()
    {
        // 实际项目中通过JSLib调用抖音小游戏的长振动API
    }
    
    private void TTNavigateToMiniGame(string appId, string path, string extraData, Action success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用抖音小游戏的跳转API
    }
    #endif
} 