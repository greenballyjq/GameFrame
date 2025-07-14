using System;
using System.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Utility;
using UnityEngine;


    /// <summary>
    /// 握手状态枚举
    /// </summary>
    public enum HandshakeState
    {
        None,           // 未开始
        WaitingChallenge,   // 等待挑战码
        SendingClientKey,   // 发送客户端密钥
        WaitingServerKey,   // 等待服务器密钥
        SendingResponse,    // 发送响应
        Completed,      // 握手完成
        Failed          // 握手失败
    }
    
    /// <summary>
    /// 握手管理器 - 处理与服务器的握手验证流程
    /// </summary>
    public class HandshakeManager : IManager
    {
        // 握手状态
        public HandshakeState State { get; private set; } = HandshakeState.None;
        
        // 握手数据
        private byte[] _challenge;          // 服务器挑战码
        private byte[] _clientPrivateKey;   // 客户端私钥
        private byte[] _clientPublicKey;    // 客户端公钥
        private byte[] _serverPublicKey;    // 服务器公钥
        private byte[] _sharedSecret;       // 共享密钥
        
        // 握手超时时间（秒）
        private const float HANDSHAKE_TIMEOUT = 30f;
        
        // 握手开始时间
        private float _handshakeStartTime;
        
        // 握手完成回调
        public event Action<bool, byte[]> OnHandshakeCompleted;
        
        /// <summary>
        /// 获取共享密钥
        /// </summary>
        public byte[] SharedSecret => _sharedSecret;
        
        /// <summary>
        /// 是否握手完成
        /// </summary>
        public bool IsHandshakeCompleted => State == HandshakeState.Completed;
        
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            Debug.Log("[HandshakeManager] 初始化握手管理器");
            Reset();
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            // 检查握手超时
            if (State != HandshakeState.None && State != HandshakeState.Completed && State != HandshakeState.Failed)
            {
                if (Time.time - _handshakeStartTime > HANDSHAKE_TIMEOUT)
                {
                    Debug.LogError("[HandshakeManager] 握手超时");
                    SetState(HandshakeState.Failed);
                    OnHandshakeCompleted?.Invoke(false, null);
                }
            }
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Release()
        {
            Debug.Log("[HandshakeManager] 释放握手管理器");
            Reset();
        }
        
        /// <summary>
        /// 重置握手状态
        /// </summary>
        public void Reset()
        {
            State = HandshakeState.None;
            _challenge = null;
            _clientPrivateKey = null;
            _clientPublicKey = null;
            _serverPublicKey = null;
            _sharedSecret = null;
            _handshakeStartTime = 0f;
        }
        
        /// <summary>
        /// 开始握手流程
        /// </summary>
        /// <param name="onCompleted">握手完成回调</param>
        public void StartHandshake(Action<bool, byte[]> onCompleted = null)
        {
            if (State != HandshakeState.None)
            {
                Debug.LogWarning("[HandshakeManager] 握手已在进行中或已完成");
                return;
            }
            
            Debug.Log("[HandshakeManager] 开始握手流程");
            
            OnHandshakeCompleted = onCompleted;
            _handshakeStartTime = Time.time;
            
            // 生成客户端密钥对
            CryptoUtils.GenerateClientKeyPair(out _clientPublicKey, out _clientPrivateKey);
            
            SetState(HandshakeState.WaitingChallenge);
            
            Debug.Log($"[HandshakeManager] 客户端密钥对已生成，公钥: {CryptoUtils.BytesToHex(_clientPublicKey)}");
        }
        
        /// <summary>
        /// 处理服务器挑战码
        /// </summary>
        /// <param name="challengeBase64">Base64编码的挑战码</param>
        /// <returns>Base64编码的客户端公钥</returns>
        public string ProcessChallenge(string challengeBase64)
        {
            if (State != HandshakeState.WaitingChallenge)
            {
                Debug.LogError("[HandshakeManager] 握手状态错误，无法处理挑战码");
                return null;
            }
            
            try
            {
                // 解码挑战码
                _challenge = CryptoUtils.FromBase64(challengeBase64);
                
                if (_challenge.Length != 8)
                {
                    Debug.LogError("[HandshakeManager] 挑战码长度错误");
                    SetState(HandshakeState.Failed);
                    OnHandshakeCompleted?.Invoke(false, null);
                    return null;
                }
                
                Debug.Log($"[HandshakeManager] 收到挑战码: {CryptoUtils.BytesToHex(_challenge)}");
                
                SetState(HandshakeState.SendingClientKey);
                
                // 返回客户端公钥
                return CryptoUtils.ToBase64(_clientPublicKey);
            }
            catch (Exception e)
            {
                Debug.LogError($"[HandshakeManager] 处理挑战码失败: {e.Message}");
                SetState(HandshakeState.Failed);
                OnHandshakeCompleted?.Invoke(false, null);
                return null;
            }
        }
        
        /// <summary>
        /// 处理服务器公钥
        /// </summary>
        /// <param name="serverKeyBase64">Base64编码的服务器公钥</param>
        /// <returns>Base64编码的HMAC响应</returns>
        public string ProcessServerKey(string serverKeyBase64)
        {
            if (State != HandshakeState.WaitingServerKey)
            {
                Debug.LogError("[HandshakeManager] 握手状态错误，无法处理服务器密钥");
                return null;
            }
            
            try
            {
                // 解码服务器公钥
                _serverPublicKey = CryptoUtils.FromBase64(serverKeyBase64);
                
                if (_serverPublicKey.Length != 8)
                {
                    Debug.LogError("[HandshakeManager] 服务器公钥长度错误");
                    SetState(HandshakeState.Failed);
                    OnHandshakeCompleted?.Invoke(false, null);
                    return null;
                }
                
                Debug.Log($"[HandshakeManager] 收到服务器公钥: {CryptoUtils.BytesToHex(_serverPublicKey)}");
                
                // 计算共享密钥
                _sharedSecret = CryptoUtils.ComputeSharedSecret(_clientPrivateKey, _serverPublicKey);
                
                Debug.Log($"[HandshakeManager] 共享密钥计算完成: {CryptoUtils.BytesToHex(_sharedSecret)}");
                
                // 计算HMAC响应
                byte[] hmacResponse = CryptoUtils.ComputeHMAC(_challenge, _sharedSecret);
                
                Debug.Log($"[HandshakeManager] HMAC响应: {CryptoUtils.BytesToHex(hmacResponse)}");
                
                SetState(HandshakeState.SendingResponse);
                
                return CryptoUtils.ToBase64(hmacResponse);
            }
            catch (Exception e)
            {
                Debug.LogError($"[HandshakeManager] 处理服务器密钥失败: {e.Message}");
                SetState(HandshakeState.Failed);
                OnHandshakeCompleted?.Invoke(false, null);
                return null;
            }
        }
        
        /// <summary>
        /// 完成握手
        /// </summary>
        /// <param name="success">握手是否成功</param>
        public void CompleteHandshake(bool success)
        {
            if (State != HandshakeState.SendingResponse)
            {
                Debug.LogError("[HandshakeManager] 握手状态错误，无法完成握手");
                return;
            }
            
            if (success)
            {
                SetState(HandshakeState.Completed);
                Debug.Log("[HandshakeManager] 握手成功完成");
                OnHandshakeCompleted?.Invoke(true, _sharedSecret);
            }
            else
            {
                SetState(HandshakeState.Failed);
                Debug.LogError("[HandshakeManager] 握手失败");
                OnHandshakeCompleted?.Invoke(false, null);
            }
        }
        
        /// <summary>
        /// 设置握手状态
        /// </summary>
        /// <param name="newState">新状态</param>
        private void SetState(HandshakeState newState)
        {
            if (State != newState)
            {
                Debug.Log($"[HandshakeManager] 握手状态变更: {State} -> {newState}");
                State = newState;
            }
        }
        
        /// <summary>
        /// 准备等待服务器密钥
        /// </summary>
        public void PrepareWaitingServerKey()
        {
            if (State == HandshakeState.SendingClientKey)
            {
                SetState(HandshakeState.WaitingServerKey);
            }
        }
        
        /// <summary>
        /// 使用共享密钥加密数据
        /// </summary>
        /// <param name="data">要加密的数据</param>
        /// <returns>加密后的数据</returns>
        public byte[] EncryptData(byte[] data)
        {
            if (!IsHandshakeCompleted)
            {
                throw new InvalidOperationException("握手未完成，无法加密数据");
            }
            
            if (_sharedSecret == null)
            {
                throw new InvalidOperationException("共享密钥为空");
            }
            
            return CryptoUtils.DESEncrypt(data, _sharedSecret);
        }
        
        /// <summary>
        /// 使用共享密钥解密数据
        /// </summary>
        /// <param name="encryptedData">加密的数据</param>
        /// <returns>解密后的数据</returns>
        public byte[] DecryptData(byte[] encryptedData)
        {
            if (!IsHandshakeCompleted)
            {
                throw new InvalidOperationException("握手未完成，无法解密数据");
            }
            
            if (_sharedSecret == null)
            {
                throw new InvalidOperationException("共享密钥为空");
            }
            
            return CryptoUtils.DESDecrypt(encryptedData, _sharedSecret);
        }
        
        /// <summary>
        /// 加密字符串数据
        /// </summary>
        /// <param name="data">要加密的字符串</param>
        /// <returns>Base64编码的加密数据</returns>
        public string EncryptString(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return string.Empty;
            }
            
            byte[] dataBytes = CryptoUtils.StringToBytes(data);
            byte[] encryptedBytes = EncryptData(dataBytes);
            return CryptoUtils.ToBase64(encryptedBytes);
        }
        
        /// <summary>
        /// 解密字符串数据
        /// </summary>
        /// <param name="encryptedData">Base64编码的加密数据</param>
        /// <returns>解密后的字符串</returns>
        public string DecryptString(string encryptedData)
        {
            if (string.IsNullOrEmpty(encryptedData))
            {
                return string.Empty;
            }
            
            byte[] encryptedBytes = CryptoUtils.FromBase64(encryptedData);
            byte[] decryptedBytes = DecryptData(encryptedBytes);
            return CryptoUtils.BytesToString(decryptedBytes);
        }
    }
