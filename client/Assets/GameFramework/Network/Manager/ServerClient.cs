using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFramework.Core;
using Google.Protobuf;
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
        private string serverUrl = "http://43.143.53.219:9999";
        private bool isConnected = false;
        private Dictionary<Type, Func<S2C_Message, IMessage>> MessageAccessors;
        private Dictionary<Type, Action<C2S_Message, IMessage>> MessageSetters;
        
        /// <summary>
        /// Session计数器，每次发送请求时自动递增
        /// </summary>
        private ulong sessionCounter = 0;
        
        /// <summary>
        /// 记录已发送但未收到响应的请求信息
        /// Key: session, Value: 请求信息(消息类型, 发送时间)
        /// </summary>
        private Dictionary<ulong, (string messageType, DateTime sendTime)> pendingRequests = new Dictionary<ulong, (string, DateTime)>();

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
            // Debug.Log("[ServerClient] 初始化服务器客户端");

            // 初始化HTTP管理器
            httpManager = new HttpManager();
            httpManager.Init();

            // 设置服务器URL
            httpManager.SetBaseUrl(serverUrl);

            // Debug.Log($"[ServerClient] 服务器地址设置为：{serverUrl}");
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
            // Debug.Log("[ServerClient] 释放服务器客户端");
            
            // 输出未完成的请求信息
            if (pendingRequests.Count > 0)
            {
                Debug.LogWarning($"[ServerClient] 释放时仍有 {pendingRequests.Count} 个请求未收到响应：");
                foreach (var kvp in pendingRequests)
                {
                    Debug.LogWarning($"  Session: {kvp.Key}, 类型: {kvp.Value.messageType}, 发送时间: {kvp.Value.sendTime:yyyy-MM-dd HH:mm:ss.fff}");
                }
            }
            
            pendingRequests.Clear();
            httpManager?.Release();
            isConnected = false;
        }

        /// <summary>
        /// 发送C2S消息到服务器
        /// </summary>
        /// <param name="message">要发送的消息</param>
        /// <param name="endpoint">API端点</param>
        /// <returns>服务器响应的S2C消息</returns>
        public async Task<T> PostMessageAsync<T>(IMessage message) where T : IMessage<T>, new()
        {
            //try
            //{
            var postMessage = new C2S_Message();
            // 递增 session 计数器，每次发送请求时自动加一
            sessionCounter++;
            postMessage.Session = sessionCounter;
            var messageType = message.GetType();
            if (MessageSetters.TryGetValue(messageType, out var handler))
            {
                handler(postMessage, message);
            }

            // 记录发送的请求信息
            DateTime sendTime = DateTime.Now;
            pendingRequests[postMessage.Session] = (messageType.Name, sendTime);
            Debug.Log($"[ServerClient] 时间: {sendTime:yyyy-MM-dd HH:mm:ss.fff} - 发送消息 - Session: {postMessage.Session}, 类型: {messageType.Name}, URL: {httpManager._baseUrl}, 待响应数: {pendingRequests.Count}");

            byte[] bodyRaw = postMessage.ToByteArray();
            var responseData = await httpManager.PostAsync(bodyRaw);
            
            // TODO: 这里出现responseData为空的情况，需要处理
            if (responseData != null && responseData.Bytes.Length > 0)
            {
                // 反序列化响应
                S2C_Message wrapper = S2C_Message.Parser.ParseFrom(responseData.Bytes);

                // 检查响应的 session 是否在待响应列表中
                DateTime receiveTime = DateTime.Now;
                if (pendingRequests.TryGetValue(wrapper.Session, out var requestInfo))
                {
                    // 计算请求耗时
                    TimeSpan elapsed = receiveTime - requestInfo.sendTime;
                    
                    Debug.Log($"[ServerClient] 接收消息 - Session: {wrapper.Session}, 状态码: {wrapper.Code}, 消息类型: {typeof(T).Name}, " +
                             $"请求类型: {requestInfo.messageType}, 耗时: {elapsed.TotalMilliseconds:F2}ms, 剩余待响应: {pendingRequests.Count - 1}");
                    
                    // 移除已响应的请求
                    pendingRequests.Remove(wrapper.Session);
                }
                else
                {
                    Debug.LogWarning($"[ServerClient] 时间: {receiveTime:yyyy-MM-dd HH:mm:ss.fff} - 收到未配对的响应 - Session: {wrapper.Session}, 状态码: {wrapper.Code}, " +
                                    $"消息类型: {typeof(T).Name}，该 Session 可能已超时或重复响应");
                }

                if (MessageAccessors.TryGetValue(typeof(T), out var accessor))
                {
                    return (T)accessor(wrapper);
                }

                Debug.Log("[ServerClient] No parser");
                throw new InvalidOperationException($"No parser found for {typeof(T).Name}");
            }
            else
            {
                Debug.LogError("[ServerClient] responseData可能为空，请求可能失败");
                throw new InvalidOperationException($"ResponseData is null");
            }
        }


        public async Task<T> GetMessageAsync<T>(String endpoint = "") where T : IMessage<T>, new()
        {
            try
            {
                var responseData = await httpManager.GetAsync(endpoint, 300);
                if (responseData != null && responseData.Success && responseData.Bytes != null &&
                    responseData.Bytes.Length > 0)
                {
                    // 反序列化响应
                    var parser = new MessageParser<T>(() => new T());
                    T result = parser.ParseFrom(responseData.Bytes);
                    return result;
                }
                else
                {
                    string errorMsg = responseData?.Error ?? "响应数据为空";
                    throw new InvalidOperationException($"获取 {typeof(T).Name} 失败: {errorMsg}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ServerClient] 发送消息失败：{e.Message}");
                throw;
            }
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


        public void SetMessageHandler(Dictionary<Type, Action<C2S_Message, IMessage>> messageSetters,
            Dictionary<Type, Func<S2C_Message, IMessage>> messageAccessors)
        {
            MessageSetters = messageSetters;
            MessageAccessors = messageAccessors;
        }

        public void SetHeader(string key, string value)
        {
            httpManager?.SetDefaultHeader(key, value);
        }

        /// <summary>
        /// 获取当前待响应的请求数量
        /// </summary>
        public int GetPendingRequestCount()
        {
            return pendingRequests.Count;
        }

        /// <summary>
        /// 获取所有待响应的请求信息（用于调试）
        /// </summary>
        public Dictionary<ulong, (string messageType, DateTime sendTime)> GetPendingRequests()
        {
            return new Dictionary<ulong, (string, DateTime)>(pendingRequests);
        }
    }
}