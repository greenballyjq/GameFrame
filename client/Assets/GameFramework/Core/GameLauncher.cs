using System.Threading.Tasks;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace GameFramework.Core
{
    /// <summary>
    /// 游戏启动器 - 游戏入口点
    /// </summary>
    public class GameLauncher : MonoBehaviour
    {
        // 单例实例
        private static GameLauncher _instance;
        public static GameLauncher Instance => _instance;
        
        // 是否已初始化
        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;
        
        // 游戏配置
        [SerializeField] private GameConfig _gameConfig;
        
        // 资源更新UI
        [SerializeField] private GameObject _updatePanel;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Text _progressText;
        [SerializeField] private Text _statusText;
        [SerializeField] private Button _retryButton;
        
        // 框架管理器
        private GameFrameworkManager _frameworkManager;
        
        // 平台管理器
        private PlatformManager _platformManager;
        
        // 资源管理器
        private ResourceManager _resourceManager;
        
        
        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 创建框架管理器
            var frameworkObj = new GameObject("GameFrameworkManager");
            frameworkObj.transform.SetParent(transform);
            _frameworkManager = frameworkObj.AddComponent<GameFrameworkManager>();
        }
        
        private async void Start()
        {
            // 初始化框架
            _frameworkManager.OnInitialized += OnFrameworkInitialized;
            
            // 初始化游戏
            await InitGameAsync();
        }
        
        private void Update()
        {
            if (!_isInitialized)
                return;
            
            // 游戏逻辑更新
        }
        
        /// <summary>
        /// 框架初始化完成回调
        /// </summary>
        private void OnFrameworkInitialized()
        {
            Debug.Log("[GameLauncher] 框架初始化完成");
            
            // 获取各个管理器
            _platformManager = _frameworkManager.GetManager<PlatformManager>();
            _resourceManager = _frameworkManager.GetManager<ResourceManager>();
            
            // 初始化UI
            if (_updatePanel != null)
                _updatePanel.SetActive(false);
                
            if (_retryButton != null)
                _retryButton.onClick.AddListener(RetryResourceUpdate);
        }
        
        /// <summary>
        /// 初始化游戏
        /// </summary>
        private async Task InitGameAsync()
        {
            Debug.Log("[GameLauncher] 开始初始化游戏");
            
            // 等待框架初始化完成
            while (!_frameworkManager.IsInitialized)
            {
                await Task.Delay(10);
            }
            
            // 初始化资源系统
            bool resourceInitSuccess = await InitResourceSystem();
            if (!resourceInitSuccess)
            {
                Debug.LogError("[GameLauncher] 资源系统初始化失败");
                return;
            }
            
            // 加载游戏启动场景
            _resourceManager.LoadSceneAsync(_gameConfig.StartScenePath);
            
            // 游戏初始化完成
            _isInitialized = true;
            
            Debug.Log("[GameLauncher] 游戏初始化完成");
        }
        
        
        
        /// <summary>
        /// 初始化资源系统
        /// </summary>
        private async Task<bool> InitResourceSystem()
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
            
            // 初始化默认资源包
            bool initSuccess = await _resourceManager.InitPackage("DefaultPackage");
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
            await Task.Delay(1500);
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
            bool success = await InitResourceSystem();
            if (success)
            {
                // 继续游戏初始化流程
                _resourceManager.LoadSceneAsync(_gameConfig.StartScenePath);
                _isInitialized = true;
                Debug.Log("[GameLauncher] 游戏初始化完成");
            }
        }
    }
    
    /// <summary>
    /// 游戏配置
    /// </summary>
    [System.Serializable]
    public class GameConfig
    {
        // 游戏名称
        public string GameName = "GooseCatcher";
        
        // 游戏版本
        public string GameVersion = "1.0.0";
        
        // 游戏启动场景路径
        public string StartScenePath = "Assets/Scenes/GameMain.unity";
        
        // 默认语言
        public SystemLanguage DefaultLanguage = SystemLanguage.ChineseSimplified;
        
        // 目标帧率
        public int TargetFrameRate = 60;
        
        // 是否显示FPS
        public bool ShowFPS = false;
        
        // 是否启用CDN
        public bool IsCDNEnabled = true;
        
        // CDN地址
        public string CDNUrl = "http://127.0.0.1/CDN/";
    }
} 