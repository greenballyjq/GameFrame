using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameFramework.Network.Protocol
{
    /// <summary>
    /// 协议工厂 - 负责协议的注册、创建和管理
    /// </summary>
    public class ProtocolFactory
    {
        private static ProtocolFactory _instance;
        public static ProtocolFactory Instance => _instance ??= new ProtocolFactory();

        // 协议类型映射表（ID -> Type）
        private readonly Dictionary<int, Type> _protocolTypes = new Dictionary<int, Type>();
        
        // 协议ID映射表（Type -> ID）
        private readonly Dictionary<Type, int> _protocolIds = new Dictionary<Type, int>();
        
        // 注释掉序列化器映射表，暂时不使用
        // private readonly Dictionary<Type, IProtocolSerializer> _serializers = new Dictionary<Type, IProtocolSerializer>();

        private ProtocolFactory()
        {
            // 自动注册所有带有ProtocolAttribute的类
            AutoRegisterProtocols();
        }

        /// <summary>
        /// 自动注册所有协议类型
        /// </summary>
        private void AutoRegisterProtocols()
        {
            try
            {
                // 获取所有程序集
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        // 获取所有类型
                        var types = assembly.GetTypes();
                        
                        foreach (var type in types)
                        {
                            // 检查是否有ProtocolAttribute特性
                            var protocolAttr = type.GetCustomAttribute<ProtocolAttribute>();
                            if (protocolAttr != null && typeof(IProtocolMessage).IsAssignableFrom(type))
                            {
                                RegisterProtocol(type, protocolAttr.Id);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[ProtocolFactory] 加载程序集 {assembly.FullName} 时出错: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProtocolFactory] 自动注册协议时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 手动注册协议类型
        /// </summary>
        /// <param name="type">协议类型</param>
        /// <param name="protocolId">协议ID</param>
        public void RegisterProtocol(Type type, int protocolId)
        {
            if (type == null)
            {
                Debug.LogError("[ProtocolFactory] 协议类型不能为null");
                return;
            }

            if (!typeof(IProtocolMessage).IsAssignableFrom(type))
            {
                Debug.LogError($"[ProtocolFactory] 类型 {type.Name} 必须实现 IProtocolMessage 接口");
                return;
            }

            if (_protocolTypes.ContainsKey(protocolId))
            {
                Debug.LogWarning($"[ProtocolFactory] 协议ID {protocolId} 已注册，类型：{_protocolTypes[protocolId].Name}");
                return;
            }

            if (_protocolIds.ContainsKey(type))
            {
                Debug.LogWarning($"[ProtocolFactory] 协议类型 {type.Name} 已注册，ID：{_protocolIds[type]}");
                return;
            }

            _protocolTypes[protocolId] = type;
            _protocolIds[type] = protocolId;

            Debug.Log($"[ProtocolFactory] 注册协议：{type.Name} -> ID:{protocolId}");
        }

        /// <summary>
        /// 根据协议ID创建协议消息实例
        /// </summary>
        /// <param name="protocolId">协议ID</param>
        /// <returns>协议消息实例</returns>
        public IProtocolMessage CreateMessage(int protocolId)
        {
            if (!_protocolTypes.TryGetValue(protocolId, out var type))
            {
                Debug.LogError($"[ProtocolFactory] 未找到协议ID {protocolId} 对应的类型");
                return null;
            }

            try
            {
                return Activator.CreateInstance(type) as IProtocolMessage;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProtocolFactory] 创建协议 {type.Name} 实例失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 根据类型创建协议消息实例
        /// </summary>
        /// <typeparam name="T">协议类型</typeparam>
        /// <returns>协议消息实例</returns>
        public T CreateMessage<T>() where T : class, IProtocolMessage, new()
        {
            return new T();
        }

        /// <summary>
        /// 获取协议的ID
        /// </summary>
        /// <param name="type">协议类型</param>
        /// <returns>协议ID，如果未注册则返回-1</returns>
        public int GetProtocolId(Type type)
        {
            return _protocolIds.TryGetValue(type, out var id) ? id : -1;
        }

        /// <summary>
        /// 获取协议的类型
        /// </summary>
        /// <param name="protocolId">协议ID</param>
        /// <returns>协议类型，如果未注册则返回null</returns>
        public Type GetProtocolType(int protocolId)
        {
            return _protocolTypes.TryGetValue(protocolId, out var type) ? type : null;
        }

        /// <summary>
        /// 检查协议是否已注册
        /// </summary>
        /// <param name="protocolId">协议ID</param>
        /// <returns>是否已注册</returns>
        public bool IsProtocolRegistered(int protocolId)
        {
            return _protocolTypes.ContainsKey(protocolId);
        }

        /// <summary>
        /// 检查协议类型是否已注册
        /// </summary>
        /// <param name="type">协议类型</param>
        /// <returns>是否已注册</returns>
        public bool IsProtocolRegistered(Type type)
        {
            return _protocolIds.ContainsKey(type);
        }

        /// <summary>
        /// 获取所有已注册的协议信息
        /// </summary>
        /// <returns>协议信息列表</returns>
        public List<(int Id, Type Type)> GetAllProtocols()
        {
            var protocols = new List<(int, Type)>();
            foreach (var kvp in _protocolTypes)
            {
                protocols.Add((kvp.Key, kvp.Value));
            }
            return protocols;
        }

        /// <summary>
        /// 清空所有协议注册
        /// </summary>
        public void Clear()
        {
            _protocolTypes.Clear();
            _protocolIds.Clear();
            // _serializers.Clear();
            Debug.Log("[ProtocolFactory] 已清空所有协议注册");
        }
    }


} 