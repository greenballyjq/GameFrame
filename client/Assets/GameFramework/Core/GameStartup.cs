//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;

//namespace GameFramework.Core
//{
//    /// <summary>
//    /// 游戏启动脚本 - 演示资源初始化和热更新流程
//    /// </summary>
//    public class GameStartup : MonoBehaviour
//    {
//        // 资源更新UI
//        [SerializeField] private GameObject _updatePanel;
//        [SerializeField] private Slider _progressBar;
//        [SerializeField] private Text _progressText;
//        [SerializeField] private Text _statusText;
//        [SerializeField] private Button _retryButton;
        
//        // 远程资源服务器地址
//        [SerializeField] private string _defaultHostServer = "http://127.0.0.1/CDN/";
//        [SerializeField] private string _fallbackHostServer = "http://127.0.0.1/CDN/";
        
//        // 资源管理器引用
//        private ResourceManager _resourceManager;
        
//        // 资源更新管理器引用
//        private ResourceUpdateManager _resourceUpdateManager;
        
//        // 是否初始化完成
//        private bool _isInitialized = false;
        
//        private void Start()
//        {
//            // 隐藏更新面板
//            if (_updatePanel != null)
//                _updatePanel.SetActive(false);
            
//            // 等待框架初始化完成
//            GameFrameworkManager.Instance.OnInitialized += OnFrameworkInitialized;
            
//            // 如果框架已经初始化完成，则直接调用
//            if (GameFrameworkManager.Instance.IsInitialized)
//            {
//                OnFrameworkInitialized();
//            }
//        }
        
//        /// <summary>
//        /// 框架初始化完成回调
//        /// </summary>
//        private void OnFrameworkInitialized()
//        {
//            _resourceManager = GameFrameworkManager.Instance.GetManager<ResourceManager>();
            
//            // 创建资源更新管理器
//            var updateObj = new GameObject("ResourceUpdateManager");
//            _resourceUpdateManager = updateObj.AddComponent<ResourceUpdateManager>();
            
//            // 设置远程服务器地址
//            _resourceManager.SetHostServer(_defaultHostServer, _fallbackHostServer);
            
//            // 注册更新事件
//            _resourceUpdateManager.OnUpdateProgressChanged += OnUpdateProgressChanged;
//            _resourceUpdateManager.OnUpdateCompleted += OnUpdateCompleted;
            
//            // 绑定重试按钮
//            if (_retryButton != null)
//                _retryButton.onClick.AddListener(RetryUpdate);
            
//            // 开始资源初始化
//            StartCoroutine(StartResourceInitialize());
//        }
        
//        /// <summary>
//        /// 开始资源初始化
//        /// </summary>
//        private IEnumerator StartResourceInitialize()
//        {
//            // 显示更新面板
//            if (_updatePanel != null)
//                _updatePanel.SetActive(true);
            
//            // 设置初始状态
//            UpdateStatus("正在初始化资源系统...");
//            UpdateProgress(0);
            
//            // 隐藏重试按钮
//            if (_retryButton != null)
//                _retryButton.gameObject.SetActive(false);
            
//            // 等待一帧，确保UI已更新
//            yield return null;
            
//            // 开始资源更新
//            _resourceUpdateManager.StartUpdateResources();
//        }
        
//        /// <summary>
//        /// 更新进度回调
//        /// </summary>
//        private void OnUpdateProgressChanged(float progress)
//        {
//            UpdateProgress(progress);
//        }
        
//        /// <summary>
//        /// 更新完成回调
//        /// </summary>
//        private void OnUpdateCompleted(bool success)
//        {
//            if (success)
//            {
//                UpdateStatus("资源更新完成");
//                UpdateProgress(1);
                
//                // 初始化完成
//                _isInitialized = true;
                
//                // 隐藏重试按钮
//                if (_retryButton != null)
//                    _retryButton.gameObject.SetActive(false);
                
//                // 延迟关闭更新面板
//                StartCoroutine(DelayHideUpdatePanel());
                
//                // 进入游戏
//                EnterGame();
//            }
//            else
//            {
//                UpdateStatus("资源更新失败，请检查网络后重试");
                
//                // 显示重试按钮
//                if (_retryButton != null)
//                    _retryButton.gameObject.SetActive(true);
//            }
//        }
        
//        /// <summary>
//        /// 更新状态文本
//        /// </summary>
//        private void UpdateStatus(string status)
//        {
//            if (_statusText != null)
//                _statusText.text = status;
            
//            Debug.Log($"[GameStartup] {status}");
//        }
        
//        /// <summary>
//        /// 更新进度条
//        /// </summary>
//        private void UpdateProgress(float progress)
//        {
//            if (_progressBar != null)
//                _progressBar.value = progress;
            
//            if (_progressText != null)
//                _progressText.text = $"{(int)(progress * 100)}%";
//        }
        
//        /// <summary>
//        /// 重试更新
//        /// </summary>
//        private void RetryUpdate()
//        {
//            // 隐藏重试按钮
//            if (_retryButton != null)
//                _retryButton.gameObject.SetActive(false);
            
//            UpdateStatus("正在重新尝试更新...");
//            UpdateProgress(0);
            
//            // 重新开始更新
//            _resourceUpdateManager.StartUpdateResources();
//        }
        
//        /// <summary>
//        /// 延迟隐藏更新面板
//        /// </summary>
//        private IEnumerator DelayHideUpdatePanel()
//        {
//            yield return new WaitForSeconds(1.5f);
            
//            if (_updatePanel != null)
//                _updatePanel.SetActive(false);
//        }
        
//        /// <summary>
//        /// 进入游戏
//        /// </summary>
//        private void EnterGame()
//        {
//            Debug.Log("[GameStartup] 进入游戏");
            
//            // 在这里加载游戏主场景或执行其他初始化逻辑
//            // 例如：
//            // _resourceManager.LoadSceneAsync("Assets/Scenes/MainScene.unity");
//        }
//    }
//} 