#if WEIXINMINIGAME
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

using WeChatWASM;


/// <summary>
/// 微信平台实现
/// </summary>
public class WeChatPlatform : IPlatform
{
    // 是否已初始化
    private bool _isInitialized = false;

    public void Init()
    {
        Debug.Log("[WeChatPlatform] 初始化微信平台");

        // 调用微信小游戏的初始化API
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // 这里调用微信小游戏的JS接口
            // 实际项目中需要通过JSLib来实现
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
            // 调用微信小游戏的获取系统信息API
            // 实际项目中需要通过JSLib来实现
            // 这里模拟获取系统信息
            var systemInfo = WXGetSystemInfo();
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

    private void WXInit()
    {
#if WEIXINMINIGAME
        WX.InitSDK((code) =>
        {
            Debug.Log("微信SDK初始化成功");

        });
#endif
    }


    private void WXLogin(Action<string> successAction, Action<string> failAction)
    {
#if WEIXINMINIGAME
        LoginOption loginOption = new LoginOption()
        {
            success = res => {
                successAction(res.code);
            },


            fail = (result) => {
                failAction(result.errMsg);
                Debug.Log($"登录失败 code={result.errno} msg={result.errMsg}");
            }
        };

        WX.Login(loginOption);
#endif
        
    }

    private void WXShare(string title, string desc, string imageUrl, string link, Action success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用微信小游戏的分享API
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

    private SystemInfo WXGetSystemInfo()
    {
        // 实际项目中通过JSLib调用微信小游戏的获取系统信息API
        return new SystemInfo();
    }

    private void WXVibrateShort()
    {
        // 实际项目中通过JSLib调用微信小游戏的短振动API
        WX.VibrateShort(new VibrateShortOption()
        {
            complete = callbackResult =>
            {
                Debug.Log($"[WechatPlatform] 微信平台短振动完成: {callbackResult}");
            },
            fail = callbackResult =>
            {
                
            },
            success = callbackResult =>
            {
                
            },
            type = "heavy"
        });
    }

    private void WXVibrateLong()
    {
        // 实际项目中通过JSLib调用微信小游戏的长振动API
    }

    private void WXNavigateToMiniProgram(string appId, string path, string extraData, Action success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用微信小游戏的跳转API
    }

    //微信登录
    public void Login(Action<string> success, Action<string> fail)
    {

        LoginOption loginOption = new LoginOption()
        {
            success = res => {
                //successAction(res.code);
                Debug.Log($"[WechatPlatform] 登录成功");
                success.Invoke(res.code);
            },


            fail = (result) => {
                fail.Invoke(result.errMsg);
                Debug.Log($"登录失败 code={result.errno} msg={result.errMsg}");
            }
        };

        WX.Login(loginOption);


    }

    //检测玩家是否授权成功
    public void CheckUserAuthorization(Action<bool> success, Action<string> fail)
    {
#if WEIXINMINIGAME
        WeChatWASM.GetSettingOption option = new WeChatWASM.GetSettingOption();
        option.success = (res) => {
            if (res.authSetting.ContainsKey("scope.userInfo") && res.authSetting["scope.userInfo"])   //有个人信息授权
            {
                WX.GetUserInfo(new GetUserInfoOption
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
                WX.Authorize(new AuthorizeOption
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
#endif
    }

    public void GetUserInfo(Action<UserInfoResult> success, Action<string> fail)
    {
        throw new NotImplementedException();
    }

    public IUserInfoButton CreateUserInfoButton(int x, int y, int width, int height, Action<UserInfoResult> onTap)
    {
#if WEIXINMINIGAME
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
#else
        return (IUserInfoButton)(new UserInfoResult());
#endif

    }
    //#endif
}
#endif