using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Manager;
using UnityEngine;

namespace GameFramework.Game
{
    /// <summary>
    /// 游戏应用流程基类
    /// </summary>
    public abstract class GameAppFlow : MonoBehaviour
    {
        /// <summary>
        /// 游戏应用流程状态枚举
        /// </summary>
        private enum GameAppFlowState
        {
            /// <summary>
            /// 无状态
            /// </summary>
            None,

            /// <summary>
            /// 启动中
            /// </summary>
            Starting,

            /// <summary>
            /// 运行中
            /// </summary>
            Running,

            /// <summary>
            /// 退出中
            /// </summary>
            Exiting
        }

        /// <summary>
        /// 游戏应用流程当前状态
        /// </summary>
        private GameAppFlowState _currentState = GameAppFlowState.None;
        
        /// <summary>
        /// 游戏app是否开始
        /// </summary>
        public bool GameAppStarted { get; protected set; }

        /// <summary>
        /// 应用级管理器列表
        /// </summary>
        private readonly List<IAppManager> _appManagers = new List<IAppManager>();

        private void Update()
        {
            if (_currentState != GameAppFlowState.Running)
                return;

            float dt = Time.deltaTime;
            for (int i = 0; i < _appManagers.Count; i++)
            {
                if (_appManagers[i] is IAppUpdatable updatable)
                    updatable.DoUpdate(dt);
            }

            OnAppRunning(dt);
        }

        #region 公共方法
        /// <summary>
        /// 启动应用
        /// </summary>
        public async UniTask StartAppAsync()
        {
            _currentState = GameAppFlowState.Starting;

            await WaitForGameLauncherAsync();

            await WaitForConfigManagerAsync();

            GameLogin();
            
            RegisterAppManagers();
            foreach (var manager in _appManagers)
                manager.Init();
            
            await OnAppStartAsync();

            GetFrameworkManager<EventManager>().Trigger("GameAppStarted");

            Debug.Log($"[GameAppFlow] 启动应用成功------");
            GameAppStarted = true;
            _currentState = GameAppFlowState.Running;
        }

        /// <summary>
        /// 退出应用
        /// </summary>
        public async UniTask ExitAppAsync()
        {
            _currentState = GameAppFlowState.Exiting;

            await OnAppExitAsync();

            for (int i = _appManagers.Count - 1; i >= 0; i--)
                _appManagers[i].Dispose();
            _appManagers.Clear();

            _currentState = GameAppFlowState.None;
        }

        /// <summary>
        /// 获取框架管理器
        /// </summary>
        public T GetFrameworkManager<T>() where T : class, IManager
        {
            return GameFrameworkManager.Instance.GetManager<T>();
        }

        /// <summary>
        /// 获取应用级管理器
        /// </summary>
        public T GetAppManager<T>() where T : class, IAppManager
        {
            for (int i = 0; i < _appManagers.Count; i++)
            {
                if (_appManagers[i] is T result)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// 等待应用启动完成
        /// fixme 一直在等待 ，_currentState = none
        /// </summary>
        public async UniTask WaitForAppStartedAsync()
        {
            // while (_currentState == GameAppFlowState.None || _currentState == GameAppFlowState.Starting)
            while (_currentState != GameAppFlowState.Running)
            {
                // Debug.Log($"[GameAppFlow] 当前状态打印: {_currentState}------");
                await UniTask.Yield();
            }
            Debug.Log($"[GameAppFlow] 等待应用启动完成------------");
        }

        /// <summary>
        /// 等待应用启动完成
        /// </summary>
        public async UniTask WaitForGameAppStaredAsync()
        {
            while (!GameAppStarted)
            {
                await UniTask.Yield();
            }
            Debug.Log($"[GameAppFlow] 等待应用启动完成------------");
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 等待游戏启动器初始化
        /// </summary>
        private async UniTask WaitForGameLauncherAsync()
        {
            await GameFrameLauncher.Instance.WaitForInitializationAsync();
        }

        /// <summary>
        /// 等待配置管理器初始化
        /// </summary>
        private async UniTask WaitForConfigManagerAsync()
        {
            var config = GetConfigManager();
            config.Init();
            await config.WaitForInitializationAsync();
        }

        /// <summary>
        /// 获取配置管理器
        /// </summary>
        /// <returns>配置管理器</returns>
        protected abstract BaseConfigManager GetConfigManager();

        /// <summary>
        /// 游戏登录
        /// </summary>
        /// <returns></returns>
        protected abstract void GameLogin();

        /// <summary>
        /// 注册单个应用级管理器
        /// </summary>
        /// <param name="manager">应用级管理器</param>
        protected void RegisterAppManager(IAppManager manager)
        {
            _appManagers.Add(manager);
        }

        /// <summary>
        /// 注册应用级管理器
        /// </summary>
        protected abstract void RegisterAppManagers();
        #endregion

        #region 钩子方法
        /// <summary>
        /// 应用启动钩子
        /// </summary>
        protected virtual UniTask OnAppStartAsync() => UniTask.CompletedTask;

        /// <summary>
        /// 应用退出钩子
        /// </summary>
        protected virtual UniTask OnAppExitAsync() => UniTask.CompletedTask;

        /// <summary>
        /// 应用运行时钩子
        /// </summary>
        /// <param name="dt">时间增量</param>
        protected virtual void OnAppRunning(float dt) { }
        #endregion
    }
}