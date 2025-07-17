using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Network;
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
    private const int DEFAULT_RETRY_COUNT = 1;
    
    // HTTP请求头
    private Dictionary<string, string> _defaultHeaders = new Dictionary<string, string>();
    
    // 基础URL
    public string _baseUrl = "";


    
    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        Debug.Log("[HttpManager] 初始化HTTP管理器");
        
        // 设置默认请求头
        _defaultHeaders.Clear();
        _defaultHeaders.Add("Content-Type", "application/json");
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
    public async Task<HttpResponse> GetAsync(string url, Dictionary<string, string> headers = null, float timeout = DEFAULT_TIMEOUT, int retryCount = DEFAULT_RETRY_COUNT)
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
    public async Task<HttpResponse> GetAsync(string url, float timeout)
    {
        return await GetAsync(url, null, timeout, DEFAULT_RETRY_COUNT);
    }
    
    /// <summary>
    /// 发送POST请求（JSON数据）
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="jsonData">JSON数据</param>
    /// <param name="headers">自定义请求头</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="retryCount">重试次数</param>
    /// <returns>HTTP响应</returns>
    public async Task<HttpResponse> PostAsync(string url, string jsonData, Dictionary<string, string> headers = null, float timeout = DEFAULT_TIMEOUT, int retryCount = DEFAULT_RETRY_COUNT)
    {
        string fullUrl = GetFullUrl(url);
        
        UnityWebRequest request = new UnityWebRequest(fullUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        return await SendRequestAsync(request, headers, timeout, retryCount);
    }
    
    /// <summary>
    /// 发送POST请求（字节数据）
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="data">字节数据</param>
    /// <param name="headers">自定义请求头</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="retryCount">重试次数</param>
    /// <returns>HTTP响应</returns>
    public async Task<HttpResponse> PostAsync(string url, byte[] data, Dictionary<string, string> headers = null, float timeout = DEFAULT_TIMEOUT, int retryCount = DEFAULT_RETRY_COUNT)
    {
        string fullUrl = GetFullUrl(url);
        
        UnityWebRequest request = new UnityWebRequest(fullUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(data);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        return await SendRequestAsync(request, headers, timeout, retryCount);
    }
    
    /// <summary>
    /// 发送C2S消息并接收S2C消息
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="request">请求消息</param>
    /// <param name="timeout">超时时间</param>
    /// <returns>S2C消息响应</returns>
    public async Task<ProtobufResponse<S2C_Message>> SendC2SMessageAsync(
        string url, 
        C2S_Message request, 
        float timeout = DEFAULT_TIMEOUT)
    {
        var protobufResponse = new ProtobufResponse<S2C_Message>
        {
            Success = false,
            Error = "未知错误"
        };



        try
        {
            // 序列化请求消息
            byte[] requestData = ProtobufManager.Serialize(request);
            if (requestData == null)
            {
                protobufResponse.Error = "请求消息序列化失败";
                return protobufResponse;
            }

            // 发送HTTP请求
            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/x-protobuf" }
            };
            
            HttpResponse httpResponse = await PostAsync(url, requestData, headers, timeout);
            
            protobufResponse.StatusCode = httpResponse.StatusCode;
            
            if (!httpResponse.Success)
            {
                protobufResponse.Error = $"HTTP请求失败: {httpResponse.Error}";
                return protobufResponse;
            }

            // 反序列化响应消息
            if (httpResponse.Bytes != null && httpResponse.Bytes.Length > 0)
            {
                S2C_Message responseMessage = ProtobufManager.DeserializeS2CMessage(httpResponse.Bytes);
                if (responseMessage != null)
                {
                    protobufResponse.Success = true;
                    protobufResponse.Data = responseMessage;
                    protobufResponse.RawData = httpResponse.Bytes;
                    protobufResponse.Error = "";
                }
                else
                {
                    protobufResponse.Error = "响应消息反序列化失败";
                }
            }
            else
            {
                protobufResponse.Error = "服务器响应数据为空";
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[HttpManager] 发送Protobuf消息异常: {e.Message}");
            protobufResponse.Error = $"发送Protobuf消息异常: {e.Message}";
        }

        return protobufResponse;
    }
    
    /// <summary>
    /// 发送 Protobuf 数据并返回 Protobuf 响应
    /// </summary>
    /// <param name="requestData">要发送的protobuf字节数组</param>
    /// <param name="endpoint">API端点（可选，默认为 /api/message）</param>
    /// <returns>服务器响应的protobuf字节数组</returns>
    public async Task<byte[]> SendProtobufAsync(byte[] requestData, string endpoint = "/api/message")
    {
        if (requestData == null || requestData.Length == 0)
        {
            throw new ArgumentException("请求数据不能为空", nameof(requestData));
        }

        try
        {
            string url = _baseUrl + endpoint;
            Debug.Log($"[HttpManager] 发送Protobuf数据到：{url}，大小：{requestData.Length}字节");

            using (var request = UnityWebRequest.PostWwwForm(url, ""))
            {
                // 设置protobuf内容
                var uploadHandler = new UploadHandlerRaw(requestData);
                uploadHandler.contentType = "application/x-protobuf";
                request.uploadHandler = uploadHandler;

                // 设置下载处理器
                request.downloadHandler = new DownloadHandlerBuffer();

                // 设置请求头
                request.SetRequestHeader("Content-Type", "application/x-protobuf");
                request.SetRequestHeader("Accept", "application/x-protobuf");

                // 发送请求
                var operation = request.SendWebRequest();
                
                // 等待完成
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                // 检查结果
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var responseData = request.downloadHandler.data;
                    Debug.Log($"[HttpManager] 收到Protobuf响应，大小：{responseData?.Length ?? 0}字节");
                    return responseData;
                }
                else
                {
                    var error = $"HTTP错误：{request.responseCode} - {request.error}";
                    Debug.LogError($"[HttpManager] {error}");
                    throw new Exception(error);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[HttpManager] 发送Protobuf请求失败：{e.Message}");
            throw;
        }
    }

    /// <summary>
    /// 发送JSON数据并返回JSON响应
    /// </summary>
    /// <param name="jsonData">JSON字符串</param>
    /// <param name="endpoint">API端点（可选，默认为 /api/json）</param>
    /// <returns>服务器响应的JSON字符串</returns>
    public async Task<string> SendJsonAsync(string jsonData, string endpoint = "/api/json")
    {
        if (string.IsNullOrEmpty(jsonData))
        {
            throw new ArgumentException("JSON数据不能为空", nameof(jsonData));
        }

        try
        {
            string url = _baseUrl + endpoint;
            Debug.Log($"[HttpManager] 发送JSON数据到：{url}");

            using (var request = UnityWebRequest.Post(url, jsonData, "application/json"))
            {
                // 发送请求
                var operation = request.SendWebRequest();
                
                // 等待完成
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                // 检查结果
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var responseText = request.downloadHandler.text;
                    Debug.Log($"[HttpManager] 收到JSON响应：{responseText}");
                    return responseText;
                }
                else
                {
                    var error = $"HTTP错误：{request.responseCode} - {request.error}";
                    Debug.LogError($"[HttpManager] {error}");
                    throw new Exception(error);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[HttpManager] 发送JSON请求失败：{e.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// 获取完整URL
    /// </summary>
    private string GetFullUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException("URL不能为空");
        }
        
        // 如果URL已经是完整的，直接返回
        if (url.StartsWith("http://") || url.StartsWith("https://"))
        {
            return url;
        }
        
        // 如果设置了基础URL，拼接返回
        if (!string.IsNullOrEmpty(_baseUrl))
        {
            return _baseUrl.TrimEnd('/') + "/" + url.TrimStart('/');
        }
        
        // 否则直接返回
        return url;
    }
    
    /// <summary>
    /// 发送HTTP请求
    /// </summary>
    private async Task<HttpResponse> SendRequestAsync(UnityWebRequest request, Dictionary<string, string> headers, float timeout, int retryCount)
    {
        var response = new HttpResponse();
        
        try
        {
            // 设置超时
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
            
            // 等待完成
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            
            // 处理响应
            response.StatusCode = (int)request.responseCode;
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                response.Success = true;
                response.Data = request.downloadHandler.text;
                response.Bytes = request.downloadHandler.data;
            }
            else
            {
                response.Success = false;
                response.Error = $"{request.error} (状态码: {request.responseCode})";
                
                // 如果有重试次数，尝试重试
                if (retryCount > 0)
                {
                    Debug.LogWarning($"[HttpManager] 请求失败，尝试重试。剩余重试次数: {retryCount}");
                    request.Dispose();
                    
                    // 重新创建请求并重试
                    return await SendRequestAsync(
                        request, 
                        headers, 
                        timeout, 
                        retryCount - 1
                    );
                }
            }
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Error = $"网络请求异常: {e.Message}";
            Debug.LogError($"[HttpManager] 网络请求异常: {e.Message}");
        }
        finally
        {
            request?.Dispose();
        }
        
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