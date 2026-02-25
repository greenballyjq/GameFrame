using Cysharp.Threading.Tasks;
using GameFramework.Core;
using UnityEngine;
using YooAsset;

namespace GameFramework.Manager
{
    /// <summary>
    /// 资源管理器 - 支持多种资源加载方式的统一管理器
    /// </summary>
    public class ResourceManager : IManager
    {
        /// <summary>
        /// 资源加载模式
        /// </summary>
        public enum ResourceLoadMode
        {
            YooAsset,   // 使用YooAsset加载
            Resources   // 使用Unity Resources加载
        }
    
        // 当前使用的资源加载器
        private IResourceLoader _currentLoader;
    
        // 当前资源加载模式
        private ResourceLoadMode _currentMode = ResourceLoadMode.YooAsset;
    
        // 远程资源地址
        private string _defaultHostServer = "http://127.0.0.1/CDN/";
    
        public void Init()
        {
            Debug.Log("[ResourceManager] 初始化资源管理器");
        
            // 默认使用YooAsset模式
            SetResourceLoadMode(ResourceLoadMode.YooAsset);
        }
    
        /// <summary>
        /// 设置资源加载模式
        /// </summary>
        public void SetResourceLoadMode(ResourceLoadMode mode)
        {
            if (_currentMode == mode && _currentLoader != null)
            {
                Debug.Log($"[ResourceManager] 资源加载模式已经是 {mode}，无需切换");
                return;
            }
        
            Debug.Log($"[ResourceManager] 切换资源加载模式从 {_currentMode} 到 {mode}");
        
            // 释放当前加载器
            if (_currentLoader != null)
            {
                _currentLoader.Release();
                _currentLoader = null;
            }
        
            // 创建新的加载器
            _currentMode = mode;
            switch (mode)
            {
                case ResourceLoadMode.YooAsset:
                    _currentLoader = new YooAssetResourceLoader();
                    break;
                case ResourceLoadMode.Resources:
                    _currentLoader = new ResourcesResourceLoader();
                    break;
                default:
                    Debug.LogError($"[ResourceManager] 不支持的资源加载模式: {mode}");
                    break;
            }
        
            Debug.Log($"[ResourceManager] 资源加载模式切换完成: {mode}");
        }


        public void Release()
        {
            if (_currentLoader != null)
            {
                _currentLoader.Release();
                _currentLoader = null;
            }
        }
    
        /// <summary>
        /// 获取当前资源加载模式
        /// </summary>
        public ResourceLoadMode GetCurrentMode()
        {
            return _currentMode;
        }
    
        /// <summary>
        /// 设置远程服务器地址
        /// </summary>
        public void SetHostServer(string defaultHostServer, string fallbackHostServer = null)
        {
            _defaultHostServer = defaultHostServer;
        
            if (_currentLoader != null)
            {
                _currentLoader.SetHostServer(defaultHostServer);
            }
        }
    
        /// <summary>
        /// 初始化资源包
        /// </summary>
        public async UniTask<bool> InitializeAsync()
        {
            if (_currentLoader == null)
            {
                Debug.LogError("[ResourceManager] 当前没有可用的资源加载器");
                return false;
            }
        
            return await _currentLoader.InitializeAsync();
        }
    
        /// <summary>
        /// 初始化资源包 (Task版本，向后兼容)
        /// </summary>
        public async UniTask<bool> InitPackage(string packageName)
        {
            return await InitializeAsync();
        }
    
    
        /// <summary>
        /// 请求资源包版本 (向后兼容)
        /// </summary>
        public string RequestPackageVersion(string packageName = null, int timeout = 30)
        {
            // 由于是同步方法，这里返回当前应用版本作为默认值
            return Application.version;
        }

        public async UniTask<string> RequestPackageVersionAsync(string packageName = null, int timeout = 30)
        {
            if (_currentLoader == null)
            {
                Debug.LogError("[ResourceManager] 当前没有可用的资源加载器");
                return null;
            }
        
            return await _currentLoader.RequestPackageVersionAsync();
        }
    

    
        /// <summary>
        /// 更新资源包清单
        /// </summary>
        public async UniTask<bool> UpdatePackageManifestAsync(string packageVersion, string packageName = null)
        {
            if (_currentLoader == null)
            {
                Debug.LogError("[ResourceManager] 当前没有可用的资源加载器");
                return false;
            }
        
            return await _currentLoader.UpdatePackageManifestAsync(packageVersion);
        }
    
        /// <summary>
        /// 创建资源下载器
        /// </summary>
        public ResourceDownloaderOperation CreateResourceDownloader(string packageName = null, int downloadingMaxNumber = 10, int failedTryAgain = 3, int timeout = 60)
        {
            if (_currentLoader == null)
            {
                Debug.LogError("[ResourceManager] 当前没有可用的资源加载器");
                return null;
            }
        
            return _currentLoader.CreateResourceDownloader();
        }
    
        /// <summary>
        /// 创建资源下载器（按标签）
        /// </summary>
        public ResourceDownloaderOperation CreateResourceDownloaderByTags(string packageName = null, int downloadingMaxNumber = 10, int failedTryAgain = 3, int timeout = 60)
        {
            return CreateResourceDownloader(packageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
    
        /// <summary>
        /// 异步加载资源
        /// </summary>
        public async UniTask<T> LoadAssetAsync<T>(string assetPath, string packageName = null) where T : UnityEngine.Object
        {
            if (_currentLoader == null)
            {
                Debug.LogError("[ResourceManager] 当前没有可用的资源加载器");
                return null;
            }
        
            return await _currentLoader.LoadAssetAsync<T>(assetPath);
        }
    
        /// <summary>
        /// 异步加载场景
        /// </summary>
        public SceneHandle LoadSceneAsync(string scenePath, string packageName = null, bool activateOnLoad = true, uint priority = 100)
        {
            if (_currentLoader == null)
            {
                Debug.LogError("[ResourceManager] 当前没有可用的资源加载器");
                return null;
            }
        
            return _currentLoader.LoadSceneAsync(scenePath, activateOnLoad, priority);
        }
    
        /// <summary>
        /// 卸载所有资源
        /// </summary>
        public void UnloadAllAssets(string packageName = null)
        {
            if (_currentLoader == null)
            {
                Debug.LogError("[ResourceManager] 当前没有可用的资源加载器");
                return;
            }
        
            _currentLoader.UnloadAllAssets();
        }

        public void DoUpdate(float dt)
        {
            // 根据需要更新当前加载器
        }
    }
}