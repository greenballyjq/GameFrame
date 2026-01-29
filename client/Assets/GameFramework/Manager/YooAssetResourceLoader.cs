using Cysharp.Threading.Tasks;
using GameFramework.Core;
using System.Collections.Generic;
using UnityEngine;
#if WEIXINMINIGAME
using WeChatWASM;
#endif
using YooAsset;

/// <summary>
/// YooAsset资源加载器 - 基于YooAsset实现的资源加载器
/// </summary>
public class YooAssetResourceLoader : IResourceLoader
{
    // 默认资源包名称
    private const string DefaultPackageName = "DefaultPackage";
    
    // 资源包字典
    private readonly Dictionary<string, ResourcePackage> _packages = new Dictionary<string, ResourcePackage>();
    
    // 默认资源包
    public ResourcePackage DefaultPackage => GetPackage(DefaultPackageName);
    
    // 资源系统运行模式
    private EPlayMode _playMode = EPlayMode.EditorSimulateMode;
    
    // 远程资源地址
    private string _defaultHostServer = "http://127.0.0.1/CDN/";
    private string _fallbackHostServer = "http://127.0.0.1/CDN/";
    
    public async UniTask<bool> InitializeAsync()
    {
        Debug.Log("[YooAssetResourceLoader] 初始化YooAsset资源加载器");
        
        // 初始化YooAsset
        YooAssets.Initialize();

        // 设置运行模式
#if UNITY_EDITOR
        _playMode = EPlayMode.EditorSimulateMode;
#elif UNITY_WEBGL
        _playMode = EPlayMode.WebPlayMode;
#else
        _playMode = EPlayMode.HostPlayMode;
#endif
        _playMode = EPlayMode.WebPlayMode;

        Debug.Log($"[YooAssetResourceLoader] 当前运行模式：{_playMode}");

        // 创建默认资源包
        var defaultPackage = CreatePackage(DefaultPackageName);
        YooAssets.SetDefaultPackage(defaultPackage);
        
        // 初始化默认资源包
        return await InitPackage(DefaultPackageName);
    }
    
    public void SetHostServer(string hostServer)
    {
        _defaultHostServer = hostServer;
        _fallbackHostServer = hostServer;
    }
    
    public async UniTask<string> RequestPackageVersionAsync()
    {
        var package = GetPackage(DefaultPackageName);
        if (package == null)
        {
            Debug.LogError("[YooAssetResourceLoader] 请求资源包版本失败: 默认资源包不存在");
            return null;
        }
        
        var op = package.RequestPackageVersionAsync();
        await op;

        return op.PackageVersion;
    }
    
    public async UniTask<bool> UpdatePackageManifestAsync(string packageVersion)
    {
        Debug.Log($"[YooAssetResourceLoader] UpdatePackageManifestAsync packageVersion: {packageVersion}");
        var package = GetPackage(DefaultPackageName);
        if (package == null)
        {
            Debug.LogError("[YooAssetResourceLoader] 更新资源包清单失败: 默认资源包不存在");
            return false;
        }

        var operation = package.UpdatePackageManifestAsync(packageVersion);
        await operation.ToUniTask();
        
        Debug.Log($"[YooAssetResourceLoader] 资源包清单更新状态: {operation.Status}, {(operation.Status == EOperationStatus.Succeed ? "成功" : $"失败: {operation.Error}")}");
        
        return operation.Status == EOperationStatus.Succeed;
    }
    
    public ResourceDownloaderOperation CreateResourceDownloader()
    {
        var package = GetPackage(DefaultPackageName);
        if (package == null)
        {
            Debug.LogError("[YooAssetResourceLoader] 创建资源下载器失败: 默认资源包不存在");
            return null;
        }
        
        var downloader = package.CreateResourceDownloader(10, 3);
        return downloader;
    }
    
    public async UniTask<T> LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object
    {
        var package = GetPackage(DefaultPackageName);
        if (package == null)
        {
            Debug.LogError("[YooAssetResourceLoader] 加载资源失败: 默认资源包不存在");
            return null;
        }
        
        var assetOperationHandle = package.LoadAssetAsync<T>(assetPath);
        await assetOperationHandle.ToUniTask();
        
        if (assetOperationHandle.Status != EOperationStatus.Succeed)
        {
            Debug.LogError($"[YooAssetResourceLoader] 加载资源失败: {assetPath}, 错误: {assetOperationHandle.LastError}");
            return null;
        }
        
        return assetOperationHandle.AssetObject as T;
    }
    
    public SceneHandle LoadSceneAsync(string scenePath, bool activateOnLoad = true, uint priority = 100)
    {
        var package = GetPackage(DefaultPackageName);
        if (package == null)
        {
            Debug.LogError("[YooAssetResourceLoader] 加载场景失败: 默认资源包不存在");
            return null;
        }
        
        return package.LoadSceneAsync(scenePath, UnityEngine.SceneManagement.LoadSceneMode.Single, UnityEngine.SceneManagement.LocalPhysicsMode.None, activateOnLoad, priority);
    }
    
    public void UnloadAllAssets()
    {
        var package = GetPackage(DefaultPackageName);
        if (package == null)
        {
            Debug.LogError("[YooAssetResourceLoader] 卸载资源失败: 默认资源包不存在");
            return;
        }
        
        package.UnloadAllAssetsAsync();
    }
    
    public void Release()
    {
        foreach (var package in _packages.Values)
        {
            package.UnloadUnusedAssetsAsync();
        }
        _packages.Clear();
    }
    
    #region 私有方法
    
    /// <summary>
    /// 创建资源包
    /// </summary>
    private ResourcePackage CreatePackage(string packageName)
    {
        if (_packages.TryGetValue(packageName, out var package))
        {
            Debug.LogWarning($"[YooAssetResourceLoader] 资源包 {packageName} 已存在");
            return package;
        }
        
        package = YooAssets.CreatePackage(packageName);
        _packages.Add(packageName, package);
        Debug.Log($"[YooAssetResourceLoader] 创建资源包: {packageName}");
        return package;
    }
    
    /// <summary>
    /// 获取资源包
    /// </summary>
    private ResourcePackage GetPackage(string packageName)
    {
        if (_packages.TryGetValue(packageName, out var package))
        {
            return package;
        }
        
        Debug.LogWarning($"[YooAssetResourceLoader] 资源包 {packageName} 不存在");
        return null;
    }
    
    /// <summary>
    /// 初始化资源包
    /// </summary>
    private async UniTask<bool> InitPackage(string packageName)
    {
        var package = GetPackage(packageName);
        if (package == null)
        {
            package = CreatePackage(packageName);
        }
        
        // 初始化参数
        InitializeParameters initParameters = null;
        
        if (_playMode == EPlayMode.EditorSimulateMode)
        {
            var buildResult = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
            var packageRoot = buildResult.PackageRootDirectory;
            Debug.Log($"[YooAssetResourceLoader] PackageRoot : {packageRoot}");
            var editorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
  
            var editorInitParameters = new EditorSimulateModeParameters();
            
            editorInitParameters.EditorFileSystemParameters = editorFileSystemParameters;
            initParameters = editorInitParameters;
        }
        else if (_playMode == EPlayMode.OfflinePlayMode)
        {
            var offlineInitParameters = new OfflinePlayModeParameters();
            var buildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            offlineInitParameters.BuildinFileSystemParameters = buildinFileSystemParameters;
            initParameters = offlineInitParameters;
        }
        else if (_playMode == EPlayMode.HostPlayMode)
        {
            IRemoteServices remoteServices = new RemoteServices(_defaultHostServer, _fallbackHostServer);
            var cacheFileSystemParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

            var hostInitParameters = new HostPlayModeParameters();
            hostInitParameters.BuildinFileSystemParameters = null; //设置为NULL解决报错！

            hostInitParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);

            initParameters = hostInitParameters;

        }
        else if (_playMode == EPlayMode.WebPlayMode)
        {
            Debug.Log($"[YooAssetResourceLoader] WebPlayMode : {_defaultHostServer}");
            Debug.Log($"[YooAssetResourceLoader] 当前URL: {Application.absoluteURL}");
			IRemoteServices remoteServices = new RemoteServices(_defaultHostServer, _fallbackHostServer);
			var webInitParameters = new WebPlayModeParameters();
			string packageRoot = "yoo";
#if WEIXINMINIGAME
            Debug.Log($"[YooAssetResourceLoader] WebPlayMode :WEIXINMINIGAME");
            packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE/yoo";
			webInitParameters.WebServerFileSystemParameters = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices, null);
            Debug.Log($"[YooAssetResourceLoader] PackageRoot : {packageRoot}");
#else
            // 为普通WebGL环境配置WebServerFileSystemParameters
            webInitParameters.WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
#endif
            initParameters = webInitParameters;
        }

        // 初始化操作
        var initOperation = package.InitializeAsync(initParameters);
        await initOperation.ToUniTask();
        
        Debug.Log($"[YooAssetResourceLoader] 资源包 {packageName} 初始化状态: {initOperation.Status}, {(initOperation.Status == EOperationStatus.Succeed ? "成功" : $"失败: {initOperation.Error}")}");
        
        return initOperation.Status == EOperationStatus.Succeed;
    }
    
    #endregion
}

public class RemoteServices : IRemoteServices
{
    private readonly string _defaultHostServer;
    private readonly string _fallbackHostServer;

    public RemoteServices(string defaultHostServer, string fallbackHostServer)
    {
        _defaultHostServer = defaultHostServer.TrimEnd('/');
        _fallbackHostServer = fallbackHostServer.TrimEnd('/');
    }

    string IRemoteServices.GetRemoteMainURL(string fileName)
    {
        fileName = fileName.TrimStart('/');
        return $"{_defaultHostServer}/{fileName}";
    }

    string IRemoteServices.GetRemoteFallbackURL(string fileName)
    {
        fileName = fileName.TrimStart('/');
        return $"{_fallbackHostServer}/{fileName}";
    }
}