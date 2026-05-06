using Cysharp.Threading.Tasks;
using GameFramework.Manager;
using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Game
{
    /// <summary>
    /// 游戏应用逻辑类
    /// </summary>
    public abstract class GameLogic : MonoSingleton<GameLogic>
    {
        /// <summary>
        /// 游戏应用状态
        /// </summary>
        public enum GameState
        {
            None,           // 未初始化
            Initializing,   // 初始化中
            Ready,          // 准备就绪
            Playing,        // 游戏中
            Paused,         // 暂停
            GameOver,       // 游戏结束
        }
        
        public enum GameScene
        {
            StartScene,
            GooseGame,
        }

        public GameScene StartScene => GameScene.StartScene;

        /// <summary>
        /// 游戏应用当前状态
        /// </summary>
        public GameState CurrentState { get; protected set; } = GameState.None;

        /// <summary>
        /// 场景对应的游戏业务管理器类型集合
        /// </summary>
        protected readonly Dictionary<string, List<Type>>
            SceneToGameManagerTypes = new Dictionary<string, List<Type>>();

        /// <summary>
        /// 游戏业务管理器字典
        /// </summary>
        protected readonly Dictionary<Type, BaseGameManager> GameManagers = new Dictionary<Type, BaseGameManager>();
        
        /// <summary>
        /// 场景对应的游戏业务管理器集合
        /// </summary>
        protected readonly Dictionary<string, List<BaseGameManager>> SceneManagers = new Dictionary<string, List<BaseGameManager>>();

        private readonly Dictionary<string, bool> SceneManagerInitStateMap = new Dictionary<string, bool>();

        protected virtual void Update()
        {
            switch (CurrentState)
            {
                case GameState.Playing:
                    OnGamePlaying();
                    break;
                case GameState.Paused:
                    OnGamePaused();
                    break;
                case GameState.GameOver:
                    OnGameOver();
                    break;
            }
        }

        protected virtual void OnDestroy()
        {
            ReleaseGameManagers();

            if (_instance == this)
                _instance = null;
        }

        #region 公开方法
        /// <summary>
        /// 异步初始化游戏应用
        /// </summary>
        /// <returns></returns>
        public async UniTask InitGameAsync()
        {
            // 进入初始化状态
            CurrentState = GameState.Initializing;

            // 异步等待配置管理器初始化
            await WaitForConfigManagerAsync();
            
            // 注册场景存在的游戏业务管理器
            RegisterSceneGameManagerTypes();
            
            // RegisterSceneGameManagers();
            
            // InitializeSceneManagers();

            var gameSceneManager = GameFrameworkManager.Instance.GetManager<Manager.GameSceneManager>();
            gameSceneManager.OnSceneChanged += OnSceneChanged;
            gameSceneManager.OnSceneLoadComplete += OnSceneLoadComplete;

            // // 注册所有游戏业务管理器
            // RegisterGameManagers();
            //
            // // 初始化所有游戏业务管理器
            // InitializeGameManagers();

            // 调用游戏应用初始化钩子
            await OnGameInitAsync();

            // 完成初始化
            CurrentState = GameState.Ready;

            // 进入准备就绪状态
            OnGameReady();
        }

        private void OnSceneChanged(string oldScene, string newScene)
        {
            List<Type> managerTypeList;
            if (SceneToGameManagerTypes.TryGetValue(oldScene, out managerTypeList))
            {
                foreach (var type in managerTypeList)
                {
                    UnregisterGameManager(type);
                }
            }
        }
        
        private void OnSceneLoadComplete(object sender, SceneLoadEventArgs e)
        {
            List<Type> managerTypeList;
            SceneManagers[e.SceneName] = new List<BaseGameManager>();
            if (SceneToGameManagerTypes.TryGetValue(e.SceneName, out managerTypeList))
            {
                foreach (var type in managerTypeList)
                {
                    BaseGameManager manager = RegisterGameManager(type);
                    SceneManagers[e.SceneName].Add(manager);
                }
                InitializeSceneManagers();
            }
        }

        /// <summary>
        /// 游戏应用开始
        /// </summary>
        public virtual async UniTask StartGameAsync()
        {
            if (CurrentState != GameState.Ready)
            {
                Debug.LogWarning("[GameLogic] 游戏未准备好，无法开始");
                return;
            }

            CurrentState = GameState.Playing;
            await OnGameStartAsync();

            // 触发游戏开始事件
            GetFrameworkManager<EventManager>().Trigger("GameStarted");
        }

        /// <summary>
        /// 游戏应用暂停
        /// </summary>
        public virtual void PauseGame()
        {
            if (CurrentState != GameState.Playing)
            {
                Debug.LogWarning("[GameLogic] 游戏未进行中，无法暂停");
                return;
            }

            CurrentState = GameState.Paused;
            OnGamePause();

            // 触发游戏暂停事件
            GetFrameworkManager<EventManager>().Trigger("GamePaused");
        }

        /// <summary>
        /// 恢复游戏应用
        /// </summary>
        public virtual void ResumeGame()
        {
            if (CurrentState != GameState.Paused)
            {
                Debug.LogWarning("[GameLogic] 游戏未暂停，无法恢复");
                return;
            }

            CurrentState = GameState.Playing;
            OnGameResume();

            // 触发游戏恢复事件
            GetFrameworkManager<EventManager>().Trigger("GameResumed");
        }

        /// <summary>
        /// 结束游戏应用
        /// </summary>
        public virtual void EndGame()
        {
            if (CurrentState != GameState.Playing && CurrentState != GameState.Paused)
            {
                Debug.LogWarning("[GameLogic] 游戏未进行中或暂停，无法结束");
                return;
            }

            CurrentState = GameState.GameOver;
            OnGameEnd();

            // 触发游戏结束事件
            GetFrameworkManager<EventManager>().Trigger("GameEnded");
        }

        /// <summary>
        /// 获取框架管理器
        /// </summary>
        public T GetFrameworkManager<T>() where T : class, IManager
        {
            return GameFrameworkManager.Instance.GetManager<T>();
        }

        /// <summary>
        /// 获取游戏业务管理器
        /// </summary>
        public T GetGameManager<T>() where T : BaseGameManager
        {
            if (GameManagers.TryGetValue(typeof(T), out var manager))
                return (T)manager;

            return null;
        }

        /// <summary>
        /// 尝试获取游戏业务管理器
        /// </summary>
        /// <param name="manager">out 游戏业务管理器</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGetGameManager<T>(out T manager) where T : BaseGameManager
        {
            manager = null;
            if (GameManagers.TryGetValue(typeof(T), out var mgr))
            {
                manager = (T)mgr;
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// 尝试获取游戏业务管理器
        /// </summary>
        /// <param name="manager">out 游戏业务管理器</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGetGameManager(Type type, out BaseGameManager manager) 
        {
            manager = null;
            if (GameManagers.TryGetValue(type, out var mgr))
            {
                manager = mgr;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 等待初始化完成
        /// </summary>
        public async UniTask WaitForInitializationAsync()
        {
            while (CurrentState == GameState.None || CurrentState == GameState.Initializing)
                await UniTask.Yield();
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 获取配置管理器实例（由子类实现）
        /// </summary>
        protected abstract BaseConfigManager GetConfigManager();

        /// <summary>
        /// 异步等待配置管理器初始化
        /// </summary>
        private async UniTask WaitForConfigManagerAsync()
        {
            Debug.Log("[GameLogic] 等待配置管理器初始化...");

            var configManager = GetConfigManager();
            configManager.Init();
            await configManager.WaitForInitializationAsync();

            Debug.Log("[GameLogic] 配置管理器初始化完成");
        }

        protected abstract void RegisterSceneGameManagerTypes();

        private void RegisterSceneGameManagers()
        {
            GameSceneManager gameSceneManager = GetFrameworkManager<GameSceneManager>();
            string currentSceneName = gameSceneManager.CurrentSceneName;
            List<Type> managerTypes = SceneToGameManagerTypes.GetValueOrDefault(currentSceneName, null);
            if (managerTypes == null) return;
            SceneManagers[currentSceneName] = new List<BaseGameManager>();
            foreach (var managerType in managerTypes)
            {
                BaseGameManager manager = RegisterGameManager(managerType);
                SceneManagers[currentSceneName].Add(manager);
            }
        }
        
        // protected void Regist

        /// <summary>
        /// 注册单个游戏业务管理器
        /// </summary>
        protected void RegisterGameManager<T>() where T : BaseGameManager
        {
            var managerType = typeof(T);

            if (GameManagers.ContainsKey(managerType))
            {
                Debug.LogWarning($"[GameLogic] 管理器 {managerType.Name} 已注册");
                return;
            }

            var manager = FindObjectOfType<T>();
            if (manager == null)
            {
                var go = new GameObject(managerType.Name);
                manager = go.AddComponent<T>();
            }

            if (manager.DonotDestroyOnLoad)
            {
                DontDestroyOnLoad(manager.gameObject);
            }

            GameManagers[managerType] = manager;
            Debug.Log($"[GameLogic] 注册管理器: {managerType.Name}");
        }

        protected BaseGameManager RegisterGameManager(Type type)
        {
            if (GameManagers.ContainsKey(type))                   
            {
                Debug.LogWarning($"[GameLogic] 管理器 {type.Name} 已注册");
                return GameManagers[type];
            }

            var manager = FindObjectOfType(type) as BaseGameManager;
            if (manager == null)
            {
                var go = new GameObject(type.Name);
                manager = go.AddComponent(type) as BaseGameManager;
            }
            
            if (manager.DonotDestroyOnLoad)
            {
                DontDestroyOnLoad(manager.gameObject);
            }
            
            GameManagers[type] = manager;
            Debug.Log($"[GameLogic] 注册管理器: {type.Name}, {GameManagers.Count}");
            return manager;
        }

        protected void UnregisterGameManager<T>() where T : BaseGameManager
        {
            var managerType = typeof(T);
            if (!GameManagers.ContainsKey(managerType)) return;
            GameManagers[managerType].Release();
            GameManagers.Remove(managerType);
        }

        protected void UnregisterGameManager(Type type)
        {
            if (!GameManagers.ContainsKey(type)) return;
            GameManagers[type].Release();
            GameManagers.Remove(type);
        }

        protected void RegisterSceneManager(string sceneName, List<Type> types)
        {
            SceneToGameManagerTypes[sceneName] = types;
        }

        /// <summary>
        /// 初始化所有游戏业务管理器
        /// </summary>
        private void InitializeGameManagers()
        {
            Debug.Log("[GameLogic] 初始化游戏业务管理器");

            foreach (var manager in GameManagers.Values)
                manager.Init();

            Debug.Log("[GameLogic] 初始化游戏业务管理器完成");
        }

        private void InitializeSceneManagers()
        {
            GameSceneManager gameSceneManager = GetFrameworkManager<GameSceneManager>();
            string currentSceneName = gameSceneManager.CurrentSceneName;
            bool initialized = SceneManagerInitStateMap.GetValueOrDefault(currentSceneName, false);
            if (initialized) return;

            List<BaseGameManager> baseGameManagers = SceneManagers.GetValueOrDefault(currentSceneName, null);
            if (baseGameManagers == null) return;

            foreach (var manager in baseGameManagers)
            {
                manager.Init();
            }
            SceneManagerInitStateMap[currentSceneName] = true;
        }

        /// <summary>
        /// 释放所有游戏业务管理器
        /// </summary>
        private void ReleaseGameManagers()
        {
            Debug.Log("[GameLogic] 释放游戏业务管理器");

            foreach (var manager in GameManagers.Values)
                manager.Release();

            GameManagers.Clear();
        }
        #endregion

        #region 钩子方法
        /// <summary>
        /// 游戏应用初始化钩子
        /// </summary>
        protected virtual UniTask OnGameInitAsync() => UniTask.CompletedTask;

        /// <summary>
        /// 游戏应用准备钩子
        /// </summary>
        protected virtual void OnGameReady() { }

        /// <summary>
        /// 游戏应用开始钩子
        /// </summary>
        protected virtual async UniTask OnGameStartAsync() {}

        /// <summary>
        /// 游戏应用暂停钩子
        /// </summary>
        protected virtual void OnGamePause() { }

        /// <summary>
        /// 游戏应用恢复钩子
        /// </summary>
        protected virtual void OnGameResume() { }

        /// <summary>
        /// 游戏应用结束钩子
        /// </summary>
        protected virtual void OnGameEnd() { }

        /// <summary>
        /// 游戏应用游玩中钩子
        /// </summary>
        protected virtual void OnGamePlaying() { }

        /// <summary>
        /// 游戏应用暂停中钩子
        /// </summary>
        protected virtual void OnGamePaused() { }

        /// <summary>
        /// 游戏应用结束钩子
        /// </summary>
        protected virtual void OnGameOver() { }
        
        /// <summary>
        /// 游戏自定义登录
        /// </summary>
        /// <returns></returns>
        public abstract UniTask Login(string code, ProviderLogin providerLogin = null);
        
        #endregion
    }
}