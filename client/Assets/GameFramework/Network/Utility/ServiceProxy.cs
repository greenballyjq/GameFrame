using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using Google.Protobuf;
using UnityEngine;

namespace GameFramework.Network.Utility
{
    public class ServiceProxy : Singleton<ServiceProxy>, IDisposable
    {
        private ServerClient _serverClient;

        // 保存token和uuid
        private string _authToken = "";
        private string _uuid = "";

        public string AuthToken => _authToken;
        public string Uuid => _uuid;

        // 服务端连接成功回调事件
        public event System.Action OnServerConnected;

        // 客户端登录成功回调
        public event System.Action OnClientLogined;

        /// <summary>
        /// 服务端是否已连接
        /// </summary>
        public bool IsServerConnected { get; private set; } = false;

        // 心跳定时器相关
        private bool _isPingActive = false;
        private CancellationTokenSource _pingCancellationTokenSource;

        public async UniTask InitAsync()
        {
            _serverClient = new ServerClient();
            _serverClient.Init();
            _serverClient.SetServerUrl("https://dev.sanqianpan.com:10000");

            IsServerConnected = true;
            OnServerConnected?.Invoke();
        }

        public void Dispose()
        {
            StopPingTimer();
            _serverClient?.Release();
            _pingCancellationTokenSource?.Dispose();
        }

        #region Heartbeat

        /// <summary>
        /// 开始定时心跳
        /// </summary>
        public void StartPingTimer()
        {
            if (_isPingActive)
                return;

            _isPingActive = true;
            _pingCancellationTokenSource = new CancellationTokenSource();
            PingTimerLoop(_pingCancellationTokenSource.Token).Forget();
        }

        /// <summary>
        /// 停止定时心跳
        /// </summary>
        public void StopPingTimer()
        {
            if (!_isPingActive)
                return;

            _isPingActive = false;
            _pingCancellationTokenSource?.Cancel();
            _pingCancellationTokenSource?.Dispose();
            _pingCancellationTokenSource = null;
        }

        /// <summary>
        /// 心跳定时器循环
        /// </summary>
        private async UniTaskVoid PingTimerLoop(CancellationToken cancellationToken)
        {
            while (_isPingActive && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await HeartbeatAsync();
                    await UniTask.Delay(30000, cancellationToken: cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log($"[ServiceProxy] 心跳定时器被取消");
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ServiceProxy] 心跳定时器出错: {e.Message}");
                    await UniTask.Delay(30000, cancellationToken: cancellationToken);
                }
            }
        }

        /// <summary>
        /// 异步心跳
        /// </summary>
        private async UniTask HeartbeatAsync()
        {
            var root = new C2SUser
            {
                Session = _serverClient.NextSession(),
                Heartbeat = new C2SHeartbeat
                {
                    ClientTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            };
            S2CUser resp = await _serverClient.PostDomainAsync<S2CUser>("/user", root);
            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.Heartbeat != null)
            {
                Debug.Log($"[ServiceProxy] 心跳成功 - ServerTime:{resp.Heartbeat.ServerTime}");
            }
            else
            {
                Debug.Log($"[ServiceProxy] 心跳失败, code: {resp?.Code}");
            }
        }

        #endregion

        #region API 接口

        /// <summary>
        /// 登录（按实际平台填充 platform oneof）
        /// </summary>
        public async UniTask Login(PlatformManager.PlatformType type, string code, string phone = "")
        {
            var loginMsg = new C2SLogin
            {
                GameId = "Goose"
            };

            // 微信小程序登录
            loginMsg.WechatMini = new WeChatMiniProgram { Code = code };

            var root = new C2SAuth
            {
                Session = _serverClient.NextSession(),
                Login = loginMsg
            };

            S2CAuth resp = await _serverClient.PostDomainAsync<S2CAuth>("/auth", root);

            if (resp != null && resp.Code == S2COpcode.OpSuccess && resp.Login != null)
            {
                _authToken = resp.Login.Token;
                _uuid = resp.Login.Uuid;

                Debug.Log($"[ServiceProxy] 登录成功 token: {_authToken}, uuid: {_uuid}, isNew: {resp.Login.IsNew}");

                _serverClient.SetHeader("Authorization", _authToken);

                StartPingTimer();
                OnClientLogined?.Invoke();
            }
            else
            {
                Debug.Log($"[ServiceProxy] 登录失败, code: {resp?.Code}");
            }
        }

        #endregion
    }
}
