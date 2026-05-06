using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YooAsset;
using Debug = UnityEngine.Debug;

namespace GameFramework.Game
{
    /// <summary>
    /// 游戏启动器 - 游戏入口点
    /// </summary>
    public class GameLauncher : MonoSingleton<GameLauncher>
    {
        /// <summary>
        /// 游戏配置
        /// </summary>
        [System.Serializable]
        public class GameConfig
        {
            // 游戏名称
            public string GameName = "GameName";

            // 游戏版本
            public string GameVersion = "1.0.0";

            // 游戏启动场景路径
            public string StartScenePath = "StartScenePath";

            // 默认语言
            public SystemLanguage DefaultLanguage = SystemLanguage.ChineseSimplified;

            // 目标帧率
            public int TargetFrameRate = 60;

            // 是否显示FPS
            public bool ShowFPS = false;

            // 资源加载模式
            public ResourceManager.ResourceLoadMode ResourceLoadMode = ResourceManager.ResourceLoadMode.YooAsset;

            // 是否启用CDN
            public bool IsCDNEnabled = true;

            // CDN地址
            public string CDNUrl = "http://127.0.0.1/CDN/";

            // 加载界面
            public GameObject loading;
        }

        /// <summary>
        /// 游戏配置
        /// </summary>
        [SerializeField] private GameConfig _gameConfig;

        // 资源更新UI
        [SerializeField] private GameObject _updatePanel;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Text _progressText;
        [SerializeField] private Text _statusText;
        [SerializeField] private Button _retryButton;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;

        /// <summary>W
        /// 初始化完成事件
        /// </summary>
        public event Action OnInitialized;

        /// <summary>
        /// 框架管理器
        /// </summary>
        private GameFrameworkManager _frameworkManager;
        /// <summary>
        /// 资源管理器
        /// </summary>
        private ResourceManager _resourceManager;

        /// <summary>
        /// 游戏逻辑类
        /// </summary>
        private GameLogic _gameLogic;

        protected override async void Awake()
        {
            base.Awake();
            await InitGameAsync();
        }

        /// <summary>
        /// 异步启动游戏
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async UniTask InitGameAsync()
        {
            Stopwatch sw = new Stopwatch();
            Debug.Log($"[GameLauncher] 启动游戏");

            sw.Start();
            // 显示游戏加载界面
            if (_gameConfig.loading) 
                _gameConfig.loading.SetActive(true);

            // 初始化框架
            InitFramework();

            // 初始化资源系统
            await InitResourceAsync();

            // 初始化游戏逻辑
            await InitGameLogicAsync();
            
            // 微信登录操作
            await PlatformLoginAsync();

            // 隐藏游戏加载界面
            if (_gameConfig.loading) 
                _gameConfig.loading.SetActive(false);
            
            sw.Stop();

            Debug.Log($"[{GetType().Name}] LaunchGameAsync 启动游戏异步花费：{sw.ElapsedMilliseconds / 1000f}");
        }

        // public void StartGame()
        // {
        //     _gameLogic.StartGameAsync();
        // }

        /// <summary>
        /// 框架初始化
        /// </summary>
        private void InitFramework()
        {
            // 初始化框架管理器
            var gameFrameworkManager = FindObjectOfType<GameFrameworkManager>();
            if (gameFrameworkManager == null)
            {
                var frameworkObj = new GameObject("GameFrameworkManager");
                _frameworkManager = frameworkObj.AddComponent<GameFrameworkManager>();
            }
            else
            {
                _frameworkManager = GameFrameworkManager.Instance;
            }
            _frameworkManager.InitFramework();
        }

        private async UniTask PlatformLoginAsync()
        {
            Debug.Log($"[GameLauncher] 登录");
            var platformManager = _frameworkManager.GetManager<PlatformManager>();
#if WEIXINMINIGAME
            platformManager.CurrentPlatform.Login(
            async (code) =>
            {
                Debug.Log($"[{GetType().Name}] 微信登录成功, code:{code}");
                await _gameLogic.Login(code);
                //做点别的 授权等
                platformManager.CurrentPlatform.CheckUserAuthorization(
                    (any) =>
                    {
                        
                    },
                    (error) =>
                    {
                        //未授权
                        // RectTransform rectTransform = _startButton.GetComponent<RectTransform>();
                        // float width = rectTransform.sizeDelta.x;
                        // float height = rectTransform.sizeDelta.y;
                        // platformManager.CurrentPlatform.CreateUserInfoButton((int)_startButton.transform.position.x,
                        //     (int)_startButton.transform.position.y, (int)width, (int)height,
                        //     (info) =>
                        //     {
                        //         //授权数据
                        //         // OnStartButtonClick();
                        //         // Debug.Log($"[UIMainMenu] 创建用户信息按钮 OnTap");
                        //     });
                    }
                );
            },
            (error) =>
            {
                Debug.LogError($"[GameLauncher] 微信登录失败, msg: {error}");
            });
#elif QUICKMINIGAME
            platformManager.CurrentPlatform.Login(null, (error) =>
                {
                    Debug.LogError($"[GameLauncher] 快游戏登录失败, msg: {error}");
                }, async providerLogin =>
                {
                    Debug.Log($"[GameLauncher] 快游戏登录成功, providerLogin: {providerLogin.Token}");
                    await _gameLogic.Login(string.Empty, providerLogin);
                });
#endif
            
        }

        /// <summary>
        /// 初始化游戏逻辑类
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">GameLogic子类对象未找到</exception>
        private async UniTask InitGameLogicAsync()
        { 
            // 初始化游戏逻辑类
            var gameLogic = FindObjectOfType<GameLogic>();
            if (gameLogic == null)
            {
                throw new InvalidOperationException("GameLogic子类对象未找到！");
            }

            Debug.Log($"[{GetType().Name}] {gameLogic.GetType().Name}");
            _gameLogic = gameLogic;
            await _gameLogic.InitGameAsync();
        }

        private async UniTask InitResourceAsync()
        {
            _resourceManager = _frameworkManager.GetManager<ResourceManager>();
            // Debug.Log($"[GameLauncher] _resourceManager 获取结果: {(_resourceManager != null ? "成功" : "失败")}");

            // 根据配置设置资源加载模式
            if (_resourceManager != null)
            {
                _resourceManager.SetResourceLoadMode(_gameConfig.ResourceLoadMode);
                // Debug.Log($"[GameLauncher] 设置资源加载模式为: {_gameConfig.ResourceLoadMode}");
            }

            bool resourceInitSuccess = await InitResourceSystemAsync();
            if (!resourceInitSuccess)
            {
                Debug.LogError("[GameLauncher] 资源系统初始化失败");
                return;
            }
            _isInitialized = true;
        }

        /// <summary>
        /// 初始化资源系统
        /// </summary>
        private async UniTask<bool> InitResourceSystemAsync()
        {
            // 显示更新面板
            if (_updatePanel != null)
                _updatePanel.SetActive(true);

            // 隐藏重试按钮
            if (_retryButton != null)
                _retryButton.gameObject.SetActive(false);

            // 更新状态
            UpdateStatus("正在初始化资源系统...");
            UpdateProgress(0);

            // 设置CDN地址（根据微信小游戏环境调整）
            string cdnUrl = _gameConfig.IsCDNEnabled ? _gameConfig.CDNUrl : "";
            _resourceManager.SetHostServer(cdnUrl);

            // 初始化资源加载器
            bool initSuccess = await _resourceManager.InitializeAsync();
            if (!initSuccess)
            {
                UpdateStatus("资源系统初始化失败");
                if (_retryButton != null)
                    _retryButton.gameObject.SetActive(true);
                return false;
            }

            // 尝试获取最新版本
            UpdateStatus("正在检查资源更新...");
            string latestVersion = await _resourceManager.RequestPackageVersionAsync();
            // string latestVersion = _resourceManager.RequestPackageVersion();

            Debug.Log($"[ResourceManager] LatestVersion: {latestVersion}");

            if (!string.IsNullOrEmpty(latestVersion))
            {
                // 网络正常，更新资源清单
                UpdateStatus("正在更新资源清单...");
                bool success = await _resourceManager.UpdatePackageManifestAsync(latestVersion);
                if (success)
                {
                    // 保存版本号，用于弱网环境
                    UnityEngine.PlayerPrefs.SetString("LastSuccessVersion", latestVersion);

                    // 创建下载器
                    var downloader = _resourceManager.CreateResourceDownloader();
                    if (downloader != null && downloader.TotalDownloadCount > 0)
                    {
                        // 有资源需要更新
                        UpdateStatus($"正在下载资源，总数: {downloader.TotalDownloadCount}，总大小: {downloader.TotalDownloadBytes / 1024f / 1024f:F2}MB");

                        // 注册下载进度回调
                        downloader.DownloadUpdateCallback = OnDownloadProgressCallback;
                        downloader.DownloadErrorCallback = OnDownloadErrorCallback;

                        // 开始下载
                        downloader.BeginDownload();
                        await downloader.Task;

                        // 检查下载结果
                        if (downloader.Status != YooAsset.EOperationStatus.Succeed)
                        {
                            UpdateStatus("资源下载失败，请检查网络后重试");
                            if (_retryButton != null)
                                _retryButton.gameObject.SetActive(true);
                            return false;
                        }
                    }
                }
                else
                {
                    // 更新资源清单失败，尝试使用本地版本
                    UpdateStatus("更新资源清单失败，尝试使用本地版本");
                    string localVersion = UnityEngine.PlayerPrefs.GetString("LastSuccessVersion", "");
                    if (!string.IsNullOrEmpty(localVersion))
                    {
                        bool localSuccess = await _resourceManager.UpdatePackageManifestAsync(localVersion);
                        if (!localSuccess)
                        {
                            UpdateStatus("加载本地资源清单失败");
                            if (_retryButton != null)
                                _retryButton.gameObject.SetActive(true);
                            return false;
                        }
                    }
                }
            }
            else
            {
                // 网络异常，尝试使用本地版本
                UpdateStatus("网络连接异常，尝试使用本地版本");
                string localVersion = UnityEngine.PlayerPrefs.GetString("LastSuccessVersion", "");
                if (!string.IsNullOrEmpty(localVersion))
                {
                    bool localSuccess = await _resourceManager.UpdatePackageManifestAsync(localVersion);
                    if (!localSuccess)
                    {
                        UpdateStatus("加载本地资源清单失败");
                        if (_retryButton != null)
                            _retryButton.gameObject.SetActive(true);
                        return false;
                    }
                }
            }

            // 资源初始化完成
            UpdateStatus("资源系统初始化完成");
            UpdateProgress(1);

            // 延迟关闭更新面板
            await UniTask.Delay(1500);
            if (_updatePanel != null)
                _updatePanel.SetActive(false);

            return true;
        }

        /// <summary>
        /// 下载进度回调
        /// </summary>
        private void OnDownloadProgressCallback(DownloadUpdateData data)
        {
            float progress = (float)data.CurrentDownloadBytes / data.TotalDownloadBytes;
            UpdateProgress(progress);
        }

        /// <summary>
        /// 下载错误回调
        /// </summary>
        private void OnDownloadErrorCallback(DownloadErrorData data)
        {
            Debug.LogError($"[ResourceUpdateManager] 资源下载出错: {data.FileName}, 错误: {data.ErrorInfo}");
        }


        /// <summary>
        /// 更新状态文本
        /// </summary>
        private void UpdateStatus(string status)
        {
            if (_statusText != null)
                _statusText.text = status;

            Debug.Log($"[GameLauncher] {status}");
        }

        /// <summary>
        /// 更新进度条
        /// </summary>
        private void UpdateProgress(float progress)
        {
            if (_progressBar != null)
                _progressBar.value = progress;

            if (_progressText != null)
                _progressText.text = $"{(int)(progress * 100)}%";
        }

        /// <summary>
        /// 重试资源更新
        /// </summary>
        private async void RetryResourceUpdate()
        {
            // 隐藏重试按钮
            if (_retryButton != null)
                _retryButton.gameObject.SetActive(false);

            UpdateStatus("正在重新尝试更新...");
            UpdateProgress(0);

            // 重新初始化资源系统
            bool success = await InitResourceSystemAsync();
            if (success)
            {
                // 游戏初始化完成
                _isInitialized = true;
                OnInitialized?.Invoke(); // 触发事件
                Debug.Log("[GameLauncher] 重试游戏初始化完成");
            }
        }

        /// <summary>
        /// 等待初始化完成
        /// </summary>
        public async UniTask WaitForInitializationAsync()
        {
            while (!_isInitialized)
            {
                await UniTask.Yield();
            }
        }
    }

   
}