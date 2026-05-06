#if QUICKMINIGAME
using System;
using QGMiniGame;
using UnityEngine;

public class QuickPlatform : IPlatform
{
    public PlatformUserInfo UserInfo { get; set; }
    public void Init()
    {
        if (QG.GetProvider() == QG.Provider_Xiaomi || QG.isMi())
        {
            Debug.Log($"[QuickPlatform] 小米平台初始化");
            MiBridge.Instance.Init();
        }
    }

    public void Update()
    {
        
    }

    public void Release()
    {
        
    }

    public void Login(Action<string> success, Action<string> fail, Action<ProviderLogin> getProviderLogin = null)
    {
        string provider = QG.GetProvider();
        Debug.Log($"[QuickPlatform] Provider: {provider}, isMi: {QG.isMi()}");
        // 除小米平台需要获取Login().session外，其他平台只需要Login().token
        if (provider == QG.Provider_Xiaomi || QG.isMi())
        {
            Debug.Log($"[QuickPlatform] 该平台为小米平台，使用小米Login");
            MiBridge.LoginSuccess loginSuccess = (accountId, session) =>
            {
                Debug.Log($"[QuickPlatform] XiaoMi Login success, accountId: {accountId}, session: {session}");
                ProviderLogin providerLogin = new ProviderLogin();
                MiBridge.GetUserInfoSuccess getUserInfoSuccess = (nickname, avatar, gender) =>
                {
                    providerLogin.Nickname = nickname;
                    providerLogin.Avatar = avatar;
                };
                MiBridge.GetUserInfoFailure getUserInfoFailure = (errorCode, errorMsg) =>
                {
                    Debug.LogError($"[QuickPlatform] XiaoMi GetUserInfo fail, errorCode: {errorCode}, errorMsg: {errorMsg}");
                };
                MiBridge.Instance.GetUserInfo(getUserInfoSuccess, getUserInfoFailure);
                providerLogin.Uid = accountId.ToString();
                providerLogin.Token = session;
                providerLogin.Provider = provider;
                
                success?.Invoke(session);
                getProviderLogin?.Invoke(providerLogin);
            };
            MiBridge.LoginFailure loginFailure = (errorCode, errorMsg) =>
            {
                fail?.Invoke(errorMsg);
                Debug.Log($"[QuickPlatform] XiaoMi Login fail, errorCode: {errorCode}, msg: {errorMsg}");
            };
            MiBridge.Instance.Login(loginSuccess, loginFailure);
        }
        else
        {
            Debug.Log($"[QuickPlatform] 该平台为非小米平台，使用快应用联盟Login");
            QG.Login(successRes =>
            {
                Debug.Log($"[QuickPlatform] Login success, token: {successRes.data.token}");
                success?.Invoke(successRes.data.token);
                ProviderLogin providerLogin = new ProviderLogin
                {
                    Provider = provider,
                    Token = successRes.data.token
                };
                getProviderLogin?.Invoke(providerLogin);
            }, failRes =>
            {
                Debug.Log($"[QuickPlatform] Login fail, code: {failRes.errCode}, msg: {failRes.errMsg}");
                fail?.Invoke(failRes.errMsg);
            });
        }
    }

    public void CheckUserAuthorization(Action<bool> success, Action<string> fail)
    {
        
    }

    public IUserInfoButton CreateUserInfoButton(int x, int y, int width, int height, Action<UserInfoResult> onTap)
    {
        return null;
    }

    public void CreatGameClubButton(int left, int top, int width, int height)
    {
        
    }

    public void Share(ShareInfo shareInfo, Action success, Action<string> fail)
    {
        
    }

    public void ShowAd(AdType adType, string adId, Action success, Action<AdResult> close, Action<string> fail)
    {
        
    }

    public void ShowRewardAd(string adId, string userId, Action success, Action<AdResult> close, Action<string> fail)
    {
        QGRewardedVideoAd qgRewardedVideoAd = QG.CreateRewardedVideoAd(new QGCreateBannerAdParam
        {
            adUnitId = adId,
        });

        Action<QGBaseResponse> onShowSuccess = successRes =>
        {
            Debug.Log($"[QuickPlatform] 广告显示成功");
        };

        Action<QGBaseResponse> onShowFail = failRes =>
        {
            Debug.Log($"[QuickPlatform] 广告显示失败，{failRes.errCode}, {failRes.errMsg}");
        };

        Action onLoad = () =>
        {
            Debug.Log($"[QuickPlatform] 广告加载完成");
            qgRewardedVideoAd.Show(onShowSuccess, onShowFail);
        };

        Action<QGRewardedVideoResponse> onClose = res =>
        {
            Debug.Log($"[QuickPlatform] 广告关闭，是否完整观看：{res.isEnded}");
            close?.Invoke(new AdResult
            {
                IsCompleted = res.isEnded,
                AdId = adId,
            });
        };

        Action<QGBaseResponse> onError = errorRes =>
        {
            Debug.Log($"[QuickPlatform] 广告错误，{errorRes.errCode}, {errorRes.errMsg}");
        };

        Action<QGBaseResponse> onLoadSuccess = successRes =>
        {
            Debug.Log($"[QuickPlatform] 广告加载成功");
        };
        Action<QGBaseResponse> onLoadFail = failRes =>
        {
            Debug.Log($"[QuickPlatform] 广告加载失败，{failRes.errCode}, {failRes.errMsg}");
        };

        qgRewardedVideoAd.OnLoad(onLoad);
        qgRewardedVideoAd.OnClose(onClose);
        qgRewardedVideoAd.OnError(onError);
        qgRewardedVideoAd.Load(onLoadSuccess, onLoadFail);
    }

    public void Pay(PayInfo payInfo, Action<PayResult> success, Action<string> fail)
    {
        
    }

    public void GetUserInfo(Action<UserInfoResult> success, Action<string> fail)
    {
        if (QG.GetProvider() == QG.Provider_Xiaomi || QG.isMi())
        {
            MiBridge.GetUserInfoSuccess getUserInfoSuccess = (nickname, avatar, gender) =>
            {
                UserInfoResult userInfoResult = new UserInfoResult()
                {
                    AvatarUrl = avatar,
                    NickName = nickname,
                    Gender = gender,
                };
                success?.Invoke(userInfoResult);
            };
            MiBridge.GetUserInfoFailure getUserInfoFailure = (errorCode, errorMsg) =>
            {
                Debug.LogError($"[QuickPlatform] XiaoMi GetUserInfo fail, errorCode: {errorCode}, errorMsg: {errorMsg}");
                fail?.Invoke(errorMsg);
            };
            MiBridge.Instance.GetUserInfo(getUserInfoSuccess, getUserInfoFailure);
        }
        else
        {
            QG.GetUserInfo(successRes =>
            {
                UserInfoResult userInfoResult = new UserInfoResult
                {
                    AvatarUrl = successRes.data.smallAvatar,
                    NickName = successRes.data.nickName,
                    Gender = successRes.data.gender,
                };
                success?.Invoke(userInfoResult);
            }, failRes =>
            {
                Debug.LogError($"[QuickPlatform] QuickGame GetUserInfo fail, errorCode:{failRes.errCode}, errorMsg:{failRes.errMsg}");
                fail?.Invoke(failRes.errMsg);
            });
        }
    }

    public void GetPhoneNumber(Action<string> success, Action<string> fail)
    {
        
    }

    public SystemInfo GetSystemInfo()
    {
        return new SystemInfo();
    }

    public void Vibrate(bool isShort)
    {
        
    }

    public void NavigateToMiniGame(string appId, string path, string extraData, Action success, Action<string> fail)
    {
        
    }

    public void ShowToast(string title, string icon = "none", int duration = 1500, bool mask = false, Action success = null,
        Action<string> fail = null)
    {
        
    }

    public void HideToast(Action success = null, Action<string> fail = null)
    {
        
    }

    public void ShowModal(string title, string content, bool showCancel = true, string confirmText = "确定",
        string cancelText = "取消", Action<bool, string> success = null, Action<string> fail = null)
    {
        
    }

    public void ShowLoading(string title, bool mask = false, Action success = null, Action<string> fail = null)
    {
        
    }

    public void HideLoading(Action success = null, Action<string> fail = null)
    {
        
    }

    public void ShowActionSheet(string[] itemList, string itemColor = "#000000", string alertText = "", Action<int> success = null,
        Action<string> fail = null)
    {
        
    }

    public void SetUserCloudStorage(UserStorageData[] data, Action success = null, Action<string> fail = null)
    {
        
    }
}
#endif