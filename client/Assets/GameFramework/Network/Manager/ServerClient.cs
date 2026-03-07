using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFramework.Core;
using Google.Protobuf;
using UnityEngine;

namespace GameFramework.Network
{
    /// <summary>
    /// 服务器客户端管理器 - 域路由通信层
    /// 使用 Google Protobuf 协议，按业务域拆分端点进行通信
    /// </summary>
    public class ServerClient : IManager
    {
        private HttpManager httpManager;
        private string serverUrl = "";
        private bool isConnected = false;

        /// <summary>
        /// Session计数器，每次发送请求时自动递增
        /// </summary>
        private ulong sessionCounter = 0;

        /// <summary>
        /// 记录已发送但未收到响应的请求信息
        /// Key: session, Value: 请求信息(消息类型, 发送时间)
        /// </summary>
        private Dictionary<ulong, (string messageType, string endpoint, DateTime sendTime)> pendingRequests =
            new Dictionary<ulong, (string, string, DateTime)>();

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
        /// 获取下一个session ID
        /// </summary>
        public ulong NextSession()
        {
            sessionCounter++;
            return sessionCounter;
        }

        /// <summary>
        /// 初始���
        /// </summary>
        public void Init()
        {
            httpManager = new HttpManager();
            httpManager.Init();

            if (!string.IsNullOrEmpty(serverUrl))
            {
                httpManager.SetBaseUrl(serverUrl);
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void DoUpdate(float dt)
        {
            httpManager?.DoUpdate(dt);
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Release()
        {
            if (pendingRequests.Count > 0)
            {
                Debug.LogWarning($"[ServerClient] 释放时仍有 {pendingRequests.Count} 个请求未收到响应：");
                foreach (var kvp in pendingRequests)
                {
                    Debug.LogWarning(
                        $"  [{kvp.Key}] {kvp.Value.messageType} -> {kvp.Value.endpoint} (sent: {kvp.Value.sendTime:HH:mm:ss.fff})");
                }
            }

            pendingRequests.Clear();
            httpManager?.Release();
            isConnected = false;
        }

        /// <summary>
        /// 发送域根消息到对应端点，并解析域根响应
        /// </summary>
        /// <typeparam name="TResponse">域根响应类型（如 S2CAuth、S2CUser 等）</typeparam>
        /// <param name="endpoint">API端点（如 "/auth"、"/user"、"/game"、"/payment"、"/social"）</param>
        /// <param name="rootRequest">已构造好的域根消息（如 C2SAuth、C2SUser 等）</param>
        /// <param name="delayMs">发送前延迟（毫秒），用于等待广告验证等场景，0=不延迟</param>
        /// <returns>解析后的域根响应</returns>
        public async Task<TResponse> PostDomainAsync<TResponse>(
            string endpoint, IMessage rootRequest, int delayMs = 0
        ) where TResponse : IMessage<TResponse>, new()
        {
            if (delayMs > 0)
            {
                Debug.Log($"[ServerClient] Delay {delayMs}ms before request -> {endpoint}");
                await Cysharp.Threading.Tasks.UniTask.Delay(delayMs, ignoreTimeScale: true);
            }

            string requestType = rootRequest.GetType().Name;

            // 记录发送的请求信息
            ulong session = sessionCounter; // 当前session（调用方已调用NextSession）
            DateTime sendTime = DateTime.Now;
            pendingRequests[session] = (requestType, endpoint, sendTime);
            Debug.Log(
                $"[ServerClient] >>> [{session}] {requestType} -> {endpoint} (pending:{pendingRequests.Count})\n" +
                $"{JsonFormatter.Default.Format(rootRequest)}");

            // 序列化并发送
            byte[] bodyRaw = rootRequest.ToByteArray();
            var responseData = await httpManager.PostAsync(endpoint, bodyRaw);

            if (responseData != null && responseData.Bytes != null && responseData.Bytes.Length > 0)
            {
                // 反序列化响应
                var parser = new MessageParser<TResponse>(() => new TResponse());
                TResponse result = parser.ParseFrom(responseData.Bytes);

                // 跟踪请求耗时
                if (pendingRequests.TryGetValue(session, out var requestInfo))
                {
                    double elapsed = (DateTime.Now - requestInfo.sendTime).TotalMilliseconds;
                    pendingRequests.Remove(session);
                    Debug.Log(
                        $"[ServerClient] <<< [{session}] {typeof(TResponse).Name} <- {requestInfo.endpoint} ({elapsed:F0}ms, pending:{pendingRequests.Count})\n" +
                        $"{JsonFormatter.Default.Format(result)}");
                }

                isConnected = true;
                return result;
            }
            else
            {
                pendingRequests.Remove(session);
                Debug.LogError(
                    $"[ServerClient] 请求失败 - Session: {session}, endpoint: {endpoint}, error: {responseData?.Error}");
                throw new InvalidOperationException(
                    $"PostDomainAsync failed for {endpoint}: {responseData?.Error ?? "ResponseData is null"}");
            }
        }

        /// <summary>
        /// 设置服务器地址
        /// </summary>
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
        /// 设置请求头
        /// </summary>
        public void SetHeader(string key, string value)
        {
            httpManager?.SetDefaultHeader(key, value);
        }

        /// <summary>
        /// 获取HttpManager
        /// </summary>
        public HttpManager GetHttpManager()
        {
            return httpManager;
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
        public Dictionary<ulong, (string messageType, string endpoint, DateTime sendTime)> GetPendingRequests()
        {
            return new Dictionary<ulong, (string, string, DateTime)>(pendingRequests);
        }
    }
}
