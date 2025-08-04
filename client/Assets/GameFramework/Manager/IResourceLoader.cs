using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace GameFramework.Core
{
    /// <summary>
    /// 资源加载器接口 - 定义资源加载的统一接口
    /// </summary>
    public interface IResourceLoader
    {
        /// <summary>
        /// 初始化资源加载器
        /// </summary>
        UniTask<bool> InitializeAsync();
        
        /// <summary>
        /// 设置远程服务器地址
        /// </summary>
        void SetHostServer(string hostServer);
        
        /// <summary>
        /// 请求资源包版本
        /// </summary>
        UniTask<string> RequestPackageVersionAsync();
        
        /// <summary>
        /// 更新资源包清单
        /// </summary>
        UniTask<bool> UpdatePackageManifestAsync(string packageVersion);
        
        /// <summary>
        /// 创建资源下载器
        /// </summary>
        ResourceDownloaderOperation CreateResourceDownloader();
        
        /// <summary>
        /// 异步加载资源
        /// </summary>
        UniTask<T> LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object;
        
        /// <summary>
        /// 异步加载场景
        /// </summary>
        SceneHandle LoadSceneAsync(string scenePath, bool activateOnLoad = true, uint priority = 100);
        
        /// <summary>
        /// 卸载所有资源
        /// </summary>
        void UnloadAllAssets();
        
        /// <summary>
        /// 释放资源加载器
        /// </summary>
        void Release();
    }
}