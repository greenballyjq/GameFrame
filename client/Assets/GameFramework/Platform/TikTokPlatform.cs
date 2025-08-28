#if DOUYINMINIGAME
using System;
using System.Collections.Generic;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;

/// <summary>
/// 抖音平台实现
/// </summary>
public class TikTokPlatform : IPlatform
{
    // 是否已初始化
    private bool _isInitialized = false;

    // Banner广告实例字典(用于管理Banner广告生命周期)
    private Dictionary<string, TTBannerAd> _bannerAdInstances = new Dictionary<string, TTBannerAd>();

    /// <summary>
    /// 初始化
    /// </summary>
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
    
    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
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

    /// <summary>
    /// 分享
    /// </summary>
    /// <param name="shareInfo"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
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

    #region 广告相关方法
    /// <summary>
    /// 展示广告
    /// </summary>
    /// <param name="adType">广告类型：Banner、插屏、激励视频</param>
    /// <param name="adId"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
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

    /// <summary>
    /// 隐藏Banner广告
    /// </summary>
    /// <param name="adId">广告位ID</param>
    public void HideBannerAd(string adId)
    {
        if (_bannerAdInstances.TryGetValue(adId, out TTBannerAd bannerAd) && !bannerAd.IsInvalid())
        {
            bannerAd.Hide();
            Debug.Log($"[TikTokPlatform] 隐藏Banner广告，AdId: {adId}");
        }
    }

    /// <summary>
    /// 销毁Banner广告实例
    /// </summary>
    /// <param name="adId">广告位ID</param>
    public void DestroyBannerAd(string adId)
    {
        if (_bannerAdInstances.TryGetValue(adId, out TTBannerAd bannerAd))
        {
            bannerAd.Destroy();
            _bannerAdInstances.Remove(adId);
            Debug.Log($"[TikTokPlatform] 销毁Banner广告实例，AdId: {adId}");
        }
    }
    #endregion

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

    /// <summary>
    /// 检查用户是否已经授权给当前游戏读取个人信息
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void CheckUserAuthorization(Action<bool> success, Action<string> fail)
    {
        TTGetUserInfoAuth(success, fail);
    }

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
    /// 获取系统信息
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 振动
    /// </summary>
    /// <param name="isShort">是否是短振动</param>
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

    /// <summary>
    /// 跳转其它小游戏
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="path"></param>
    /// <param name="extraData"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
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

    /// <summary>
    /// TT初始化
    /// </summary>
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
                // 注：这里可以额外做保存启动参数的操作，目前并未保存

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
    
    /// <summary>
    /// TT登录
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void TTLogin(Action<string> success, Action<string> fail)
    {
        TTSDK.TT.Login(
               (code, anonymousCode, isLogin) =>
               {
                    Debug.Log($"Login: code:{code},anonymousCode:{anonymousCode},isLogin:{isLogin}");
                    Debug.Log("[TikTokPlaform] 抖音平台登录成功");
                    success(code + "||" + anonymousCode);
               },
               (msg) => {
                   fail(msg);
                   Debug.Log($"[TikTokPlaform] 登录失败 msg={msg}");
               });
    }

    /// <summary>
    /// TT分享
    /// </summary>
    /// <param name="title"></param>
    /// <param name="desc"></param>
    /// <param name="imageUrl"></param>
    /// <param name="link"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void TTShare(string title, string desc, string imageUrl, string link, Action success, Action<string> fail)
    {
        // 构建分享参数JSON
        JsonData shareJson = new JsonData();
        // 分享标题
        shareJson["title"] = title;

        // 分享描述
        shareJson["desc"] = desc;

        // 分享图片
        shareJson["imageUrl"] = imageUrl;

        // 分享链接
        shareJson["channel"] = "link";
        shareJson["query"] = link;

        //调用抖音SDK分享接口
        TT.ShareAppMessage(
            shareJson,
            // 成功回调
            (data) =>
            {
                Debug.Log("[TikTokPlatform] 分享成功");
                success?.Invoke();
            },
            // 失败回调
            (errMsg) =>
            {
                Debug.LogError($"[TikTokPlatform] 分享失败: {errMsg}");
                fail?.Invoke(errMsg);
            },
            // 取消回调
            () =>
            {
                Debug.Log("[TikTokPlatform] 用户取消分享");
                fail?.Invoke("用户取消分享");
            }
        );
    }

    private void TTPay(string orderId, string amount, string desc, Action<string> success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用抖音小游戏的支付API
    }

    #region TT广告相关方法
    /// <summary>
    /// TTBanner广告
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void TTShowBannerAd(string adId, Action success, Action<string> fail)
    {
        try
        {
            // 检查是否已有实例，无效则销毁重建
            if (_bannerAdInstances.TryGetValue(adId, out TTBannerAd existingAd))
            {
                if (existingAd.IsInvalid())
                {
                    Debug.Log($"[TikTokPlatform] Banner实例无效，AdId: {adId}");
                    existingAd.Destroy();
                    _bannerAdInstances.Remove(adId);
                    existingAd = null;
                }
                else
                {
                    // 已有有效实例，直接显示
                    Debug.Log($"[TikTokPlatform] 复用Banner实例并显示，AdId: {adId}");
                    existingAd.Show();
                    success?.Invoke();
                    return;
                }
            }

            // 配置Banner样式，底部水平居中
            TTBannerStyle bannerStyle = new TTBannerStyle();
            bannerStyle.top = Screen.height - 100;
            bannerStyle.left = (Screen.width - 320) / 2;
            bannerStyle.width = GetValidBannerWidth(); // 自动计算合法宽度

            // 配置广告参数（30秒自动刷新一次）
            CreateBannerAdParam adParam = new CreateBannerAdParam
            {
                BannerAdId = adId,
                Style = bannerStyle,
                AdIntervals = 30
            };

            // 创建Banner实例
            TTBannerAd newBannerAd = TT.CreateBannerAd(adParam);
            if (newBannerAd == null)
            {
                string errorMsg = $"[TikTokPlatform] Banner实例创建失败，AdId: {adId}";
                Debug.LogError(errorMsg);
                fail?.Invoke(errorMsg);
                return;
            }

            // 加载成功事件
            newBannerAd.OnLoad += () =>
            {
                Debug.Log($"[TikTokPlatform] Banner加载成功，AdId: {adId}");
                newBannerAd.Show();
                success?.Invoke();
            };

            // 错误事件
            newBannerAd.OnError += (errorCode, errorMsg) =>
            {
                string fullError = $"[TikTokPlatform] Banner错误码:{errorCode}, 描述:{errorMsg}";
                // 特殊错误处理
                switch (errorCode)
                {
                    case 1004:
                        fullError += "（当前无合适广告）";
                        break;
                    case 20002:
                        fullError += "（当前平台不支持Banner）";
                        break;
                    case 120002:
                        fullError += "（当日广告次数已达上限）";
                        break;
                }
                Debug.LogError(fullError);
                fail?.Invoke(fullError);

                // 无效实例移除并销毁
                if (newBannerAd.IsInvalid() && _bannerAdInstances.ContainsKey(adId))
                {
                    _bannerAdInstances.Remove(adId);
                    newBannerAd.Destroy();
                }
            };

            // 关闭事件（仅WebGL有效）
            newBannerAd.OnClose += () =>
            {
                Debug.Log($"[TikTokPlatform] Banner被关闭，AdId: {adId}");
            };

            // 存储Banner广告实例并触发加载
            _bannerAdInstances[adId] = newBannerAd;
            Debug.Log($"[TikTokPlatform] 开始加载Banner广告，AdId: {adId}");
            newBannerAd.Show();

        }
        catch (Exception ex)
        {
            string errorMsg = $"[TikTokPlatform] Banner处理异常: {ex.Message}";
            Debug.LogError(errorMsg);
            fail?.Invoke(errorMsg);
        }
    }

    /// <summary>
    /// 根据横竖屏获取合法的Banner宽度
    /// </summary>
    private int GetValidBannerWidth()
    {
        // 判断横竖屏
        bool isLandscape = Screen.width > Screen.height;

        if (isLandscape)
        {
            // 横屏：宽度限制128~208，取中间值160
            return Mathf.Clamp(160, 128, 208);
        }
        else
        {
            // 竖屏：宽度限制0.8*屏幕宽 ~ 屏幕宽，取0.9倍屏幕宽
            float minWidth = Screen.width * 0.8f;
            float maxWidth = Screen.width;
            return Mathf.Clamp((int)(Screen.width * 0.9f), (int)minWidth, (int)maxWidth);
        }
    }

    /// <summary>
    /// TT插屏广告
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void TTShowInterstitialAd(string adId, Action success, Action<string> fail)
    {
        // 局部变量存储插屏广告实例
        TTInterstitialAd interstitialAd = null;
        try
        {
            // 配置广告参数
            CreateInterstitialAdParam adParam = new CreateInterstitialAdParam
            {
                InterstitialAdId = adId
            };

            // 创建插屏广告实例
            interstitialAd = TT.CreateInterstitialAd(adParam);

            // 绑定加载成功事件
            interstitialAd.OnLoad += () =>
            {
                Debug.Log("[TikTokPlatform] 插屏广告加载成功，准备展示");
                try
                {
                    interstitialAd.Show();
                }
                catch (Exception showEx)
                {
                    string errorMsg = $"[TikTokPlatform] 插屏广告展示失败: {showEx.Message}";
                    Debug.LogError(errorMsg);
                    fail?.Invoke(errorMsg);
                    // 展示失败主动销毁实例
                    interstitialAd?.Destroy();
                }
            };

            // 绑定错误事件
            interstitialAd.OnError += (errorCode, errorMsg) =>
            {
                string fullError = $"[TikTokPlatform] 插屏广告错误码:{errorCode}, 描述:{errorMsg}";

                // 频率限制错误提示
                switch (errorCode)
                {
                    case 2001:
                        fullError += "（小程序启动15秒内不允许展示插屏广告）";
                        break;
                    case 2002:
                        fullError += "（两次广告间隔需≥30秒，请稍后再试）";
                        break;
                    case 2003:
                        fullError += "（当前已有广告正在播放，无法重复展示）";
                        break;
                }
                Debug.LogError(fullError);
                fail?.Invoke(fullError);
                // 错误时主动销毁实例
                interstitialAd?.Destroy();
            };

            // 绑定关闭事件（广告关闭即完成流程）
            interstitialAd.OnClose += () =>
            {
                Debug.Log("[TikTokPlatform] 插屏广告已关闭");
                try
                {
                    success?.Invoke();
                }
                catch (Exception closeEx)
                {
                    string errorMsg = $"[TikTokPlatform] 插屏广告关闭回调异常: {closeEx.Message}";
                    Debug.LogError(errorMsg);
                    fail?.Invoke(errorMsg);
                }
                finally
                {
                    interstitialAd?.Destroy();
                    Debug.Log("[TikTokPlatform] 插屏广告实例已销毁");
                }
            };

            // 触发广告加载
            Debug.Log("[TikTokPlatform] 开始加载插屏广告");
            interstitialAd.Load();
        }
        catch (Exception ex)
        {
            // 捕获整体流程异常
            string errorMsg = $"[TikTokPlatform] 插屏广告处理异常: {ex.Message}";
            Debug.LogError(errorMsg);
            fail?.Invoke(errorMsg);
            // 异常时销毁已创建的实例
            interstitialAd?.Destroy();
        }
    }
    
    /// <summary>
    /// TT激励广告
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void TTShowRewardedAd(string adId, Action success, Action<string> fail)
    {
        // 激励视频实例变量声明
        TTRewardedVideoAd rewardedAd = null;

        try
        {
            // 配置广告参数
            CreateRewardedVideoAdParam adParam = new CreateRewardedVideoAdParam
            {
                AdUnitId = adId,
                Multiton = false,
                ProgressTip = false
            };

            // 创建激励视频实例
            rewardedAd = TT.CreateRewardedVideoAd(adParam);

            // 绑定加载成功事件
            rewardedAd.OnLoad += () =>
            {
                Debug.Log("[TikTokPlatform] 激励视频加载成功，准备展示");
                try
                {
                    rewardedAd.Show();
                }
                catch (Exception showEx)
                {
                    string errorMsg = $"[TikTokPlatform] 激励视频展示失败: {showEx.Message}";
                    Debug.LogError(errorMsg);
                    fail?.Invoke(errorMsg);
                    // 展示失败销毁实例
                    if (rewardedAd != null)
                    {
                        rewardedAd.Destroy();
                    }
                }
            };

            // 绑定错误事件
            rewardedAd.OnError += (errorCode, errorMsg) =>
            {
                string fullError = $"[TikTokPlatform] 激励视频错误码:{errorCode}, 描述:{errorMsg}";
                Debug.LogError(fullError);
                fail?.Invoke(fullError);
                // 错误时销毁实例
                if (rewardedAd != null)
                {
                    rewardedAd.Destroy();
                }
            };

            // 绑定关闭事件
            rewardedAd.OnClose += (isEnded, watchCount) =>
            {
                Debug.Log($"[TikTokPlatform] 激励视频关闭 - 完整观看:{isEnded}, 次数:{watchCount}");
                try
                {
                    // 完整观看才触发成功回调
                    if (isEnded)
                    {
                        Debug.Log("[TikTokPlatform] 完整观看，触发奖励回调");
                        success?.Invoke();
                    }
                    else
                    {
                        string cancelMsg = "[TikTokPlatform] 中途关闭，未获奖励";
                        Debug.Log(cancelMsg);
                        fail?.Invoke(cancelMsg);
                    }
                }
                catch (Exception closeEx)
                {
                    string errorMsg = $"[TikTokPlatform] 关闭回调异常: {closeEx.Message}";
                    Debug.LogError(errorMsg);
                    fail?.Invoke(errorMsg);
                }
                finally
                {
                    // 无论结果，关闭后必销毁实例
                    if (rewardedAd != null)
                    {
                        rewardedAd.Destroy();
                        rewardedAd = null;
                    }
                    Debug.Log("[TikTokPlatform] 激励视频实例已销毁");
                }
            };

            // 触发广告加载
            Debug.Log("[TikTokPlatform] 开始加载激励视频");
            rewardedAd.Load();
        }
        catch (Exception ex)
        {
            // 捕获整体异常
            string errorMsg = $"[TikTokPlatform] 激励视频处理异常: {ex.Message}";
            Debug.LogError(errorMsg);
            fail?.Invoke(errorMsg);
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
            }
        }
    }
    #endregion

    #region TT用户信息授权相关方法 
    /// <summary>
    /// TT判断用户是否已经授权给当前游戏读取个人信息
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void TTGetUserInfoAuth(Action<bool> success, Action<string> fail)
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
    /// TT提前向用户发出授权请求
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
    /// TT引导用户授权
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

    /// <summary>
    /// TT获取系统信息
    /// </summary>
    /// <returns></returns>
    private SystemInfo TTGetSystemInfo()
    {
        // 调用抖音SDK获取系统信息
        TTSystemInfo ttInfo = TT.GetSystemInfo();

        // 转换为通用的SystemInfo对象
        return new SystemInfo
        {
            Platform = ttInfo.hostName, // 宿主名称（如Douyin）
            Version = ttInfo.hostVersion, // 宿主版本号（如19.2.0）
            ScreenWidth = ttInfo.screenWidth, // 屏幕宽度（逻辑像素）
            ScreenHeight = ttInfo.screenHeight, // 屏幕高度（逻辑像素）
            Language = ttInfo.language // 系统语言（如zh_CN）
        };
    }
    
    /// <summary>
    /// TT短振动
    /// </summary>
    private void TTVibrateShort()
    {
         TT.VibrateShort(new VibrateShortParam()
        {
            Success = (res) => 
            {
                Debug.Log($"[TikTokPlatform] 短震动成功，ErrMsg: {res.ErrMsg}");
            },
            Fail = (errInfo) =>
            {
                Debug.LogError($"[TikTokPlatform] 短震动失败，错误码: {errInfo.ErrorCode}，错误信息: {errInfo.ErrMsg}");
            },
        });
    }
    
    /// <summary>
    /// TT长振动
    /// </summary>
    private void TTVibrateLong()
    {
        TT.VibrateLong(new VibrateLongParam()
        {
            Success = (res) => 
            {
                Debug.Log($"[TikTokPlatform] 长振动成功，ErrMsg: {res.ErrMsg}");
            },
            Fail = (errInfo) =>
            {
                Debug.LogError($"[TikTokPlatform] 长振动失败，错误码: {errInfo.ErrorCode}，错误信息: {errInfo.ErrMsg}");
            },
        });
    }
    
    /// <summary>
    /// TT跳转其它小游戏
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="path"></param>
    /// <param name="extraData"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void TTNavigateToMiniGame(string appId, string path, string extraData, Action success, Action<string> fail)
    {
        // 创建跳转参数实例
        NavigateToMiniProgramParam param = new NavigateToMiniProgramParam
        {
            AppId = appId,
            Path = path,
            ExtraData = new Dictionary<string, string>(),
            EnvVersion = EnvVersionEnum.Current
        };

        // 绑定成功回调
        param.Success = (res) =>
        {
            Debug.Log($"[TikTokPlatform] 跳转小游戏成功，AppId: {appId}");
            success?.Invoke();
        };

        // 绑定失败回调
        param.Fail = (errorInfo) =>
        {
            string errorMsg = $"[TikTokPlatform] 跳转小游戏失败 - 错误码: {errorInfo.ErrorCode}, 描述: {errorInfo.ErrMsg}";
            Debug.LogError(errorMsg);
            fail?.Invoke(errorMsg);
        };

        // 调用抖音SDK的跳转接口
        TT.NavigateToMiniProgram(param);
    }
} 
#endif