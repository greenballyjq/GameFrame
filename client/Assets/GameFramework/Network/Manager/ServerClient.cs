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
            postMessage.Session = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var messageType = message.GetType();
            if (MessageSetters.TryGetValue(messageType, out var handler))
            {
                handler(postMessage, message);
            }

            Debug.Log($"ok postMessage, type: {messageType}, message: {postMessage}" +
                           $", url: {httpManager._baseUrl}, timestamp:{DateTime.Now}, Token: {httpManager.GetHeader("Authorization")}");

            byte[] bodyRaw = postMessage.ToByteArray();
            var responseData = await httpManager.PostAsync(bodyRaw);
            
            if (responseData != null && responseData.Bytes.Length > 0)
            {
                // 反序列化响应
                S2C_Message wrapper = S2C_Message.Parser.ParseFrom(responseData.Bytes);

                Debug.Log($"S2C_Message内容：{wrapper.Code}, {wrapper}");

                if (MessageAccessors.TryGetValue(typeof(T), out var accessor))
                {
                    return (T)accessor(wrapper);
                }

                Debug.Log("[ServerClient] No parser");
                throw new InvalidOperationException($"No parser found for {typeof(T).Name}");
            }
            else
            {
                Debug.Log("[ServerClient] responseData可能为空");
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
    }
}