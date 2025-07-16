using System;
using System.Collections.Generic;
using System.Reflection;
using GameFramework.Manager;
using GameFramework.Network.Protocol;
using UnityEngine;

namespace GameFramework.Utility
{
    /// <summary>
    /// 协议工具类 - 提供协议相关的便捷方法
    /// </summary>
    public static class ProtocolUtil
    {
        /// <summary>
        /// 自动注册程序集中的所有协议类型
        /// </summary>
        /// <param name="protobufManager">Protobuf管理器</param>
        /// <param name="assembly">程序集，默认为当前程序集</param>
        /// <param name="protocolIdAttribute">协议ID特性类型</param>
        public static void AutoRegisterProtocols(ProtobufManager protobufManager, Assembly assembly = null, Type protocolIdAttribute = null)
        {
            if (protobufManager == null)
            {
                Debug.LogError("[ProtocolUtil] ProtobufManager 为空");
                return;
            }
            
            if (assembly == null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }
            
            if (protocolIdAttribute == null)
            {
                protocolIdAttribute = typeof(ProtocolIdAttribute);
            }
            
            int registeredCount = 0;
            
            try
            {
                Type[] types = assembly.GetTypes();
                
                foreach (Type type in types)
                {
                    // 检查是否有协议ID特性
                    object[] attributes = type.GetCustomAttributes(protocolIdAttribute, false);
                    
                    if (attributes.Length > 0 && attributes[0] is ProtocolIdAttribute protocolAttr)
                    {
                        // 检查类型是否符合协议要求
                        if (IsValidProtocolType(type))
                        {
                            // 使用反射调用泛型方法
                            MethodInfo registerMethod = typeof(ProtobufManager).GetMethod("RegisterProtocol");
                            MethodInfo genericMethod = registerMethod.MakeGenericMethod(type);
                            
                            genericMethod.Invoke(protobufManager, new object[] { protocolAttr.Id });
                            registeredCount++;
                        }
                        else
                        {
                            Debug.LogWarning($"[ProtocolUtil] 类型 {type.Name} 不符合协议要求，跳过注册");
                        }
                    }
                }
                
                Debug.Log($"[ProtocolUtil] 自动注册完成，共注册 {registeredCount} 个协议");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProtocolUtil] 自动注册协议失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 检查类型是否为有效的协议类型
        /// </summary>
        /// <param name="type">要检查的类型</param>
        /// <returns>是否为有效的协议类型</returns>
        private static bool IsValidProtocolType(Type type)
        {
            // 必须是类
            if (!type.IsClass)
                return false;
            
            // 不能是抽象类
            if (type.IsAbstract)
                return false;
            
            // 必须有无参构造函数
            if (type.GetConstructor(Type.EmptyTypes) == null)
                return false;
            
            return true;
        }
        
        /// <summary>
        /// 创建协议实例
        /// </summary>
        /// <typeparam name="T">协议类型</typeparam>
        /// <returns>协议实例</returns>
        public static T CreateProtocol<T>() where T : class, new()
        {
            return new T();
        }
        
        /// <summary>
        /// 创建协议实例
        /// </summary>
        /// <param name="protocolType">协议类型</param>
        /// <returns>协议实例</returns>
        public static object CreateProtocol(Type protocolType)
        {
            if (protocolType == null)
            {
                Debug.LogError("[ProtocolUtil] 协议类型为空");
                return null;
            }
            
            try
            {
                return Activator.CreateInstance(protocolType);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProtocolUtil] 创建协议实例失败: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 获取协议的显示名称
        /// </summary>
        /// <param name="protocolType">协议类型</param>
        /// <returns>显示名称</returns>
        public static string GetProtocolDisplayName(Type protocolType)
        {
            if (protocolType == null)
                return "Unknown";
            
            // 尝试获取显示名称特性
            object[] attributes = protocolType.GetCustomAttributes(typeof(ProtocolDisplayNameAttribute), false);
            
            if (attributes.Length > 0 && attributes[0] is ProtocolDisplayNameAttribute displayAttr)
            {
                return displayAttr.DisplayName;
            }
            
            // 返回类型名称
            return protocolType.Name;
        }
        
        /// <summary>
        /// 获取协议的描述信息
        /// </summary>
        /// <param name="protocolType">协议类型</param>
        /// <returns>描述信息</returns>
        public static string GetProtocolDescription(Type protocolType)
        {
            if (protocolType == null)
                return "";
            
            // 尝试获取描述特性
            object[] attributes = protocolType.GetCustomAttributes(typeof(ProtocolDescriptionAttribute), false);
            
            if (attributes.Length > 0 && attributes[0] is ProtocolDescriptionAttribute descAttr)
            {
                return descAttr.Description;
            }
            
            return "";
        }
        
        /// <summary>
        /// 验证协议数据
        /// </summary>
        /// <param name="protocol">协议对象</param>
        /// <returns>验证结果</returns>
        public static ProtocolValidationResult ValidateProtocol(object protocol)
        {
            if (protocol == null)
            {
                return new ProtocolValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "协议对象为空"
                };
            }
            
            Type protocolType = protocol.GetType();
            
            // 检查是否实现了验证接口
            if (protocol is IProtocolValidatable validatable)
            {
                return validatable.Validate();
            }
            
            // 默认验证通过
            return new ProtocolValidationResult
            {
                IsValid = true,
                ErrorMessage = ""
            };
        }
    }
    
    /// <summary>
    /// 协议ID特性 - 用于标记协议的ID
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ProtocolIdAttribute : Attribute
    {
        public int Id { get; }
        
        public ProtocolIdAttribute(int id)
        {
            Id = id;
        }
    }
    
    /// <summary>
    /// 协议显示名称特性 - 用于标记协议的显示名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ProtocolDisplayNameAttribute : Attribute
    {
        public string DisplayName { get; }
        
        public ProtocolDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
    
    /// <summary>
    /// 协议描述特性 - 用于标记协议的描述信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ProtocolDescriptionAttribute : Attribute
    {
        public string Description { get; }
        
        public ProtocolDescriptionAttribute(string description)
        {
            Description = description;
        }
    }
    
    /// <summary>
    /// 协议验证接口 - 协议类可以实现此接口来提供自定义验证逻辑
    /// </summary>
    public interface IProtocolValidatable
    {
        ProtocolValidationResult Validate();
    }
    
    /// <summary>
    /// 协议验证结果
    /// </summary>
    public class ProtocolValidationResult
    {
        public bool IsValid { get; set; } = true;
        public string ErrorMessage { get; set; } = "";
        public List<string> FieldErrors { get; set; } = new List<string>();
    }
} 