using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Etcd;
using GameFramework.Core;
using Google.Protobuf;
using UnityEngine;

namespace GameFramework.Network.Utility
{
    public class ServiceProxy : Singleton<ServiceProxy>, IDisposable
    {
        //协议注册
        private readonly Dictionary<Type, Action<C2S_Message, IMessage>> MessageSetters = new()
        {
            { typeof(C2S_Ping), (wrapper, msg) => wrapper.Ping = (C2S_Ping)msg },
            { typeof(C2S_Login), (wrapper, msg) => wrapper.Login = (C2S_Login)msg },
            // { typeof(C2S_Mall), (wrapper, msg) => wrapper.MallItems = (C2S_Mall)msg },
            // { typeof(C2S_Item), (wrapper, msg) => wrapper.ItemData = (C2S_Item)msg },
            // { typeof(C2S_Buy), (wrapper, msg) => wrapper.Buy = (C2S_Buy)msg },
            // { typeof(C2S_OrderList), (wrapper, msg) => wrapper.OrderList = (C2S_OrderList)msg },
            // { typeof(C2S_Configuration), (wrapper, msg) => wrapper.Configuration = (C2S_Configuration)msg },
        };

        private readonly Dictionary<Type, Func<S2C_Message, IMessage>> MessageAccessors = new()
        {
            { typeof(S2C_Ping), wrapper => wrapper.Ping },
            { typeof(S2C_Login), wrapper => wrapper.Login },
            // { typeof(S2C_Currency), wrapper => wrapper.CurrencyData },
            // { typeof(S2C_Message), wrapper => wrapper },
            // { typeof(S2C_Mall), wrapper => wrapper.MallItems },
            // { typeof(S2C_Item), wrapper => wrapper.ItemData },
            // { typeof(S2C_ItemUpdate), wrapper => wrapper.ItemData },
            // { typeof(S2C_Buy), wrapper => wrapper.Buy },
            // { typeof(S2C_OrderList), wrapper => wrapper.OrderList },
            // { typeof(S2C_Configuration), wrapper => wrapper.Configuration },
        };
        
        private ServerClient _serverClient;

        // 保存token
        private string _authToken = "";
        
        // 服务端连接成功回调事件
        public event System.Action OnServerConnected;
        // 客户端登录成功回调
        public event System.Action OnClientLogined;
        /// <summary>
        /// 服务端是否已连接
        /// </summary>
        public bool IsServerConnected { get; private set; } = false;

        // ping 定时器相关
        private bool _isPingActive = false;
        
        private CancellationTokenSource _pingCancellationTokenSource;

        private string gameAddress = "";
        private string loginAddress = "";


        public async UniTask InitAsync()
        {
            _serverClient = new ServerClient();
            _serverClient.Init();
            _serverClient.SetServerUrl("https://dev.sanqianpan.com:9999");
            _serverClient.SetMessageHandler(MessageSetters, MessageAccessors);
            
            await ConnectServerAsync();
        }

        public void Dispose()
        {
            _serverClient.Release();
            _pingCancellationTokenSource?.Dispose();
        }

        public async UniTask ConnectServerAsync()
        {
            var list = await _serverClient.GetMessageAsync<ServerList>("/serverlist");
            if (list != null)
            {
                var server = list.Servers[0];
                gameAddress = server.Address;
                Uri uri = new Uri(gameAddress);
                
                // 取出 schema + host + port
                string baseUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
                
                // 拼接新的路径
                loginAddress = baseUrl + "/login";
                // Debug.Log($"[GameServiceProxy] LoginAddress: {loginAddress}, gameAddress: {gameAddress}");
                _serverClient.SetServerUrl(loginAddress);

                IsServerConnected = true;
                // 服务器连接成功，触发回调
                // Debug.Log($"[GameServiceProxy] 服务器连接成功，触发回调");
                OnServerConnected?.Invoke();
            }
            else
            {
                Debug.Log($"[HttpProxy] 服务器连接失败");
            }
        }

        #region Ping

        /// <summary>
        /// 开始定时 ping
        /// </summary>
        public void StartPingTimer()
        {
            if (_isPingActive)
            {
                // Debug.Log($"[GameServiceProxy] Ping 定时器已经在运行");
                return;
            }

            _isPingActive = true;
            _pingCancellationTokenSource = new CancellationTokenSource();

            // Debug.Log($"[GameServiceProxy] 开始 30 秒定时 ping");
            PingTimerLoop(_pingCancellationTokenSource.Token).Forget();
        }
        
        /// <summary>
        /// 停止定时 ping
        /// </summary>
        public void StopPingTimer()
        {
            if (!_isPingActive)
                return;

            _isPingActive = false;
            _pingCancellationTokenSource?.Cancel();
            _pingCancellationTokenSource?.Dispose();
            _pingCancellationTokenSource = null;

            // Debug.Log($"[GameServiceProxy] 停止定时 ping");
        }
        
        /// <summary>
        /// ping 定时器循环
        /// </summary>
        private async UniTaskVoid PingTimerLoop(CancellationToken cancellationToken)
        {
            while (_isPingActive && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 执行 ping
                    await PingAsync();

                    // 等待 30 秒
                    await UniTask.Delay(30000, cancellationToken: cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log($"[GameServiceProxy] Ping 定时器被取消");
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[GameServiceProxy] Ping 定时器出错: {e.Message}");
                    // 即使出错也继续，等待 30 秒后重试
                    await UniTask.Delay(30000, cancellationToken: cancellationToken);
                }
            }
        }
        
        /// <summary>
        /// 异步 ping 测试
        /// </summary>
        private async UniTask PingAsync()
        {
            var data = new C2S_Ping();
            data.Clienttime = (ulong)(Time.realtimeSinceStartup * 1000);
            //var messaage = new C2S_Message
            //{
            //    Ping = data
            //};
            S2C_Ping respose = await _serverClient.PostMessageAsync<S2C_Ping>(data);
            if (respose != null)
            {
                Debug.Log($"[GameServiceProxy] Ping 成功 - ServiceTime:{respose.Servertime}");
            }
            else
            {
                Debug.Log($"[GameServiceProxy] ping解析失败");
            }
        }

        #endregion

        #region API 接口

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="type">平台类型</param>
        /// <param name="code">微信登录code</param>
        /// <param name="phone">手机号</param>
        public async UniTask Login(PlatformManager.PlatformType type, string code, string phone = "")
        {
            var data = new C2S_Login();
            if (type == PlatformManager.PlatformType.WeChat)
            {
                data.Wechat = new WeChat
                {
                    Code = code
                };
            }
            else
            {
                data.Phone = phone;
            }

            S2C_Login response = await _serverClient.PostMessageAsync<S2C_Login>(data);

            if (response != null)
            {
                Debug.Log($"[ServiceProxy] LoginWX 成功 {response.Token}");

                // 保存token
                _authToken = response.Token;

                // 重新设置Authorization header
                // _serverClient.SetServerUrl(loginAddress);
                _serverClient.SetServerUrl(gameAddress);

                // _serverClient.SetHeader("Authorization", $"Bearer {_authToken}");
                _serverClient.SetHeader("Authorization", _authToken);
                Debug.Log($"[ServiceProxy] 已设置Authorization header");

                // 开始ping定时器
                StartPingTimer();
                Debug.Log($"[ServiceProxy] 登录成功，已开始ping定时器");

                // uiGameMessage.ShowMessage("[ServiceProxy] 登录成功");
                OnClientLogined?.Invoke();
            }
            else
            {
                // uiGameMessage.ShowMessage("[GameServiceProxy] 登录失败");
                Debug.Log($"[ServiceProxy] Login 失败");
            }
        }

        #endregion
    }
}