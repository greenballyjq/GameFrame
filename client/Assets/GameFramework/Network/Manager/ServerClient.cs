using System;
using System.Threading.Tasks;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Network
{
    /// <summary>
    /// 服务器客户端管理器 - 作为统一的服务器通信出口
    /// 使用 Google Protobuf 协议进行通信
    /// </summary>
    public class ServerClient : IManager
    {
        private HttpManager httpManager;
        private string serverUrl = "http://localhost:8080";
        private bool isConnected = false;
        
        /// <summary>
        /// 是否已连接到服务器
        /// </summary>
        public bool IsConnected => isConnected;
        
        /// <summary>
        /// 服务器URL
        /// </summary>
        public string ServerUrl 
        { 
            get => serverUrl; 
            set => serverUrl = value; 
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            Debug.Log("[ServerClient] 初始化服务器客户端");
            
            // 初始化HTTP管理器
            httpManager = new HttpManager();
            httpManager.Init();
            
            // 设置服务器URL
            httpManager.SetBaseUrl(serverUrl);
            
            Debug.Log($"[ServerClient] 服务器地址设置为：{serverUrl}");
        }
        
        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            httpManager?.Update();
        }
        
        /// <summary>
        /// 释放
        /// </summary>
        public void Release()
        {
            Debug.Log("[ServerClient] 释放服务器客户端");
            httpManager?.Release();
            isConnected = false;
        }
        
        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <returns>是否连接成功</returns>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                Debug.Log("[ServerClient] 开始连接服务器...");
                
                // 发送Ping测试连接
                var pingMessage = ProtobufManager.CreatePingMessage();
                var pingResult = await SendMessageAsync(pingMessage);
                
                if (pingResult != null)
                {
                    isConnected = true;
                    Debug.Log("[ServerClient] 服务器连接成功");
                    return true;
                }
                else
                {
                    isConnected = false;
                    Debug.LogWarning("[ServerClient] 服务器连接失败 - Ping无响应");
                    return false;
                }
            }
            catch (Exception e)
            {
                isConnected = false;
                Debug.LogError($"[ServerClient] 服务器连接失败：{e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 断开与服务器的连接
        /// </summary>
        public void Disconnect()
        {
            isConnected = false;
            Debug.Log("[ServerClient] 已断开服务器连接");
        }
        
        /// <summary>
        /// 发送C2S消息到服务器
        /// </summary>
        /// <param name="message">要发送的消息</param>
        /// <returns>服务器响应的S2C消息</returns>
        public async Task<S2C_Message> SendMessageAsync(C2S_Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            
            try
            {
                Debug.Log($"[ServerClient] 发送消息：Session={message.Session}, Type={message.MsgCase}");
                
                // 使用GameFramework的ProtobufManager序列化
                var requestData = ProtobufManager.SerializeC2SMessage(message);
                
                // 通过HTTP发送
                var responseData = await httpManager.SendProtobufAsync(requestData);
                
                if (responseData != null && responseData.Length > 0)
                {
                    // 反序列化响应
                    var response = ProtobufManager.DeserializeS2CMessage(responseData);
                    Debug.Log($"[ServerClient] 收到响应：Session={response.Session}, Type={response.MsgCase}");
                    return response;
                }
                else
                {
                    Debug.LogWarning("[ServerClient] 服务器响应为空");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ServerClient] 发送消息失败：{e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 发送JSON消息到服务器（用于兼容或调试）
        /// </summary>
        /// <param name="jsonData">JSON字符串</param>
        /// <returns>服务器响应的JSON字符串</returns>
        public async Task<string> SendJsonAsync(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                throw new ArgumentException("JSON数据不能为空", nameof(jsonData));
            }
            
            try
            {
                Debug.Log($"[ServerClient] 发送JSON消息：{jsonData}");
                
                var response = await httpManager.SendJsonAsync(jsonData);
                
                Debug.Log($"[ServerClient] 收到JSON响应：{response}");
                return response;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ServerClient] 发送JSON消息失败：{e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 发送登录请求
        /// </summary>
        /// <param name="account">账户名</param>
        /// <returns>登录响应</returns>
        public async Task<S2C_Message> LoginAsync(string account)
        {
            if (string.IsNullOrEmpty(account))
            {
                throw new ArgumentException("账户名不能为空", nameof(account));
            }
            
            Debug.Log($"[ServerClient] 发送登录请求：{account}");
            
            var loginMessage = ProtobufManager.CreateLoginMessage(account);
            return await SendMessageAsync(loginMessage);
        }
        
        /// <summary>
        /// 发送Ping请求
        /// </summary>
        /// <returns>Ping响应</returns>
        public async Task<S2C_Message> PingAsync()
        {
            Debug.Log("[ServerClient] 发送Ping请求");
            
            var pingMessage = ProtobufManager.CreatePingMessage();
            return await SendMessageAsync(pingMessage);
        }
        
        /// <summary>
        /// 请求客户端数据
        /// </summary>
        /// <returns>客户端数据响应</returns>
        public async Task<S2C_Message> RequestClientDataAsync()
        {
            Debug.Log("[ServerClient] 请求客户端数据");
            
            var message = ProtobufManager.CreateClientDataMessage();
            return await SendMessageAsync(message);
        }
        
        /// <summary>
        /// 上传客户端数据
        /// </summary>
        /// <param name="data">要上传的数据</param>
        /// <returns>上传响应</returns>
        public async Task<S2C_Message> UploadClientDataAsync(byte[] data)
        {
            Debug.Log($"[ServerClient] 上传客户端数据，大小：{data?.Length ?? 0}字节");
            
            var message = ProtobufManager.CreateUploadClientDataMessage(data);
            return await SendMessageAsync(message);
        }
        
        /// <summary>
        /// 请求基础数据
        /// </summary>
        /// <returns>基础数据响应</returns>
        public async Task<S2C_Message> RequestBaseDataAsync()
        {
            Debug.Log("[ServerClient] 请求基础数据");
            
            var message = ProtobufManager.CreateBaseMessage();
            return await SendMessageAsync(message);
        }
        
        /// <summary>
        /// 请求道具数据
        /// </summary>
        /// <returns>道具数据响应</returns>
        public async Task<S2C_Message> RequestItemDataAsync()
        {
            Debug.Log("[ServerClient] 请求道具数据");
            
            var message = ProtobufManager.CreateItemMessage();
            return await SendMessageAsync(message);
        }
        
        /// <summary>
        /// 发送登出请求
        /// </summary>
        /// <returns>登出响应</returns>
        public async Task<S2C_Message> LogoutAsync()
        {
            Debug.Log("[ServerClient] 发送登出请求");
            
            var logoutMessage = ProtobufManager.CreateLogoutMessage();
            return await SendMessageAsync(logoutMessage);
        }
        
        /// <summary>
        /// 设置服务器地址
        /// </summary>
        /// <param name="url">服务器URL</param>
        public void SetServerUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("服务器URL不能为空", nameof(url));
            }
            
            serverUrl = url;
            httpManager?.SetBaseUrl(url);
            Debug.Log($"[ServerClient] 服务器地址已更新为：{url}");
        }
        
        /// <summary>
        /// 获取连接状态信息
        /// </summary>
        /// <returns>连接状态描述</returns>
        public string GetConnectionStatus()
        {
            return $"连接状态：{(isConnected ? "已连接" : "未连接")}，服务器：{serverUrl}";
        }
    }
}

