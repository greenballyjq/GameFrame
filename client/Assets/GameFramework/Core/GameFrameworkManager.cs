using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework.Audio;
using GameFramework.Manager;
using GameFramework.UI;
using GameFramework.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameFramework.Core
{
    /// <summary>
    /// 游戏框架管理器
    /// </summary>
    public class GameFrameworkManager : MonoSingleton<GameFrameworkManager>
    {
        /// <summary>
        /// 游戏配置
        /// </summary>
        [Serializable]
        public class GameConfig
        {
            public ResourceManager.ResourceLoadMode ResourceLoadMode;
            public GameObject loading;
            public UISettings UISettings;
            [FormerlySerializedAs("AudioSettings")]
            public AudioConfig AudioConfig;
        }

        [SerializeField] private GameConfig _gameConfig;

        private readonly Dictionary<Type, IManager> _managers = new Dictionary<Type, IManager>();
        private readonly List<IManager> _managerList = new List<IManager>();
        private readonly List<IManagerUpdatable> _updatables = new List<IManagerUpdatable>();
        private readonly UniTaskCompletionSource _initialized = new UniTaskCompletionSource();

        private bool _isInitialized;
        public bool IsInitialized => _isInitialized;

        public event Action OnInitialized;

        public UniTask WaitForInitializationAsync() => _initialized.Task;

        protected override async void OnSingletonAwake()
        {
            await InitFrameworkAsync();
        }

        private async UniTask InitFrameworkAsync()
        {
            Log.Info("[GameFramework] 开始初始化框架...");

            if (_gameConfig.loading)
                _gameConfig.loading.SetActive(true);

            RegisterManager(new EventManager());
            RegisterManager(new ResourceManager(_gameConfig.ResourceLoadMode));
            RegisterManager(new GameObjectPoolManager());
            RegisterManager(new UIManager(_gameConfig.UISettings));
            RegisterManager(new AudioManager(_gameConfig.AudioConfig));
            RegisterManager(new EffectManager());

            for (int i = 0; i < _managerList.Count; i++)
                await _managerList[i].InitAsync();

            Log.Info("[GameFramework] 框架初始化完成");
            _isInitialized = true;
            _initialized.TrySetResult();
            OnInitialized?.Invoke();
        }

        public void RegisterManager<T>(T manager) where T : class, IManager
        {
            _managers.Add(typeof(T), manager);
            _managerList.Add(manager);
            if (manager is IManagerUpdatable updatable)
                _updatables.Add(updatable);
            Log.Info($"[GameFramework] 注册管理器: {typeof(T).Name}");
        }

        public T GetManager<T>() where T : class, IManager
        {
            return (T)_managers[typeof(T)];
        }

        private void Update()
        {
            if (!_isInitialized)
                return;

            for (int i = 0; i < _updatables.Count; i++)
                _updatables[i].DoUpdate(Time.deltaTime);
        }

        protected override void OnSingletonDestroy()
        {
            for (int i = _managerList.Count - 1; i >= 0; i--)
                _managerList[i].Dispose();
            _managers.Clear();
            _managerList.Clear();
            _updatables.Clear();
        }
    }
}
