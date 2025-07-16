using System;
using System.Reflection;
using UnityEngine;

namespace GameFramework.Network.Protocol
{
    /// <summary>
    /// 协议消息基类 - 提供协议消息的基础实现
    /// </summary>
    public abstract class ProtocolMessageBase : IProtocolMessage
    {
        private int? _cachedProtocolId;

        /// <summary>
        /// 协议ID（从类的ProtocolAttribute特性中获取）
        /// </summary>
        public virtual int ProtocolId
        {
            get
            {
                if (!_cachedProtocolId.HasValue)
                {
                    var protocolAttr = GetType().GetCustomAttribute<ProtocolAttribute>();
                    if (protocolAttr != null)
                    {
                        _cachedProtocolId = protocolAttr.Id;
                    }
                    else
                    {
                        Debug.LogWarning($"[ProtocolMessageBase] 类 {GetType().Name} 缺少 ProtocolAttribute 特性");
                        _cachedProtocolId = 0;
                    }
                }
                return _cachedProtocolId.Value;
            }
        }

        /// <summary>
        /// 序列化到字节数组（需要子类实现具体序列化逻辑）
        /// </summary>
        /// <returns>序列化后的字节数组</returns>
        public abstract byte[] Serialize();

        /// <summary>
        /// 从字节数组反序列化（需要子类实现具体反序列化逻辑）
        /// </summary>
        /// <param name="data">字节数组</param>
        public abstract void Deserialize(byte[] data);

        /// <summary>
        /// 获取协议名称（用于调试）
        /// </summary>
        public virtual string GetProtocolName()
        {
            return GetType().Name;
        }

        /// <summary>
        /// 验证协议消息（可在子类中重写）
        /// </summary>
        /// <returns>验证结果</returns>
        public virtual bool Validate()
        {
            return true;
        }
    }

    /// <summary>
    /// 客户端到服务器消息基类
    /// </summary>
    public abstract class C2SMessageBase : ProtocolMessageBase, IC2SMessage
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public ulong Session { get; set; }

        protected C2SMessageBase()
        {
            Session = 0;
        }

        protected C2SMessageBase(ulong session)
        {
            Session = session;
        }
    }

    /// <summary>
    /// 服务器到客户端消息基类
    /// </summary>
    public abstract class S2CMessageBase : ProtocolMessageBase, IS2CMessage
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public ulong Session { get; set; }

        protected S2CMessageBase()
        {
            Session = 0;
        }

        protected S2CMessageBase(ulong session)
        {
            Session = session;
        }
    }
} 