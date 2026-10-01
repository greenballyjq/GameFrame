using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Utility;
using UnityEngine;

/// <summary>
/// Resources资源加载器 - 基于Unity Resources实现的资源加载器
/// </summary>
public class ResourcesResourceLoader : IResourceLoader
{
    private bool _isInitialized = false;
    
    public async UniTask<bool> InitializeAsync()
    {
        Log.Info("[ResourcesResourceLoader] 初始化Resources资源加载器");
        
        // Resources不需要特殊的初始化过程
        _isInitialized = true;
        
        // 模拟异步操作
        await UniTask.Yield();
        
        Log.Info("[ResourcesResourceLoader] Resources资源加载器初始化完成");
        return true;
    }

    public async UniTask<T> LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object
    {
        if (!_isInitialized)
        {
            Log.Error("[ResourcesResourceLoader] 资源加载器未初始化");
            return null;
        }

        string resourcesPrefix = "Assets/";
        if (assetPath.StartsWith(resourcesPrefix))
        {
            assetPath = assetPath.Substring(resourcesPrefix.Length);
        }

        // 从Resources目录加载资源
        Log.Info($"[ResourcesResourceLoader] 开始从Resources加载资源: {assetPath}");
        
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
            Log.Error($"[ResourcesResourceLoader] 从Resources加载资源失败: {assetPath}");
        }
        else
        {
            Log.Info($"[ResourcesResourceLoader] 从Resources加载资源成功: {assetPath}");
        }
        
        return asset;
    }

    public void ReleaseAsset(string assetPath)
    {
    }

    public void UnloadAllAssets()
    {
        Log.Info("[ResourcesResourceLoader] 卸载所有Resources资源");
        Resources.UnloadUnusedAssets();
    }
    
    public void Release()
    {
        Log.Info("[ResourcesResourceLoader] 释放Resources资源加载器");
        UnloadAllAssets();
        _isInitialized = false;
    }
}



