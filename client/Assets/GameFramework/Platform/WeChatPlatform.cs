#if WEIXINMINIGAME
using System;
using System.Collections.Generic;
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

    // 保存Banner广告实例的字典（key=adUnitId）
    private Dictionary<string, WXCustomAd> _bannerAdDict = new Dictionary<string, WXCustomAd>();

    // 记录Banner广告显示状态（key=adUnitId，value=是否正在显示）
    private Dictionary<string, bool> _bannerShowStateDict = new Dictionary<string, bool>();

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
            Debug.LogError($"[WeChatPlatform] 微信平台初始化失败 errMsg:{e.Message}");
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
    /// 登录
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void Login(Action<string> success, Action<string> fail)
    {
        Debug.Log("[WeChatPlatform] 微信平台登录");

        if (!_isInitialized)
        {
            fail?.Invoke("[WeChatPlatform] 微信平台未初始化");
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
            Debug.LogError($"[WeChatPlatform] 微信平台登录失败 errMsg:{e.Message}");
        }
        WXLogin(success, fail);
#else
        // 编辑器模式下模拟登录成功
        Debug.Log("[WeChatPlatform] 编辑器模式下模拟微信平台登录成功");
#endif
    }

    /// <summary>
    /// 分享
    /// </summary>
    /// <param name="shareInfo"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void Share(ShareInfo shareInfo, Action success, Action<string> fail)
    {
        Debug.Log("[WeChatPlatform] 微信平台分享");

        if (!_isInitialized)
        {
            fail?.Invoke("[WeChatPlatform] 微信平台未初始化");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
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
            fail?.Invoke($"微信分享失败 errMsg:{e.Message}");
        }
#else
        // 编辑器模式下模拟分享成功
        success?.Invoke();
#endif
    }

    #region 广告相关
    /// <summary>
    /// 展示广告
    /// </summary>
    /// <param name="adType"></param>
    /// <param name="adId"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void ShowAd(AdType adType, string adId, Action<AdResult> success, Action<string> fail)
    {
        Debug.Log($"[WeChatPlatform] 微信平台显示广告 adType:{adType} adId:{adId}");

        if (!_isInitialized)
        {
            fail?.Invoke("[WeChatPlatform] 微信平台未初始化");
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
                            AdResult result = new AdResult 
                            { 
                                IsCompleted = true, 
                                AdId = adId 
                            };
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
                            var result = new AdResult 
                            { 
                                IsCompleted = true, 
                                AdId = adId 
                            };
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
                            var result = new AdResult 
                            { 
                                IsCompleted = true, 
                                AdId = adId 
                            };
                            success?.Invoke(result);
                        },
                        (error) => {
                            fail?.Invoke(error);
                        }
                    );
                    break;
                
                default:
                    fail?.Invoke($"[WeChatPlatform] 微信平台不支持 {adType} 类型的广告");
                    break;
            }
        }
        catch (Exception e)
        {
            fail?.Invoke($"[WeChatPlatform] 微信显示广告失败 errMsg:{e.Message}");
        }
#else
        // 编辑器模式下模拟广告完成
        success?.Invoke(new AdResult 
        { 
            IsCompleted = true,
            AdId = adId 
        });
        
#endif
    }

    #region Banner广告相关
    /// <summary>
    /// 显示Banner广告
    /// </summary>
    public void ShowBannerAd(string adId, Action success, Action<string> fail)
    {
        WXShowBannerAd(adId, success, fail);
    }

    /// <summary>
    /// 隐藏Banner广告
    /// </summary>
    public void HideBannerAd(string adId, Action success, Action<string> fail)
    {
        if (!_bannerAdDict.TryGetValue(adId, out WXCustomAd bannerAd))
        {
            fail?.Invoke($"[WeChatPlatform] 隐藏失败，Banner实例不存在 adId:{adId}");
            return;
        }

        bannerAd.Hide(
            (hideRes) =>
            {
                Debug.Log($"[WeChatPlatform] Banner广告隐藏成功 adId:{adId}");
                _bannerShowStateDict[adId] = false;
                success?.Invoke();
            },
            (hideFailRes) =>
            {
                string errorMsg = $"[WeChatPlatform] Banner广告隐藏失败 adId:{adId} errCode:{hideFailRes.errCode}";
                Debug.LogError(errorMsg);
                fail?.Invoke(errorMsg);
            }
        );
    }

    /// <summary>
    /// 查询Banner广告是否正在显示
    /// </summary>
    public bool IsBannerAdShowing(string adId)
    {
        _bannerShowStateDict.TryGetValue(adId, out bool isShowing);
        return isShowing;
    }

    /// <summary>
    /// 设置Banner广告位置（左上角坐标）
    /// </summary>
    public void SetBannerPosition(string adId, int left, int top)
    {
        if (_bannerAdDict.TryGetValue(adId, out WXCustomAd bannerAd))
        {
            bannerAd.style.left = left;
            bannerAd.style.top = top;
            Debug.Log($"[WeChatPlatform] Banner位置已更新 adId:{adId} left:{left} top:{top}");
        }
    }

    /// <summary>
    /// 获取Banner广告当前位置
    /// </summary>
    public void GetBannerCurrentPosition(string adId, out int left, out int top)
    {
        left = 0;
        top = 0;

        if (_bannerAdDict.TryGetValue(adId, out WXCustomAd bannerAd))
        {
            left = bannerAd.style.left;
            top = bannerAd.style.top;
        }
    }

    /// <summary>
    /// 销毁Banner广告实例
    /// </summary>
    public void DestroyBannerAd(string adId)
    {
        if (_bannerAdDict.TryGetValue(adId, out WXCustomAd bannerAd))
        {
            // 清理事件监听
            bannerAd.OffLoad(null);
            bannerAd.OffError(null);
            bannerAd.OffClose(null);
            bannerAd.OffHide(null);

            // 销毁实例
            bannerAd.Destroy();

            // 从字典移除
            _bannerAdDict.Remove(adId);
            _bannerShowStateDict.Remove(adId);

            Debug.Log($"[WeChatPlatform] Banner广告已销毁 adId:{adId}");
        }
    }

    /// <summary>
    /// 判断Banner广告实例是否存在
    /// </summary>
    public bool IsBannerAdExist(string adId)
    {
        return _bannerAdDict.ContainsKey(adId);
    }

    #endregion

    #endregion

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

    /// <summary>
    /// 检测玩家是否已经授权，若未授权则尝试自动授权
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void CheckUserAuthorization(Action<bool> success, Action<string> fail)
    {
        GetSettingOption option = new GetSettingOption();

        // 检查授权成功
        option.success = (res) => {
            // 如果已经授权，则直接调用成功回调
            if (res.authSetting.ContainsKey("scope.userInfo") && res.authSetting["scope.userInfo"])
            {
                success(true);
            }
            // 如果未授权，则调用授权API
            else
            {
                WX.Authorize(new AuthorizeOption
                {
                    scope = "scope.userInfo",
                    success = res => {
                        success(true);
                    },
                    fail = (_) => {
                        fail("[WeChatPlatform] 玩家未授权，尝试授权失败");
                    },
                });
            }
        };

        // 检查授权失败
        option.fail = (res) =>
        {
            fail($"[WeChatPlatform] 检查授权失败 errMsg:{res.errMsg}");
        };
        WeChatWASM.WX.GetSetting(option);
    }

    /// <summary>
    /// 创建玩家手动授权按钮
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="onTap"></param>
    /// <returns></returns>
    public IUserInfoButton CreateUserInfoButton(int x, int y, int width, int height, Action<UserInfoResult> onTap)
    {
        WXUserInfoButton btn = WX.CreateUserInfoButton(x, y, width, height, "zh_CN", false);
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
                    Language = res.userInfo.language,

                    // 废弃字段强制设为无效值（忽略接口返回的旧数据）
                    Gender = 0,
                    Country = string.Empty,
                    Province = string.Empty,
                    City = string.Empty,

                    // 保留加密数据（若实际业务需要）
                    EncryptedData = res.encryptedData ?? string.Empty,
                    Iv = res.iv ?? string.Empty
                };
                onTap?.Invoke(userInfo);
            }

            // 隐藏按钮
            btn.Hide();
        });

        Debug.Log("[WeChatPlatform] 创建微信用户信息按钮成功");

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
        Debug.Log("[WeChatPlatform] 获取微信用户信息");

        if (!_isInitialized)
        {
            fail?.Invoke("[WeChatPlatform] 微信平台未初始化");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
    try
    {
        // 调用内部实现的微信用户信息获取方法
        WXGetUserInfo(success, fail);
    }
    catch (Exception e)
    {
        fail?.Invoke($"[WeChatPlatform] 获取微信用户信息失败 errMsg:{e.Message}");
    }
#else
        // 编辑器模式下模拟用户信息
        success?.Invoke(new UserInfoResult() 
        {
            NickName = "UnityPlayer",
            AvatarUrl = "https://www.example.com/avatar.png",
            Gender = 1,
            Country = "中国",
            Province = "广东",
            City = "广州",
            Language = "zh_CN"
        });
#endif
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
            return  WXGetSystemInfo();
        }
        catch (Exception e)
        {
            Debug.LogError($"[WeChatPlatform] 获取微信系统信息失败 errMsg:{e.Message}");
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
            Debug.LogWarning("[WeChatPlatform] 微信平台未初始化");
            return;
        }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
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
            Debug.LogError($"[WeChatPlatform] 微信平台振动失败 errMsg:{e.Message}");
        }
#else
        // 编辑器模式下模拟振动
        Debug.Log($"[WeChatPlatform] 编辑器模式下模拟微信振动: {(isShort ? "短" : "长")}成功");
#endif
    }

   /// <summary>
   /// 跳转小游戏
   /// </summary>
   /// <param name="appId"></param>
   /// <param name="path"></param>
   /// <param name="extraData"></param>
   /// <param name="success"></param>
   /// <param name="fail"></param>
    public void NavigateToMiniGame(string appId, string path, string extraData, Action success, Action<string> fail)
    {
        Debug.Log($"[WeChatPlatform] 微信平台跳转小游戏:{appId} Path:{path}");

        if (!_isInitialized)
        {
            fail?.Invoke("微信平台未初始化");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
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
            fail?.Invoke($"微信跳转小游戏失败: errMsg:{e.Message}");
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
            Debug.Log($"[WeChatPlatform] 微信SDK初始化成功 code:{code}");
        });
    }

    /// <summary>
    /// WX登录
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void WXLogin(Action<string> success, Action<string> fail)
    {
        // 构建登录参数
        WeChatWASM.LoginOption loginOption = new WeChatWASM.LoginOption()
        {
            success = (res) => {
                Debug.Log($"[WechatPlatform] 微信登录成功 code:{res.code} errMsg:{res.errMsg}");
                success.Invoke(res.code);
            },


            fail = (result) => {
                Debug.Log($"[WechatPlatform] 微信登录失败 errno:{result.errno} errMsg:{result.errMsg}");
                fail.Invoke(result.errMsg);
            }
        };

        // 调用微信登录API
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

        Debug.Log($"[WechatPlatform] 微信分享完成");
    }

    /// <summary>
    /// WX显示Banner广告
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void WXShowBannerAd(string adId, Action success, Action<string> fail)
    {
        // 检查是否已存在该adId的Banner实例，存在则直接显示
        if (_bannerAdDict.TryGetValue(adId, out WXCustomAd existingBannerAd))
        {
            existingBannerAd.Show(
                (showRes) =>
                {
                    Debug.Log($"[WeChatPlatform] Banner广告显示成功 adId:{adId}");
                    _bannerShowStateDict[adId] = true;
                    success?.Invoke();
                },
                (showFailRes) =>
                {
                    fail?.Invoke($"[WeChatPlatform] Banner广告显示失败 adId:{adId} errCode:{showFailRes.errCode}");
                }
            );
            return;
        }

        // 构建Banner广告默认样式
        CustomStyle defaultStyle = new CustomStyle
        {
            left = 0,
            top = 0,
            width = 300
        };

        // 构建Banner广告创建参数
        WXCreateCustomAdParam createAdParam = new WXCreateCustomAdParam
        {
            adUnitId = adId,
            adIntervals = 30, // 自动刷新间隔30秒（符合API≥30秒要求）
            style = defaultStyle,
            styleRaw = string.Empty
        };

        // 创建Banner广告实例
        WXCustomAd bannerAd = WX.CreateCustomAd(createAdParam);
        if (bannerAd == null)
        {
            fail?.Invoke($"[WeChatPlatform] Banner广告创建失败 adId:{adId}");
            return;
        }

        // 提前声明所有事件
        Action<WXADLoadResponse> loadAction = null;
        Action<WXADErrorResponse> errorAction = null;
        Action closeAction = null;
        Action hideAction = null;

        // Banner广告加载完成事件（加载完成后自动显示）
        loadAction = (loadRes) =>
        {
            Debug.Log($"[WeChatPlatform] Banner广告加载成功 adId:{adId}");
            bannerAd.Show(
                (showRes) =>
                {
                    Debug.Log($"[WeChatPlatform] Banner广告首次显示成功 adId:{adId}");
                    _bannerShowStateDict[adId] = true;
                    success?.Invoke();
                },
                (showFailRes) =>
                {
                    fail?.Invoke($"[WeChatPlatform] Banner广告首次显示失败 adId:{adId} errCode:{showFailRes.errCode}");
                }
            );
        };
        bannerAd.OnLoad(loadAction);

        // Banner广告错误事件
        errorAction = (errorRes) =>
        {
            // 清理事件与实例
            bannerAd.OffLoad(loadAction);
            bannerAd.OffError(errorAction);
            bannerAd.OffClose(closeAction);
            bannerAd.OffHide(hideAction);
            bannerAd.Destroy();

            if (_bannerAdDict.ContainsKey(adId))
            {
                _bannerAdDict.Remove(adId);
                _bannerShowStateDict.Remove(adId);
            }

            fail?.Invoke($"[WeChatPlatform] Banner广告错误 adId:{adId} errCode:{errorRes.errCode}");
        };
        bannerAd.OnError(errorAction);

        // Banner广告关闭事件（部分模板支持关闭）
        closeAction = () =>
        {
            Debug.Log($"[WeChatPlatform] Banner广告关闭 adId:{adId}");

            // 清理事件与实例
            bannerAd.OffLoad(loadAction);
            bannerAd.OffError(errorAction);
            bannerAd.OffClose(closeAction);
            bannerAd.OffHide(hideAction);
            bannerAd.Destroy();

            if (_bannerAdDict.ContainsKey(adId))
            {
                _bannerAdDict.Remove(adId);
                _bannerShowStateDict.Remove(adId);
            }
        };
        bannerAd.OnClose(closeAction);

        // Banner广告隐藏事件
        hideAction = () =>
        {
            Debug.Log($"[WeChatPlatform] Banner广告隐藏 adId:{adId}");
            _bannerShowStateDict[adId] = false;
        };
        bannerAd.OnHide(hideAction);

        // 将实例加入字典保存，用于后续复用/管理
        _bannerAdDict.Add(adId, bannerAd);
        // 初始状态为未显示
        _bannerShowStateDict[adId] = false; 
    }

    /// <summary>
    /// WX显示插屏广告
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void WXShowInterstitialAd(string adId, Action success, Action<string> fail)
    {
        // 构建插屏广告参数
        WXCreateInterstitialAdParam createAdParam = new WXCreateInterstitialAdParam
        {
            adUnitId = adId
        };

        // 创建插屏广告实例
        WXInterstitialAd interstitialAd = WX.CreateInterstitialAd(createAdParam);
        if (interstitialAd == null)
        {
            fail?.Invoke($"[WeChatPlatform] 插屏广告创建失败 adId:{adId}");
            return;
        }

        // 提前声明所有事件
        Action closeAction = null;
        Action<WXADErrorResponse> errorAction = null;
        Action<WXADLoadResponse> loadAction = null;
        Action<WXTextResponse> showSuccessAction = null;
        Action<WXTextResponse> showFailAction = null;

        // 插屏广告关闭事件
        closeAction = () =>
        {
            Debug.Log($"[WeChatPlatform] 插屏广告关闭 adId:{adId}");
            success?.Invoke();

            // 清理所有事件监听
            interstitialAd.OffClose(closeAction);
            interstitialAd.OffError(errorAction);
            interstitialAd.OffLoad(loadAction);
            // 销毁实例（符合微信插屏实例不跨页面使用要求）
            interstitialAd.Destroy();
        };
        interstitialAd.OnClose(closeAction);

        // 插屏广告错误事件
        errorAction = (errorRes) =>
        {
            // 清理所有事件监听
            interstitialAd.OffClose(closeAction);
            interstitialAd.OffError(errorAction);
            interstitialAd.OffLoad(loadAction);
            interstitialAd.Destroy();

            fail?.Invoke($"[WeChatPlatform] 插屏广告错误 adId:{adId} errCode:{errorRes.errCode}");
        };
        interstitialAd.OnError(errorAction);

        // 插屏广告加载完成事件
        loadAction = (loadRes) =>
        {
            Debug.Log($"[WeChatPlatform] 插屏广告加载完成 adId:{adId}");
            // 加载完成后显示广告
            interstitialAd.Show(showSuccessAction, showFailAction);
        };
        interstitialAd.OnLoad(loadAction);

        // 插屏广告显示成功回调
        showSuccessAction = (successRes) =>
        {
            Debug.Log($"[WeChatPlatform] 插屏广告显示成功 adId:{adId}");
        };

        // 插屏广告显示失败回调
        showFailAction = (failRes) =>
        {
            Debug.LogError($"[WeChatPlatform] 插屏广告显示失败 adId:{adId} errCode:{failRes.errCode}");
        };

        // 发起广告加载请求
        interstitialAd.Load(
            (loadSuccessRes) => Debug.Log($"[WeChatPlatform] 插屏广告加载请求成功 adId:{adId}"),
            null
        );
    }

    /// <summary>
    /// WX显示激励广告
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void WXShowRewardedAd(string adId, Action success, Action<string> fail)
    {
        // 构建激励广告参数
        WXCreateRewardedVideoAdParam createAdParam = new WXCreateRewardedVideoAdParam
        {
            adUnitId = adId,
            multiton = false // 单例模式
        };

        // 创建广告实例
        WXRewardedVideoAd rewardedAd = WX.CreateRewardedVideoAd(createAdParam);
        if (rewardedAd == null)
        {
            fail?.Invoke($"[WeChatPlatform] 激励广告创建失败 adId:{adId}");
            return;
        }

        // 提前声明所有事件
        Action<WXRewardedVideoAdOnCloseResponse> closeAction = null;
        Action<WXADErrorResponse> errorAction = null;
        Action<WXADLoadResponse> loadAction = null;
        Action<WXTextResponse> showSuccessAction = null;
        Action<WXTextResponse> showFailAction = null;

        // 激励广告关闭事件
        closeAction = (closeRes) =>
        {
            // 清理所有事件监听
            rewardedAd.OffClose(closeAction);
            rewardedAd.OffError(errorAction);
            rewardedAd.OffLoad(loadAction);

            // 判断是否完整观看
            if (closeRes.isEnded)
            {
                Debug.Log($"[WeChatPlatform] 激励广告完整观看 adId:{adId}");
                success?.Invoke();
            }
            else
            {
                fail?.Invoke($"[WeChatPlatform] 激励广告未完整观看 adId:{adId}");
            }
        };
        rewardedAd.OnClose(closeAction);

        // 激励广告错误事件
        errorAction = (errorRes) =>
        {
            // 清理所有事件监听
            rewardedAd.OffClose(closeAction);
            rewardedAd.OffError(errorAction);
            rewardedAd.OffLoad(loadAction);

            fail?.Invoke($"[WeChatPlatform] 激励广告错误 adId:{adId} errCode:{errorRes.errCode}");
        };
        rewardedAd.OnError(errorAction);

        // 激励广告加载完成事件
        loadAction = (loadRes) =>
        {
            Debug.Log($"[WeChatPlatform] 激励广告加载完成 adId:{adId}");
            rewardedAd.Show(showSuccessAction, showFailAction);
        };
        rewardedAd.OnLoad(loadAction);

        // 激励广告显示成功回调
        showSuccessAction = (successRes) =>
        {
            Debug.Log($"[WeChatPlatform] 激励广告显示成功 adId:{adId}");
        };

        // 激励广告显示失败回调
        showFailAction = (failRes) =>
        {
            Debug.LogError($"[WeChatPlatform] 激励广告显示失败 adId:{adId} errCode:{failRes.errCode}");
        };

        // 发起广告加载请求
        rewardedAd.Load((loadSuccessRes) => Debug.Log($"[WeChatPlatform] 激励广告加载请求成功 adId:{adId}"),null);
    }

    private void WXPay(string orderId, string amount, string desc, Action<string> success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用微信小游戏的支付API
    }

    /// <summary>
    /// WX获取用户信息
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void WXGetUserInfo(Action<UserInfoResult> success, Action<string> fail)
    {
        GetUserInfoOption userInfoOption = new GetUserInfoOption
        {
            // 获取用户信息成功
            success = (res) =>
            {
                UserInfoResult result = new UserInfoResult
                {
                    // 仅保留微信当前实际返回的有效字段
                    NickName = res.userInfo.nickName,
                    AvatarUrl = res.userInfo.avatarUrl,
                    Language = res.userInfo.language,

                    // 处理已废弃字段（按公告明确标记为无效值）
                    Gender = 0,  // 0表示未知，符合微信最新规范
                    Country = string.Empty,
                    Province = string.Empty,
                    City = string.Empty,

                    // 微信UserInfo中没有的字段保持默认值
                    EncryptedData = string.Empty,
                    Iv = string.Empty
                };

                success?.Invoke(result);
            },
            // 获取用户信息失败
            fail = (res) =>
            {
                fail?.Invoke($"[WeChatPlatform] 获取用户信息失败 errMsg:{res.errMsg}");
            }
        };

        // 调用微信获取用户信息API
        WX.GetUserInfo(userInfoOption);
    }

    /// <summary>
    /// WX获取系统信息
    /// </summary>
    /// <returns></returns>
    private SystemInfo WXGetSystemInfo()
    {
        WeChatWASM.SystemInfo sysInfo = null;
        GetSystemInfoOption sysInfoOption = new GetSystemInfoOption
        {
            // 获取成功
            success = (info) =>
            {
                sysInfo = info;
            },
            // 获取失败
            fail = (err) =>
            {
                Debug.LogError($"[WeChatPlatform] 获取微信系统信息失败 errMsg:{err.errMsg}");
            }
        };

        // 调用微信SDK获取系统信息
        WX.GetSystemInfo(sysInfoOption);

        // 转换为通用的SystemInfo对象
        return new SystemInfo
        {
            Platform = sysInfo.platform,
            Version = sysInfo.version,
            ScreenWidth = (float)sysInfo.screenWidth,
            ScreenHeight = (float)sysInfo.screenHeight,
            Language = sysInfo.language
        };
    }

    /// <summary>
    /// WX短振动
    /// </summary>
    private void WXVibrateShort()
    {
        WX.VibrateShort(new VibrateShortOption()
        {
            type = "medium",
            success = (callbackResult) =>
            {
                Debug.Log($"[WeChatPlatform] 短振动成功 errMsg:{callbackResult.errMsg}");
            },
            fail = (callbackResult) =>
            {
                Debug.Log($"[WeChatPlatform] 短振动失败 errMsg:{callbackResult.errMsg}");
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
                Debug.Log($"[WeChatPlatform] 长振动成功 errMsg:{callbackResult.errMsg}");
            },
            fail = (callbackResult) =>
            {
                Debug.Log($"[WeChatPlatform] 长振动失败 errMsg:{callbackResult.errMsg}");
            }
        });
    }

    /// <summary>
    /// WX跳转小游戏
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="path"></param>
    /// <param name="extraData"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void WXNavigateToMiniProgram(string appId, string path, string extraData, Action success, Action<string> fail)
    {
        // 构建跳转参数对象
        NavigateToMiniProgramOption navigateOption = new NavigateToMiniProgramOption
        {
            appId = appId,
            path = path,
            envVersion = "release", // 默认跳转正式版
            noRelaunchIfPathUnchanged = false // 默认reLaunch
        };

        // 绑定成功回调
        navigateOption.success = (successRes) =>
        {
            Debug.Log($"[WeChatPlatform] 跳转小游戏成功 appId:{appId}");
            success?.Invoke();
        };

        // 绑定失败回调
        navigateOption.fail = (failRes) =>
        {
            fail?.Invoke($"[WeChatPlatform] 跳转小游戏失败 appId:{appId} errMsg:{failRes.errMsg}");
        };

        // 调用微信跳转API
        WX.NavigateToMiniProgram(navigateOption);
    }

}
#endif