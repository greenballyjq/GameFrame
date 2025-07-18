using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using Luban;
using UnityEngine;
using UnityEngine.Networking;

namespace GameFramework.Manager
{
    /// <summary>
    /// 配置管理器基类接口
    /// 提供通用的Luban配置加载功能
    /// </summary>
    public abstract class BaseConfigManager : MonoBehaviour, IManager
    {
        /// <summary>
        /// 配置文件路径
        /// </summary>
        protected virtual string ConfigPath => $"{Application.streamingAssetsPath}/Bundle/Raw/Configs/bytes";
        
        /// <summary>
        /// 配置表名称列表 - 子类需要重写
        /// </summary>
        protected abstract List<string> TableNames { get; }
        
        /// <summary>
        /// 配置表容器
        /// </summary>
        protected cfg.Tables _tables;
        
        /// <summary>
        /// 初始化状态
        /// </summary>
        public bool Initialized { get; protected set; }

        /// <summary>
        /// 框架管理器接口实现
        /// </summary>
        public virtual void Init()
        {
            Debug.Log($"[{GetType().Name}] 初始化配置管理器");
            // 异步初始化配置
            InitializeAsync();
        }

        /// <summary>
        /// 框架管理器接口实现
        /// </summary>
        public virtual void Update()
        {
            // 配置管理器通常不需要每帧更新
        }

        /// <summary>
        /// 框架管理器接口实现
        /// </summary>
        public virtual void Release()
        {
            _tables = null;
            Initialized = false;
            Debug.Log($"[{GetType().Name}] 配置管理器已释放");
        }

        /// <summary>
        /// 异步初始化配置表
        /// </summary>
        protected virtual async void InitializeAsync()
        {
            try
            {
                Debug.Log($"[{GetType().Name}] 开始加载配置表");
                await LoadTablesAsync();
                Initialized = true;
                Debug.Log($"[{GetType().Name}] 配置表加载完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{GetType().Name}] 配置表加载失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载配置表
        /// </summary>
        public virtual async Task LoadTablesAsync()
        {
#if UNITY_EDITOR
            _tables = new cfg.Tables(s => new ByteBuf(File.ReadAllBytes($"{ConfigPath}/{s}.bytes")));
#elif UNITY_STANDALONE_WIN
            _tables = new cfg.Tables(s => new ByteBuf(File.ReadAllBytes($"{ConfigPath}/{s}.bytes")));
#elif UNITY_ANDROID || UNITY_WEBGL
            List<ByteBuf> byteBuffs = new List<ByteBuf>();
            foreach (var tableName in TableNames)
            {
                UnityWebRequest request = UnityWebRequest.Get($"{ConfigPath}/{tableName}.bytes");
                await request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[{GetType().Name}] 加载配置表失败: {tableName}, 错误: {request.error}");
                    continue;
                }
                
                byteBuffs.Add(new ByteBuf(request.downloadHandler.data));
            }

            int index = 0;
            _tables = new cfg.Tables(_ => byteBuffs[index++]);
#endif
        }

        /// <summary>
        /// 等待配置管理器初始化完成
        /// </summary>
        public virtual async Task WaitForInitializationAsync()
        {
            while (!Initialized)
            {
                await UniTask.Delay(10);
            }
        }
    }

    /// <summary>
    /// 通用配置管理器泛型基类
    /// 提供单例模式支持和自动创建机制
    /// </summary>
    public abstract class ConfigManager<T> : BaseConfigManager where T : ConfigManager<T>
    {
        private static T _instance;
        
        /// <summary>
        /// 单例实例，如果不存在则自动创建
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 尝试从场景中查找
                    _instance = FindObjectOfType<T>();
                    
                    // 如果场景中没有，则自动创建
                    if (_instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).Name);
                        _instance = go.AddComponent<T>();
                        Debug.Log($"[{typeof(T).Name}] 自动创建 {typeof(T).Name} 实例");
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this as T;
        }
    }
} 