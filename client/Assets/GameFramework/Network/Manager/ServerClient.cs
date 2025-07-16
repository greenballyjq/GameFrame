using System;
using System.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Network.Protocol;
using UnityEngine;


    /// <summary>
    /// 服务器客户端 - 封装完整的服务器通讯流程
    /// </summary>
    public class ServerClient : IManager
    {
        // 服务器URL
        private string _serverUrl;
        
        // HTTP管理器引用
        private HttpManager _httpManager;
        
        // 握手管理器引用
        private HandshakeManager _handshakeManager;
        
        // 连接状态
        public bool IsConnected { get; private set; } = false;
        
        // 连接事件
        public event Action<bool> OnConnectionChanged;
        
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            Debug.Log("[ServerClient] 初始化服务器客户端");
            
            // 获取管理器引用
            _httpManager = GameFrameworkManager.Instance.GetManager<HttpManager>();
            _handshakeManager = GameFrameworkManager.Instance.GetManager<HandshakeManager>();
            
            IsConnected = false;
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            // 无需每帧更新
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Release()
        {
            Debug.Log("[ServerClient] 释放服务器客户端");
            
            if (IsConnected)
            {
                Disconnect();
            }
        }
        
        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="serverUrl">服务器URL</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>连接结果</returns>
        public async Task<ServerConnectionResult> ConnectAsync(string serverUrl, float timeout = 30f)
        {
            if (IsConnected)
            {
                Debug.LogWarning("[ServerClient] 已连接到服务器");
                return new ServerConnectionResult { Success = true, Message = "已连接" };
            }
            
            if (_httpManager == null || _handshakeManager == null)
            {
                Debug.LogError("[ServerClient] 管理器未初始化");
                return new ServerConnectionResult { Success = false, Message = "管理器未初始化" };
            }
            
            try
            {
                Debug.Log($"[ServerClient] 开始连接服务器: {serverUrl}");
                
                _serverUrl = serverUrl;
                
                // 执行握手流程
                ServerHandshakeResult handshakeResult = await _httpManager.PerformHandshakeAsync(serverUrl, timeout);
                
                if (handshakeResult.Success)
                {
                    IsConnected = true;
                    Debug.Log("[ServerClient] 成功连接到服务器");
                    OnConnectionChanged?.Invoke(true);
                    
                    return new ServerConnectionResult 
                    { 
                        Success = true, 
                        Message = "连接成功",
                        SharedSecret = handshakeResult.SharedSecret
                    };
                }
                else
                {
                    Debug.LogError($"[ServerClient] 连接失败: {handshakeResult.Error}");
                    return new ServerConnectionResult 
                    { 
                        Success = false, 
                        Message = $"连接失败: {handshakeResult.Error}"
                    };
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ServerClient] 连接异常: {e.Message}");
                return new ServerConnectionResult 
                { 
                    Success = false, 
                    Message = $"连接异常: {e.Message}"
                };
            }
        }
        
        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[ServerClient] 未连接到服务器");
                return;
            }
            
            Debug.Log("[ServerClient] 断开服务器连接");
            
            IsConnected = false;
            _serverUrl = null;
            
            // 重置握手状态
            _handshakeManager?.Reset();
            
            OnConnectionChanged?.Invoke(false);
        }
        
        /// <summary>
        /// 发送Protobuf消息
        /// </summary>
        /// <typeparam name="TRequest">请求消息类型</typeparam>
        /// <typeparam name="TResponse">响应消息类型</typeparam>
        /// <param name="endpoint">API端点</param>
        /// <param name="request">请求消息</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>响应结果</returns>
        public async Task<ServerMessageResult<TResponse>> SendMessageAsync<TRequest, TResponse>(
            string endpoint, 
            TRequest request, 
            float timeout = 10f)
            where TRequest : class
            where TResponse : class, new()
        {
            if (!IsConnected)
            {
                Debug.LogError("[ServerClient] 未连接到服务器");
                return new ServerMessageResult<TResponse> 
                { 
                    Success = false, 
                    Error = "未连接到服务器" 
                };
            }
            
            if (_httpManager == null)
            {
                Debug.LogError("[ServerClient] HTTP管理器未初始化");
                return new ServerMessageResult<TResponse> 
                { 
                    Success = false, 
                    Error = "HTTP管理器未初始化" 
                };
            }
            
            try
            {
                string url = _serverUrl + "/" + endpoint.TrimStart('/');
                
                EncryptedServerResponse<TResponse> response = await _httpManager.SendEncryptedProtobufAsync<TRequest, TResponse>(
                    url, request, timeout);
                
                if (response.Success)
                {
                    return new ServerMessageResult<TResponse>
                    {
                        Success = true,
                        Data = response.Data,
                        StatusCode = response.StatusCode
                    };
                }
                else
                {
                    return new ServerMessageResult<TResponse>
                    {
                        Success = false,
                        Error = response.Error,
                        StatusCode = response.StatusCode
                    };
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ServerClient] 发送消息异常: {e.Message}");
                return new ServerMessageResult<TResponse>
                {
                    Success = false,
                    Error = $"发送消息异常: {e.Message}"
                };
            }
        }
        
        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="endpoint">API端点</param>
        /// <param name="message">文本消息</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>响应结果</returns>
        public async Task<ServerMessageResult<string>> SendTextMessageAsync(
            string endpoint, 
            string message, 
            float timeout = 10f)
        {
            if (!IsConnected)
            {
                Debug.LogError("[ServerClient] 未连接到服务器");
                return new ServerMessageResult<string> 
                { 
                    Success = false, 
                    Error = "未连接到服务器" 
                };
            }
            
            if (_httpManager == null)
            {
                Debug.LogError("[ServerClient] HTTP管理器未初始化");
                return new ServerMessageResult<string> 
                { 
                    Success = false, 
                    Error = "HTTP管理器未初始化" 
                };
            }
            
            try
            {
                string url = _serverUrl + "/" + endpoint.TrimStart('/');
                
                EncryptedServerResponse<string> response = await _httpManager.SendEncryptedTextAsync(
                    url, message, timeout);
                
                if (response.Success)
                {
                    return new ServerMessageResult<string>
                    {
                        Success = true,
                        Data = response.Data,
                        StatusCode = response.StatusCode
                    };
                }
                else
                {
                    return new ServerMessageResult<string>
                    {
                        Success = false,
                        Error = response.Error,
                        StatusCode = response.StatusCode
                    };
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ServerClient] 发送文本消息异常: {e.Message}");
                return new ServerMessageResult<string>
                {
                    Success = false,
                    Error = $"发送文本消息异常: {e.Message}"
                };
            }
        }
        
        /// <summary>
        /// 登录到服务器
        /// </summary>
        /// <param name="loginRequest">登录请求</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>登录结果</returns>
        public async Task<ServerMessageResult<S2C_Login>> LoginAsync(C2S_Login loginRequest, float timeout = 10f)
        {
            return await SendMessageAsync<C2S_Login, S2C_Login>("login", loginRequest, timeout);
        }
        
        /// <summary>
        /// 登出服务器
        /// </summary>
        /// <param name="logoutRequest">登出请求</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>登出结果</returns>
        public async Task<ServerMessageResult<S2C_Logout>> LogoutAsync(C2S_Logout logoutRequest, float timeout = 10f)
        {
            return await SendMessageAsync<C2S_Logout, S2C_Logout>("logout", logoutRequest, timeout);
        }
        
        /// <summary>
        /// 发送Ping消息
        /// </summary>
        /// <param name="pingRequest">Ping请求</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>Ping结果</returns>
        public async Task<ServerMessageResult<S2C_Ping>> PingAsync(C2S_Ping pingRequest, float timeout = 5f)
        {
            return await SendMessageAsync<C2S_Ping, S2C_Ping>("ping", pingRequest, timeout);
        }
        
        /// <summary>
        /// 获取服务器URL
        /// </summary>
        /// <returns>服务器URL</returns>
        public string GetServerUrl()
        {
            return _serverUrl;
        }
        
        /// <summary>
        /// 检查连接状态
        /// </summary>
        /// <returns>是否连接</returns>
        public bool CheckConnection()
        {
            return IsConnected && _handshakeManager != null && _handshakeManager.IsHandshakeCompleted;
        }
    }
    
    /// <summary>
    /// 服务器连接结果
    /// </summary>
    public class ServerConnectionResult
    {
        // 连接是否成功
        public bool Success { get; set; } = false;
        
        // 结果消息
        public string Message { get; set; } = "";
        
        // 共享密钥
        public byte[] SharedSecret { get; set; } = null;
    }
    
    /// <summary>
    /// 服务器消息结果
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    public class ServerMessageResult<T>
    {
        // 请求是否成功
        public bool Success { get; set; } = false;
        
        // 错误信息
        public string Error { get; set; } = "";
        
        // 状态码
        public int StatusCode { get; set; } = 0;
        
        // 响应数据
        public T Data { get; set; } = default(T);
    }

