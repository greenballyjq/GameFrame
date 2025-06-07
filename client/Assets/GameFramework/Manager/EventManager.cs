using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

/// <summary>
/// 事件管理器 - 处理游戏内事件广播
/// </summary>
public class EventManager : IManager
{
    // 事件字典，保存所有注册的事件
    private readonly Dictionary<string, Delegate> _events = new Dictionary<string, Delegate>();
    
    public void Init()
    {
        Debug.Log("[EventManager] 初始化事件管理器");
    }

    public void Update()
    {
        // 事件管理器无需更新
    }

    public void Release()
    {
        _events.Clear();
        Debug.Log("[EventManager] 释放事件管理器");
    }
    
    /// <summary>
    /// 添加无参事件监听
    /// </summary>
    public void AddListener(string eventName, Action callback)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("[EventManager] 事件名称不能为空");
            return;
        }
        
        if (callback == null)
        {
            Debug.LogError("[EventManager] 回调不能为空");
            return;
        }
        
        if (!_events.TryGetValue(eventName, out var existingDelegate))
        {
            _events[eventName] = callback;
        }
        else
        {
            if (existingDelegate is Action action)
            {
                _events[eventName] = Delegate.Combine(action, callback);
            }
            else
            {
                Debug.LogError($"[EventManager] 事件 {eventName} 类型不匹配，已存在不同参数的事件");
            }
        }
    }
    
    /// <summary>
    /// 添加单参数事件监听
    /// </summary>
    public void AddListener<T>(string eventName, Action<T> callback)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("[EventManager] 事件名称不能为空");
            return;
        }
        
        if (callback == null)
        {
            Debug.LogError("[EventManager] 回调不能为空");
            return;
        }
        
        if (!_events.TryGetValue(eventName, out var existingDelegate))
        {
            _events[eventName] = callback;
        }
        else
        {
            if (existingDelegate is Action<T> action)
            {
                _events[eventName] = Delegate.Combine(action, callback);
            }
            else
            {
                Debug.LogError($"[EventManager] 事件 {eventName} 类型不匹配，已存在不同参数的事件");
            }
        }
    }
    
    /// <summary>
    /// 添加双参数事件监听
    /// </summary>
    public void AddListener<T1, T2>(string eventName, Action<T1, T2> callback)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("[EventManager] 事件名称不能为空");
            return;
        }
        
        if (callback == null)
        {
            Debug.LogError("[EventManager] 回调不能为空");
            return;
        }
        
        if (!_events.TryGetValue(eventName, out var existingDelegate))
        {
            _events[eventName] = callback;
        }
        else
        {
            if (existingDelegate is Action<T1, T2> action)
            {
                _events[eventName] = Delegate.Combine(action, callback);
            }
            else
            {
                Debug.LogError($"[EventManager] 事件 {eventName} 类型不匹配，已存在不同参数的事件");
            }
        }
    }
    
    /// <summary>
    /// 移除无参事件监听
    /// </summary>
    public void RemoveListener(string eventName, Action callback)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("[EventManager] 事件名称不能为空");
            return;
        }
        
        if (callback == null)
        {
            Debug.LogError("[EventManager] 回调不能为空");
            return;
        }
        
        if (_events.TryGetValue(eventName, out var existingDelegate))
        {
            if (existingDelegate is Action action)
            {
                _events[eventName] = Delegate.Remove(action, callback);
                
                if (_events[eventName] == null)
                {
                    _events.Remove(eventName);
                }
            }
            else
            {
                Debug.LogError($"[EventManager] 事件 {eventName} 类型不匹配");
            }
        }
    }
    
    /// <summary>
    /// 移除单参数事件监听
    /// </summary>
    public void RemoveListener<T>(string eventName, Action<T> callback)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("[EventManager] 事件名称不能为空");
            return;
        }
        
        if (callback == null)
        {
            Debug.LogError("[EventManager] 回调不能为空");
            return;
        }
        
        if (_events.TryGetValue(eventName, out var existingDelegate))
        {
            if (existingDelegate is Action<T> action)
            {
                _events[eventName] = Delegate.Remove(action, callback);
                
                if (_events[eventName] == null)
                {
                    _events.Remove(eventName);
                }
            }
            else
            {
                Debug.LogError($"[EventManager] 事件 {eventName} 类型不匹配");
            }
        }
    }
    
    /// <summary>
    /// 移除双参数事件监听
    /// </summary>
    public void RemoveListener<T1, T2>(string eventName, Action<T1, T2> callback)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("[EventManager] 事件名称不能为空");
            return;
        }
        
        if (callback == null)
        {
            Debug.LogError("[EventManager] 回调不能为空");
            return;
        }
        
        if (_events.TryGetValue(eventName, out var existingDelegate))
        {
            if (existingDelegate is Action<T1, T2> action)
            {
                _events[eventName] = Delegate.Remove(action, callback);
                
                if (_events[eventName] == null)
                {
                    _events.Remove(eventName);
                }
            }
            else
            {
                Debug.LogError($"[EventManager] 事件 {eventName} 类型不匹配");
            }
        }
    }
    
    /// <summary>
    /// 触发无参事件
    /// </summary>
    public void Trigger(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("[EventManager] 事件名称不能为空");
            return;
        }
        
        if (_events.TryGetValue(eventName, out var existingDelegate))
        {
            if (existingDelegate is Action action)
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventManager] 触发事件 {eventName} 出错: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"[EventManager] 事件 {eventName} 类型不匹配");
            }
        }
    }
    
    /// <summary>
    /// 触发单参数事件
    /// </summary>
    public void Trigger<T>(string eventName, T arg)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("[EventManager] 事件名称不能为空");
            return;
        }
        
        if (_events.TryGetValue(eventName, out var existingDelegate))
        {
            if (existingDelegate is Action<T> action)
            {
                try
                {
                    action(arg);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventManager] 触发事件 {eventName} 出错: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"[EventManager] 事件 {eventName} 类型不匹配");
            }
        }
    }
    
    /// <summary>
    /// 触发双参数事件
    /// </summary>
    public void Trigger<T1, T2>(string eventName, T1 arg1, T2 arg2)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("[EventManager] 事件名称不能为空");
            return;
        }
        
        if (_events.TryGetValue(eventName, out var existingDelegate))
        {
            if (existingDelegate is Action<T1, T2> action)
            {
                try
                {
                    action(arg1, arg2);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventManager] 触发事件 {eventName} 出错: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"[EventManager] 事件 {eventName} 类型不匹配");
            }
        }
    }
    
    /// <summary>
    /// 清除所有事件
    /// </summary>
    public void ClearAllEvents()
    {
        _events.Clear();
        Debug.Log("[EventManager] 清除所有事件");
    }
    
    /// <summary>
    /// 清除指定事件
    /// </summary>
    public void ClearEvent(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("[EventManager] 事件名称不能为空");
            return;
        }
        
        if (_events.ContainsKey(eventName))
        {
            _events.Remove(eventName);
            Debug.Log($"[EventManager] 清除事件: {eventName}");
        }
    }
} 