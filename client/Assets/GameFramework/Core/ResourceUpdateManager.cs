//using Cysharp.Threading.Tasks;
//using System;
//using System.Collections;
//using UnityEngine;
//using YooAsset;

///// <summary>
///// 资源更新管理器 - 负责资源热更新流程
///// </summary>
//public class ResourceUpdateManager : MonoBehaviour
//{
//    // 单例实例
//    private static ResourceUpdateManager _instance;
//    public static ResourceUpdateManager Instance => _instance;

//    // 资源管理器引用
//    private ResourceManager _resourceManager;
    
//    // 是否正在更新资源
//    private bool _isUpdating = false;
    
//    // 更新进度回调
//    public event Action<float> OnUpdateProgressChanged;
    
//    // 更新完成回调
//    public event Action<bool> OnUpdateCompleted;
    
//    private void Awake()
//    {
//        if (_instance != null)
//        {
//            Destroy(gameObject);
//            return;
//        }
        
//        _instance = this;
//        DontDestroyOnLoad(gameObject);
//    }
    
//    private void Start()
//    {
//        _resourceManager = GameFrameworkManager.Instance.GetManager<ResourceManager>();
//    }
    
//    /// <summary>
//    /// 开始资源更新流程
//    /// </summary>
//    public void StartUpdateResources(string packageName = "DefaultPackage")
//    {
//        if (_isUpdating)
//        {
//            Debug.LogWarning("[ResourceUpdateManager] 资源更新已在进行中");
//            return;
//        }
        
//        _isUpdating = true;
//        StartCoroutine(UpdateResourcesCoroutine(packageName));
//    }
    
//    /// <summary>
//    /// 资源更新协程
//    /// </summary>
//    private IEnumerator UpdateResourcesCoroutine(string packageName)
//    {
//        Debug.Log("[ResourceUpdateManager] 开始资源更新流程");

//		// 1. 初始化资源包
//		bool initResult = false;
//		var initTask = _resourceManager.InitPackage(packageName);
//		yield return initTask.ToCoroutine(x => initResult = x);

//		if (!initResult)
//		{
//            Debug.LogError("[ResourceUpdateManager] 资源包初始化失败");
//            _isUpdating = false;
//            OnUpdateCompleted?.Invoke(false);
//            yield break;
//        }
        
//        // 2. 获取最新版本
//        //var versionTask = _resourceManager.RequestPackageVersion(packageName);
//        //yield return new WaitUntil(() => versionTask.IsCompleted);
        
//        string packageVersion = _resourceManager.RequestPackageVersion(packageName);
//        if (string.IsNullOrEmpty(packageVersion))
//        {
//            Debug.LogWarning("[ResourceUpdateManager] 获取最新版本失败，将使用内置版本");
//            _isUpdating = false;
//            OnUpdateCompleted?.Invoke(true); // 使用内置版本也算成功
//            yield break;
//        }

//		// 3. 更新资源清单
//		bool manifestResult = false;

//		var manifestTask = _resourceManager.UpdatePackageManifestAsync(packageVersion, packageName);
//		yield return manifestTask.ToCoroutine(x => manifestResult = x);

//		if (!manifestResult)
//        {
//            Debug.LogError("[ResourceUpdateManager] 更新资源清单失败");
//            _isUpdating = false;
//            OnUpdateCompleted?.Invoke(false);
//            yield break;
//        }
        
//        // 4. 创建资源下载器
//        var downloader = _resourceManager.CreateResourceDownloader(packageName);
//        if (downloader == null)
//        {
//            Debug.LogError("[ResourceUpdateManager] 创建资源下载器失败");
//            _isUpdating = false;
//            OnUpdateCompleted?.Invoke(false);
//            yield break;
//        }
        
//        // 如果没有需要下载的资源
//        if (downloader.TotalDownloadCount == 0)
//        {
//            Debug.Log("[ResourceUpdateManager] 没有需要更新的资源");
//            _isUpdating = false;
//            OnUpdateCompleted?.Invoke(true);
//            yield break;
//        }
        
//        // 5. 下载资源
//        Debug.Log($"[ResourceUpdateManager] 开始下载资源，总数: {downloader.TotalDownloadCount}，总大小: {downloader.TotalDownloadBytes / 1024f / 1024f:F2}MB");
        
//        // 注册下载进度回调
//        downloader.DownloadUpdateCallback = OnDownloadProgressCallback;
//        downloader.DownloadErrorCallback = OnDownloadErrorCallback;
        
//        // 开始下载
//        downloader.BeginDownload();
//        yield return downloader;
        
//        // 检查下载结果
//        if (downloader.Status == EOperationStatus.Succeed)
//        {
//            Debug.Log("[ResourceUpdateManager] 资源下载完成");
//            _isUpdating = false;
//            OnUpdateCompleted?.Invoke(true);
//        }
//        else
//        {
//            Debug.LogError($"[ResourceUpdateManager] 资源下载失败: {downloader.Error}");
//            _isUpdating = false;
//            OnUpdateCompleted?.Invoke(false);
//        }
//    }

    
//    /// <summary>
//    /// 按标签更新资源
//    /// </summary>
//    public void UpdateResourcesByTags(string[] tags, string packageName = "DefaultPackage")
//    {
//        if (_isUpdating)
//        {
//            Debug.LogWarning("[ResourceUpdateManager] 资源更新已在进行中");
//            return;
//        }
        
//        _isUpdating = true;
//        StartCoroutine(UpdateResourcesByTagsCoroutine(tags, packageName));
//    }
    
//    /// <summary>
//    /// 按标签更新资源协程
//    /// </summary>
//    private IEnumerator UpdateResourcesByTagsCoroutine(string[] tags, string packageName)
//    {
//        Debug.Log($"[ResourceUpdateManager] 开始按标签更新资源: {string.Join(", ", tags)}");
        
//        // 1. 初始化资源包（如果尚未初始化）
//        var package = _resourceManager.GetPackage(packageName);
//        if (package == null /*|| !package.IsInitialized*/)
//        {
//			bool initResult = false;
//			var initTask = _resourceManager.InitPackage(packageName);
//			yield return initTask.ToCoroutine(x => initResult = x);

//			if (!initResult)
//			{
//                Debug.LogError("[ResourceUpdateManager] 资源包初始化失败");
//                _isUpdating = false;
//                OnUpdateCompleted?.Invoke(false);
//                yield break;
//            }
//        }
        
//        // 2. 获取最新版本（如果尚未获取）
//        if (string.IsNullOrEmpty(package.GetPackageVersion()))
//        {
//            //var versionTask = _resourceManager.RequestPackageVersion(packageName);
//            //yield return new WaitUntil(() => versionTask.IsCompleted);
            
//            string packageVersion = _resourceManager.RequestPackageVersion(packageName);
//            if (string.IsNullOrEmpty(packageVersion))
//            {
//                Debug.LogWarning("[ResourceUpdateManager] 获取最新版本失败，将使用内置版本");
//                _isUpdating = false;
//                OnUpdateCompleted?.Invoke(true);
//                yield break;
//            }

//            // 3. 更新资源清单
//            bool manifestResult = false;

//			var manifestTask = _resourceManager.UpdatePackageManifestAsync(packageVersion, packageName);
//			yield return manifestTask.ToCoroutine(x => manifestResult = x);
            
//            if (!manifestResult)
//            {
//                Debug.LogError("[ResourceUpdateManager] 更新资源清单失败");
//                _isUpdating = false;
//                OnUpdateCompleted?.Invoke(false);
//                yield break;
//            }
//        }
        
//        // 4. 创建资源下载器（按标签）
//        var downloader = _resourceManager.CreateResourceDownloaderByTags(packageName);
//        if (downloader == null)
//        {
//            Debug.LogError("[ResourceUpdateManager] 创建资源下载器失败");
//            _isUpdating = false;
//            OnUpdateCompleted?.Invoke(false);
//            yield break;
//        }
        
//        // 如果没有需要下载的资源
//        if (downloader.TotalDownloadCount == 0)
//        {
//            Debug.Log("[ResourceUpdateManager] 没有需要更新的资源");
//            _isUpdating = false;
//            OnUpdateCompleted?.Invoke(true);
//            yield break;
//        }
        
//        // 5. 下载资源
//        Debug.Log($"[ResourceUpdateManager] 开始下载资源，总数: {downloader.TotalDownloadCount}，总大小: {downloader.TotalDownloadBytes / 1024f / 1024f:F2}MB");
        
//        // 注册下载进度回调
//        downloader.DownloadUpdateCallback = OnDownloadProgressCallback;
//        downloader.DownloadErrorCallback = OnDownloadErrorCallback;
        
//        // 开始下载
//        downloader.BeginDownload();
//        yield return downloader;
        
//        // 检查下载结果
//        if (downloader.Status == EOperationStatus.Succeed)
//        {
//            Debug.Log("[ResourceUpdateManager] 资源下载完成");
//            _isUpdating = false;
//            OnUpdateCompleted?.Invoke(true);
//        }
//        else
//        {
//            Debug.LogError($"[ResourceUpdateManager] 资源下载失败: {downloader.Error}");
//            _isUpdating = false;
//            OnUpdateCompleted?.Invoke(false);
//        }
//    }

//    private void OnDownloadErrorCallback(DownloadErrorData data)
//    {
//        Debug.LogError($"[ResourceUpdateManager] 资源下载出错: {data.FileName}, 错误: {data.ErrorInfo}");
//    }

//    private void OnDownloadProgressCallback(DownloadUpdateData data)
//    {
//        float progress = (float)data.CurrentDownloadBytes/ data.TotalDownloadBytes;
//        OnUpdateProgressChanged?.Invoke(progress);
//    }
//} 