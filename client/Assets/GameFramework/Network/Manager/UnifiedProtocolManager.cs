using System;
using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.Network.Protocol;
using UnityEngine;

namespace GameFramework.Network.Manager
{
    /// <summary>
    /// 统一协议管理器 - 整合协议抽象层和protobuf-net的管理器
    /// </summary>
    public class UnifiedProtocolManager : IManager
    {
        private ProtocolFactory _protocolFactory;
        
        // 协议ID常量定义（可以移到单独的常量类中）
        public static class ProtocolIds
        {
            public const int C2S_LOGIN = 1001;
            public const int S2C_LOGIN = 1002;
            public const int C2S_PING = 1003;
            public const int S2C_PING = 1004;
            public const int C2S_BASE = 1005;
            public const int S2C_BASE = 1006;
            public const int C2S_CLIENT_DATA = 1007;
            public const int S2C_CLIENT_DATA = 1008;
            public const int C2S_UPLOAD_CLIENT_DATA = 1009;
            public const int C2S_ITEM = 1010;
            public const int S2C_ITEM = 1011;
        }

        public void Init()
        {
            Debug.Log("[UnifiedProtocolManager] 初始化统一协议管理器");
            
            _protocolFactory = ProtocolFactory.Instance;
            
            // 可以在这里手动注册一些协议（如果自动注册没有工作）
            // RegisterProtocolsManually();
        }

        public void Update()
        {
            // 协议管理器通常不需要每帧更新
        }

        public void Release()
        {
            Debug.Log("[UnifiedProtocolManager] 释放统一协议管理器");
            _protocolFactory?.Clear();
        }

        /// <summary>
        /// 手动注册协议类型（备用方案）
        /// </summary>
        private void RegisterProtocolsManually()
        {
            // 这里可以手动注册一些协议类型作为备用方案
            Debug.Log("[UnifiedProtocolManager] 手动注册协议类型（如需要）");
        }

        /// <summary>
        /// 创建protobuf-net消息的适配器
        /// </summary>
        /// <param name="protobufNetMessage">protobuf-net消息对象</param>
        /// <param name="protocolId">协议ID</param>
        /// <param name="session">会话ID（可选）</param>
        /// <returns>协议消息适配器</returns>
        public IProtocolMessage CreateProtobufNetAdapter(object protobufNetMessage, int protocolId, ulong session = 0)
        {
            try
            {
                // 根据消息类型判断是C2S还是S2C
                var messageType = protobufNetMessage.GetType();
                var typeName = messageType.Name;

                if (typeName.StartsWith("C2S_"))
                {
                    return ProtobufNetAdapterFactory.CreateC2SAdapter(protobufNetMessage, protocolId, session);
                }
                else if (typeName.StartsWith("S2C_"))
                {
                    return ProtobufNetAdapterFactory.CreateS2CAdapter(protobufNetMessage, protocolId, session);
                }
                else
                {
                    // 默认创建通用适配器
                    return new ProtobufNetProtocolAdapter(protobufNetMessage, protocolId);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnifiedProtocolManager] 创建protobuf-net适配器失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 序列化协议消息
        /// </summary>
        /// <param name="message">协议消息</param>
        /// <returns>序列化后的字节数组</returns>
        public byte[] SerializeMessage(IProtocolMessage message)
        {
            if (message == null)
            {
                Debug.LogError("[UnifiedProtocolManager] 消息不能为null");
                return new byte[0];
            }

            try
            {
                return message.Serialize();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnifiedProtocolManager] 序列化消息失败: {ex.Message}");
                return new byte[0];
            }
        }

        /// <summary>
        /// 反序列化协议消息
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <param name="protocolId">协议ID</param>
        /// <returns>协议消息</returns>
        public IProtocolMessage DeserializeMessage(byte[] data, int protocolId)
        {
            if (data == null || data.Length == 0)
            {
                Debug.LogError("[UnifiedProtocolManager] 数据不能为空");
                return null;
            }

            try
            {
                var message = _protocolFactory.CreateMessage(protocolId);
                if (message != null)
                {
                    message.Deserialize(data);
                    return message;
                }
                else
                {
                    Debug.LogError($"[UnifiedProtocolManager] 无法创建协议ID {protocolId} 的消息实例");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnifiedProtocolManager] 反序列化消息失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取协议工厂实例
        /// </summary>
        /// <returns>协议工厂</returns>
        public ProtocolFactory GetProtocolFactory()
        {
            return _protocolFactory;
        }

        /// <summary>
        /// 获取所有已注册的协议信息
        /// </summary>
        /// <returns>协议信息列表</returns>
        public List<(int Id, Type Type)> GetAllRegisteredProtocols()
        {
            return _protocolFactory.GetAllProtocols();
        }

        /// <summary>
        /// 检查协议是否已注册
        /// </summary>
        /// <param name="protocolId">协议ID</param>
        /// <returns>是否已注册</returns>
        public bool IsProtocolRegistered(int protocolId)
        {
            return _protocolFactory.IsProtocolRegistered(protocolId);
        }

        /// <summary>
        /// 创建登录消息适配器
        /// </summary>
        /// <param name="account">账户名</param>
        /// <param name="session">会话ID</param>
        /// <returns>登录消息适配器</returns>
        public IProtocolMessage CreateLoginMessage(string account, ulong session = 1)
        {
            try
            {
                // 这里需要根据实际的protobuf-net消息类型来创建
                // 由于GameFramework中没有直接的protobuf-net依赖，我们使用反射
                var loginMessageType = Type.GetType("GameFramework.Network.Protocol.C2S_Login");
                if (loginMessageType != null)
                {
                    var loginMessage = Activator.CreateInstance(loginMessageType);
                    // 设置账户名（需要根据实际的属性名）
                    var accountProperty = loginMessageType.GetProperty("Account");
                    accountProperty?.SetValue(loginMessage, account);
                    
                    return CreateProtobufNetAdapter(loginMessage, ProtocolIds.C2S_LOGIN, session);
                }
                else
                {
                    Debug.LogWarning("[UnifiedProtocolManager] 未找到C2S_Login类型，请确保已正确引用协议库");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnifiedProtocolManager] 创建登录消息失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 创建Ping消息适配器
        /// </summary>
        /// <param name="clientTime">客户端时间戳</param>
        /// <param name="session">会话ID</param>
        /// <returns>Ping消息适配器</returns>
        public IProtocolMessage CreatePingMessage(ulong clientTime, ulong session = 1)
        {
            try
            {
                // 使用反射创建C2S_Ping消息
                var pingMessageType = Type.GetType("GameFramework.Network.Protocol.C2S_Ping");
                if (pingMessageType != null)
                {
                    var pingMessage = Activator.CreateInstance(pingMessageType);
                    // 设置客户端时间
                    var clientTimeProperty = pingMessageType.GetProperty("Clienttime");
                    clientTimeProperty?.SetValue(pingMessage, clientTime);
                    
                    return CreateProtobufNetAdapter(pingMessage, ProtocolIds.C2S_PING, session);
                }
                else
                {
                    Debug.LogWarning("[UnifiedProtocolManager] 未找到C2S_Ping类型，请确保已正确引用协议库");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnifiedProtocolManager] 创建Ping消息失败: {ex.Message}");
                return null;
            }
        }
    }
} 