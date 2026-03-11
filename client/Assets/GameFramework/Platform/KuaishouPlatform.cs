#if KUAISHOUMINIGAME
using KSWASM;
using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 快手平台实现
/// </summary>
public class KuaishouPlatform : IPlatform
{
    // 是否已初始化
    private bool _isInitialized = false;

    // 保存Banner广告实例的字典（key=adUnitId）
    private Dictionary<string, BannerAd> _bannerAdDict = new Dictionary<string, BannerAd>();

    // 记录Banner广告显示状态（key=adUnitId，value=是否正在显示）
    private Dictionary<string, bool> _bannerShowStateDict = new Dictionary<string, bool>();

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        // Debug.Log("[KuaishouPlatform] 初始化快手平台");
        

        // 调用快手小游戏的初始化API
#if UNITY_WEBGL && !UNITY_EDITOR
    try
    {
        KSInit();
        _isInitialized = true;
        Debug.Log("[KuaishouPlatform] 快手平台初始化成功");
    }
    catch (Exception e)
    {
        Debug.LogError($"[KuaishouPlatform] 快手平台初始化失败 errMsg:{e.Message}");
    }
#else
        // 编辑器模式下模拟初始化成功
        _isInitialized = true;
        Debug.Log("[KuaishouPlatform] 编辑器模式下模拟快手平台初始化");
#endif
    }

    public void Update()
    {
        // 快手平台更新
    }

    public void Release()
    {
        Debug.Log("[KuaishouPlatform] 释放快手平台");
        _isInitialized = false;
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void Login(Action<string> success, Action<string> fail)
    {
        Debug.Log("[KuaishouPlatform] 快手平台登录");

        if (!_isInitialized)
        {
            fail?.Invoke("快手平台未初始化");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            KSLogin(success, fail);
        }
        catch(Exception e)
        {
            Debug.LogError($"[KuaishouPlatform] 快手登录失败： {e.Message}");
        }
#else
        Debug.Log("[KuaishouPlatform] 编辑器模式下模拟快手登录成功");
#endif
    }

    #region public void Login(Action<LoginResult> success, Action<string> fail)
    //    public void Login(Action<LoginResult> success, Action<string> fail)
    //    {
    //        Debug.Log("[KuaishouPlatform] 快手平台登录");

    //        if (!_isInitialized)
    //        {
    //            fail?.Invoke("快手平台未初始化");
    //            return;
    //        }

    //#if UNITY_WEBGL && !UNITY_EDITOR
    //        try
    //        {
    //            // 调用快手小游戏的登录API
    //            // 实际项目中需要通过JSLib来实现
    //            KSLogin(
    //                (code) => {
    //                    // 使用code调用游戏服务器换取用户信息
    //                    // 这里模拟获取用户信息
    //                    var result = new LoginResult
    //                    {
    //                        UserId = "ks_user_" + UnityEngine.Random.Range(10000, 99999),
    //                        UserName = "KuaishouUser",
    //                        Avatar = "",
    //                        Token = code,
    //                        ExtraData = new Dictionary<string, object>()
    //                    };

    //                    success?.Invoke(result);
    //                },
    //                (error) => {
    //                    fail?.Invoke(error);
    //                }
    //            );
    //        }
    //        catch (Exception e)
    //        {
    //            fail?.Invoke($"快手登录失败: {e.Message}");
    //        }
    //#else
    //        // 编辑器模式下模拟登录成功
    //        var result = new LoginResult
    //        {
    //            UserId = "ks_user_12345",
    //            UserName = "KuaishouUser",
    //            Avatar = "",
    //            Token = "ks_token_12345",
    //            ExtraData = new Dictionary<string, object>()
    //        };

    //        success?.Invoke(result);
    //#endif
    //}
    #endregion

    /// <summary>
    /// 分享
    /// </summary>
    /// <param name="shareInfo"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void Share(ShareInfo shareInfo, Action success, Action<string> fail)
    {
        Debug.Log("[KuaishouPlatform] 快手平台分享");

        if (!_isInitialized)
        {
            fail?.Invoke("[KuaishouPlatform] 快手平台未初始化");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
    try
    {
        KSShare(
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
        fail?.Invoke($"快手分享失败 errMsg:{e.Message}");
    }
#else
        // 编辑器模式下模拟分享成功
        Debug.Log("[KuaishouPlatform] 编辑器模式下模拟快手平台分享成功");
        success?.Invoke();
#endif
    }

    /// <summary>
    /// 展示广告
    /// </summary>
    /// <param name="adType"></param>
    /// <param name="adId"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void ShowAd(AdType adType, string adId, Action<AdResult> success, Action<string> fail)
    {
        Debug.Log($"[KuaishouPlatform] 快手平台显示广告 adType:{adType} adId:{adId}");

        if (!_isInitialized)
        {
            fail?.Invoke("[KuaishouPlatform] 快手平台未初始化");
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
                KSShowBannerAd(
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
                KSShowInterstitialAd(
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
                KSShowRewardedAd(
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
                fail?.Invoke($"[KuaishouPlatform] 快手平台不支持 {adType} 类型的广告");
                break;
        }
    }
    catch (Exception e)
    {
        fail?.Invoke($"[KuaishouPlatform] 快手显示广告失败 errMsg:{e.Message}");
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

    public void ShowRewardAd(string adId, string userId, Action success, Action<AdResult> close, Action<string> fail)
    {
        Debug.Log($"[KuaishouPlatform] 显示激励广告(服务端验证) adId:{adId} userId:{userId}");
        KSShowRewardedAd(adId,
            () => {
                success?.Invoke();
                close?.Invoke(new AdResult { IsCompleted = true, AdId = adId });
            },
            fail);
    }

    #region Banner广告相关
    /// <summary>
    /// 显示Banner广告
    /// </summary>
    public void ShowBannerAd(string adId, Action success, Action<string> fail)
    {
        KSShowBannerAd(adId, success, fail);
    }

    /// <summary>
    /// 隐藏Banner广告
    /// </summary>
    public void HideBannerAd(string adId, Action success, Action<string> fail)
    {
        if (!_bannerAdDict.TryGetValue(adId, out BannerAd bannerAd))
        {
            fail?.Invoke($"[KuaishouPlatform] 隐藏失败，Banner实例不存在 adId:{adId}");
            return;
        }

        try
        {
            bannerAd.Hide();
            _bannerShowStateDict[adId] = false;
            Debug.Log($"[KuaishouPlatform] Banner广告隐藏成功 adId:{adId}");
            success?.Invoke();
        }
        catch (Exception e)
        {
            fail?.Invoke($"[KuaishouPlatform] Banner广告隐藏失败 adId:{adId} errMsg:{e.Message}");
        }
    }

    /// <summary>
    /// 查询Banner广告是否正在显示
    /// </summary>
    public bool IsBannerAdShowing(string adId)
    {
        _bannerShowStateDict.TryGetValue(adId, out bool isShowing);
        Debug.Log($"[KuaishouPlatform] 查询Banner显示状态 adId:{adId} isShowing:{isShowing}");
        return isShowing;
    }

    /// <summary>
    /// 设置Banner广告位置（左上角坐标）
    /// </summary>
    public void SetBannerPosition(string adId, int left, int top)
    {
        if (_bannerAdDict.TryGetValue(adId, out BannerAd bannerAd))
        {
            bannerAd.style.left = left;
            bannerAd.style.top = top;
            Debug.Log($"[KuaishouPlatform] Banner位置已更新 adId:{adId} left:{left} top:{top}");
        }
    }

    /// <summary>
    /// 获取Banner广告当前位置
    /// </summary>
    public void GetBannerCurrentPosition(string adId, out int left, out int top)
    {
        left = 0;
        top = 0;

        if (_bannerAdDict.TryGetValue(adId, out BannerAd bannerAd))
        {
            left = bannerAd.style.left;
            top = bannerAd.style.top;
            Debug.Log($"[KuaishouPlatform] 获取Banner位置 adId:{adId} left:{left} top:{top}");
        }
    }

    /// <summary>
    /// 销毁Banner广告实例
    /// </summary>
    public void DestroyBannerAd(string adId)
    {
        if (_bannerAdDict.TryGetValue(adId, out BannerAd bannerAd))
        {
            // 清理所有事件监听（传入null移除所有同类监听）
            bannerAd.OffLoad(null);
            bannerAd.OffError(null);
            bannerAd.OffClose(null);
            bannerAd.OffResize(null);

            // 销毁广告实例
            bannerAd.Destroy();

            // 从字典移除记录
            _bannerAdDict.Remove(adId);
            _bannerShowStateDict.Remove(adId);

            Debug.Log($"[KuaishouPlatform] Banner广告已销毁 adId:{adId}");
        }
    }

    /// <summary>
    /// 判断Banner广告实例是否存在
    /// </summary>
    public bool IsBannerAdExist(string adId)
    {
        bool isExist = _bannerAdDict.ContainsKey(adId);
        Debug.Log($"[KuaishouPlatform] 查询Banner实例是否存在 adId:{adId} isExist:{isExist}");
        return isExist;
    }
    #endregion

    public void Pay(PayInfo payInfo, Action<PayResult> success, Action<string> fail)
    {
        Debug.Log($"[KuaishouPlatform] 快手平台支付: {payInfo.Amount} {payInfo.Currency}");
        
        if (!_isInitialized)
        {
            fail?.Invoke("快手平台未初始化");
            return;
        }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // 调用快手小游戏的支付API
            // 实际项目中需要通过JSLib来实现
            KSPay(
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
            fail?.Invoke($"快手支付失败: {e.Message}");
        }
#else
        // 编辑器模式下模拟支付成功
        var result = new PayResult
        {
            OrderId = payInfo.OrderId,
            IsSuccess = true,
            Receipt = "ks_receipt_12345"
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
        GetSettingOption settingOption = new GetSettingOption();

        // 检查授权成功
        settingOption.success = (res) => {

            // 如果已经授权，则直接调用成功回调
            if (res.authSetting.ContainsKey("scope.userInfo") && res.authSetting["scope.userInfo"])
            {
                Debug.Log("[KuaishouPlatform] 玩家已授权用户信息");
                success(true);
            }
            // 如果未授权，则调用自动授权API
            else
            {
                Debug.Log("[KuaishouPlatform] 玩家未授权，尝试自动授权");
                AuthorizeOption authorizeOption = new AuthorizeOption
                {
                    scope = "scope.userInfo",
                    // 授权成功
                    success = (authRes) => {
                        Debug.Log($"[KuaishouPlatform] 用户信息授权成功 code:{authRes.code} result:{authRes.result}");
                        success(true);
                    },
                    // 授权失败
                    fail = (authErr) => {
                        fail($"[KuaishouPlatform] 玩家未授权，尝试授权失败 code:{authErr.code} errMsg:{authErr.msg}");
                    },
                };

                // 调用快手授权API
                KS.Authorize(authorizeOption);
            }
        };

        // 检查授权失败
        settingOption.fail = (settingErr) => {
            fail($"[KuaishouPlatform] 检查授权失败 code:{settingErr.code} errMsg:{settingErr.msg}");
        };

        // 调用快手获取授权设置API
        KS.GetSetting(settingOption);
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
        KSUserInfoButton btn = KSWASM.KS.CreateUserInfoButton(x, y, width, height, "zh_CN", false);

        // 监听按钮点击事件
        btn.OnTap((res) =>
        {
            if (res.errCode == 0)
            {
                // 映射快手返回的用户信息到通用UserInfoResult
                UserInfoResult userInfo = new UserInfoResult
                {
                    NickName = res.userInfo.nickName,
                    AvatarUrl = res.userInfo.avatarUrl,
                    Language = res.userInfo.language,
                    Gender = res.userInfo.gender,
                    Country = res.userInfo.country,
                    Province = res.userInfo.province,
                    City = res.userInfo.city,
                    EncryptedData = res.encryptedData,
                    Iv = res.iv
                };
                onTap?.Invoke(userInfo);
            }

            // 点击后隐藏按钮
            btn.Hide();
        });

        Debug.Log("[KuaishouPlatform] 创建快手用户信息按钮成功");

        return (IUserInfoButton)btn;
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void GetUserInfo(Action<UserInfoResult> success, Action<string> fail)
    {
        Debug.Log("[KuaishouPlatform] 获取快手用户信息");

        if (!_isInitialized)
        {
            fail?.Invoke("[KuaishouPlatform] 快手平台未初始化");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
    try
    {
        KSGetUserInfo(success, fail);
    }
    catch (Exception e)
    {
        fail?.Invoke($"[KuaishouPlatform] 获取快手用户信息失败 errMsg:{e.Message}");
    }
#else
        Debug.Log("[KuaishouPlatform] 编辑器模式下模拟快手用户信息返回");
        success?.Invoke(new UserInfoResult
        {
            NickName = "模拟快手用户",
            AvatarUrl = "https://example.com/avatar.png",
            Gender = 0,
            Country = "",
            Province = "",
            City = "",
            Language = "",
            EncryptedData = "",
            Iv = ""
        });
#endif
    }

    /// <summary>
    /// 获取系统信息
    /// </summary>
    /// <returns></returns>
    public SystemInfo GetSystemInfo()
    {
        Debug.Log("[KuaishouPlatform] 获取快手系统信息");

        if (!_isInitialized)
        {
            Debug.LogWarning("[KuaishouPlatform] 快手平台未初始化，返回默认系统信息");
            return new SystemInfo
            {
                Platform = "Kuaishou",
                Version = "Unknown",
                ScreenWidth = Screen.width,
                ScreenHeight = Screen.height,
                Language = "zh_CN"
            };
        }

#if UNITY_WEBGL && !UNITY_EDITOR
    try
    {
        return KSGetSystemInfo();
    }
    catch (Exception e)
    {
        Debug.LogError($"[KuaishouPlatform] 获取快手系统信息失败 errMsg:{e.Message}");
        return new SystemInfo
        {
            Platform = "Kuaishou",
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
            Platform = "Kuaishou",
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
        Debug.Log($"[KuaishouPlatform] 快手平台振动: {(isShort ? "短" : "长")}");

        if (!_isInitialized)
        {
            Debug.LogWarning("[KuaishouPlatform] 快手平台未初始化");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
    try
    {
        if (isShort)
        {
            KSVibrateShort();
        }
        else
        {
            KSVibrateLong();
        }
    }
    catch (Exception e)
    {
        Debug.LogError($"[KuaishouPlatform] 快手平台振动失败 errMsg:{e.Message}");
    }
#else
        // 编辑器模式下模拟振动
        Debug.Log($"[KuaishouPlatform] 编辑器模式下模拟快手振动: {(isShort ? "短" : "长")}成功");
#endif
    }

    /// <summary>
    /// 跳转到小游戏
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="path"></param>
    /// <param name="extraData"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void NavigateToMiniGame(string appId, string path, string extraData, Action success, Action<string> fail)
    {
        Debug.Log($"[KuaishouPlatform] 快手平台跳转小游戏: {appId}, Path: {path}");

        if (!_isInitialized)
        {
            fail?.Invoke("快手平台未初始化");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
    try
    {
        KSNavigateToMiniGame(
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
        fail?.Invoke($"快手跳转小游戏失败: {e.Message}");
    }
#else
        // 编辑器模式下模拟跳转成功
        success?.Invoke();
#endif
    }

    /// <summary>
    /// KS初始化
    /// </summary>
    private void KSInit()
    {
        KS.InitSDK((code) =>
        {
            Debug.Log($"[KuaishouPlatform] 快手SDK初始化成功 code:{code}");
        });
    }

    /// <summary>
    /// KS登录
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void KSLogin(Action<string> success, Action<string> fail)
    {
        // 构建登录参数
        LoginOption loginOption = new LoginOption()
        {
            success = (res) => {
                Debug.Log($"[KuaishouPlatform] 快手登录成功 code:{res.code}");
                success.Invoke(res.code);
            },
            fail = (result) => {
                Debug.Log($"[KuaishouPlatform] 快手登录失败 errMsg:{result.msg}");
                fail.Invoke(result.msg);
            }
        };

        // 调用快手登录API
        KS.Login(loginOption);
    }

    /// <summary>
    /// KS分享
    /// </summary>
    /// <param name="title"></param>
    /// <param name="desc"></param>
    /// <param name="imageUrl"></param>
    /// <param name="link"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void KSShare(string title, string desc, string imageUrl, string link, Action success, Action<string> fail)
    {
        // 构建分享参数
        ShareAppMessageOption shareOption = new ShareAppMessageOption
        {
            query = link.Contains("?") ? link.Split('?')[1] : string.Empty
            // 不处理templateId（保持默认值）,不传递title/desc/imageUrl（API不支持）
        };

        // 绑定分享成功回调
        shareOption.success = (res) => {
            Debug.Log($"[KuaishouPlatform] 快手分享成功 errMsg:{res.errMsg}");
            success.Invoke();
        };

        // 绑定分享失败回调
        shareOption.fail = (result) => {
            Debug.Log($"[KuaishouPlatform] 快手分享失败 errMsg:{result.errMsg}");
            fail.Invoke(result.errMsg);
        };

        // 调用快手分享API
        KS.ShareAppMessage(shareOption);     
    }

    /// <summary>
    /// KS显示Banner广告（默认位置与样式，自动刷新间隔30秒）
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void KSShowBannerAd(string adId, Action success, Action<string> fail)
    {
        // 检查是否已存在该adId的Banner实例，存在则直接显示
        if (_bannerAdDict.TryGetValue(adId, out BannerAd existingBannerAd))
        {
            existingBannerAd.Show(
                (showRes) =>
                {
                    Debug.Log($"[KuaishouPlatform] Banner广告显示成功 adId:{adId}");
                    _bannerShowStateDict[adId] = true;
                    success?.Invoke();
                },
                (showFailRes) =>
                {
                    fail?.Invoke($"[KuaishouPlatform] Banner广告显示失败 adId:{adId} errCode:{showFailRes.errCode} errMsg:{showFailRes.errMsg}");
                }
            );
            return;
        }

        // 构建Banner广告默认样式（符合快手API要求：left/top/width/height必传，width≥308）
        KSWASM.Style defaultStyle = new Style
        {
            left = 0,                  // 默认左上角x坐标0
            top = 0,                   // 默认左上角y坐标0
            width = 308,               // 最小宽度308
            height = 50                // 默认高度50（常规Banner尺寸，可后续调整）
        };

        // 构建Banner广告创建参数
        KSCreateBannerAdParam createAdParam = new KSCreateBannerAdParam
        {
            adUnitId = adId,
            adIntervals = 30,          // 自动刷新间隔30秒（符合快手API≥30秒要求）
            style = defaultStyle       // 传入默认样式
        };

        // 创建Banner广告实例
        BannerAd bannerAd = KS.CreateBannerAd(createAdParam);
        if (bannerAd == null)
        {
            fail?.Invoke($"[KuaishouPlatform] Banner广告创建失败 adId:{adId}");
            return;
        }

        // 提前声明所有事件
        Action<KSADLoadResponse> loadAction = null;
        Action<KSADErrorResponse> errorAction = null;
        Action closeAction = null;
        Action<KSADResizeResponse> resizeAction = null;

        // Banner广告加载完成事件（加载完成后自动显示）
        loadAction = (loadRes) =>
        {
            Debug.Log($"[KuaishouPlatform] Banner广告加载成功 adId:{adId}");
            bannerAd.Show(
                (showRes) =>
                {
                    Debug.Log($"[KuaishouPlatform] Banner广告首次显示成功 adId:{adId}");
                    _bannerShowStateDict[adId] = true;
                    success?.Invoke();
                },
                (showFailRes) =>
                {
                    fail?.Invoke($"[KuaishouPlatform] Banner广告首次显示失败 adId:{adId} errCode:{showFailRes.errCode} errMsg:{showFailRes.errMsg}");
                }
            );
        };
        bannerAd.OnLoad(loadAction);

        // Banner广告错误事件
        errorAction = (errorRes) =>
        {
            // 清理所有事件监听
            bannerAd.OffLoad(loadAction);
            bannerAd.OffError(errorAction);
            bannerAd.OffClose(closeAction);
            bannerAd.OffResize(resizeAction);
            // 销毁广告实例
            bannerAd.Destroy();

            // 移除字典中的记录
            if (_bannerAdDict.ContainsKey(adId))
            {
                _bannerAdDict.Remove(adId);
                _bannerShowStateDict.Remove(adId);
            }

            fail?.Invoke($"[KuaishouPlatform] Banner广告错误 adId:{adId} errCode:{errorRes.errCode} errMsg:{errorRes.errMsg}");
        };
        bannerAd.OnError(errorAction);

        // Banner广告关闭事件
        closeAction = () =>
        {
            Debug.Log($"[KuaishouPlatform] Banner广告关闭 adId:{adId}");

            // 清理所有事件监听
            bannerAd.OffLoad(loadAction);
            bannerAd.OffError(errorAction);
            bannerAd.OffClose(closeAction);
            bannerAd.OffResize(resizeAction);
            // 销毁广告实例
            bannerAd.Destroy();

            if (_bannerAdDict.ContainsKey(adId))
            {
                _bannerAdDict.Remove(adId);
                _bannerShowStateDict.Remove(adId);
            }
        };
        bannerAd.OnClose(closeAction);

        // Banner广告尺寸变化事件
        resizeAction = (resizeRes) =>
        {
            Debug.Log($"[KuaishouPlatform] Banner广告尺寸变化 adId:{adId} 新尺寸: {resizeRes.width}x{resizeRes.height}");
            // 可后续扩展：根据新尺寸调整UI适配逻辑
        };
        bannerAd.OnResize(resizeAction);

        // 将实例与状态加入字典，用于后续复用
        _bannerAdDict.Add(adId, bannerAd);
        // 初始状态为未显示
        _bannerShowStateDict[adId] = false;
    }

    /// <summary>
    /// 插屏广告
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void KSShowInterstitialAd(string adId, Action success, Action<string> fail)
    {
        // 创建插屏广告实例
        KSWASM.InterstitialAd interstitialAd = KS.CreateInterstitialAd(adId);
        if (interstitialAd == null)
        {
            fail?.Invoke($"[KuaishouPlatform] 插屏广告创建失败 adId:{adId}");
            return;
        }

        // 提前声明所有事件
        Action closeAction = null;
        Action<KSADErrorResponse> errorAction = null;
        Action<KSADLoadResponse> loadAction = null;
        Action<KSTextResponse> showSuccessAction = null;
        Action<KSTextResponse> showFailAction = null;

        // 插屏广告关闭事件
        closeAction = () =>
        {
            Debug.Log($"[KuaishouPlatform] 插屏广告关闭 adId:{adId}");
            success?.Invoke();

            // 清理所有事件监听
            interstitialAd.OffClose(closeAction);
            interstitialAd.OffError(errorAction);
            interstitialAd.OffLoad(loadAction);
            // 销毁实例
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

            fail?.Invoke($"[KuaishouPlatform] 插屏广告错误 adId:{adId} errCode:{errorRes.errCode} errMsg:{errorRes.errMsg}");
        };
        interstitialAd.OnError(errorAction);

        // 插屏广告加载完成事件
        loadAction = (loadRes) =>
        {
            Debug.Log($"[KuaishouPlatform] 插屏广告加载完成 adId:{adId}");
            // 加载完成后显示广告
            interstitialAd.Show(showSuccessAction, showFailAction);
        };
        interstitialAd.OnLoad(loadAction);

        // 插屏广告显示成功回调
        showSuccessAction = (successRes) =>
        {
            Debug.Log($"[KuaishouPlatform] 插屏广告显示成功 adId:{adId}");
        };

        // 插屏广告显示失败回调
        showFailAction = (failRes) =>
        {
            fail?.Invoke($"[KuaishouPlatform] 插屏广告显示失败 adId:{adId} errCode:{failRes.errCode} errMsg:{failRes.errMsg}");

            // 显示失败时销毁实例
            interstitialAd.Destroy();
        };

        // 发起广告加载请求
        interstitialAd.Load(
            (loadSuccessRes) => Debug.Log($"[KuaishouPlatform] 插屏广告加载请求成功 adId:{adId}"),
            null
        );
    }

    /// <summary>
    /// KS显示激励广告
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void KSShowRewardedAd(string adId, Action success, Action<string> fail)
    {
        // 创建激励广告实例
        RewardVideoAd rewardedAd = KS.CreateRewardedVideoAd(adId);
        if (rewardedAd == null)
        {
            fail?.Invoke($"[KuaishouPlatform] 激励广告创建失败 adId:{adId}");
            return;
        }

        // 提前声明所有事件
        Action<KSRewardedVideoAdOnCloseResponse> closeAction = null;
        Action<KSADErrorResponse> errorAction = null;
        Action<KSADLoadResponse> loadAction = null;
        Action<KSTextResponse> showSuccessAction = null;
        Action<KSTextResponse> showFailAction = null;

        // 激励广告关闭事件
        closeAction = (closeRes) =>
        {
            // 清理所有事件监听
            rewardedAd.OffClose(closeAction);
            rewardedAd.OffError(errorAction);
            rewardedAd.OffLoad(loadAction);

            // 判断是否完整观看
            if (closeRes.count > 0 || closeRes.isEnded)
            {
                Debug.Log($"[KuaishouPlatform] 激励广告完整观看 adId:{adId} 观看次数:{closeRes.count}");
                success?.Invoke();
            }
            else
            {
                fail?.Invoke($"[KuaishouPlatform] 激励广告未完整观看 adId:{adId}");
            }

            // 销毁广告实例
            rewardedAd.Destroy();
        };
        rewardedAd.OnClose(closeAction);

        // 激励广告错误事件
        errorAction = (errorRes) =>
        {
            // 清理所有事件监听
            rewardedAd.OffClose(closeAction);
            rewardedAd.OffError(errorAction);
            rewardedAd.OffLoad(loadAction);

            fail?.Invoke($"[KuaishouPlatform] 激励广告错误 adId:{adId} errCode:{errorRes.errCode} errMsg:{errorRes.errMsg}");

            // 销毁广告实例
            rewardedAd.Destroy();
        };
        rewardedAd.OnError(errorAction);

        // 激励广告加载完成事件
        loadAction = (loadRes) =>
        {
            Debug.Log($"[KuaishouPlatform] 激励广告加载完成 adId:{adId}");
            rewardedAd.Show(showSuccessAction, showFailAction);
        };
        rewardedAd.OnLoad(loadAction);

        // 激励广告显示成功回调
        showSuccessAction = (successRes) =>
        {
            Debug.Log($"[KuaishouPlatform] 激励广告显示成功 adId:{adId} errCode:{successRes.errCode}");
        };

        // 激励广告显示失败回调
        showFailAction = (failRes) =>
        {
            fail?.Invoke($"[KuaishouPlatform] 激励广告显示失败 adId:{adId} errCode:{failRes.errCode} errMsg:{failRes.errMsg}");

            // 显示失败时销毁实例
            rewardedAd.Destroy();
        };

        // 触发广告加载（快手BaseAd无显式Load方法，依赖OnLoad事件监听加载状态）
        Debug.Log($"[KuaishouPlatform] 激励广告发起加载请求 adId:{adId}");
    }

    private void KSPay(string orderId, string amount, string desc, Action<string> success, Action<string> fail)
    {
        // 实际项目中通过JSLib调用快手小游戏的支付API
    }

    /// <summary>
    /// KS获取用户信息
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void KSGetUserInfo(Action<UserInfoResult> success, Action<string> fail)
    {
        GetUserInfoOption userInfoOption = new GetUserInfoOption
        {
            // 获取用户信息成功
            success = (res) =>
            {
                UserInfoResult result = new UserInfoResult
                {
                    NickName = res.userInfo.nickName,
                    AvatarUrl = res.userInfo.avatarUrl,
                    Gender = res.userInfo.gender == "M" ? 1 : res.userInfo.gender == "F" ? 2 : 0,
                    Country = res.userInfo.country,
                    Province = res.userInfo.province,
                    City = res.userInfo.userCity ?? res.userInfo.city,
                    Language = res.userInfo.language,
                    EncryptedData = res.encryptedData,
                    Iv = res.iv
                };
                Debug.Log($"[KuaishouPlatform] 获取用户信息成功 errMsg:{res.errMsg}");
                success?.Invoke(result);
            },
            // 获取用户信息失败
            fail = (res) =>
            {
                string errorMsg = $"[KuaishouPlatform] 获取用户信息失败 code:{res.code} errMsg:{res.msg}";
                Debug.LogError(errorMsg);
                fail?.Invoke(errorMsg);
            },
        };

        // 调用快手获取用户信息API
        KS.GetUserInfo(userInfoOption);
    }

    /// <summary>
    /// KS获取系统信息
    /// </summary>
    /// <returns></returns>
    private SystemInfo KSGetSystemInfo()
    {
        KSWASM.SystemInfo ksSysInfo = null;
        GetSystemInfoOption sysInfoOption = new GetSystemInfoOption
        {
            // 获取成功
            success = (info) =>
            {
                ksSysInfo = info;
            },
            // 获取失败
            fail = (err) =>
            {
                Debug.LogError($"[KuaishouPlatform] 获取快手系统信息失败 errMsg:{err.errMsg}");
            }
        };

        // 调用快手SDK获取系统信息
        KS.GetSystemInfo(sysInfoOption);

        // 转换为通用的SystemInfo对象
        return new SystemInfo
        {
            Platform = ksSysInfo.platform,
            Version = ksSysInfo.version,
            ScreenWidth = (float)ksSysInfo.screenWidth,
            ScreenHeight = (float)ksSysInfo.screenHeight,
            Language = ksSysInfo.language
        };
    }

    /// <summary>
    /// KS短振动
    /// </summary>
    private void KSVibrateShort()
    {
        KS.VibrateShort(new VibrateShortOption
        {
            type = "light", // 快手支持的震动类型：light/medium/heavy
            success = (callbackResult) =>
            {
                Debug.Log($"[KuaishouPlatform] 短振动成功 errMsg:{callbackResult.errMsg}");
            },
            fail = (callbackResult) =>
            {
                Debug.Log($"[KuaishouPlatform] 短振动失败 errMsg:{callbackResult.errMsg}");
            }
        });
    }

    /// <summary>
    /// KS长振动
    /// </summary>
    private void KSVibrateLong()
    {
        KS.VibrateLong(new VibrateLongOption
        {
            success = (callbackResult) =>
            {
                Debug.Log($"[KuaishouPlatform] 长振动成功 errMsg:{callbackResult.errMsg}");
            },
            fail = (callbackResult) =>
            {
                Debug.Log($"[KuaishouPlatform] 长振动失败 errMsg:{callbackResult.errMsg}");
            }
        });
    }

    /// <summary>
    /// KS跳转到小游戏
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="path"></param>
    /// <param name="extraData"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    private void KSNavigateToMiniGame(string appId, string path, string extraData, Action success, Action<string> fail)
    {
        // 构建跳转参数对象
        KSWASM.NavigateToMiniProgramOption navigateOption = new KSWASM.NavigateToMiniProgramOption
        {
            appId = appId,
            path = path,
            envVersion = "release", // 默认跳转正式版
            noRelaunchIfPathUnchanged = false // 默认reLaunch
        };

        // 绑定成功回调
        navigateOption.success = (successRes) =>
        {
            Debug.Log($"[KuaishouPlatform] 跳转小游戏成功 appId:{appId}");
            success?.Invoke();
        };

        // 绑定失败回调
        navigateOption.fail = (failRes) =>
        {
            fail?.Invoke($"[KuaishouPlatform] 跳转小游戏失败 appId:{appId} errMsg:{failRes.errMsg}");
        };

        // 绑定完成回调
        navigateOption.complete = (completeRes) =>
        {
            Debug.Log($"[KuaishouPlatform] 跳转小游戏操作完成 appId:{appId}");
        };

        // 调用快手跳转API
        KSWASM.KS.NavigateToMiniProgram(navigateOption);
    }

    public void ShowToast(string title, string icon = "none", int duration = 1500, bool mask = false, Action success = null, Action<string> fail = null)
    {
        Debug.Log($"[KuaishouPlatform] ShowToast: {title}");
        success?.Invoke();
    }

    public void HideToast(Action success = null, Action<string> fail = null)
    {
        Debug.Log("[KuaishouPlatform] HideToast");
        success?.Invoke();
    }

    public void ShowModal(string title, string content, bool showCancel = true, string confirmText = "确定", string cancelText = "取消", Action<bool, string> success = null, Action<string> fail = null)
    {
        Debug.Log($"[KuaishouPlatform] ShowModal: {title} - {content}");
        success?.Invoke(true, "");
    }

    public void ShowLoading(string title, bool mask = false, Action success = null, Action<string> fail = null)
    {
        Debug.Log($"[KuaishouPlatform] ShowLoading: {title}");
        success?.Invoke();
    }

    public void HideLoading(Action success = null, Action<string> fail = null)
    {
        Debug.Log("[KuaishouPlatform] HideLoading");
        success?.Invoke();
    }

    public void ShowActionSheet(string[] itemList, string itemColor = "#000000", string alertText = "", Action<int> success = null, Action<string> fail = null)
    {
        Debug.Log($"[KuaishouPlatform] ShowActionSheet: itemCount:{itemList?.Length}");
        success?.Invoke(0);
    }
}
#endif