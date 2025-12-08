using Cysharp.Threading.Tasks;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

/// <summary>
/// Resources资源加载器 - 基于Unity Resources实现的资源加载器
/// </summary>
public class ResourcesResourceLoader : IResourceLoader
{
    private bool _isInitialized = false;
    
    public async UniTask<bool> InitializeAsync()
    {
        // Debug.Log("[ResourcesResourceLoader] 初始化Resources资源加载器");
        
        // Resources不需要特殊的初始化过程
        _isInitialized = true;
        
        // 模拟异步操作
        await UniTask.Yield();
        
        // Debug.Log("[ResourcesResourceLoader] Resources资源加载器初始化完成");
        return true;
    }
    
    public void SetHostServer(string hostServer)
    {
        // Resources不使用远程服务器，忽略此设置
        // Debug.Log($"[ResourcesResourceLoader] Resources模式不支持远程服务器设置，忽略: {hostServer}");
    }
    
    public async UniTask<string> RequestPackageVersionAsync()
    {
        // Resources不需要版本管理，返回本地版本
        await UniTask.Yield();
        return Application.version;
    }
    
    public async UniTask<bool> UpdatePackageManifestAsync(string packageVersion)
    {
        // Resources不需要更新清单，直接返回成功
        // Debug.Log($"[ResourcesResourceLoader] Resources模式不需要更新清单，版本: {packageVersion}");
        await UniTask.Yield();
        return true;
    }
    
    public ResourceDownloaderOperation CreateResourceDownloader()
    {
        // Resources不需要下载器，返回null或空的下载器操作
        // Debug.Log("[ResourcesResourceLoader] Resources模式不需要下载器，返回null");
        return null;
    }
    
    public async UniTask<T> LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object
    {
        if (!_isInitialized)
        {
            // Debug.LogError("[ResourcesResourceLoader] 资源加载器未初始化");
            return null;
        }

        string resourcesPrefix = "Assets/";
        if (assetPath.StartsWith(resourcesPrefix))
        {
            assetPath = assetPath.Substring(resourcesPrefix.Length);
        }

        // 从Resources目录加载资源
        // Debug.Log($"[ResourcesResourceLoader] 开始从Resources加载资源: {assetPath}");
        
        // 移除可能的文件扩展名，Resources.Load不需要扩展名
        string resourcePath = assetPath;
        int dotIndex = resourcePath.LastIndexOf('.');
        if (dotIndex >= 0)
        {
            resourcePath = resourcePath.Substring(0, dotIndex);
        }
        
        // 异步加载资源
        var request = Resources.LoadAsync<T>(resourcePath);
        
        // 等待加载完成
        while (!request.isDone)
        {
            await UniTask.Yield();
        }
        
        var asset = request.asset as T;
        if (asset == null)
        {
            // Debug.LogError($"[ResourcesResourceLoader] 从Resources加载资源失败: {assetPath}");
        }
        else
        {
            // Debug.Log($"[ResourcesResourceLoader] 从Resources加载资源成功: {assetPath}");
        }
        
        return asset;
    }
    
  
    /// <summary>
    /// 直接加载场景的便捷方法
    /// </summary>
    public async UniTask LoadSceneDirectAsync(string scenePath, bool activateOnLoad = true)
    {
        if (!_isInitialized)
        {
            // Debug.LogError("[ResourcesResourceLoader] 资源加载器未初始化");
            return;
        }
        
        // Debug.Log($"[ResourcesResourceLoader] 直接加载场景: {scenePath}");
        
        // 使用Unity的SceneManager直接加载场景
        var operation = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
        if (operation != null)
        {
            operation.allowSceneActivation = activateOnLoad;
            
            // 等待场景加载完成
            while (!operation.isDone)
            {
                await UniTask.Yield();
            }
            
            // Debug.Log($"[ResourcesResourceLoader] 场景加载完成: {scenePath}");
        }
        else
        {
            // Debug.LogError($"[ResourcesResourceLoader] 场景加载失败: {scenePath}");
        }
    }
    
    public void UnloadAllAssets()
    {
        // Debug.Log("[ResourcesResourceLoader] 卸载所有Resources资源");
        Resources.UnloadUnusedAssets();
    }
    
    public void Release()
    {
        // Debug.Log("[ResourcesResourceLoader] 释放Resources资源加载器");
        UnloadAllAssets();
        _isInitialized = false;
    }

    public SceneHandle LoadSceneAsync(string scenePath, bool activateOnLoad = true, uint priority = 100)
    {
        throw new System.NotImplementedException();
        //LoadSceneDirectAsync(scenePath, activateOnLoad);
    }
}



