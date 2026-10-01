using Cysharp.Threading.Tasks;

namespace GameFramework.Core
{
    /// <summary>
    /// 资源加载器接口
    /// </summary>
    public interface IResourceLoader
    {
        UniTask<bool> InitializeAsync();
        UniTask<T> LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object;
        void ReleaseAsset(string assetPath);
        void UnloadAllAssets();
        void Release();
    }
}
