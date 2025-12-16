using Cysharp.Threading.Tasks;
using GameFramework.Game;
using GameFramework.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Core
{
    public abstract class GameLogic : MonoSingleton<GameLogic>
    {
        /// <summary>
        /// 游戏状态
        /// </summary>
        public enum GameState
        {
            None,           // 未初始化
            Initializing,   // 初始化中
            Ready,          // 准备开始
            Playing,        // 游戏中
            Paused,         // 暂停
            GameOver,       // 游戏结束
        }

        /// <summary>
        /// 当前游戏状态
        /// </summary>
        public GameState CurrentState { get; protected set; } = GameState.None;

        /// <summary>
        /// 游戏业务管理器字典
        /// </summary>
        protected readonly Dictionary<Type, IGameManager> GameManagers = new Dictionary<Type, IGameManager>();

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

        /// <summary>
        /// 异步初始化游戏
        /// </summary>
        /// <returns></returns>
        public async UniTask InitGameAsync()
        {
            CurrentState = GameState.Initializing;

            // 初始化配置管理器
            await WaitForConfigManagerAsync();

            // 注册游戏业务管理器
            RegisterGameManagers();

            // 初始化游戏业务管理器
            InitializeGameManagers();

            // 调用子类初始化
            await OnGameInitAsync();

            // 完成初始化
            CurrentState = GameState.Ready;

            // 调用游戏准备事件
            OnGameReady();
        }

        /// <summary>
        /// 获取配置管理器实例（由子类实现）
        /// </summary>
        protected abstract BaseConfigManager GetConfigManager();

        /// <summary>
        /// 等待配置管理器初始化
        /// </summary>
        private async UniTask WaitForConfigManagerAsync()
        {
            Debug.Log("[GameLogic] 等待配置管理器初始化...");

            var configManager = GetConfigManager();
            configManager.Init();
            await configManager.WaitForInitializationAsync();

            Debug.Log("[GameLogic] 配置管理器初始化完成");
        }

        /// <summary>
        /// 注册游戏业务管理器（由子类实现）
        /// </summary>
        protected abstract void RegisterGameManagers();

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
        public T GetGameManager<T>() where T : class, IGameManager
        {
            if (GameManagers.TryGetValue(typeof(T), out var manager))
                return (T)manager;

            return null;
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

        #region 游戏状态控制方法
        public virtual void StartGame()
        {
            if (CurrentState != GameState.Ready)
            {
                Debug.LogWarning("[GameLogic] 游戏未准备好，无法开始");
                return;
            }

            CurrentState = GameState.Playing;
            OnGameStart();

            // 触发游戏开始事件
            GetFrameworkManager<EventManager>().Trigger("GameStarted");
        }

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
        #endregion

        #region 钩子虚方法供子类重写
        protected virtual UniTask OnGameInitAsync() => UniTask.CompletedTask;
        protected virtual void OnGameReady() { }
        protected virtual void OnGameStart() { }
        protected virtual void OnGamePause() { }
        protected virtual void OnGameResume() { }
        protected virtual void OnGameEnd() { }
        protected virtual void OnGamePlaying() { }
        protected virtual void OnGamePaused() { }
        protected virtual void OnGameOver() { }
        #endregion

        /// <summary>
        /// 等待初始化完成
        /// </summary>
        public async UniTask WaitForInitializationAsync()
        {
            while (CurrentState == GameState.None || CurrentState == GameState.Initializing)
            {
                await UniTask.Yield();
            }
        }
    }
}