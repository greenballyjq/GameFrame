using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Manager;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameFramework.Game
{
    /// <summary>
    /// 游戏应用流程基类
    /// </summary>
    public abstract class GameAppFlow<T> : MonoSingleton<T> where T : GameAppFlow<T>
    {
        private enum EAppFlowState
        {
            None,
            Starting,
            Running,
            Exiting
        }

        private readonly Dictionary<Type, IAppManager> _appManagers = new Dictionary<Type, IAppManager>();
        private readonly List<IAppManager> _appManagerList = new List<IAppManager>();
        private readonly List<IAppUpdatable> _updatables = new List<IAppUpdatable>();
        private readonly UniTaskCompletionSource _started = new UniTaskCompletionSource();

        private EAppFlowState _state = EAppFlowState.None;

        public async UniTask StartAppAsync()
        {
            if (_state != EAppFlowState.None)
                return;

            _state = EAppFlowState.Starting;

            await GameFrameworkManager.Instance.WaitForInitializationAsync();

            RegisterAppManagers();
            for (int i = 0; i < _appManagerList.Count; i++)
                await _appManagerList[i].InitAsync();

            await OnAppStartAsync();

            if (_state != EAppFlowState.Starting)
                return;

            GameFrameworkManager.Instance.GetManager<EventManager>().PushScope(ResourceScope.App);
            GameFrameworkManager.Instance.GetManager<EventManager>().Trigger("GameAppStarted");
            _state = EAppFlowState.Running;
            _started.TrySetResult();
        }

        public async UniTask ExitAppAsync()
        {
            if (_state == EAppFlowState.Exiting)
                return;

            _state = EAppFlowState.Exiting;

            await OnAppExitAsync();

            for (int i = _appManagerList.Count - 1; i >= 0; i--)
                _appManagerList[i].Dispose();
            _appManagers.Clear();
            _appManagerList.Clear();
            _updatables.Clear();

            Quit();
        }

        public TManager GetAppManager<TManager>() where TManager : class, IAppManager
        {
            return (TManager)_appManagers[typeof(TManager)];
        }

        public UniTask WaitForAppStartedAsync() => _started.Task;

        protected void RegisterAppManager(IAppManager manager)
        {
            _appManagers.Add(manager.GetType(), manager);
            _appManagerList.Add(manager);
            if (manager is IAppUpdatable updatable)
                _updatables.Add(updatable);
        }

        protected abstract void RegisterAppManagers();

        protected virtual UniTask OnAppStartAsync() => UniTask.CompletedTask;

        protected virtual UniTask OnAppExitAsync() => UniTask.CompletedTask;

        protected virtual void OnAppRunning(float dt) { }

        private void Start()
        {
            StartAppAsync().Forget();
        }

        private void OnApplicationQuit()
        {
            ExitAppAsync().Forget();
        }

        private void Update()
        {
            if (_state != EAppFlowState.Running)
                return;

            float dt = Time.deltaTime;
            for (int i = 0; i < _updatables.Count; i++)
                _updatables[i].DoUpdate(dt);

            OnAppRunning(dt);
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
