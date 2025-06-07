using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
    private const int DEFAULT_RETRY_COUNT = 1;
    
    // HTTP请求头
    private Dictionary<string, string> _defaultHeaders = new Dictionary<string, string>();
    
    // 基础URL
    private string _baseUrl = "";
    
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
    public async Task<HttpResponse> GetAsync(string url, Dictionary<string, string> headers = null, float timeout = DEFAULT_TIMEOUT, int retryCount = DEFAULT_RETRY_COUNT)
    {
        string fullUrl = GetFullUrl(url);
        return await SendRequestAsync(UnityWebRequest.Get(fullUrl), headers, timeout, retryCount);
    }
    
    /// <summary>
    /// 发送POST请求（JSON数据）
    /// </summary>
    public async Task<HttpResponse> PostJsonAsync(string url, object data, Dictionary<string, string> headers = null, float timeout = DEFAULT_TIMEOUT, int retryCount = DEFAULT_RETRY_COUNT)
    {
        string fullUrl = GetFullUrl(url);
        string jsonData = JsonUtility.ToJson(data);
        
        UnityWebRequest request = new UnityWebRequest(fullUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        // 添加JSON内容类型
        if (headers == null)
        {
            headers = new Dictionary<string, string>();
        }
        
        if (!headers.ContainsKey("Content-Type"))
        {
            headers.Add("Content-Type", "application/json");
        }
        
        return await SendRequestAsync(request, headers, timeout, retryCount);
    }
    
    /// <summary>
    /// 发送POST请求（表单数据）
    /// </summary>
    public async Task<HttpResponse> PostFormAsync(string url, Dictionary<string, string> formData, Dictionary<string, string> headers = null, float timeout = DEFAULT_TIMEOUT, int retryCount = DEFAULT_RETRY_COUNT)
    {
        string fullUrl = GetFullUrl(url);
        
        WWWForm form = new WWWForm();
        if (formData != null)
        {
            foreach (var pair in formData)
            {
                form.AddField(pair.Key, pair.Value);
            }
        }
        
        UnityWebRequest request = UnityWebRequest.Post(fullUrl, form);
        return await SendRequestAsync(request, headers, timeout, retryCount);
    }
    
    /// <summary>
    /// 下载文件
    /// </summary>
    public async Task<HttpResponse> DownloadFileAsync(string url, string filePath, Dictionary<string, string> headers = null, float timeout = DEFAULT_TIMEOUT, int retryCount = DEFAULT_RETRY_COUNT)
    {
        string fullUrl = GetFullUrl(url);
        
        UnityWebRequest request = UnityWebRequest.Get(fullUrl);
        request.downloadHandler = new DownloadHandlerFile(filePath);
        
        return await SendRequestAsync(request, headers, timeout, retryCount);
    }
    
    /// <summary>
    /// 上传文件
    /// </summary>
    public async Task<HttpResponse> UploadFileAsync(string url, string filePath, string fieldName = "file", Dictionary<string, string> formData = null, Dictionary<string, string> headers = null, float timeout = DEFAULT_TIMEOUT, int retryCount = DEFAULT_RETRY_COUNT)
    {
        string fullUrl = GetFullUrl(url);
        
        WWWForm form = new WWWForm();
        form.AddBinaryData(fieldName, System.IO.File.ReadAllBytes(filePath), System.IO.Path.GetFileName(filePath));
        
        if (formData != null)
        {
            foreach (var pair in formData)
            {
                form.AddField(pair.Key, pair.Value);
            }
        }
        
        UnityWebRequest request = UnityWebRequest.Post(fullUrl, form);
        return await SendRequestAsync(request, headers, timeout, retryCount);
    }
    
    /// <summary>
    /// 获取完整URL
    /// </summary>
    private string GetFullUrl(string url)
    {
        // 如果URL已经是完整URL，则直接返回
        if (url.StartsWith("http://") || url.StartsWith("https://"))
        {
            return url;
        }
        
        // 否则，拼接基础URL
        if (string.IsNullOrEmpty(_baseUrl))
        {
            Debug.LogWarning("[HttpManager] 基础URL为空，请先设置基础URL");
            return url;
        }
        
        // 确保URL正确拼接
        if (_baseUrl.EndsWith("/") && url.StartsWith("/"))
        {
            return _baseUrl + url.Substring(1);
        }
        else if (!_baseUrl.EndsWith("/") && !url.StartsWith("/"))
        {
            return _baseUrl + "/" + url;
        }
        else
        {
            return _baseUrl + url;
        }
    }
    
    /// <summary>
    /// 发送请求
    /// </summary>
    private async Task<HttpResponse> SendRequestAsync(UnityWebRequest request, Dictionary<string, string> headers, float timeout, int retryCount)
    {
        // 设置超时
        request.timeout = (int)timeout;
        
        // 添加默认请求头
        foreach (var header in _defaultHeaders)
        {
            request.SetRequestHeader(header.Key, header.Value);
        }
        
        // 添加自定义请求头
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
        }
        
        HttpResponse response = new HttpResponse();
        
        // 尝试发送请求，失败后重试
        for (int i = 0; i <= retryCount; i++)
        {
            try
            {
                // 发送请求
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                
                // 等待请求完成
                while (!operation.isDone)
                {
                    await Task.Delay(10);
                }
                
                // 处理响应
                if (request.result == UnityWebRequest.Result.Success)
                {
                    response.Success = true;
                    response.Data = request.downloadHandler.text;
                    response.Bytes = request.downloadHandler.data;
                    response.StatusCode = (int)request.responseCode;
                    break; // 成功则跳出循环
                }
                else
                {
                    // 记录错误
                    response.Success = false;
                    response.Error = request.error;
                    response.StatusCode = (int)request.responseCode;
                    
                    // 如果不是最后一次尝试，则等待后重试
                    if (i < retryCount)
                    {
                        Debug.LogWarning($"[HttpManager] 请求失败，将重试 ({i+1}/{retryCount}): {request.error}");
                        await Task.Delay(500); // 重试前等待500毫秒
                        continue;
                    }
                }
            }
            catch (Exception e)
            {
                // 捕获异常
                response.Success = false;
                response.Error = e.Message;
                
                // 如果不是最后一次尝试，则等待后重试
                if (i < retryCount)
                {
                    Debug.LogWarning($"[HttpManager] 请求异常，将重试 ({i+1}/{retryCount}): {e.Message}");
                    await Task.Delay(500); // 重试前等待500毫秒
                    continue;
                }
            }
        }
        
        // 处理请求
        request.Dispose();
        
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