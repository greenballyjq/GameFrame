using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;
using WeChatWASM;

/// <summary>
/// 平台管理器 - 处理不同平台的接入
/// </summary>
public class PlatformManager : IManager
{
    // 平台类型
    public enum PlatformType
    {
        Default,    // 默认平台
        WeChat,     // 微信平台
        TikTok,     // 抖音平台
        Kuaishou    // 快手平台
    }
    
    // 当前平台类型
    private PlatformType _currentPlatformType = PlatformType.Default;
    
    // 平台实现字典
    private readonly Dictionary<PlatformType, IPlatform> _platforms = new Dictionary<PlatformType, IPlatform>();
    
    // 当前平台实现
    public IPlatform CurrentPlatform { get; private set; }
    
    public void Init()
    {
         Debug.Log("[PlatformManager] 初始化平台管理器");
 
        // 自动检测并注册当前平台
        DetectCurrentPlatform();
        
        // 初始化当前平台
        InitCurrentPlatform();
        
        // Debug.Log($"[{GetType().Name}] USER_DATA_PATH: {WeChatWASM.WX.env.USER_DATA_PATH}");
    }

    public void DoUpdate(float dt)
    {
        // 更新当前平台
        CurrentPlatform?.Update();
    }

    public void Release()
    {
        // 释放当前平台
        CurrentPlatform?.Release();
    }
    
    /// <summary>
    /// 注册平台实现
    /// </summary>
    private void RegisterPlatform(PlatformType platformType, IPlatform platform)
    {
        if (_platforms.ContainsKey(platformType))
        {
             Debug.LogWarning($"[PlatformManager] 平台 {platformType} 已经注册");
            return;
        }
        
        _platforms.Add(platformType, platform);
        Debug.Log($"[PlatformManager] 注册平台: {platformType}");
    }
    
    /// <summary>
    /// 检测当前平台类型
    /// </summary>
    private void DetectCurrentPlatform()
    {

#if UNITY_EDITOR
        RegisterPlatform(PlatformType.WeChat, new WeChatPlatform());
        _currentPlatformType = PlatformType.WeChat;
        WXBase.CreateVideo(new CreateVideoOption()).OnPlay(() =>
        {

        });
#elif WEIXINMINIGAME
        RegisterPlatform(PlatformType.WeChat, new WeChatPlatform());
        _currentPlatformType = PlatformType.WeChat;
#elif DOUYINMINIGAME
        RegisterPlatform(PlatformType.TikTok, new TikTokPlatform());
        _currentPlatformType = PlatformType.TikTok;
#elif KUAISHOUMINIGAME
        RegisterPlatform(PlatformType.Kuaishou, new KuaishouPlatform());
        _currentPlatformType = PlatformType.Kuaishou;
#endif
        Debug.Log($"[PlatformManager] 当前平台: {_currentPlatformType}");
    }
    
    /// <summary>
    /// 初始化当前平台
    /// </summary>
    private void InitCurrentPlatform()
    {
        if (_platforms.TryGetValue(_currentPlatformType, out var platform))
        {
            CurrentPlatform = platform;
            CurrentPlatform.Init();
            Debug.Log($"[PlatformManager] 初始化平台: {_currentPlatformType}");
        }
        else
        {
            Debug.LogError($"[PlatformManager] 平台 {_currentPlatformType} 未注册");
        }
    }
    
    /// <summary>
    /// 切换平台
    /// </summary>
    public void SwitchPlatform(PlatformType platformType)
    {
        if (_currentPlatformType == platformType)
        {
            Debug.Log($"[PlatformManager] 已经是平台 {platformType}");
            return;
        }
        
        if (!_platforms.TryGetValue(platformType, out var platform))
        {
            Debug.LogError($"[PlatformManager] 平台 {platformType} 未注册");
            return;
        }
        
        // 释放当前平台
        CurrentPlatform?.Release();
        
        // 切换平台
        _currentPlatformType = platformType;
        CurrentPlatform = platform;
        CurrentPlatform.Init();
        
        Debug.Log($"[PlatformManager] 切换到平台: {platformType}");
    }

    public PlatformType GetCurrentPlatform()
    {
        return _currentPlatformType;
    }
} 