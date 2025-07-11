using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Core
{
    /// <summary>
    /// 游戏框架管理器 - 框架核心入口 
    /// @author: Eva
    /// </summary>
    public class GameFrameworkManager : MonoBehaviour
    {
        private static GameFrameworkManager _instance;
        public static GameFrameworkManager Instance => _instance;

        // 管理器集合
        private readonly Dictionary<Type, IManager> _managers = new Dictionary<Type, IManager>();

        // 初始化事件
        public event Action OnInitialized;
        
        // 框架是否已初始化
        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitFramework();
        }

        /// <summary>
        /// 初始化框架
        /// </summary>
        public void InitFramework()
        {
            Debug.Log("[GameFramework] 开始初始化框架...");

            // 注册各个管理器
            RegisterManager<ResourceManager>();
            // ConfigManager 由业务层独立管理，框架不注册
            RegisterManager<PlatformManager>();
            RegisterManager<EventManager>();
            RegisterManager<UIManager>();
            RegisterManager<HttpManager>();  // 注册HTTP管理器
            
            // 初始化各个管理器
            foreach (var manager in _managers.Values)
            {
                manager.Init();
            }
            
            IsInitialized = true;
            OnInitialized?.Invoke();
            
            Debug.Log("[GameFramework] 框架初始化完成");
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
    }
} 