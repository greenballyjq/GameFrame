using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Utility;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

/// <summary>
/// YooAsset资源加载器，只走本地包
/// </summary>
public class YooAssetResourceLoader : IResourceLoader
{
    private const string DefaultPackageName = "DefaultPackage";

    private readonly Dictionary<string, ResourcePackage> _packages = new Dictionary<string, ResourcePackage>();
    private readonly Dictionary<string, AssetHandle> _assetHandles = new Dictionary<string, AssetHandle>();

    public ResourcePackage DefaultPackage => GetPackage(DefaultPackageName);

    public async UniTask<bool> InitializeAsync()
    {
        Log.Info("[YooAssetResourceLoader] 初始化YooAsset资源加载器");

        YooAssets.Initialize();

        var defaultPackage = CreatePackage(DefaultPackageName);
        YooAssets.SetDefaultPackage(defaultPackage);

        return await InitPackage(DefaultPackageName);
    }

    public async UniTask<T> LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object
    {
        if (_assetHandles.TryGetValue(assetPath, out AssetHandle existing) && existing.IsValid)
            return existing.AssetObject as T;

        var package = GetPackage(DefaultPackageName);
        if (package == null)
        {
            Log.Error("[YooAssetResourceLoader] 加载资源失败: 默认资源包不存在");
            return null;
        }

        var assetOperationHandle = package.LoadAssetAsync<T>(assetPath);
        await assetOperationHandle.ToUniTask();

        if (assetOperationHandle.Status != EOperationStatus.Succeed)
        {
            Log.Error($"[YooAssetResourceLoader] 加载资源失败: {assetPath}, 错误: {assetOperationHandle.LastError}");
            return null;
        }

        _assetHandles[assetPath] = assetOperationHandle;
        return assetOperationHandle.AssetObject as T;
    }

    public void ReleaseAsset(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
            return;

        if (!_assetHandles.TryGetValue(assetPath, out AssetHandle handle))
            return;

        handle.Release();
        _assetHandles.Remove(assetPath);
    }

    public void UnloadAllAssets()
    {
        var package = GetPackage(DefaultPackageName);
        if (package == null)
        {
            Log.Error("[YooAssetResourceLoader] 卸载资源失败: 默认资源包不存在");
            return;
        }

        package.UnloadAllAssetsAsync();
    }

    public void Release()
    {
        foreach (var handle in _assetHandles.Values)
            handle.Release();
        _assetHandles.Clear();

        foreach (var package in _packages.Values)
        {
            if (package.InitializeStatus == EOperationStatus.Succeed)
                package.UnloadUnusedAssetsAsync();
        }
        _packages.Clear();
    }

    private ResourcePackage CreatePackage(string packageName)
    {
        if (_packages.TryGetValue(packageName, out var package))
        {
            Log.Warning($"[YooAssetResourceLoader] 资源包 {packageName} 已存在");
            return package;
        }

        package = YooAssets.CreatePackage(packageName);
        _packages.Add(packageName, package);
        Log.Info($"[YooAssetResourceLoader] 创建资源包: {packageName}");
        return package;
    }

    private ResourcePackage GetPackage(string packageName)
    {
        if (_packages.TryGetValue(packageName, out var package))
            return package;

        Log.Warning($"[YooAssetResourceLoader] 资源包 {packageName} 不存在");
        return null;
    }

    private async UniTask<bool> InitPackage(string packageName)
    {
        var package = GetPackage(packageName);
        if (package == null)
            package = CreatePackage(packageName);

        InitializeParameters initParameters;
#if UNITY_EDITOR
        var buildResult = EditorSimulateModeHelper.SimulateBuild(DefaultPackageName);
        Log.Info($"[YooAssetResourceLoader] PackageRoot : {buildResult.PackageRootDirectory}");
        var editorInitParameters = new EditorSimulateModeParameters();
        editorInitParameters.EditorFileSystemParameters =
            FileSystemParameters.CreateDefaultEditorFileSystemParameters(buildResult.PackageRootDirectory);
        initParameters = editorInitParameters;
        Log.Info("[YooAssetResourceLoader] 当前运行模式：EditorSimulateMode");
#else
        var offlineInitParameters = new OfflinePlayModeParameters();
        offlineInitParameters.BuildinFileSystemParameters =
            FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
        initParameters = offlineInitParameters;
        Log.Info("[YooAssetResourceLoader] 当前运行模式：OfflinePlayMode");
#endif

        var initOperation = package.InitializeAsync(initParameters);
        await initOperation.ToUniTask();
        if (initOperation.Status != EOperationStatus.Succeed)
        {
            Log.Error($"[YooAssetResourceLoader] 资源包 {packageName} 初始化失败: {initOperation.Error}");
            return false;
        }

        var versionOperation = package.RequestPackageVersionAsync();
        await versionOperation.ToUniTask();
        if (versionOperation.Status != EOperationStatus.Succeed)
        {
            Log.Error($"[YooAssetResourceLoader] 读取本地资源版本失败: {versionOperation.Error}");
            return false;
        }

        var manifestOperation = package.UpdatePackageManifestAsync(versionOperation.PackageVersion);
        await manifestOperation.ToUniTask();
        if (manifestOperation.Status != EOperationStatus.Succeed)
        {
            Log.Error($"[YooAssetResourceLoader] 加载本地资源清单失败: {manifestOperation.Error}");
            return false;
        }

        Log.Info($"[YooAssetResourceLoader] 资源包 {packageName} 初始化完成");
        return true;
    }
}
