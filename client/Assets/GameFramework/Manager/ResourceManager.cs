using Cysharp.Threading.Tasks;
using GameFramework.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using WeChatWASM;
using YooAsset;

/// <summary>
/// 资源管理器 - 基于YooAsset实现
/// </summary>
public class ResourceManager : IManager
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
    
    public void Init()
    {
        Debug.Log("[ResourceManager] 初始化资源管理器");
        
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
        
        Debug.Log($"[ResourceManager] 当前运行模式：{_playMode}");

        // 创建默认资源包
        var defaultPackage = CreatePackage(DefaultPackageName);
        YooAssets.SetDefaultPackage(defaultPackage);
    }


    public void Release()
    {
        foreach (var package in _packages.Values)
        {
            package.UnloadUnusedAssetsAsync();
        }
    }
    
    /// <summary>
    /// 创建资源包
    /// </summary>
    public ResourcePackage CreatePackage(string packageName)
    {
        if (_packages.TryGetValue(packageName, out var package))
        {
            Debug.LogWarning($"[ResourceManager] 资源包 {packageName} 已存在");
            return package;
        }
        
        package = YooAssets.CreatePackage(packageName);
        _packages.Add(packageName, package);
        Debug.Log($"[ResourceManager] 创建资源包: {packageName}");
        return package;
    }
    
    /// <summary>
    /// 获取资源包
    /// </summary>
    public ResourcePackage GetPackage(string packageName)
    {
        if (_packages.TryGetValue(packageName, out var package))
        {
            return package;
        }
        
        Debug.LogWarning($"[ResourceManager] 资源包 {packageName} 不存在");
        return null;
    }
    
    /// <summary>
    /// 设置远程服务器地址
    /// </summary>
    public void SetHostServer(string defaultHostServer, string fallbackHostServer = null)
    {
        _defaultHostServer = defaultHostServer;
        _fallbackHostServer = fallbackHostServer ?? defaultHostServer;
    }
    
    /// <summary>
    /// 初始化资源包 (UniTask版本)
    /// </summary>
    public async UniTask<InitializationOperation> InitPackageAsync(string packageName)
    {
        var package = GetPackage(packageName);
        if (package == null)
        {
            package = CreatePackage(packageName);
        }
        
        // 初始化参数
        InitializeParameters initParameters = null;
        
        Debug.Log($"[ResourceManager] PackageRoot2: {WeChatWASM.WX.env.USER_DATA_PATH}/yoo_builtin");
        
        if (_playMode == EPlayMode.EditorSimulateMode)
        {
            var buildResult = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
            var packageRoot = buildResult.PackageRootDirectory;
            Debug.Log($"[ResourceManager] PackageRoot : {packageRoot}");
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
            var PlatformManager = GameFrameworkManager.Instance.GetManager<PlatformManager>();
            var curType = PlatformManager.GetCurrentPlatform();
			IRemoteServices remoteServices = new RemoteServices(_defaultHostServer, _fallbackHostServer);
			var webInitParameters = new WebPlayModeParameters();
			string packageRoot = "yoo";
			switch (curType)
            {
                case PlatformManager.PlatformType.WeChat:
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
// #if UNITY_WEBGL && !UNITY_EDITOR
                   string builtinPath = $"{WeChatWASM.WX.env.USER_DATA_PATH}/yoo_builtin";
					packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE/yoo";
					webInitParameters.WebServerFileSystemParameters = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices, null);
                   Debug.LogError($"[ResourceManager] PackageRoot : {builtinPath}");
#endif

                    break;
                case PlatformManager.PlatformType.TikTok:
		

					// 创建解密服务类
					//var decryptionServices = new WebDecryption();

					//// 随意填写
					// packageRoot = "yoo";

					//// 创建初始化参数

					//webInitParameters.WebServerFileSystemParameters = TiktokFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices, decryptionServices);

					break;
                case PlatformManager.PlatformType.Kuaishou:
                    break;
            }
			initParameters = webInitParameters;

		}
        
        // 初始化操作
        var initOperation = package.InitializeAsync(initParameters);
        await initOperation.ToUniTask();
        
        Debug.Log($"[ResourceManager] 资源包 {packageName} 初始化状态: {initOperation.Status}, {(initOperation.Status == EOperationStatus.Succeed ? "成功" : $"失败: {initOperation.Error}")}");
        
        return initOperation;
    }
    
    /// <summary>
    /// 初始化资源包 (Task版本，向后兼容)
    /// </summary>
    public async UniTask<bool> InitPackage(string packageName)
    {
        var operation = await InitPackageAsync(packageName);
        return operation.Status == EOperationStatus.Succeed;
    }
    
    
    /// <summary>
    /// 请求资源包版本 (Task版本，向后兼容)
    /// </summary>
    public string RequestPackageVersion(string packageName = DefaultPackageName, int timeout = 30)
    {
        var package = GetPackage(packageName);
        if (package == null)
        {
            Debug.LogError($"[ResourceManager] 请求资源包版本失败: 资源包 {packageName} 不存在");
            return null;
        }
        
        return package.GetPackageVersion();
    }

    public async UniTask<string> RequestPackageVersionAsync(string packageName = DefaultPackageName, int timeout = 30)
    {
        var package = GetPackage(packageName);
        if (package == null)
        {
            Debug.LogError($"[ResourceManager] 请求资源包版本失败: 资源包 {packageName} 不存在");
            return null;
        }
        
        var op = package.RequestPackageVersionAsync();
        await op;

        return op.PackageVersion;
    }
    
    /// <summary>
    /// 更新资源包清单 (UniTask版本)
    /// </summary>
    //public async UniTask<UpdatePackageManifestOperation> UpdatePackageManifestAsync(string packageVersion, string packageName = DefaultPackageName)
    //{
    //    var package = GetPackage(packageName);
    //    if (package == null)
    //    {
    //        Debug.LogError($"[ResourceManager] 更新资源包清单失败: 资源包 {packageName} 不存在");
    //        return null;
    //    }
        
    //    var operation = package.UpdatePackageManifestAsync(packageVersion);
    //    await operation.ToUniTask();
        
    //    Debug.Log($"[ResourceManager] 资源包清单更新状态: {operation.Status}, {(operation.Status == EOperationStatus.Succeed ? "成功" : $"失败: {operation.Error}")}");
        
    //    return operation;
    //}
    
    /// <summary>
    /// 更新资源包清单 (Task版本，向后兼容)
    /// </summary>
    public async UniTask<bool> UpdatePackageManifestAsync(string packageVersion, string packageName = DefaultPackageName)
    {
        Debug.Log($"[ResourceManager] UpdatePackageManifestAsync packageVersion: {packageVersion}");
        var package = GetPackage(packageName);
        if (package == null)
        {
            Debug.LogError($"[ResourceManager] 更新资源包清单失败: 资源包 {packageName} 不存在");
            return false;
        }

        // var op = package.RequestPackageVersionAsync();
        // await op;
        var operation = package.UpdatePackageManifestAsync(packageVersion);
        // var operation = package.UpdatePackageManifestAsync(op.PackageVersion);
        await operation.ToUniTask();
        
        Debug.Log($"[ResourceManager] 资源包清单更新状态: {operation.Status}, {(operation.Status == EOperationStatus.Succeed ? "成功" : $"失败: {operation.Error}")}");
        
        return operation.Status == EOperationStatus.Succeed;
    }
    
    /// <summary>
    /// 创建资源下载器
    /// </summary>
    public ResourceDownloaderOperation CreateResourceDownloader(string packageName = DefaultPackageName, int downloadingMaxNumber = 10, int failedTryAgain = 3, int timeout = 60)
    {
        var package = GetPackage(packageName);
        if (package == null)
        {
            Debug.LogError($"[ResourceManager] 创建资源下载器失败: 资源包 {packageName} 不存在");
            return null;
        }
        
        var downloader = package.CreateResourceDownloader(downloadingMaxNumber, failedTryAgain, timeout);
        return downloader;
    }
    
    /// <summary>
    /// 创建资源下载器（按标签）
    /// </summary>
    public ResourceDownloaderOperation CreateResourceDownloaderByTags(string packageName = DefaultPackageName, int downloadingMaxNumber = 10, int failedTryAgain = 3, int timeout = 60)
    {
        var package = GetPackage(packageName);
        if (package == null)
        {
            Debug.LogError($"[ResourceManager] 创建资源下载器失败: 资源包 {packageName} 不存在");
            return null;
        }
        
        var downloader = package.CreateResourceDownloader(downloadingMaxNumber, failedTryAgain, timeout);
        return downloader;
    }
    
    /// <summary>
    /// 异步加载资源 (UniTask版本)
    /// </summary>
    public async UniTask<AssetHandle> LoadAssetAsyncWithUniTask<T>(string assetPath, string packageName = DefaultPackageName) where T : UnityEngine.Object
    {
        var package = GetPackage(packageName);
        if (package == null)
        {
            Debug.LogError($"[ResourceManager] 加载资源失败: 资源包 {packageName} 不存在");
            return null;
        }
        
        var assetOperationHandle = package.LoadAssetAsync<T>(assetPath);
        await assetOperationHandle.ToUniTask();
        
        if (assetOperationHandle.Status != EOperationStatus.Succeed)
        {
            Debug.LogError($"[ResourceManager] 加载资源失败: {assetPath}, 错误: {assetOperationHandle.LastError}");
        }
        
        return assetOperationHandle;
    }
    
    /// <summary>
    /// 异步加载资源 (Task版本，向后兼容)
    /// </summary>
    public async UniTask<T> LoadAssetAsync<T>(string assetPath, string packageName = DefaultPackageName) where T : UnityEngine.Object
    {
        var handle = await LoadAssetAsyncWithUniTask<T>(assetPath, packageName);
        return handle?.AssetObject as T;
    }
    
    /// <summary>
    /// 异步加载场景 (UniTask版本)
    /// </summary>
    public SceneHandle LoadSceneAsync(string scenePath, string packageName = DefaultPackageName, bool activateOnLoad = true, uint priority = 100)
    {
        var package = GetPackage(packageName);
        if (package == null)
        {
            Debug.LogError($"[ResourceManager] 加载场景失败: 资源包 {packageName} 不存在");
            return null;
        }
        
        return package.LoadSceneAsync(scenePath, UnityEngine.SceneManagement.LoadSceneMode.Single, UnityEngine.SceneManagement.LocalPhysicsMode.None,activateOnLoad, priority);
    }
    
    /// <summary>
    /// 卸载所有资源
    /// </summary>
    public void UnloadAllAssets(string packageName = DefaultPackageName)
    {
        var package = GetPackage(packageName);
        if (package == null)
        {
            Debug.LogError($"[ResourceManager] 卸载资源失败: 资源包 {packageName} 不存在");
            return;
        }
        
        package.UnloadAllAssetsAsync();
    }

    public void Update()
    {
        // 更新YooAsset
        //YooAssets.Update();
    }
}


public class RemoteServices : IRemoteServices
{
    private readonly string _defaultHostServer;
    private readonly string _fallbackHostServer;

    public RemoteServices(string defaultHostServer, string fallbackHostServer)
    {
        _defaultHostServer = defaultHostServer;
        _fallbackHostServer = fallbackHostServer;
    }
    string IRemoteServices.GetRemoteMainURL(string fileName)
    {
        return $"{_defaultHostServer}/{fileName}";
    }
    string IRemoteServices.GetRemoteFallbackURL(string fileName)
    {
        return $"{_fallbackHostServer}/{fileName}";
    }
}