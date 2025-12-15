using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Network;
using UnityEngine;

namespace GameFramework.Game
{
    /// <summary>
    /// 游戏逻辑基类 - 具体游戏逻辑的基类
    /// </summary>
    public abstract class GameLogic : MonoSingleton<GameLogic>
    {
        // 游戏状态
        public enum GameState
        {
            None, // 未初始化
            Initializing, // 加载中
            Ready, // 准备开始
            Playing, // 游戏中
            Paused, // 暂停
            GameOver, // 游戏结束
        }

        // 当前游戏状态
        public GameState CurrentState { get; protected set; } = GameState.None;
        
        public bool IsInitialized { get; private set; }
        
        protected readonly Dictionary<Type, IGameManager> GameManagers = new Dictionary<Type, IGameManager>();

        // 游戏帧更新
        protected virtual void Update()
        {
            // 根据游戏状态更新
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

        // 初始化游戏
        public async UniTask InitGameAsync()
        {
            // Debug.Log("[GameLogic] 初始化游戏");
            CurrentState = GameState.Initializing;

            // 游戏特定的初始化
            RegisterGameManagers();
            InitializeGameManagers();
            
            await OnGameInitAsync();
            OnGameInit();
            
            IsInitialized = true;

            // 加载完成，进入准备状态
            CurrentState = GameState.Ready;

            // 调用游戏准备事件
            OnGameReady();
        }

        private void OnDestroy()
        {
            ReleaseGameManagers();
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

        public async UniTask WaitForInitializationAsync()
        {
            while (!IsInitialized)
            {
                await UniTask.Yield();
            }
        }

        protected abstract void RegisterGameManagers();

        protected void RegisterGameManager<T>() where T : BaseGameManager
        {
            var type = typeof(T);
            if (GameManagers.ContainsKey(type))
            {
                return;
            }
            var manager = FindObjectOfType<T>();
            if (manager == null)
            {
                var go = new GameObject(type.Name);
                manager = go.AddComponent<T>();
            }

            GameManagers[type] = manager;
        }

        public T GetGameManager<T>() where T : BaseGameManager
        {
            if (GameManagers.TryGetValue(typeof(T), out var manager))
            {
                return (T)manager;
            }
            return null;
        }

        // 游戏初始化，子类实现
        protected abstract void OnGameInit();

        // 游戏异步初始化，子类实现
        protected abstract UniTask OnGameInitAsync();

        // 游戏准备，子类实现
        protected abstract void OnGameReady();

        // 开始游戏
        public virtual void StartGame()
        {
            if (CurrentState != GameState.Ready)
            {
                // Debug.LogWarning("[GameLogic] 游戏未准备好，无法开始");
                return;
            }
            
            CurrentState = GameState.Playing;
            OnGameStart();
        }

        // 游戏开始，子类实现
        protected abstract void OnGameStart();

        // 暂停游戏
        public virtual void PauseGame()
        {
            if (CurrentState != GameState.Playing)
            {
                // Debug.LogWarning("[GameLogic] 游戏未进行中，无法暂停");
                return;
            }

            CurrentState = GameState.Paused;
            OnGamePause();
        }

        // 游戏暂停，子类实现
        protected abstract void OnGamePause();

        // 恢复游戏
        public virtual void ResumeGame()
        {
            if (CurrentState != GameState.Paused)
            {
                // Debug.LogWarning("[GameLogic] 游戏未暂停，无法恢复");
                return;
            }

            CurrentState = GameState.Playing;
            OnGameResume();
        }

        // 游戏恢复，子类实现
        protected abstract void OnGameResume();

        // 结束游戏
        public virtual void EndGame()
        {
            if (CurrentState != GameState.Playing && CurrentState != GameState.Paused)
            {
                // Debug.LogWarning("[GameLogic] 游戏未进行中或暂停，无法结束");
                return;
            }

            CurrentState = GameState.GameOver;
            OnGameEnd();
        }

        // 游戏结束，子类实现
        protected abstract void OnGameEnd();

        // 重新开始游戏
        public virtual async void RestartGame()
        {
            // Debug.Log("[GameLogic] 重新开始游戏");
            // 结束当前游戏
            if (CurrentState == GameState.Playing || CurrentState == GameState.Paused)
            {
                EndGame();
            }

            // 重新初始化游戏
            await InitGameAsync();
        }

        // 游戏进行中更新，子类实现
        protected abstract void OnGamePlaying();

        // 游戏暂停中更新，子类实现
        protected abstract void OnGamePaused();

        // 游戏结束后更新，子类实现
        protected abstract void OnGameOver();
    }
}