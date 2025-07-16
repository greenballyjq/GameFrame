using System;
using System.Collections.Generic;
using System.IO;
using GameFramework.Core;
using ProtoBuf;
using UnityEngine;

namespace GameFramework.Network.Protocol
{
    /// <summary>
    /// Protobuf管理器 - 处理Protobuf序列化和反序列化 (protobuf-net版本)
    /// </summary>
    public class ProtobufManager : IManager
    {
        // 协议类型映射表
        private Dictionary<int, Type> _protocolTypes = new Dictionary<int, Type>();
        
        // 协议ID映射表
        private Dictionary<Type, int> _protocolIds = new Dictionary<Type, int>();
        
        // 协议序列化器缓存
        private Dictionary<Type, IProtocolSerializer> _serializers = new Dictionary<Type, IProtocolSerializer>();
        
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            Debug.Log("[ProtobufManager] 初始化Protobuf管理器 (protobuf-net版本)");
            
            // 注册所有协议类型
            RegisterProtocolTypes();
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
            Debug.Log("[ProtobufManager] 释放Protobuf管理器");
            _protocolTypes.Clear();
            _protocolIds.Clear();
            _serializers.Clear();
        }
        
        /// <summary>
        /// 注册协议类型 (protobuf-net版本)
        /// </summary>
        /// <typeparam name="T">协议类型</typeparam>
        /// <param name="protocolId">协议ID</param>
        public void RegisterProtocol<T>(int protocolId) where T : class, new()
        {
            Type type = typeof(T);
            
            if (_protocolTypes.ContainsKey(protocolId))
            {
                Debug.LogWarning($"[ProtobufManager] 协议ID {protocolId} 已注册，类型：{_protocolTypes[protocolId].Name}");
                return;
            }
            
            if (_protocolIds.ContainsKey(type))
            {
                Debug.LogWarning($"[ProtobufManager] 协议类型 {type.Name} 已注册，ID：{_protocolIds[type]}");
                return;
            }
            
            _protocolTypes[protocolId] = type;
            _protocolIds[type] = protocolId;
            
            // 创建序列化器
            var serializer = new ProtocolSerializer<T>();
            _serializers[type] = serializer;
            
            Debug.Log($"[ProtobufManager] 注册协议：{type.Name} -> ID:{protocolId}");
        }
        
        /// <summary>
        /// 序列化消息 (protobuf-net版本)
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="message">消息对象</param>
        /// <returns>序列化后的字节数组</returns>
        public byte[] Serialize<T>(T message) where T : class
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            
            try
            {
                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, message);
                    return stream.ToArray();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProtobufManager] 序列化消息失败：{e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 反序列化消息 (protobuf-net版本)
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="data">字节数组</param>
        /// <returns>反序列化后的消息对象</returns>
        public T Deserialize<T>(byte[] data) where T : class
        {
            if (data == null || data.Length == 0)
            {
                throw new ArgumentException("数据不能为空");
            }
            
            try
            {
                using (var stream = new MemoryStream(data))
                {
                    return Serializer.Deserialize<T>(stream);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProtobufManager] 反序列化消息失败：{e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 根据协议ID反序列化消息
        /// </summary>
        /// <param name="protocolId">协议ID</param>
        /// <param name="data">字节数组</param>
        /// <returns>反序列化后的消息对象</returns>
        public object Deserialize(int protocolId, byte[] data)
        {
            if (!_protocolTypes.TryGetValue(protocolId, out Type type))
            {
                throw new ArgumentException($"未注册的协议ID：{protocolId}");
            }
            
            if (!_serializers.TryGetValue(type, out IProtocolSerializer serializer))
            {
                throw new InvalidOperationException($"协议类型 {type.Name} 的序列化器未找到");
            }
            
            try
            {
                return serializer.Deserialize(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProtobufManager] 反序列化协议ID {protocolId} 失败：{e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 获取协议ID
        /// </summary>
        /// <param name="type">协议类型</param>
        /// <returns>协议ID</returns>
        public int GetProtocolId(Type type)
        {
            return _protocolIds.TryGetValue(type, out int id) ? id : -1;
        }
        
        /// <summary>
        /// 获取协议ID
        /// </summary>
        /// <typeparam name="T">协议类型</typeparam>
        /// <returns>协议ID</returns>
        public int GetProtocolId<T>() where T : class
        {
            return GetProtocolId(typeof(T));
        }
        
        /// <summary>
        /// 获取协议类型
        /// </summary>
        /// <param name="protocolId">协议ID</param>
        /// <returns>协议类型</returns>
        public Type GetProtocolType(int protocolId)
        {
            return _protocolTypes.TryGetValue(protocolId, out Type type) ? type : null;
        }
        
        /// <summary>
        /// 创建协议包
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="message">消息对象</param>
        /// <returns>协议包</returns>
        public byte[] CreateProtocolPacket<T>(T message) where T : class
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            
            int protocolId = GetProtocolId<T>();
            if (protocolId == -1)
            {
                throw new InvalidOperationException($"协议类型 {typeof(T).Name} 未注册");
            }
            
            byte[] messageData = Serialize(message);
            
            // 创建协议包：[协议ID(4字节)] + [数据长度(4字节)] + [消息数据]
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(protocolId);
                writer.Write(messageData.Length);
                writer.Write(messageData);
                
                return stream.ToArray();
            }
        }
        
        /// <summary>
        /// 解析协议包
        /// </summary>
        /// <param name="packet">协议包数据</param>
        /// <returns>协议包信息</returns>
        public ProtocolPacket ParseProtocolPacket(byte[] packet)
        {
            if (packet == null || packet.Length < 8) // 至少需要8字节（协议ID + 数据长度）
            {
                throw new ArgumentException("协议包数据无效");
            }
            
            try
            {
                using (var stream = new MemoryStream(packet))
                using (var reader = new BinaryReader(stream))
                {
                    int protocolId = reader.ReadInt32();
                    int dataLength = reader.ReadInt32();
                    
                    if (dataLength < 0 || dataLength > packet.Length - 8)
                    {
                        throw new ArgumentException($"协议包数据长度无效：{dataLength}");
                    }
                    
                    byte[] messageData = reader.ReadBytes(dataLength);
                    
                    // 反序列化消息
                    object message = Deserialize(protocolId, messageData);
                    
                    return new ProtocolPacket
                    {
                        ProtocolId = protocolId,
                        Data = messageData,
                        Message = message
                    };
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProtobufManager] 解析协议包失败：{e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 注册所有协议类型
        /// </summary>
        private void RegisterProtocolTypes()
        {
            Debug.Log("[ProtobufManager] 注册协议类型 (protobuf-net版本)");
            
            // 注册新的protobuf-net协议
            try
            {
                // 注册基础协议
                RegisterProtocol<GameFramework.Network.Protocol.PlayerBase>(1001);
                RegisterProtocol<GameFramework.Network.Protocol.C2S_Message>(1002);
                RegisterProtocol<GameFramework.Network.Protocol.S2C_Message>(1003);
                
                Debug.Log("[ProtobufManager] protobuf-net协议类型注册完成");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProtobufManager] 注册协议类型失败: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// 协议序列化器接口
    /// </summary>
    public interface IProtocolSerializer
    {
        byte[] Serialize(object message);
        object Deserialize(byte[] data);
    }
    
    /// <summary>
    /// 协议序列化器实现 (protobuf-net版本)
    /// </summary>
    /// <typeparam name="T">协议类型</typeparam>
    public class ProtocolSerializer<T> : IProtocolSerializer where T : class, new()
    {
        public byte[] Serialize(object message)
        {
            if (message is T protocolMessage)
            {
                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, protocolMessage);
                    return stream.ToArray();
                }
            }
            
            throw new ArgumentException($"消息类型不匹配，期望 {typeof(T).Name}");
        }
        
        public object Deserialize(byte[] data)
        {
            try
            {
                using (var stream = new MemoryStream(data))
                {
                    return Serializer.Deserialize<T>(stream);
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"反序列化 {typeof(T).Name} 失败：{e.Message}", e);
            }
        }
    }
    
    /// <summary>
    /// 协议包信息
    /// </summary>
    public class ProtocolPacket
    {
        public int ProtocolId { get; set; }
        public byte[] Data { get; set; }
        public object Message { get; set; }
    }
}
