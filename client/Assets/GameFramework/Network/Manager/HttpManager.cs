using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Manager;
using GameFramework.Utility;
using GameFramework.Network.Protocol;
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
    
    // Protobuf管理器引用
    private GameFramework.Network.Protocol.ProtobufManager _protobufManager;
    
    // 握手管理器引用
    private HandshakeManager _handshakeManager;
    
    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        Debug.Log("[HttpManager] 初始化HTTP管理器");
        
        // 设置默认请求头
        _defaultHeaders.Clear();
        _defaultHeaders.Add("Content-Type", "application/json");
        
        // 获取Protobuf管理器引用
        _protobufManager = GameFrameworkManager.Instance.GetManager<GameFramework.Network.Protocol.ProtobufManager>();
        
        // 获取握手管理器引用
        _handshakeManager = GameFrameworkManager.Instance.GetManager<HandshakeManager>();
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
    
    /// <summary>
    /// 发送Protobuf POST请求
    /// </summary>
    /// <typeparam name="TRequest">请求协议类型</typeparam>
    /// <typeparam name="TResponse">响应协议类型</typeparam>
    /// <param name="url">请求URL</param>
    /// <param name="request">请求协议对象</param>
    /// <param name="headers">自定义请求头</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="retryCount">重试次数</param>
    /// <returns>Protobuf响应对象</returns>
    public async Task<ProtobufResponse<TResponse>> PostProtobufAsync<TRequest, TResponse>(
        string url, 
        TRequest request, 
        Dictionary<string, string> headers = null, 
        float timeout = DEFAULT_TIMEOUT, 
        int retryCount = DEFAULT_RETRY_COUNT)
        where TRequest : class
        where TResponse : class, new()
    {
        if (_protobufManager == null)
        {
            Debug.LogError("[HttpManager] Protobuf管理器未初始化");
            return new ProtobufResponse<TResponse> { Success = false, Error = "Protobuf管理器未初始化" };
        }
        
        // 序列化请求对象
        byte[] requestData = _protobufManager.Serialize(request);
        if (requestData == null)
        {
            return new ProtobufResponse<TResponse> { Success = false, Error = "请求对象序列化失败" };
        }
        
        string fullUrl = GetFullUrl(url);
        UnityWebRequest webRequest = new UnityWebRequest(fullUrl, "POST");
        webRequest.uploadHandler = new UploadHandlerRaw(requestData);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        
        // 设置Protobuf内容类型
        if (headers == null)
        {
            headers = new Dictionary<string, string>();
        }
        
        if (!headers.ContainsKey("Content-Type"))
        {
            headers.Add("Content-Type", "application/x-protobuf");
        }
        
        // 发送HTTP请求
        HttpResponse httpResponse = await SendRequestAsync(webRequest, headers, timeout, retryCount);
        
        // 创建Protobuf响应
        ProtobufResponse<TResponse> protobufResponse = new ProtobufResponse<TResponse>
        {
            Success = httpResponse.Success,
            Error = httpResponse.Error,
            StatusCode = httpResponse.StatusCode,
            RawData = httpResponse.Bytes
        };
        
        // 如果请求成功，反序列化响应数据
        if (httpResponse.Success && httpResponse.Bytes != null)
        {
            TResponse responseData = _protobufManager.Deserialize<TResponse>(httpResponse.Bytes);
            if (responseData != null)
            {
                protobufResponse.Data = responseData;
            }
            else
            {
                protobufResponse.Success = false;
                protobufResponse.Error = "响应数据反序列化失败";
            }
        }
        
        return protobufResponse;
    }
    
    /// <summary>
    /// 发送Protobuf协议包请求
    /// </summary>
    /// <typeparam name="TRequest">请求协议类型</typeparam>
    /// <typeparam name="TResponse">响应协议类型</typeparam>
    /// <param name="url">请求URL</param>
    /// <param name="request">请求协议对象</param>
    /// <param name="headers">自定义请求头</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="retryCount">重试次数</param>
    /// <returns>Protobuf响应对象</returns>
    public async Task<ProtobufResponse<TResponse>> PostProtobufPacketAsync<TRequest, TResponse>(
        string url, 
        TRequest request, 
        Dictionary<string, string> headers = null, 
        float timeout = DEFAULT_TIMEOUT, 
        int retryCount = DEFAULT_RETRY_COUNT)
        where TRequest : class
        where TResponse : class, new()
    {
        if (_protobufManager == null)
        {
            Debug.LogError("[HttpManager] Protobuf管理器未初始化");
            return new ProtobufResponse<TResponse> { Success = false, Error = "Protobuf管理器未初始化" };
        }
        
        // 创建协议包
        byte[] packetData = _protobufManager.CreateProtocolPacket(request);
        if (packetData == null)
        {
            return new ProtobufResponse<TResponse> { Success = false, Error = "协议包创建失败" };
        }
        
        string fullUrl = GetFullUrl(url);
        UnityWebRequest webRequest = new UnityWebRequest(fullUrl, "POST");
        webRequest.uploadHandler = new UploadHandlerRaw(packetData);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        
        // 设置协议包内容类型
        if (headers == null)
        {
            headers = new Dictionary<string, string>();
        }
        
        if (!headers.ContainsKey("Content-Type"))
        {
            headers.Add("Content-Type", "application/x-protobuf-packet");
        }
        
        // 发送HTTP请求
        HttpResponse httpResponse = await SendRequestAsync(webRequest, headers, timeout, retryCount);
        
        // 创建Protobuf响应
        ProtobufResponse<TResponse> protobufResponse = new ProtobufResponse<TResponse>
        {
            Success = httpResponse.Success,
            Error = httpResponse.Error,
            StatusCode = httpResponse.StatusCode,
            RawData = httpResponse.Bytes
        };
        
        // 如果请求成功，解析响应协议包
        if (httpResponse.Success && httpResponse.Bytes != null)
        {
            ProtocolPacket packet = _protobufManager.ParseProtocolPacket(httpResponse.Bytes);
            if (packet != null && packet.Message is TResponse responseData)
            {
                protobufResponse.Data = responseData;
                protobufResponse.ProtocolId = packet.ProtocolId;
            }
            else
            {
                protobufResponse.Success = false;
                protobufResponse.Error = "响应协议包解析失败";
            }
        }
        
        return protobufResponse;
    }
    
    #region 服务器通讯相关方法
    
    /// <summary>
    /// 执行握手流程
    /// </summary>
    /// <param name="serverUrl">服务器URL</param>
    /// <param name="timeout">超时时间</param>
    /// <returns>握手结果</returns>
    public async Task<ServerHandshakeResult> PerformHandshakeAsync(string serverUrl, float timeout = DEFAULT_TIMEOUT)
    {
        if (_handshakeManager == null)
        {
            Debug.LogError("[HttpManager] 握手管理器未初始化");
            return new ServerHandshakeResult { Success = false, Error = "握手管理器未初始化" };
        }
        
        try
        {
            // 重置握手状态
            _handshakeManager.Reset();
            
            // 开始握手流程
            TaskCompletionSource<ServerHandshakeResult> tcs = new TaskCompletionSource<ServerHandshakeResult>();
            
            _handshakeManager.StartHandshake((success, sharedSecret) =>
            {
                tcs.SetResult(new ServerHandshakeResult 
                { 
                    Success = success, 
                    SharedSecret = sharedSecret,
                    Error = success ? null : "握手失败"
                });
            });
            
            // 步骤1: 等待服务器挑战码
            HttpResponse challengeResponse = await GetAsync(serverUrl + "/challenge", timeout: timeout);
            if (!challengeResponse.Success)
            {
                return new ServerHandshakeResult { Success = false, Error = $"获取挑战码失败: {challengeResponse.Error}" };
            }
            
            // 步骤2: 处理挑战码并发送客户端密钥
            string clientKeyBase64 = _handshakeManager.ProcessChallenge(challengeResponse.Data);
            if (string.IsNullOrEmpty(clientKeyBase64))
            {
                return new ServerHandshakeResult { Success = false, Error = "处理挑战码失败" };
            }
            
            HttpResponse clientKeyResponse = await PostAsync(serverUrl + "/client-key", clientKeyBase64, timeout: timeout);
            if (!clientKeyResponse.Success)
            {
                return new ServerHandshakeResult { Success = false, Error = $"发送客户端密钥失败: {clientKeyResponse.Error}" };
            }
            
            // 步骤3: 准备等待服务器密钥
            _handshakeManager.PrepareWaitingServerKey();
            
            // 步骤4: 处理服务器密钥并发送HMAC响应
            string hmacResponseBase64 = _handshakeManager.ProcessServerKey(clientKeyResponse.Data);
            if (string.IsNullOrEmpty(hmacResponseBase64))
            {
                return new ServerHandshakeResult { Success = false, Error = "处理服务器密钥失败" };
            }
            
            HttpResponse hmacResponse = await PostAsync(serverUrl + "/hmac-response", hmacResponseBase64, timeout: timeout);
            if (!hmacResponse.Success)
            {
                return new ServerHandshakeResult { Success = false, Error = $"发送HMAC响应失败: {hmacResponse.Error}" };
            }
            
            // 步骤5: 完成握手
            bool handshakeSuccess = hmacResponse.Data.Trim().ToLower() == "ok";
            _handshakeManager.CompleteHandshake(handshakeSuccess);
            
            return await tcs.Task;
        }
        catch (Exception e)
        {
            Debug.LogError($"[HttpManager] 握手流程异常: {e.Message}");
            return new ServerHandshakeResult { Success = false, Error = $"握手流程异常: {e.Message}" };
        }
    }
    
    /// <summary>
    /// 发送加密的Protobuf消息
    /// </summary>
    /// <typeparam name="TRequest">请求消息类型</typeparam>
    /// <typeparam name="TResponse">响应消息类型</typeparam>
    /// <param name="url">请求URL</param>
    /// <param name="request">请求消息</param>
    /// <param name="timeout">超时时间</param>
    /// <returns>加密通讯响应</returns>
    public async Task<EncryptedServerResponse<TResponse>> SendEncryptedProtobufAsync<TRequest, TResponse>(
        string url, 
        TRequest request, 
        float timeout = DEFAULT_TIMEOUT)
        where TRequest : class
        where TResponse : class, new()
    {
        if (_handshakeManager == null)
        {
            Debug.LogError("[HttpManager] 握手管理器未初始化");
            return new EncryptedServerResponse<TResponse> { Success = false, Error = "握手管理器未初始化" };
        }
        
        if (!_handshakeManager.IsHandshakeCompleted)
        {
            Debug.LogError("[HttpManager] 握手未完成，无法发送加密消息");
            return new EncryptedServerResponse<TResponse> { Success = false, Error = "握手未完成" };
        }
        
        if (_protobufManager == null)
        {
            Debug.LogError("[HttpManager] Protobuf管理器未初始化");
            return new EncryptedServerResponse<TResponse> { Success = false, Error = "Protobuf管理器未初始化" };
        }
        
        try
        {
            // 序列化请求消息
            byte[] requestData = _protobufManager.Serialize(request);
            if (requestData == null)
            {
                return new EncryptedServerResponse<TResponse> { Success = false, Error = "请求消息序列化失败" };
            }
            
            // 加密请求数据
            byte[] encryptedData = _handshakeManager.EncryptData(requestData);
            string encryptedBase64 = CryptoUtils.ToBase64(encryptedData);
            
            // 发送加密数据
            HttpResponse httpResponse = await PostAsync(url, encryptedBase64, timeout: timeout);
            if (!httpResponse.Success)
            {
                return new EncryptedServerResponse<TResponse> 
                { 
                    Success = false, 
                    Error = $"HTTP请求失败: {httpResponse.Error}",
                    StatusCode = httpResponse.StatusCode
                };
            }
            
            // 解密响应数据
            byte[] encryptedResponseData = CryptoUtils.FromBase64(httpResponse.Data);
            byte[] decryptedResponseData = _handshakeManager.DecryptData(encryptedResponseData);
            
            // 反序列化响应消息
            TResponse responseMessage = _protobufManager.Deserialize<TResponse>(decryptedResponseData);
            if (responseMessage == null)
            {
                return new EncryptedServerResponse<TResponse> { Success = false, Error = "响应消息反序列化失败" };
            }
            
            return new EncryptedServerResponse<TResponse>
            {
                Success = true,
                Data = responseMessage,
                StatusCode = httpResponse.StatusCode
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"[HttpManager] 发送加密消息异常: {e.Message}");
            return new EncryptedServerResponse<TResponse> { Success = false, Error = $"发送加密消息异常: {e.Message}" };
        }
    }
    
    /// <summary>
    /// 发送加密的文本消息
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="message">文本消息</param>
    /// <param name="timeout">超时时间</param>
    /// <returns>加密通讯响应</returns>
    public async Task<EncryptedServerResponse<string>> SendEncryptedTextAsync(
        string url, 
        string message, 
        float timeout = DEFAULT_TIMEOUT)
    {
        if (_handshakeManager == null)
        {
            Debug.LogError("[HttpManager] 握手管理器未初始化");
            return new EncryptedServerResponse<string> { Success = false, Error = "握手管理器未初始化" };
        }
        
        if (!_handshakeManager.IsHandshakeCompleted)
        {
            Debug.LogError("[HttpManager] 握手未完成，无法发送加密消息");
            return new EncryptedServerResponse<string> { Success = false, Error = "握手未完成" };
        }
        
        try
        {
            // 加密文本消息
            string encryptedMessage = _handshakeManager.EncryptString(message);
            
            // 发送加密数据
            HttpResponse httpResponse = await PostAsync(url, encryptedMessage, timeout: timeout);
            if (!httpResponse.Success)
            {
                return new EncryptedServerResponse<string> 
                { 
                    Success = false, 
                    Error = $"HTTP请求失败: {httpResponse.Error}",
                    StatusCode = httpResponse.StatusCode
                };
            }
            
            // 解密响应数据
            string decryptedResponse = _handshakeManager.DecryptString(httpResponse.Data);
            
            return new EncryptedServerResponse<string>
            {
                Success = true,
                Data = decryptedResponse,
                StatusCode = httpResponse.StatusCode
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"[HttpManager] 发送加密文本消息异常: {e.Message}");
            return new EncryptedServerResponse<string> { Success = false, Error = $"发送加密文本消息异常: {e.Message}" };
        }
    }
    
    /// <summary>
    /// 发送POST请求（文本数据）
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="data">文本数据</param>
    /// <param name="headers">自定义请求头</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="retryCount">重试次数</param>
    /// <returns>HTTP响应</returns>
    public async Task<HttpResponse> PostAsync(string url, string data, Dictionary<string, string> headers = null, float timeout = DEFAULT_TIMEOUT, int retryCount = DEFAULT_RETRY_COUNT)
    {
        string fullUrl = GetFullUrl(url);
        
        UnityWebRequest request = new UnityWebRequest(fullUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        // 添加文本内容类型
        if (headers == null)
        {
            headers = new Dictionary<string, string>();
        }
        
        if (!headers.ContainsKey("Content-Type"))
        {
            headers.Add("Content-Type", "text/plain");
        }
        
        return await SendRequestAsync(request, headers, timeout, retryCount);
    }
    
    #endregion
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
    
    // 协议ID（用于协议包模式）
    public int ProtocolId { get; set; } = -1;
    
    // 响应数据（已反序列化的Protobuf对象）
    public T Data { get; set; } = default(T);

    // 原始响应数据（字节数组）
    public byte[] RawData { get; set; } = null;
}

/// <summary>
/// 服务器握手结果
/// </summary>
public class ServerHandshakeResult
{
    // 握手是否成功
    public bool Success { get; set; } = false;
    
    // 错误信息
    public string Error { get; set; } = "";
    
    // 共享密钥
    public byte[] SharedSecret { get; set; } = null;
}

/// <summary>
/// 加密服务器响应
/// </summary>
/// <typeparam name="T">响应数据类型</typeparam>
public class EncryptedServerResponse<T>
{
    // 请求是否成功
    public bool Success { get; set; } = false;
    
    // 错误信息
    public string Error { get; set; } = "";
    
    // 状态码
    public int StatusCode { get; set; } = 0;
    
    // 响应数据（已解密的数据）
    public T Data { get; set; } = default(T);
} 