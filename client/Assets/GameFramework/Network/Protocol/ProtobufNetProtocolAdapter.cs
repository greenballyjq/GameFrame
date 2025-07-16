using System;
using System.IO;
using UnityEngine;

namespace GameFramework.Network.Protocol
{
    /// <summary>
    /// protobuf-net协议适配器 - 连接外部protobuf-net消息与GameFramework协议抽象层
    /// </summary>
    public class ProtobufNetProtocolAdapter : ProtocolMessageBase
    {
        private object _protobufNetMessage;
        private Type _protobufNetType;
        private int _protocolId;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="protobufNetMessage">protobuf-net消息对象</param>
        /// <param name="protocolId">协议ID</param>
        public ProtobufNetProtocolAdapter(object protobufNetMessage, int protocolId)
        {
            _protobufNetMessage = protobufNetMessage ?? throw new ArgumentNullException(nameof(protobufNetMessage));
            _protobufNetType = protobufNetMessage.GetType();
            _protocolId = protocolId;
        }

        /// <summary>
        /// 协议ID
        /// </summary>
        public override int ProtocolId => _protocolId;

        /// <summary>
        /// 获取包装的protobuf-net消息
        /// </summary>
        /// <typeparam name="T">protobuf-net消息类型</typeparam>
        /// <returns>protobuf-net消息对象</returns>
        public T GetProtobufNetMessage<T>() where T : class
        {
            return _protobufNetMessage as T;
        }

        /// <summary>
        /// 获取包装的protobuf-net消息
        /// </summary>
        /// <returns>protobuf-net消息对象</returns>
        public object GetProtobufNetMessage()
        {
            return _protobufNetMessage;
        }

        /// <summary>
        /// 序列化到字节数组（使用protobuf-net序列化）
        /// </summary>
        /// <returns>序列化后的字节数组</returns>
        public override byte[] Serialize()
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    // 使用反射调用protobuf-net的Serializer.Serialize方法
                    var serializerType = Type.GetType("ProtoBuf.Serializer, protobuf-net");
                    if (serializerType != null)
                    {
                        var serializeMethod = serializerType.GetMethod("Serialize", new[] { typeof(Stream), typeof(object) });
                        serializeMethod?.Invoke(null, new[] { stream, _protobufNetMessage });
                        return stream.ToArray();
                    }
                    else
                    {
                        Debug.LogError("[ProtobufNetProtocolAdapter] 未找到protobuf-net序列化器");
                        return new byte[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProtobufNetProtocolAdapter] 序列化失败: {ex.Message}");
                return new byte[0];
            }
        }

        /// <summary>
        /// 从字节数组反序列化（使用protobuf-net反序列化）
        /// </summary>
        /// <param name="data">字节数组</param>
        public override void Deserialize(byte[] data)
        {
            try
            {
                using (var stream = new MemoryStream(data))
                {
                    // 使用反射调用protobuf-net的Serializer.Deserialize方法
                    var serializerType = Type.GetType("ProtoBuf.Serializer, protobuf-net");
                    if (serializerType != null)
                    {
                        var deserializeMethod = serializerType.GetMethod("Deserialize", new[] { typeof(Stream), typeof(Type) });
                        var result = deserializeMethod?.Invoke(null, new object[] { stream, _protobufNetType });
                        if (result != null)
                        {
                            _protobufNetMessage = result;
                        }
                    }
                    else
                    {
                        Debug.LogError("[ProtobufNetProtocolAdapter] 未找到protobuf-net反序列化器");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProtobufNetProtocolAdapter] 反序列化失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 客户端到服务器protobuf-net适配器
    /// </summary>
    public class C2SProtobufNetAdapter : C2SMessageBase
    {
        private ProtobufNetProtocolAdapter _adapter;

        public C2SProtobufNetAdapter(object protobufNetMessage, int protocolId, ulong session = 0) : base(session)
        {
            _adapter = new ProtobufNetProtocolAdapter(protobufNetMessage, protocolId);
        }

        public override int ProtocolId => _adapter.ProtocolId;

        public T GetProtobufNetMessage<T>() where T : class
        {
            return _adapter.GetProtobufNetMessage<T>();
        }

        public object GetProtobufNetMessage()
        {
            return _adapter.GetProtobufNetMessage();
        }

        public override byte[] Serialize()
        {
            return _adapter.Serialize();
        }

        public override void Deserialize(byte[] data)
        {
            _adapter.Deserialize(data);
        }
    }

    /// <summary>
    /// 服务器到客户端protobuf-net适配器
    /// </summary>
    public class S2CProtobufNetAdapter : S2CMessageBase
    {
        private ProtobufNetProtocolAdapter _adapter;

        public S2CProtobufNetAdapter(object protobufNetMessage, int protocolId, ulong session = 0) : base(session)
        {
            _adapter = new ProtobufNetProtocolAdapter(protobufNetMessage, protocolId);
        }

        public override int ProtocolId => _adapter.ProtocolId;

        public T GetProtobufNetMessage<T>() where T : class
        {
            return _adapter.GetProtobufNetMessage<T>();
        }

        public object GetProtobufNetMessage()
        {
            return _adapter.GetProtobufNetMessage();
        }

        public override byte[] Serialize()
        {
            return _adapter.Serialize();
        }

        public override void Deserialize(byte[] data)
        {
            _adapter.Deserialize(data);
        }
    }

    /// <summary>
    /// protobuf-net适配器工厂
    /// </summary>
    public static class ProtobufNetAdapterFactory
    {
        /// <summary>
        /// 创建C2S适配器
        /// </summary>
        /// <param name="protobufNetMessage">protobuf-net消息</param>
        /// <param name="protocolId">协议ID</param>
        /// <param name="session">会话ID</param>
        /// <returns>C2S适配器</returns>
        public static C2SProtobufNetAdapter CreateC2SAdapter(object protobufNetMessage, int protocolId, ulong session = 0)
        {
            return new C2SProtobufNetAdapter(protobufNetMessage, protocolId, session);
        }

        /// <summary>
        /// 创建S2C适配器
        /// </summary>
        /// <param name="protobufNetMessage">protobuf-net消息</param>
        /// <param name="protocolId">协议ID</param>
        /// <param name="session">会话ID</param>
        /// <returns>S2C适配器</returns>
        public static S2CProtobufNetAdapter CreateS2CAdapter(object protobufNetMessage, int protocolId, ulong session = 0)
        {
            return new S2CProtobufNetAdapter(protobufNetMessage, protocolId, session);
        }
    }
} 