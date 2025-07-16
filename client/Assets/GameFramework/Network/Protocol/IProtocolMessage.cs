using System;

namespace GameFramework.Network.Protocol
{
    /// <summary>
    /// 协议消息接口 - 所有协议消息的基接口
    /// </summary>
    public interface IProtocolMessage
    {
        /// <summary>
        /// 协议ID
        /// </summary>
        int ProtocolId { get; }
        
        /// <summary>
        /// 序列化到字节数组
        /// </summary>
        /// <returns>序列化后的字节数组</returns>
        byte[] Serialize();
        
        /// <summary>
        /// 从字节数组反序列化
        /// </summary>
        /// <param name="data">字节数组</param>
        void Deserialize(byte[] data);
    }

    /// <summary>
    /// 客户端到服务器消息接口
    /// </summary>
    public interface IC2SMessage : IProtocolMessage
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        ulong Session { get; set; }
    }

    /// <summary>
    /// 服务器到客户端消息接口
    /// </summary>
    public interface IS2CMessage : IProtocolMessage
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        ulong Session { get; set; }
    }

    /// <summary>
    /// 协议特性 - 用于标记协议类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ProtocolAttribute : Attribute
    {
        public int Id { get; }
        
        public ProtocolAttribute(int id)
        {
            Id = id;
        }
    }

    /// <summary>
    /// 协议字段特性 - 用于标记协议字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ProtocolFieldAttribute : Attribute
    {
        public int Index { get; }
        
        public ProtocolFieldAttribute(int index)
        {
            Index = index;
        }
    }
} 