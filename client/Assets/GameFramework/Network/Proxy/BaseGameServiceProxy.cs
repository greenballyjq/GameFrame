using System;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Network;
using UnityEngine;

namespace GameFramework.Network.Proxy
{
    /// <summary>
    /// 游戏服务代理基类 - 处理登录/短信/账号选择等通用认证逻辑
    /// 各游戏继承此类，实现 GameId、服务器地址、Proxy 初始化顺序、登录后拉数据序列
    /// </summary>
    public abstract class BaseGameServiceProxy
    {
        protected ServerClient ServerClient { get; private set; }

        // 保存token和uuid
        private string _authToken = "";
        private string _uuid = "";

        public event Action OnServerConnected;
        public event Action OnClientLoggedIn;

        public bool IsServerConnected { get; protected set; } = false;
        public bool IsClientLoggedIn { get; protected set; } = false;

        public string AuthToken => _authToken;
        public string Uuid => _uuid;

        /// <summary>
        /// 游戏ID，如 "Goose"、"Hunting"
        /// </summary>
        protected abstract string GameId { get; }

        /// <summary>
        /// 获取服务器地址
        /// </summary>
        protected abstract string GetServerUrl();

        /// <summary>
        /// 初始化各域 Proxy（各游戏决定初始化哪些 Proxy 及顺序）
        /// </summary>
        protected abstract void InitProxies();

        /// <summary>
        /// 登录成功后的数据拉取序列（各游戏决定拉哪些数据）
        /// </summary>
        protected abstract UniTask OnLoginSuccess(S2CLogin loginResp);

        /// <summary>
        /// 初始化网络
        /// </summary>
        public void InitNetwork()
        {
            ServerClient = GameFrameworkManager.Instance.GetManager<ServerClient>();
            ServerClient.SetServerUrl(GetServerUrl());

            InitProxies();

            IsServerConnected = true;
            OnServerConnected?.Invoke();

#if UNITY_EDITOR
            IsClientLoggedIn = true;
            OnClientLoggedIn?.Invoke();
#endif
        }

        #region 登录（认证域 /auth）

        /// <summary>
        /// 登录（按实际平台填充 platform oneof）
        /// </summary>
        /// <param name="type">平台类型</param>
        /// <param name="code">平台登录code</param>
        /// <param name="phone">手机号（短信登录时使用）</param>
        public async UniTask Login(PlatformManager.PlatformType type, string code, string phone = "")
        {
            var loginMsg = new C2SLogin
            {
                GameId = GameId
            };

            // 微信小程序登录
            loginMsg.WechatMini = new WeChatMiniProgram { Code = code };

            var root = new C2SAuth
            {
                Session = ServerClient.NextSession(),
                Login = loginMsg
            };

            S2CAuth resp = await ServerClient.PostDomainAsync<S2CAuth>("/auth", root);

            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.Login != null)
            {
                _authToken = resp.Login.Token;
                _uuid = resp.Login.Uuid;

                Debug.Log($"[{GetType().Name}] 登录成功 token: {_authToken.Substring(0, System.Math.Min(8, _authToken.Length))}..., uuid: {_uuid}, isNew: {resp.Login.IsNew}");

                ServerClient.SetHeader("Authorization", _authToken);

                await OnLoginSuccess(resp.Login);

                IsClientLoggedIn = true;
                OnClientLoggedIn?.Invoke();
            }
            else
            {
                Debug.Log($"[{GetType().Name}] 登录失败, code: {resp?.Code}");
            }
        }

        #endregion

        #region 短信验证码

        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="usage">用途（如 "login", "bind_phone"）</param>
        public async UniTask<S2CSendSMS> SendSms(string phone, string usage)
        {
            var root = new C2SAuth
            {
                Session = ServerClient.NextSession(),
                SendSms = new C2SSendSMS
                {
                    Phone = phone,
                    Usage = usage
                }
            };
            S2CAuth resp = await ServerClient.PostDomainAsync<S2CAuth>("/auth", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.SendSms != null)
            {
                Debug.Log($"[{GetType().Name}] 发送短信验证码成功 - phone: {phone}, message: {resp.SendSms.Message}");
                return resp.SendSms;
            }

            Debug.LogError($"[{GetType().Name}] 发送短信验证码失败, code: {resp?.Code}");
            return null;
        }

        #endregion

        #region 选择账号

        /// <summary>
        /// 选择账号（多账号场景）
        /// </summary>
        /// <param name="uuid">目标账号UUID</param>
        public async UniTask<bool> SelectAccount(string uuid)
        {
            var root = new C2SAuth
            {
                Session = ServerClient.NextSession(),
                SelectAccount = new C2SSelectAccount
                {
                    Uuid = uuid
                }
            };
            S2CAuth resp = await ServerClient.PostDomainAsync<S2CAuth>("/auth", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.SelectAccount != null)
            {
                _authToken = resp.SelectAccount.Token;
                _uuid = resp.SelectAccount.Uuid;
                ServerClient.SetHeader("Authorization", _authToken);
                Debug.Log($"[{GetType().Name}] 选择账号成功, uuid: {_uuid}");
                return true;
            }

            Debug.LogError($"[{GetType().Name}] 选择账号失败, code: {resp?.Code}");
            return false;
        }

        #endregion

        /// <summary>
        /// 清理资源
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}
