#if DOUYINMINIGAME
using System;
using TTSDK;
using UnityEditor;
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
            // 这里调用抖音小游戏的JS接口，实际项目中需要通过JSLib来实现
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

    public void Login(Action<string> success, Action<string> fail)
    {
        Debug.Log("[TikTokPlatform] 抖音平台登录");

        if (!_isInitialized)
        {
            fail?.Invoke("抖音平台未初始化");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        // 调用抖音小游戏的登录API
        try
        {
            TTLogin(success,fail);
        }
        catch (Exception e)
        {
            Debug.LogError($"[TikTokPlatform] 抖音平台登录失败: {e.Message}");
        }
#else
        // 编辑器模式下模拟登录成功
        Debug.Log("[TikTokPlatform] 编辑器模式下模拟抖音平台登录成功");
#endif
    }


    #region public void Login(Action<LoginResult> success, Action<string> fail)
    /*
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
    */
    #endregion

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
                            AdResult result = new AdResult { IsCompleted = true, AdId = adId };
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
        AdResult result = new AdResult { IsCompleted = true, AdId = adId };
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

    public IUserInfoButton CreateUserInfoButton(int x, int y, int width, int height, Action<UserInfoResult> onTap)
    {
        return null;
    }

    #region 用户信息授权相关方法
    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <param name="success">获取成功</param>
    /// <param name="fail">获取失败</param>
    public void GetUserInfo(Action<UserInfoResult> success, Action<string> fail)
    {
        Debug.Log("[TikTokPlatform] 获取用户信息 ");

        if (!_isInitialized)
        {
            fail?.Invoke("抖音平台未初始化");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR

        // 1.检查用户是否已经授权给当前游戏读取个人信息
        CheckUserAuthorization(
            // *调用接口成功
            (isAuthorized) =>
            {
                // 已经授权直接获取用户信息
                if (isAuthorized)
                {
                    TTGetUserInfo(success, fail);
                }
                else
                {
                    // 2.先调用TTAuthorize请求授权
                    TTAuthorize(
                        // *调用接口成功
                        (isAuthSuccess) =>
                        {
                            // 受权成功，获取用户信息
                            if (isAuthSuccess)
                            {
                                TTGetUserInfo(success, fail);
                            }
                            else
                            {
                                // 3.引导用户授权
                                TTOpenSettingsPanel(
                                    // *调用接口成功
                                    (isManualAuthSuccess) =>
                                    {
                                        // 用户手动同意授权，获取用户信息
                                        if (isManualAuthSuccess)
                                        {
                                            TTGetUserInfo(success, fail);
                                        }
                                        else
                                        {
                                            fail?.Invoke("[TikTokPlatform] 用户拒绝手动授权，无法获取用户信息");
                                        }
                                    },
                                    // *调用接口失败 打开设置面板失败
                                    (errMsg) => fail?.Invoke(errMsg)
                                );
                            }
                        },
                        // *调用接口失败 请求授权失败
                        (errMsg) => fail?.Invoke(errMsg)
                    );
                }
            },
            // *调用接口失败 检查失败
            (errMsg) => fail?.Invoke(errMsg)
        );
#else
        // 编辑器模式下模拟获取用户信息成功
        success.Invoke(new UserInfoResult());
#endif

    }

    /// <summary>
    /// 检查用户是否已经授权给当前游戏读取个人信息
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void CheckUserAuthorization(Action<bool> success, Action<string> fail)
    {
        TT.GetUserInfoAuth(
            (isAuthorized) => {
                success?.Invoke(isAuthorized);
            },
            (errMsg) => {
                fail?.Invoke($"[TikTokPlatform] 检查授权失败: {errMsg}");
            }
        );
    }

    /// <summary>
    /// 提前向用户发出授权请求
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void TTAuthorize(Action<bool> success, Action<string> fail)
    {
        TT.Authorize(
            "scope.userInfo",
            // 受权成功
            (errMsg, data) =>
            {
                success?.Invoke(true); 
            },
            // 受权失败
            (code, msg) =>
            {
                success?.Invoke(false);            
            }
        );
    }

    /// <summary>
    /// 引导用户授权
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void TTOpenSettingsPanel(Action<bool> success, Action<string> fail)
    {
        TT.OpenSettingsPanel(
            // 用户接受授权
            (newIsAuthorized) =>
            {
                success?.Invoke(newIsAuthorized);
            },
            // 用户拒绝授权
            (errMsg) =>
            {
                fail?.Invoke($"[TikTokPlatform] 打开授权面板失败: {errMsg}");
            }
        );
    }

    /// <summary>
    /// TT获取用户信息
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void TTGetUserInfo(Action<UserInfoResult> success, Action<string> fail)
    {
        TT.GetUserInfo(
            (ref TTUserInfo ttUserInfo) =>
            {
                UserInfoResult result = new UserInfoResult
                {
                    ErrCode = 0,
                    ErrMsg = "获取用户信息成功",
                    NickName = ttUserInfo.nickName,
                    AvatarUrl = ttUserInfo.avatarUrl,
                    Gender = ttUserInfo.gender,
                    Country = ttUserInfo.country,
                    Province = ttUserInfo.province,
                    City = ttUserInfo.city,
                    Language = ttUserInfo.language,
                    EncryptedData = ttUserInfo.encryptedData,
                    Iv = ttUserInfo.iv,
                    Signature = ttUserInfo.signature,
                    CloudID = ttUserInfo.cloudId
                };
                success?.Invoke(result);
            },
            (errMsg) => fail?.Invoke($"[TikTokPlatform] 获取用户信息失败: {errMsg}")
        );
    }

#endregion

    private void TTInit()
    {
        TTSDK.TT.InitSDK((code,env) =>
        {
            Debug.Log("[TikTokPlaform] 抖音SDK初始化成功");
            Debug.Log("[TikTokPlaform] 错误码(0 无错误/1 TT Unity SDK 版本不支持/2 Unity Engine 版本不被支持)：" + code);
            Debug.Log("[TikTokPlaform] 宿主：" + env.m_HostEnum);
            Debug.Log("[TikTokPlaform] 是否在真机环境下：" + TT.InContainerEnv);

            // 1. 获取启动参数
            LaunchOption launchOptions = env.GetLaunchOptionsSync();

            // 2. 对启动参数进行安全检查
            if (launchOptions != null && launchOptions.Query != null)
            {
                Debug.Log($"[TikTokPlaform] 获取启动参数：");
                foreach (var item in launchOptions.Query)
                {
                    Debug.Log($"Key: {item.Key}, Value: {item.Value}");
                }
            }
            else
            {
                Debug.Log("[TikTokPlaform] 无启动参数或参数为空");
            }
        });
    }
    
    private void TTLogin(Action<string> success, Action<string> fail)
    {
        TTSDK.TT.Login(
               (code, anonymousCode, isLogin) =>
               {
                   Debug.Log($"Login: code:{code},anonymousCode:{anonymousCode},isLogin:{isLogin}");
                   if (isLogin)
                   {
                       Debug.Log("[TikTokPlaform] 抖音平台登录成功");
                       success(code);
                   }
                   else
                   {
                       fail("isLogin为false，code无效");
                       Debug.Log($"[TikTokPlaform] isLogin为false，code无效");
                   }
                   
               },
               (msg) => {
                   fail(msg);
                   Debug.Log($"[TikTokPlaform] 登录失败 msg={msg}");
               });
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

} 
#endif