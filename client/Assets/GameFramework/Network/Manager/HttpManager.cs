using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// HTTP网络管理器 - 处理HTTP请求
/// </summary>
public class HttpManager : IManager
{
    // 默认超时时间（秒）
    private const float DEFAULT_TIMEOUT = 10f;
    
    // 默认重试次数
    private const int DEFAULT_RETRY_COUNT = 10;

    // 重试间隔（毫秒）
    private const int RETRY_DELAY_MS = 1000;
    
    // HTTP请求头
    private Dictionary<string, string> _defaultHeaders = new Dictionary<string, string>();
    
    // 基础URL
    public string _baseUrl = "";
    
    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        // Debug.Log("[HttpManager] 初始化HTTP管理器");
        
        // 设置默认请求头
        _defaultHeaders.Clear();
        _defaultHeaders.Add("Content-Type", "application/x-protobuf");
        // 加入Access-Control-Allow-Origin，解决跨域问题
        _defaultHeaders.Add("Access-Control-Allow-Origin", "*");
    }

    /// <summary>
    /// 更新
    /// </summary>
    public void DoUpdate(float dt)
    {
        // 无需每帧更新
    }

    /// <summary>
    /// 释放
    /// </summary>
    public void Release()
    {
        Debug.Log("[HttpManager] 释放HTTP管理器");
        _defaultHeaders.Clear();
    }
    
    /// <summary>
    /// 设置基础URL
    /// </summary>
    public void SetBaseUrl(string baseUrl)
    {
        _baseUrl = baseUrl;
        Debug.Log($"[HttpManager] 设置基础URL: {baseUrl}");
    }
    
    /// <summary>
    /// 设置默认请求头
    /// </summary>
    public void SetDefaultHeader(string key, string value)
    {
        if (_defaultHeaders.ContainsKey(key))
        {
            _defaultHeaders[key] = value;
        }
        else
        {
            _defaultHeaders.Add(key, value);
        }
    }

    /// <summary>
    /// 获取默认请求头
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetHeader(string key)
    {
        return _defaultHeaders.GetValueOrDefault(key, "");
    }
    
    /// <summary>
    /// 清除默认请求头
    /// </summary>
    public void ClearDefaultHeaders()
    {
        _defaultHeaders.Clear();
    }
    
    /// <summary>
    /// 发送GET请求
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="headers">自定义请求头</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="retryCount">重试次数</param>
    /// <returns>HTTP响应</returns>
    public async UniTask<HttpResponse> GetAsync(string url, Dictionary<string, string> headers = null, float timeout = DEFAULT_TIMEOUT, int retryCount = DEFAULT_RETRY_COUNT)
    {
        string fullUrl = GetFullUrl(url);
        
        UnityWebRequest request = UnityWebRequest.Get(fullUrl);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        return await SendRequestAsync(request, headers, timeout, retryCount);
    }
    
    /// <summary>
    /// 发送GET请求（单个参数版本）
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="timeout">超时时间</param>
    /// <returns>HTTP响应</returns>
    public async UniTask<HttpResponse> GetAsync(string url, float timeout)
    {
        return await GetAsync(url, null, timeout, DEFAULT_RETRY_COUNT);
    }
    

    /// <summary>
    /// 发送POST请求（字节数据，POST到baseUrl）
    /// </summary>
    /// <param name="data">字节数据</param>
    /// <param name="headers">自定义请求头</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="retryCount">重试次数</param>
    /// <returns>HTTP响应</returns>
    public async UniTask<HttpResponse> PostAsync(byte[] data, Dictionary<string, string> headers = null, float timeout = DEFAULT_TIMEOUT, int retryCount = DEFAULT_RETRY_COUNT)
    {
        return await PostAsync("", data, headers, timeout, retryCount);
    }

    /// <summary>
    /// 发送POST请求（字节数据，POST到baseUrl + endpoint）
    /// </summary>
    /// <param name="endpoint">API端点路径（如 "/auth"、"/user"）</param>
    /// <param name="data">字节数据</param>
    /// <param name="headers">自定义请求头</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="retryCount">重试次数</param>
    /// <returns>HTTP响应</returns>
    public async UniTask<HttpResponse> PostAsync(string endpoint, byte[] data, Dictionary<string, string> headers = null, float timeout = DEFAULT_TIMEOUT, int retryCount = DEFAULT_RETRY_COUNT)
    {
        Debug.Log($"[HttpManager] PostAsync Start");
        
        
        string fullUrl = GetFullUrl02(endpoint);
        
        
        
        Debug.Log($"[HttpManager] PostAsync -> GetFullUrl");

        UnityWebRequest request = new UnityWebRequest(fullUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(data);
        request.uploadHandler.contentType = "application/x-protobuf";
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/x-protobuf");
        request.SetRequestHeader("Accept", "application/x-protobuf");

        if (headers == null)
        {
            headers = new Dictionary<string, string>();
        }
        headers["Content-Type"] = "application/x-protobuf";
        headers["Accept"] = "application/x-protobuf";
        
        Debug.Log($"[HttpManager] PostAsync -> End");

        return await SendRequestAsync(request, headers, timeout, retryCount);
    }
    
   
    

    
    /// <summary>
    /// 发送POST请求（JSON字符串）
    /// </summary>
    /// <param name="url">请求路径（如 "/sign"），为空则用 baseUrl</param>
    /// <param name="json">JSON字符串</param>
    /// <param name="headers">自定义请求头</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="retryCount">重试次数</param>
    /// <returns>HTTP响应</returns>
    public async UniTask<HttpResponse> PostJsonAsync(string url, string json, Dictionary<string, string> headers = null, float timeout = DEFAULT_TIMEOUT, int retryCount = DEFAULT_RETRY_COUNT)
    {
        string fullUrl = GetFullUrl(url);

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(fullUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.uploadHandler.contentType = "application/json";
        request.downloadHandler = new DownloadHandlerBuffer();

        if (headers == null)
        {
            headers = new Dictionary<string, string>();
        }
        headers["Content-Type"] = "application/json";
        headers["Accept"] = "application/json";

        return await SendRequestAsync(request, headers, timeout, retryCount);
    }

    /// <summary>
    /// 获取完整URL
    /// </summary>
    private string GetFullUrl(string url)
    {
        Debug.Log($"[HttpManager] GetFullUrl: {url}, {_baseUrl} Start!!!");
        
        if (string.IsNullOrEmpty(url))
        {
            return _baseUrl;
        }
        
        // 如果URL已经是完整的，直接返回
        if (url.StartsWith("http://") || url.StartsWith("https://"))
        {
            return url;
        }
        Debug.Log($"[HttpManager] GetFullUrl: Middle!!!");
        // 如果设置了基础URL，拼接返回
        // fixme 
        if (!string.IsNullOrWhiteSpace(_baseUrl))
        {
            Debug.Log($"[HttpManager] baseUrl: {_baseUrl}");
            return _baseUrl.TrimEnd('/') + "/" + url.TrimStart('/');
        }
        Debug.Log($"[HttpManager] GetFullUrl: {url} End");
        // 否则直接返回
        return url;
    }
    
    /// <summary>
    /// 获取完整URL
    /// </summary>
    private string GetFullUrl02(string url)
    {
        Debug.Log($"[HttpManager] GetFullUrl: {url}, {_baseUrl} Start!!!");
        
        if (string.IsNullOrEmpty(url))
        {
            return _baseUrl;
        }
        
        // 如果URL已经是完整的，直接返回
        if (url.StartsWith("http://") || url.StartsWith("https://"))
        {
            return url;
        }
        Debug.Log($"[HttpManager] GetFullUrl: Middle!!!");
        // 如果设置了基础URL，拼接返回
        // fixme 
        if (!string.IsNullOrWhiteSpace(_baseUrl))
        {
            Debug.Log($"[HttpManager] baseUrl: {_baseUrl}");
            return _baseUrl.TrimEnd('/') + "/" + url.TrimStart('/');
        }
        Debug.Log($"[HttpManager] GetFullUrl: {url} End");
        // 否则直接返回
        return url;
    }
    
    /// <summary>
    /// 发送HTTP请求（带重试）
    /// </summary>
    private async UniTask<HttpResponse> SendRequestAsync(UnityWebRequest request, Dictionary<string, string> headers, float timeout, int retryCount)
    {
        // 保存请求信息用于重试（UnityWebRequest 用完即废，重试需要重建）
        string url = request.url;
        string method = request.method;
        byte[] uploadData = request.uploadHandler?.data;
        string contentType = request.uploadHandler?.contentType;

        var response = await DoSendRequest(request, headers, timeout);

        // 失败时重试
        int attempt = 0;
        while (!response.Success && attempt < retryCount)
        {
            attempt++;
            Debug.LogWarning($"[HttpManager] Request failed, retry {attempt}/{retryCount} after {RETRY_DELAY_MS}ms - {url}");
            await UniTask.Delay(RETRY_DELAY_MS, ignoreTimeScale: true);

            // 重建 UnityWebRequest
            var retryRequest = new UnityWebRequest(url, method);
            if (uploadData != null)
            {
                retryRequest.uploadHandler = new UploadHandlerRaw(uploadData);
                retryRequest.uploadHandler.contentType = contentType ?? "application/x-protobuf";
            }
            retryRequest.downloadHandler = new DownloadHandlerBuffer();

            response = await DoSendRequest(retryRequest, headers, timeout);
        }

        return response;
    }

    /// <summary>
    /// 执行单次HTTP请求
    /// </summary>
    private async UniTask<HttpResponse> DoSendRequest(UnityWebRequest request, Dictionary<string, string> headers, float timeout)
    {
        var response = new HttpResponse();

        request.timeout = (int)timeout;

        // 设置请求头
        foreach (var header in _defaultHeaders)
        {
            request.SetRequestHeader(header.Key, header.Value);
        }

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
        }

        // 发送请求
        var operation = request.SendWebRequest();

        while (!operation.isDone)
        {
            await UniTask.Yield();
        }

        // 处理响应
        response.StatusCode = (int)request.responseCode;

        if (request.result == UnityWebRequest.Result.Success)
        {
            response.Success = true;
            response.Data = request.downloadHandler?.text ?? "";
            response.Bytes = request.downloadHandler?.data;
        }
        else
        {
            response.Success = false;
            response.Error = $"{request.error} (状态码: {request.responseCode})";
        }

        Debug.Log($"[HTTP] {request.url} done: {request.responseCode}");
        request?.Dispose();

        return response;
    }
}

/// <summary>
/// HTTP响应
/// </summary>
public class HttpResponse
{
    // 请求是否成功
    public bool Success { get; set; } = false;
    
    // 错误信息
    public string Error { get; set; } = "";
    
    // 状态码
    public int StatusCode { get; set; } = 0;
    
    // 响应数据（字符串）
    public string Data { get; set; } = "";
    
    // 响应数据（字节数组）
    public byte[] Bytes { get; set; } = null;
    
    /// <summary>
    /// 解析JSON数据
    /// </summary>
    public T ParseJson<T>() where T : class
    {
        if (string.IsNullOrEmpty(Data))
        {
            Debug.LogWarning("[HttpResponse] 响应数据为空，无法解析JSON");
            return null;
        }
        
        try
        {
            return JsonUtility.FromJson<T>(Data);
        }
        catch (Exception e)
        {
            Debug.LogError($"[HttpResponse] 解析JSON失败: {e.Message}");
            return null;
        }
        
    }

}

/// <summary>
/// Protobuf HTTP响应
/// </summary>
/// <typeparam name="T">响应数据类型</typeparam>
public class ProtobufResponse<T> where T : class, new()
{
    // 请求是否成功
    public bool Success { get; set; } = false;
    
    // 错误信息
    public string Error { get; set; } = "";
    
    // 状态码
    public int StatusCode { get; set; } = 0;
    
    // 响应数据（已反序列化的Protobuf对象）
    public T Data { get; set; } = default(T);

    // 原始响应数据（字节数组）
    public byte[] RawData { get; set; } = null;
}

 