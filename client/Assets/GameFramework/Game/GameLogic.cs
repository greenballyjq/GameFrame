using Cysharp.Threading.Tasks;
using GameFramework.Game;
using GameFramework.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Core
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

        /// <summary>
        /// 游戏应用当前状态
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

            // 注册所有游戏业务管理器
            RegisterGameManagers();

            // 初始化所有游戏业务管理器
            InitializeGameManagers();

            // 调用游戏应用初始化钩子
            await OnGameInitAsync();

            // 完成初始化
            CurrentState = GameState.Ready;

            // 进入准备就绪状态
            OnGameReady();
        }

        /// <summary>
        /// 游戏应用开始
        /// </summary>
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
        public T GetGameManager<T>() where T : class, IGameManager
        {
            if (GameManagers.TryGetValue(typeof(T), out var manager))
                return (T)manager;

            return null;
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

        /// <summary>
        /// 注册所有游戏业务管理器（由子类实现）
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
        protected virtual void OnGameStart() { }

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
        #endregion
    }
}