using System;
using System.Threading.Tasks;
using Google.Protobuf;
using GameFramework.Network.Protocol;

namespace GameFramework.Network
{
    /// <summary>
    /// Google Protobuf 消息管理器
    /// 负责处理 protobuf 序列化和反序列化，以及消息的创建
    /// </summary>
    public static class ProtobufManager
    {
        /// <summary>
        /// 序列化 C2S 消息
        /// </summary>
        /// <param name="message">要序列化的消息</param>
        /// <returns>序列化后的字节数组</returns>
        public static byte[] SerializeC2SMessage(C2S_Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
                
            return message.ToByteArray();
        }
        
        /// <summary>
        /// 序列化 S2C 消息
        /// </summary>
        /// <param name="message">要序列化的消息</param>
        /// <returns>序列化后的字节数组</returns>
        public static byte[] SerializeS2CMessage(S2C_Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
                
            return message.ToByteArray();
        }
        
        /// <summary>
        /// 反序列化 C2S 消息
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <returns>反序列化后的消息</returns>
        public static C2S_Message DeserializeC2SMessage(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data cannot be null or empty", nameof(data));
                
            return C2S_Message.Parser.ParseFrom(data);
        }
        
        /// <summary>
        /// 反序列化 S2C 消息
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <returns>反序列化后的消息</returns>
        public static S2C_Message DeserializeS2CMessage(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data cannot be null or empty", nameof(data));
                
            return S2C_Message.Parser.ParseFrom(data);
        }
        
        /// <summary>
        /// 创建包含 Ping 的 C2S 消息
        /// </summary>
        /// <param name="sessionId">会话ID</param>
        /// <returns>C2S_Message</returns>
        public static C2S_Message CreatePingMessage(ulong sessionId = 0)
        {
            return new C2S_Message
            {
                Session = sessionId,
                Ping = new global::C2S_Ping()
            };
        }
        
        /// <summary>
        /// 创建包含 Login 的 C2S 消息
        /// </summary>
        /// <param name="account">账户</param>
        /// <param name="sessionId">会话ID</param>
        /// <returns>C2S_Message</returns>
        public static C2S_Message CreateLoginMessage(string account, ulong sessionId = 0)
        {
            return new C2S_Message
            {
                Session = sessionId,
                Login = new global::C2S_Login
                {
                    Account = account
                }
            };
        }
        
        /// <summary>
        /// 创建包含 Base 的 C2S 消息
        /// </summary>
        /// <param name="sessionId">会话ID</param>
        /// <returns>C2S_Message</returns>
        public static C2S_Message CreateBaseMessage(ulong sessionId = 0)
        {
            return new C2S_Message
            {
                Session = sessionId,
                Base = new global::C2S_Base()
            };
        }
        
        /// <summary>
        /// 创建包含 Logout 的 C2S 消息
        /// </summary>
        /// <param name="sessionId">会话ID</param>
        /// <returns>C2S_Message</returns>
        public static C2S_Message CreateLogoutMessage(ulong sessionId = 0)
        {
            return new C2S_Message
            {
                Session = sessionId,
                Logout = new global::C2S_Logout()
            };
        }
        
        /// <summary>
        /// 创建包含 ClientData 的 C2S 消息
        /// </summary>
        /// <param name="sessionId">会话ID</param>
        /// <returns>C2S_Message</returns>
        public static C2S_Message CreateClientDataMessage(ulong sessionId = 0)
        {
            return new C2S_Message
            {
                Session = sessionId,
                ClientData = new global::C2S_ClientData()
            };
        }
        
        /// <summary>
        /// 创建包含 UploadClientData 的 C2S 消息
        /// </summary>
        /// <param name="data">客户端数据</param>
        /// <param name="sessionId">会话ID</param>
        /// <returns>C2S_Message</returns>
        public static C2S_Message CreateUploadClientDataMessage(byte[] data, ulong sessionId = 0)
        {
            return new C2S_Message
            {
                Session = sessionId,
                UploadClientData = new global::C2S_UploadClientData
                {
                    Data = ByteString.CopyFrom(data ?? new byte[0])
                }
            };
        }
        
        /// <summary>
        /// 创建包含 Item 的 C2S 消息
        /// </summary>
        /// <param name="sessionId">会话ID</param>
        /// <returns>C2S_Message</returns>
        public static C2S_Message CreateItemMessage(ulong sessionId = 0)
        {
            return new C2S_Message
            {
                Session = sessionId,
                Item = new global::C2S_Item()
            };
        }
        
        // 适配器方法：兼容原有的 global 命名空间调用
        
        /// <summary>
        /// 适配器：序列化全局 C2S_Message（兼容 Scripts 层调用）
        /// </summary>
        /// <param name="message">全局命名空间的消息</param>
        /// <returns>序列化后的字节数组</returns>
        public static byte[] Serialize(global::C2S_Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
                
            return message.ToByteArray();
        }
        
        /// <summary>
        /// 适配器：序列化全局 S2C_Message（兼容 Scripts 层调用）
        /// </summary>
        /// <param name="message">全局命名空间的消息</param>
        /// <returns>序列化后的字节数组</returns>
        public static byte[] Serialize(global::S2C_Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
                
            return message.ToByteArray();
        }
        
        /// <summary>
        /// 适配器：反序列化为全局 C2S_Message（兼容 Scripts 层调用）
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <returns>反序列化后的消息</returns>
        public static global::C2S_Message DeserializeToGlobalC2S(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data cannot be null or empty", nameof(data));
                
            return global::C2S_Message.Parser.ParseFrom(data);
        }
        
        /// <summary>
        /// 适配器：反序列化为全局 S2C_Message（兼容 Scripts 层调用）
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <returns>反序列化后的消息</returns>
        public static global::S2C_Message DeserializeToGlobalS2C(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data cannot be null or empty", nameof(data));
                
            return global::S2C_Message.Parser.ParseFrom(data);
        }
        
        /// <summary>
        /// 通用序列化方法
        /// </summary>
        /// <typeparam name="T">实现 IMessage 的类型</typeparam>
        /// <param name="message">要序列化的消息</param>
        /// <returns>序列化后的字节数组</returns>
        public static byte[] Serialize<T>(T message) where T : IMessage<T>
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
                
            return message.ToByteArray();
        }
        
        /// <summary>
        /// 通用反序列化方法
        /// </summary>
        /// <typeparam name="T">实现 IMessage 的类型</typeparam>
        /// <param name="data">字节数组</param>
        /// <param name="parser">消息解析器</param>
        /// <returns>反序列化后的消息</returns>
        public static T Deserialize<T>(byte[] data, MessageParser<T> parser) where T : IMessage<T>
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data cannot be null or empty", nameof(data));
                
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));
                
            return parser.ParseFrom(data);
        }
        
        /// <summary>
        /// 异步序列化方法
        /// </summary>
        /// <typeparam name="T">实现 IMessage 的类型</typeparam>
        /// <param name="message">要序列化的消息</param>
        /// <returns>序列化后的字节数组</returns>
        public static async Task<byte[]> SerializeAsync<T>(T message) where T : IMessage<T>
        {
            return await Task.Run(() => Serialize(message));
        }
        
        /// <summary>
        /// 异步反序列化方法
        /// </summary>
        /// <typeparam name="T">实现 IMessage 的类型</typeparam>
        /// <param name="data">字节数组</param>
        /// <param name="parser">消息解析器</param>
        /// <returns>反序列化后的消息</returns>
        public static async Task<T> DeserializeAsync<T>(byte[] data, MessageParser<T> parser) where T : IMessage<T>
        {
            return await Task.Run(() => Deserialize(data, parser));
        }
        
        /// <summary>
        /// 检查消息是否有效
        /// </summary>
        /// <param name="message">要检查的消息</param>
        /// <returns>是否有效</returns>
        public static bool IsValidMessage<T>(T message) where T : IMessage<T>
        {
            return message != null;
        }
        
        /// <summary>
        /// 获取消息大小
        /// </summary>
        /// <param name="message">要计算大小的消息</param>
        /// <returns>消息大小</returns>
        public static int GetMessageSize<T>(T message) where T : IMessage<T>
        {
            if (message == null)
                return 0;
                
            return message.CalculateSize();
        }
    }
}
