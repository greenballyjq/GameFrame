#if WEIXINMINIGAME
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using WeChatWASM;

/// <summary>
/// 微信平台实现
/// </summary>
public class WeChatPlatform : IPlatform
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void WXSetAdServerSideVerificationData(string adUnitId, string conf);
#endif
    /// <summary>
    /// 是否初始化
    /// </summary>
    private bool _isInitialized = false;

    private int _rewardAdSeq;
    private bool _isRewardAdShowing;

    /// <summary>
    /// Banner广告实例字典
    /// key=adUnitId
    /// </summary>
    private Dictionary<string, WXCustomAd> _bannerAdDict = new Dictionary<string, WXCustomAd>();

    /// <summary>
    /// Banner广告显示状态字典
    /// key=adUnitId
    /// value=是否正在显示
    /// </summary>
    private Dictionary<string, bool> _bannerShowStateDict = new Dictionary<string, bool>();
    
    private readonly Dictionary<string, string> SHARE_IMAGE_URL_DICT = new Dictionary<string, string>()
    {
        {"开心套只鹅", "https://mmocgame.qpic.cn/wechatgame/C7iaYsK1CibPaicuPsk8W0NwCdQibfCAtrZV7dialbDCcLEibiaY5opHjZibOyeXAgKBP9K7/0"}
    };

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        Debug.Log("[WeChatPlatform] 初始化微信平台");

#if UNITY_WEBGL
        try
        {
            WXInit();
            InitShareMessage();
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
#else
        // 编辑器模式下模拟登录成功
        Debug.Log("[WeChatPlatform] 编辑器模式下模拟微信平台登录成功");
#endif
    }

    /// <summary>
    /// 手机号快速验证，向用户申请，并在用户同意后，快速填写和验证手机
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void GetUserPhoneNumber(Action<string> success, Action<string> fail)
    {
        var getPhoneNumberOption = new GetPhoneNumberOption
        {
            complete = result =>
            {
                
            },
            success = getPhoneNumberSuccessCallbackResult =>
            {   
                success?.Invoke(getPhoneNumberSuccessCallbackResult.code);
            },
            fail = result =>
            {
                fail?.Invoke(result.errMsg);
            },
        };
        
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            WX.GetPhoneNumber(getPhoneNumberOption);
        }
        catch (Exception e)
        {
            fail?.Invoke(e.Message);
        }
#endif
    }

    /// <summary>
    /// 获取玩家模糊位置 - 经纬度（需要获取授权）
    /// </summary>
    public void GetFuzzyLocation(Action<double, double> success, Action<string> fail)
    {
        var getFuzzyLocationOption = new GetFuzzyLocationOption
        {
            complete = result => { },
            fail = result =>
            {
                fail?.Invoke(result.errMsg);
            },
            success = result =>
            {
                success?.Invoke(result.latitude, result.longitude);
            },
        };
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            WX.GetFuzzyLocation(getFuzzyLocationOption);
        }
        catch (Exception e)
        {
            fail?.Invoke(e.Message);
        }
#endif
    }

    /// <summary>
    /// 打开隐私弹窗
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void OpenPrivacyContract(Action success, Action<string> fail)
    {
        var option = new RequirePrivacyAuthorizeOption()
        {
            complete = result => { },
            success = result =>
            {
                success?.Invoke();
            },
            fail = result =>
            {
                fail?.Invoke(result.errMsg);
            }
        };
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            WX.RequirePrivacyAuthorize(option);
        }
        catch (Exception e)
        {
            fail?.Invoke(e.Message);
        }
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
        // WX.OnShareAppMessage();
        Debug.Log("[WeChatPlatform] 微信平台分享");

        if (!_isInitialized)
        {
            fail?.Invoke("[WeChatPlatform] 微信平台未初始化");
            return;
        }

#if true
// #if UNITY_WEBGL && !UNITY_EDITOR
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

    /// <summary>
    /// 展示广告
    /// </summary>
    /// <param name="adType"></param>
    /// <param name="adId"></param>
    /// <param name="success"></param>
    /// <param name="close"></param>
    /// <param name="fail"></param>
    public void ShowAd(AdType adType, string adId, Action success, Action<AdResult> close, Action<string> fail)
    {
        Debug.Log($"[WeChatPlatform] 微信平台显示广告 adType:{adType} adId:{adId}");

        if (!_isInitialized)
        {
            fail?.Invoke("[WeChatPlatform] 微信平台未初始化");
            return;
        }

        // #if UNITY_WEBGL && !UNITY_EDITOR
        #if true
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
                            success?.Invoke();
                        },
                        close,
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
                            success?.Invoke();
                        },
                        close,
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
                            success?.Invoke();
                        },
                        close,
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

    /// <summary>
    /// 显示激励广告（带服务端验证，基础库 >= v3.10.3）
    /// </summary>
    public void ShowRewardAd(string adId, string userId, Action success, Action<AdResult> close, Action<string> fail)
    {
        if (!_isInitialized)
        {
            fail?.Invoke("[WeChatPlatform] 微信平台未初始化");
            return;
        }

        if (_isRewardAdShowing)
        {
            fail?.Invoke("[WeChatPlatform] 激励广告正在播放中，请勿重复调用");
            return;
        }

        _rewardAdSeq++;
        _isRewardAdShowing = true;
        Debug.Log($"[WeChatPlatform] 显示激励广告(服务端验证) adId:{adId} userId:{userId} seq:{_rewardAdSeq}");

        try
        {
            WXShowRewardedAd(adId, success, close, fail, userId);
        }
        catch (Exception e)
        {
            _isRewardAdShowing = false;
            fail?.Invoke($"[WeChatPlatform] 激励广告(服务端验证)失败 errMsg:{e.Message}");
        }
    }

    #region Banner广告相关
    
    /// <summary>
    /// 显示Banner广告
    /// </summary>
    public void ShowBannerAd(string adId, Action success, Action<string> fail)
    {
        WXShowBannerAd(adId, success, null, fail);
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
        Debug.Log($"[WeChatPlatform] 查询Banner显示状态 adId:{adId} isShowing:{isShowing}");
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
            Debug.Log($"[WeChatPlatform] 获取Banner位置 adId:{adId} left:{left} top:{top}");
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
        bool isExist = _bannerAdDict.ContainsKey(adId);
        Debug.Log($"[WeChatPlatform] 查询Banner实例是否存在 adId:{adId} isExist:{isExist}");
        return isExist;
    }
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
        Debug.Log($"[WechatPlatform] 开始检测用户授权");
        GetSettingOption option = new GetSettingOption();

        // 检查授权成功
        option.success = (res) => {
            // 如果已经授权，则直接调用成功回调
            if (res.authSetting.ContainsKey("scope.userInfo") && res.authSetting["scope.userInfo"])
            {
                Debug.Log("[WeChatPlatform] 玩家已授权用户信息");
                success(true);
            }
            // 如果未授权，则调用授权API
            else
            {
                WX.Authorize(new AuthorizeOption
                {
                    scope = "scope.userInfo",
                    success = res => {
                        Debug.Log($"[WechatPlatform] 玩家授权成功");
                        success(true);
                    },
                    fail = (result) => {
                        Debug.Log($"[WechatPlatform] 玩家未授权，尝试授权失败, {result.errMsg}");
                        fail($"[WeChatPlatform] 玩家未授权，尝试授权失败, {result.errMsg}");
                    },
                });
            }
        };

        // 检查授权失败
        option.fail = (res) =>
        {
            Debug.Log($"[WechatPlatform] 检查授权失败 errMsg:{res.errMsg}");
            fail($"[WeChatPlatform] 检查授权失败 errMsg:{res.errMsg}");
        };

        // 调用微信获取授权设置API
        WX.GetSetting(option);
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

                    // 处理已废弃字段
                    Gender = 0,
                    Country = "",
                    Province = "",
                    City = "",

                    // 保留加密数据（若实际业务需要）
                    EncryptedData = "",
                    Iv = ""
                };
                onTap?.Invoke(userInfo);
            }

            // 隐藏按钮
            btn.Hide();
        });

        Debug.Log("[WeChatPlatform] 创建微信用户信息按钮成功");
        
        // return (IUserInfoButton)btn;
        return null;
    }

    /// <summary>
    /// 微信平台创建游戏圈按钮
    /// </summary>
    /// <param name="left"></param>
    /// <param name="top"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void CreatGameClubButton(int left, int top, int width, int height)
    {
        if (!_isInitialized)
        {
            return;
        }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        GameClubButtonStyle style = new GameClubButtonStyle
        {
            left = left,
            top = top,
            width = width,
            height = height,
            // backgroundColor = "#FF2121",
            // borderWidth = 10,
            // borderColor = "232123"
            // color = "#FFFF00"
        };
        
        WXCreateGameClubButtonParam param = new WXCreateGameClubButtonParam
        {
            icon = GameClubButtonIcon.light,
            hasRedDot = true,
            style = style,
            // text = "游戏圈",
            type = GameClubButtonType.text,
        };

        try
        {
            WXGameClubButton wxGameClubButton = WX.CreateGameClubButton(param);
        
            wxGameClubButton.OnTap(() =>
            {
                Debug.Log($"[WechatPlatform] 点击了游戏圈按钮");
            });
            wxGameClubButton.Show();
        }
        catch (Exception e)
        {
            Debug.LogError($"[WechatPlatform] 创建游戏圈按钮失败: {e.Message}");
        }
#else

#endif

    }

    /// <summary>
    /// 分享图片到游戏圈
    /// </summary>
    public void ShareImageToGameClub(string imageUrl, string title, string content, Action<bool, string> callback = null)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WXMiniGameCommon.Instance.ShareImageToGameClub(imageUrl, title, content, callback);
#else
        Debug.Log($"[WeChatPlatform] ShareImageToGameClub - Editor模式不支持");
        callback?.Invoke(false, "Editor模式不支持");
#endif
    }

    /// <summary>
    /// 分享canvas截图到游戏圈
    /// </summary>
    public void ShareCanvasToGameClub(string title, string content, Action<bool, string> callback = null)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WXMiniGameCommon.Instance.ShareCanvasToGameClub(title, content, callback);
#else
        Debug.Log($"[WeChatPlatform] ShareCanvasToGameClub - Editor模式不支持");
        callback?.Invoke(false, "Editor模式不支持");
#endif
    }

    /// <summary>
    /// 推送小游戏动态
    /// </summary>
    public void PushGameFeed(int eventId, string query, Action<bool, string> callback = null)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WXMiniGameCommon.Instance.PushFeed(eventId, query, callback);
#else
        Debug.Log($"[WeChatPlatform] PushGameFeed - Editor模式不支持");
        callback?.Invoke(false, "Editor模式不支持");
#endif
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    public void GetUserInfo(Action<UserInfoResult> success, Action<string> fail)
    {
        Debug.Log("[WeChatPlatform] 获取微信用户信息");

        if (!_isInitialized)
        {
            fail?.Invoke("[WeChatPlatform] 微信平台未初始化");
            return;
        }
#if UNITY_WEBGL && !UNITY_EDITOR
// #if true
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
            NickName = "模拟微信用户",
            AvatarUrl = "https://www.example.com/avatar.png",
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
    /// 获取用户手机号码
    /// </summary>
    /// <param name="success">成功回调（动态令牌）</param>
    /// <param name="fail">失败回调</param>
    public void GetPhoneNumber(Action<string> success, Action<string> fail)
    {
        Debug.Log("[WeChatPlatform] 获取微信用户手机号码");
        
        if (!_isInitialized)
        {
            fail?.Invoke("[WeChatPlatform] 微信平台未初始化");
            return;
        }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        var getPhoneNumberOption = new GetPhoneNumberOption
        {
            success = successResult =>
            {
                success?.Invoke(successResult.code);
            },
            fail = result =>
            {
                fail?.Invoke(result.errMsg);
            }
        };
        try
        {
            WX.GetPhoneNumber(getPhoneNumberOption);
        }
        catch (Exception e)
        {
            fail?.Invoke(e.Message);
        }
#else
        // 编辑器模式下模拟手机号码
        success?.Invoke("12345678901");
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
    /// WX初始化分享消息
    /// </summary>
    private void InitShareMessage()
    {
        WXShareAppMessageParam param = new WXShareAppMessageParam
        {
            // todo 后续根据当前游戏名称来设置
            imageUrl = SHARE_IMAGE_URL_DICT["开心套只鹅"],
            imageUrlId = "o5TeohSWSDSWvHWXYsIwlg==",
            title = "半夜还在玩的原因究竟是......"
        };
        WX.OnShareAppMessage(param, callback =>
        {
            callback.Invoke(param);
            Debug.Log($"[WeChatPlatform] 分享回调: ");
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
    /// WX显示Banner广告（默认位置与样式，自动刷新间隔30秒）
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="success"></param>
    /// <param name="close"></param>
    /// <param name="fail"></param>
    private void WXShowBannerAd(string adId, Action success, Action<AdResult> close, Action<string> fail)
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

            var adResult = new AdResult
            {
                IsCompleted = true,
                AdId = adId,
            };

            close?.Invoke(adResult);

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
    /// <param name="close"></param>
    /// <param name="fail"></param>
    public void WXShowInterstitialAd(string adId, Action success, Action<AdResult> close, Action<string> fail)
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
            
            close?.Invoke(new AdResult
            {
                IsCompleted = true,
                AdId = adId,
            });

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
    /// <param name="close"></param>
    /// <param name="fail"></param>
    public void WXShowRewardedAd(string adId, Action success, Action<AdResult> close, Action<string> fail, string userId = null)
    {
        // 构建激励广告参数
        WXCreateRewardedVideoAdParam createAdParam = new WXCreateRewardedVideoAdParam
        {
            adUnitId = adId,
            multiton = false // 单例模式
        };

        // 创建激励广告实例
        WXRewardedVideoAd rewardedAd = WX.CreateRewardedVideoAd(createAdParam);
        if (rewardedAd == null)
        {
            fail?.Invoke($"[WeChatPlatform] 激励广告创建失败 adId:{adId}");
            return;
        }

        // 设置服务端验证数据（基础库 >= v3.10.3）
        if (!string.IsNullOrEmpty(userId))
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                var param = new ServerSideVerificationParam
                {
                    userId = userId,
                    customData = $"ad_{_rewardAdSeq}",
                };
                string json = JsonUtility.ToJson(param);
                WXSetAdServerSideVerificationData(adId, json);
                Debug.Log($"[WeChatPlatform] 已设置服务端验证数据 adId:{adId} userId:{userId} customData:{param.customData}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[WeChatPlatform] 设置服务端验证数据失败（可能基础库版本低于v3.10.3） errMsg:{e.Message}");
            }
#endif
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
            _isRewardAdShowing = false;

            rewardedAd.OffClose(closeAction);
            rewardedAd.OffError(errorAction);
            rewardedAd.OffLoad(loadAction);

            string customData = $"ad_{_rewardAdSeq}";
            var adResult = new AdResult
            {
                IsCompleted = closeRes.isEnded,
                AdId = adId,
                AdSeq = _rewardAdSeq,
                CustomData = closeRes.isEnded ? customData : null,
            };
            close?.Invoke(adResult);

            if (closeRes.isEnded)
            {
                Debug.Log($"[WeChatPlatform] 激励广告完整观看 adId:{adId} seq:{_rewardAdSeq}");
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
            _isRewardAdShowing = false;

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
            // rewardedAd.Show(showSuccessAction, showFailAction);
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
        rewardedAd.Load((loadSuccessRes) =>
        {
            Debug.Log($"[WeChatPlatform] 激励广告加载请求成功 adId:{adId}");
            rewardedAd.Show(showSuccessAction, showFailAction);
        },null);
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
            withCredentials = true,
            // 获取用户信息成功
            success = (res) =>
            {
                UserInfoResult result = new UserInfoResult
                {
                    RawData = res.rawData,
                    // 仅保留微信当前实际返回的有效字段
                    NickName = res.userInfo.nickName,
                    AvatarUrl = res.userInfo.avatarUrl,
                    Language = res.userInfo.language,
                    
                    // 处理已废弃字段
                    Gender = 0,
                    Country = "",
                    Province = "",
                    City = "",

                    // 微信UserInfo中加密字段
                    EncryptedData = res.encryptedData,
                    Iv = res.iv,
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

    /// <summary>
    /// WX创建视频
    /// </summary>
    /// <param name="src">视频的资源地址</param>
    /// <param name="poster">视频的封面</param>
    public void WXCreateVideo(string src, string poster, Action<WXVideo> success, Action<string> fail = null)
    {
        Debug.Log("[WeChatPlatform] 微信平台创建视频");

        if (!_isInitialized)
        {
            fail?.Invoke("[WeChatPlatform] 微信平台未初始化");
            return;
        }

#if UNITY_WEBGL
        try
        {
            var systemInfo = GetSystemInfo();

            // WXVideo wxVideo = WXBase.CreateVideo(new WXCreateVideoParam
            // {
            //     x = 9999,
            //     y = 9999,
            //
            //     width = (int)systemInfo.ScreenWidth,
            //     height = (int)systemInfo.ScreenHeight,
            //
            //     src = src,
            //     poster = poster,
            //
            //     autoplay = false,
            //     muted = false,
            //
            //     objectFit = "cover",
            //     underGameView = false,
            //
            //     controls = false,
            //     showProgress = false,
            //     showProgressInControlMode = false,
            //     enableProgressGesture = false,
            //     enablePlayGesture = false,
            //     showCenterPlayBtn = false,
            // });
            WXVideo wxVideo = WXBase.CreateVideo(new CreateVideoOption
            {
                x = 9999,
                y = 9999,
            
                width = (int)systemInfo.ScreenWidth,
                height = (int)systemInfo.ScreenHeight,
            
                src = src,
                poster = poster,
            
                autoplay = false,
                muted = false,
            
                objectFit = "cover",
                underGameView = false,
            
                controls = false,
                showProgress = false,
                showProgressInControlMode = false,
                enableProgressGesture = false,
                enablePlayGesture = false,
                showCenterPlayBtn = false,
            });

            success?.Invoke(wxVideo);
        }
        catch (Exception e)
        {
            fail?.Invoke($"[WeChatPlatform] 创建视频失败 errMsg:{e.Message}");
        }
#else
        // 编辑器模式下模拟创建视频成功
        success?.Invoke(WXBase.CreateVideo(new CreateVideoOption()));
#endif
    }

    #region UI交互相关（Toast/Modal/Loading/ActionSheet）

    /// <summary>
    /// 显示消息提示框
    /// </summary>
    public void ShowToast(string title, string icon = "none", int duration = 1500, bool mask = false, Action success = null, Action<string> fail = null)
    {
        Debug.Log($"[WeChatPlatform] ShowToast title:{title} icon:{icon} duration:{duration}");

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            WX.ShowToast(new ShowToastOption
            {
                title = title,
                icon = icon,
                duration = duration,
                mask = mask,
                success = (res) =>
                {
                    success?.Invoke();
                },
                fail = (res) =>
                {
                    fail?.Invoke($"[WeChatPlatform] ShowToast失败 errMsg:{res.errMsg}");
                }
            });
        }
        catch (Exception e)
        {
            fail?.Invoke($"[WeChatPlatform] ShowToast异常 errMsg:{e.Message}");
        }
#else
        Debug.Log($"[WeChatPlatform] 编辑器模式下模拟ShowToast: {title}");
        success?.Invoke();
#endif
    }

    /// <summary>
    /// 隐藏消息提示框
    /// </summary>
    public void HideToast(Action success = null, Action<string> fail = null)
    {
        Debug.Log("[WeChatPlatform] HideToast");

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            WX.HideToast(new HideToastOption
            {
                success = (res) =>
                {
                    success?.Invoke();
                },
                fail = (res) =>
                {
                    fail?.Invoke($"[WeChatPlatform] HideToast失败 errMsg:{res.errMsg}");
                }
            });
        }
        catch (Exception e)
        {
            fail?.Invoke($"[WeChatPlatform] HideToast异常 errMsg:{e.Message}");
        }
#else
        Debug.Log("[WeChatPlatform] 编辑器模式下模拟HideToast");
        success?.Invoke();
#endif
    }

    /// <summary>
    /// 显示模态对话框
    /// </summary>
    public void ShowModal(string title, string content, bool showCancel = true, string confirmText = "确定", string cancelText = "取消", Action<bool, string> success = null, Action<string> fail = null)
    {
        Debug.Log($"[WeChatPlatform] ShowModal title:{title} content:{content}");

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            WX.ShowModal(new ShowModalOption
            {
                title = title,
                content = content,
                showCancel = showCancel,
                confirmText = confirmText,
                cancelText = cancelText,
                success = (res) =>
                {
                    success?.Invoke(res.confirm, res.content);
                },
                fail = (res) =>
                {
                    fail?.Invoke($"[WeChatPlatform] ShowModal失败 errMsg:{res.errMsg}");
                }
            });
        }
        catch (Exception e)
        {
            fail?.Invoke($"[WeChatPlatform] ShowModal异常 errMsg:{e.Message}");
        }
#else
        Debug.Log($"[WeChatPlatform] 编辑器模式下模拟ShowModal: {title} - {content}");
        success?.Invoke(true, "");
#endif
    }

    /// <summary>
    /// 显示Loading提示框
    /// </summary>
    public void ShowLoading(string title, bool mask = false, Action success = null, Action<string> fail = null)
    {
        Debug.Log($"[WeChatPlatform] ShowLoading title:{title}");

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            WX.ShowLoading(new ShowLoadingOption
            {
                title = title,
                mask = mask,
                success = (res) =>
                {
                    success?.Invoke();
                },
                fail = (res) =>
                {
                    fail?.Invoke($"[WeChatPlatform] ShowLoading失败 errMsg:{res.errMsg}");
                }
            });
        }
        catch (Exception e)
        {
            fail?.Invoke($"[WeChatPlatform] ShowLoading异常 errMsg:{e.Message}");
        }
#else
        Debug.Log($"[WeChatPlatform] 编辑器模式下模拟ShowLoading: {title}");
        success?.Invoke();
#endif
    }

    /// <summary>
    /// 隐藏Loading提示框
    /// </summary>
    public void HideLoading(Action success = null, Action<string> fail = null)
    {
        Debug.Log("[WeChatPlatform] HideLoading");

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            WX.HideLoading(new HideLoadingOption
            {
                success = (res) =>
                {
                    success?.Invoke();
                },
                fail = (res) =>
                {
                    fail?.Invoke($"[WeChatPlatform] HideLoading失败 errMsg:{res.errMsg}");
                }
            });
        }
        catch (Exception e)
        {
            fail?.Invoke($"[WeChatPlatform] HideLoading异常 errMsg:{e.Message}");
        }
#else
        Debug.Log("[WeChatPlatform] 编辑器模式下模拟HideLoading");
        success?.Invoke();
#endif
    }

    /// <summary>
    /// 显示操作菜单
    /// </summary>
    public void ShowActionSheet(string[] itemList, string itemColor = "#000000", string alertText = "", Action<int> success = null, Action<string> fail = null)
    {
        Debug.Log($"[WeChatPlatform] ShowActionSheet itemCount:{itemList?.Length}");

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            WX.ShowActionSheet(new ShowActionSheetOption
            {
                itemList = itemList,
                itemColor = itemColor,
                alertText = alertText,
                success = (res) =>
                {
                    success?.Invoke((int)res.tapIndex);
                },
                fail = (res) =>
                {
                    fail?.Invoke($"[WeChatPlatform] ShowActionSheet失败 errMsg:{res.errMsg}");
                }
            });
        }
        catch (Exception e)
        {
            fail?.Invoke($"[WeChatPlatform] ShowActionSheet异常 errMsg:{e.Message}");
        }
#else
        Debug.Log("[WeChatPlatform] 编辑器模式下模拟ShowActionSheet");
        success?.Invoke(0);
#endif
    }

    #endregion

    [Serializable]
    private class ServerSideVerificationParam
    {
        public string userId;
        public string customData;
    }
    
    public class WechatPoint
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }
}
#endif