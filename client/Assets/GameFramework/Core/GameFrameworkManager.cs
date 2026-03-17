using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework.Manager;
using GameFramework.Network;
using GameFramework.Network.Proxy;
using GameFramework.Network.Utility;
using UnityEngine;

namespace GameFramework.Core
{
    /// <summary>
    /// 游戏框架管理器 - 框架核心入口 
    /// @author: Eva
    /// </summary>
    public class GameFrameworkManager : MonoSingleton<GameFrameworkManager>
    {
        // 管理器集合
        private readonly Dictionary<Type, IManager> _managers = new Dictionary<Type, IManager>();

        // 初始化事件
        public event Action OnInitialized;
        
        // 框架是否已初始化
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 初始化框架
        /// </summary>
        public void InitFramework()
        {
            // Debug.Log("[GameFramework] 开始初始化框架...");
            // 注册各个管理器
            RegisterManager<EventManager>();
            RegisterManager<ResourceManager>();
            RegisterManager<GameObjectPoolManager>();
            RegisterManager<PlatformManager>();
            RegisterManager<UIManager>();
            RegisterManager<TimerManager>();
            RegisterManager<SoundManager>();
            RegisterManager<EffectManager>();
            RegisterManager<Manager.GameSceneManager>();
            // ConfigManager 由业务层独立管理，框架不注册
#if FRAMEWORK_SERIALIZE
            RegisterManager<SaveLoadManager>();
#endif
            // 注册网络相关管理器
            RegisterManager<HttpManager>();          // 保留HTTP管理器作为基础网络功能
            RegisterManager<ServerClient>();         // 注册服务器客户端
            
            // 初始化各个管理器
            foreach (var manager in _managers.Values)
            {
                manager.Init();
            }

            // Debug.Log("[GameFramework] 框架初始化完成");
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        public async UniTask InitNetworkAsync()
        {
            await ServiceProxy.Instance.InitAsync();
        }

        /// <summary>
        /// 登录 包括平台（微信、抖音等）登录 和 后端登录
        /// </summary>
        public void Login()
        {
            var platformManager = GetManager<PlatformManager>();
            // 1. Platform 登录
            platformManager.CurrentPlatform.Login(async (code) =>
            {
                Debug.LogError($"[GameFrameworkManager] Platform 登录成功, code: {code}");
                // 2.后端登录
                var serverProxy = ServiceProxy.Instance;
                await UniTask.WaitUntil(() => serverProxy.IsServerConnected);
                Debug.Log($"[GameFrameworkManager] 当前平台：{platformManager.GetCurrentPlatform()}");
                await serverProxy.Login(platformManager.GetCurrentPlatform(), code);
            }, (msg) =>
            {
                Debug.LogError($"[GameFrameworkManager] Platform 登录失败: {msg}");
            });
        }

        public async UniTask WaitForInitializationAsync()
        {
            while (!IsInitialized)
            {
                await UniTask.Yield();
            }

            Debug.Log($"[GameFrameworkManager] 等待初始化完成------------");
        }
        
        /// <summary>
        /// 注册管理器
        /// </summary>
        public void RegisterManager<T>() where T : IManager, new()
        {
            var type = typeof(T);
            if (_managers.ContainsKey(type))
            {
                Debug.LogWarning($"[GameFramework] 管理器 {type.Name} 已经注册");
                return;
            }

            var manager = new T();
            _managers.Add(type, manager);
            Debug.Log($"[GameFramework] 注册管理器: {type.Name}");
        }

        /// <summary>
        /// 获取管理器
        /// </summary>
        public T GetManager<T>() where T : class, IManager
        {
            var type = typeof(T);
            if (_managers.TryGetValue(type, out var manager))
            {
                return manager as T;
            }

            Debug.LogError($"[GameFramework] 管理器 {type.Name} 未注册");
            return null;
        }

        private void Update()
        {
            // 更新所有管理器
            foreach (var manager in _managers.Values)
            {
                manager.DoUpdate(Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            // 释放所有管理器
            foreach (var manager in _managers.Values)
            {
                manager.Release();
            }
            
            _managers.Clear();
        }
    }
}
