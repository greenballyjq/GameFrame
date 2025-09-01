#if WEIXINMINIGAME
using System;
using UnityEditor;
using UnityEngine;
using WeChatWASM;

/// <summary>
/// 微信平台实现
/// </summary>
public class WeChatPlatform : IPlatform
{
    // 是否已初始化
    private bool _isInitialized = false;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        Debug.Log("[WeChatPlatform] 初始化微信平台");

        // 调用微信小游戏的初始化API
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            WXInit();
            _isInitialized = true;
            Debug.Log("[WeChatPlatform] 微信平台初始化成功");
        }
        catch (Exception e)
        {
            Debug.LogError($"[WeChatPlatform] 微信平台初始化失败: {e.Message}");
        }
#else
        // 编辑器模式下模拟初始化成功
        _isInitialized = true;
        Debug.Log("[WeChatPlatform] 编辑器模式下模拟微信平台初始化");
#endif
    }

    public void Update()
    {
        // 微信平台更新
    }

    public void Release()
    {
        Debug.Log("[WeChatPlatform] 释放微信平台");
        _isInitialized = false;
    }

    /// <summary>
    /// 微信登录
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void Login(Action<string> success, Action<string> fail)
    {
        Debug.Log("[WeChatPlatform] 微信平台登录");

        if (!_isInitialized)
        {
            fail?.Invoke("微信平台未初始化");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        // 调用微信小游戏的登录API
        try
        {
            WXLogin(success, fail);
        }
        catch(Exception e)
        {
            Debug.LogError($"[WeChatPlatform] 微信平台登录失败: {e.Message}");
        }
        WXLogin(success, fail);
#else
        // 编辑器模式下模拟登录成功
        Debug.Log("[WeChatPlatform] 编辑器模式下模拟微信平台登录成功");
#endif

        WeChatWASM.LoginOption loginOption = new WeChatWASM.LoginOption()
        {
            success = res => {
                success(res.code);
            },


            fail = (result) => {
                fail(result.errMsg);
                Debug.Log($"登录失败 code={result.errno} msg={result.errMsg}");
            }
        };

        WeChatWASM.WX.Login(loginOption);
    }

    public void Share(ShareInfo shareInfo, Action success, Action<string> fail)
    {
        Debug.Log("[WeChatPlatform] 微信平台分享: " + shareInfo.Title);

        if (!_isInitialized)
        {
            fail?.Invoke("微信平台未初始化");
            return;
        }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // 调用微信小游戏的分享API
            // 实际项目中需要通过JSLib来实现
            WXShare(
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
            fail?.Invoke($"微信分享失败: {e.Message}");
        }
#else
        // 编辑器模式下模拟分享成功
        success?.Invoke();
#endif
    }

    public void ShowAd(AdType adType, string adId, Action<AdResult> success, Action<string> fail)
    {
        Debug.Log($"[WeChatPlatform] 微信平台显示广告: {adType}, ID: {adId}");

        if (!_isInitialized)
        {
            fail?.Invoke("微信平台未初始化");
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
                    WXShowBannerAd(
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
                    WXShowInterstitialAd(
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
                    WXShowRewardedAd(
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
                    fail?.Invoke($"微信平台不支持 {adType} 类型的广告");
                    break;
            }
        }
        catch (Exception e)
        {
            fail?.Invoke($"微信显示广告失败: {e.Message}");
        }
#else
        // 编辑器模式下模拟广告完成
        var result = new AdResult { IsCompleted = true, AdId = adId };
        success?.Invoke(result);
#endif
    }

    public void Pay(PayInfo payInfo, Action<PayResult> success, Action<string> fail)
    {
        Debug.Log($"[WeChatPlatform] 微信平台支付: {payInfo.Amount} {payInfo.Currency}");

        if (!_isInitialized)
        {
            fail?.Invoke("微信平台未初始化");
            return;
        }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // 调用微信小游戏的支付API
            // 实际项目中需要通过JSLib来实现
            WXPay(
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
            fail?.Invoke($"微信支付失败: {e.Message}");
        }
#else
        // 编辑器模式下模拟支付成功
        var result = new PayResult
        {
            OrderId = payInfo.OrderId,
            IsSuccess = true,
            Receipt = "wx_receipt_12345"
        };

        success?.Invoke(result);
#endif
    }

    //检测玩家是否授权成功
    public void CheckUserAuthorization(Action<bool> success, Action<string> fail)
    {

        WeChatWASM.GetSettingOption option = new WeChatWASM.GetSettingOption();
        option.success = (res) => {
            if (res.authSetting.ContainsKey("scope.userInfo") && res.authSetting["scope.userInfo"])   //有个人信息授权
            {
                WeChatWASM.WX.GetUserInfo(new WeChatWASM.GetUserInfoOption
                {
                    success = (res) =>
                    {
                        //res.userInfo;  //这里是成功获取的数据
                    },
                    fail = (res) => { }
                });
            }
            else
            {
                WeChatWASM.WX.Authorize(new WeChatWASM.AuthorizeOption
                {
                    scope = "scope.userInfo",
                    success = res => {
                        success(true);
                    },
                    fail = (_) => {
                        fail("授权失败");
                    },
                });
            }
        };
        WeChatWASM.WX.GetSetting(option);
    }

    public IUserInfoButton CreateUserInfoButton(int x, int y, int width, int height, Action<UserInfoResult> onTap)
    {
        WeChatWASM.WXUserInfoButton btn = WeChatWASM.WX.CreateUserInfoButton(x, y, width, height, "zh_CN", false);
        // 监听授权区域的点击
        btn.OnTap((res) =>
        {
            if (res.errCode == 0)
            {
                // 创建用户信息对象
                UserInfoResult userInfo = new UserInfoResult
                {
                    NickName = res.userInfo.nickName,
                    AvatarUrl = res.userInfo.avatarUrl,
                    Gender = res.userInfo.gender,
                    Country = res.userInfo.country,
                    Province = res.userInfo.province,
                    City = res.userInfo.city,
                    Language = res.userInfo.language,
                    EncryptedData = res.encryptedData,
                    Iv = res.iv
                };
                onTap?.Invoke(userInfo);
            }

            //隐藏
            btn.Hide();
        });

        return (IUserInfoButton)btn;

    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void GetUserInfo(Action<UserInfoResult> success, Action<string> fail)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 获取系统信息
    /// </summary>
    /// <returns></returns>
    public SystemInfo GetSystemInfo()
    {
        Debug.Log("[WeChatPlatform] 获取微信系统信息");

        if (!_isInitialized)
        {
            Debug.LogWarning("[WeChatPlatform] 微信平台未初始化，返回默认系统信息");
            return new SystemInfo
            {
                Platform = "WeChat",
                Version = "Unknown",
                ScreenWidth = Screen.width,
                ScreenHeight = Screen.height,
                Language = "zh_CN"
            };
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            SystemInfo systemInfo = WXGetSystemInfo();
            return systemInfo;
        }
        catch (Exception e)
        {
            Debug.LogError($"[WeChatPlatform] 获取微信系统信息失败: {e.Message}");
            return new SystemInfo
            {
                Platform = "WeChat",
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
            Platform = "WeChat",
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
    /// <param name="isShort"></param>
    public void Vibrate(bool isShort)
    {
        Debug.Log($"[WeChatPlatform] 微信平台振动: {(isShort ? "短" : "长")}");

        if (!_isInitialized)
        {
            Debug.LogWarning("[WeChatPlatform] 微信平台未初始化，无法振动");
            return;
        }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // 调用微信小游戏的振动API
            // 实际项目中需要通过JSLib来实现
            if (isShort)
            {
                WXVibrateShort();
            }
            else
            {
                WXVibrateLong();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[WeChatPlatform] 微信平台振动失败: {e.Message}");
        }
#else
        // 编辑器模式下模拟振动
        Debug.Log($"[WeChatPlatform] 编辑器模式下模拟微信振动: {(isShort ? "短" : "长")}");
#endif
    }

    public void NavigateToMiniGame(string appId, string path, string extraData, Action success, Action<string> fail)
    {
        Debug.Log($"[WeChatPlatform] 微信平台跳转小游戏: {appId}, Path: {path}");

        if (!_isInitialized)
        {
            fail?.Invoke("微信平台未初始化");
            return;
        }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // 调用微信小游戏的跳转API
            // 实际项目中需要通过JSLib来实现
            WXNavigateToMiniProgram(
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
            fail?.Invoke($"微信跳转小游戏失败: {e.Message}");
        }
#else
        // 编辑器模式下模拟跳转成功
        success?.Invoke();
#endif
    }

    /// <summary>
    /// WX初始化
    /// </summary>
    private void WXInit()
    {
        WeChatWASM.WX.InitSDK((code) =>
        {
            Debug.Log($"[WeChatPlatform] 微信SDK初始化成功， code={code}");
        });
    }

    /// <summary>
    /// WX登录
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void WXLogin(Action<string> success, Action<string> fail)
    {
        WeChatWASM.LoginOption loginOption = new WeChatWASM.LoginOption()
        {
            success = (res) => {
                Debug.Log($"[WechatPlatform] 微信登录成功,code:{res.code},errMsg:{res.errMsg}");
                success.Invoke(res.code);
            },


            fail = (result) => {
                Debug.Log($"[WechatPlatform] 微信登录失败,code:{result.errno},msg:{result.errMsg}");
                fail.Invoke(result.errMsg);
            }
        };

        WeChatWASM.WX.Login(loginOption);
    }

    /// <summary>
    /// WX分享
    /// </summary>
    /// <param name="title"></param>
    /// <param name="desc"></param>
    /// <param name="imageUrl"></param>
    /// <param name="link"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void WXShare(string title, string desc, string imageUrl, string link, Action success, Action<string> fail)
    {
        // 构建分享参数
        ShareAppMessageOption shareOption = new ShareAppMessageOption
        {
            title = title,
            imageUrl = imageUrl,
            path = link.Contains("?") ? link.Split('?')[0] : link,
            query = link.Contains("?") ? link.Split('?')[1] : string.Empty
        };

        // 调用微信分享API
        WX.ShareAppMessage(shareOption);

        Debug.Log($"[WeChatPlatform] 分享完成");

    }

    private void WXShowBannerAd(string adId, Action success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用微信小游戏的Banner广告API
    }

    private void WXShowInterstitialAd(string adId, Action success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用微信小游戏的插屏广告API
    }

    private void WXShowRewardedAd(string adId, Action success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用微信小游戏的激励视频API
    }

    private void WXPay(string orderId, string amount, string desc, Action<string> success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用微信小游戏的支付API
    }

    /// <summary>
    /// WX获取系统信息
    /// </summary>
    /// <returns></returns>
    private SystemInfo WXGetSystemInfo()
    {
        WeChatWASM.SystemInfo wxSysInfo = null;
        GetSystemInfoOption sysInfoOption = new GetSystemInfoOption
        {
            // 获取成功
            success = (info) =>
            {
                wxSysInfo = info;
            },
            // 获取失败
            fail = (err) =>
            {
                Debug.LogError($"[WeChatPlatform] 获取微信系统信息失败: {err.errMsg}");
            }
        };

        // 调用微信SDK获取系统信息
        WX.GetSystemInfo(sysInfoOption);

        // 转换为通用的SystemInfo对象
        return new SystemInfo
        {
            Platform = wxSysInfo.platform,
            Version = wxSysInfo.version,
            ScreenWidth = (float)wxSysInfo.screenWidth,
            ScreenHeight = (float)wxSysInfo.screenHeight,
            Language = wxSysInfo.language
        };
    }

    /// <summary>
    /// 微信短振动
    /// </summary>
    private void WXVibrateShort()
    {
        WX.VibrateShort(new VibrateShortOption()
        {
            type = "medium",
            success = (callbackResult) =>
            {
                Debug.Log($"[WeChatPlatform] 短振动成功,ErrMsg:{callbackResult.errMsg}");
            },
            fail = (callbackResult) =>
            {
                Debug.Log($"[WeChatPlatform] 短振动失败,ErrMsg:{callbackResult.errMsg}");
            }
        });
    }

    /// <summary>
    /// WX长振动
    /// </summary>
    private void WXVibrateLong()
    {
        WX.VibrateLong (new VibrateLongOption()
        {
            success = (callbackResult) =>
            {
                Debug.Log($"[WeChatPlatform] 长振动成功,ErrMsg:{callbackResult.errMsg}");
            },
            fail = (callbackResult) =>
            {
                Debug.Log($"[WeChatPlatform] 长振动失败,ErrMsg:{callbackResult.errMsg}");
            }
        });
    }

    private void WXNavigateToMiniProgram(string appId, string path, string extraData, Action success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用微信小游戏的跳转API
    }

    

    
}
#endif