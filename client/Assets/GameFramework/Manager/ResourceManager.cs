using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Utility;

namespace GameFramework.Manager
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class ResourceManager : IManager
    {
        /// <summary>
        /// 资源加载模式
        /// </summary>
        public enum ResourceLoadMode
        {
            YooAsset,
            Resources
        }

        private readonly ResourceLoadMode _mode;
        private IResourceLoader _currentLoader;
        private ResourceLoadMode _currentMode;

        public ResourceManager(ResourceLoadMode mode)
        {
            _mode = mode;
        }

        public async UniTask InitAsync()
        {
            SetResourceLoadMode(_mode);
            if (!await _currentLoader.InitializeAsync())
                Log.Error("[ResourceManager] 资源加载器初始化失败");
        }

        public async UniTask<T> LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object
        {
            return await _currentLoader.LoadAssetAsync<T>(assetPath);
        }

        public async UniTask<bool> LoadSceneAsync(string scenePath, bool activateOnLoad = true)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                sceneName,
                UnityEngine.SceneManagement.LoadSceneMode.Single);
            if (operation == null)
            {
                Log.Error($"[ResourceManager] 加载场景失败: {scenePath}");
                return false;
            }

            operation.allowSceneActivation = activateOnLoad;
            while (!operation.isDone)
                await UniTask.Yield();

            return true;
        }

        public void ReleaseAsset(string assetPath)
        {
            _currentLoader.ReleaseAsset(assetPath);
        }

        public void UnloadAllAssets()
        {
            _currentLoader.UnloadAllAssets();
        }

        public void Dispose()
        {
            _currentLoader.Release();
            _currentLoader = null;
        }

        private void SetResourceLoadMode(ResourceLoadMode mode)
        {
            if (_currentMode == mode && _currentLoader != null)
                return;

            if (_currentLoader != null)
            {
                _currentLoader.Release();
                _currentLoader = null;
            }

            _currentMode = mode;
            switch (mode)
            {
                case ResourceLoadMode.YooAsset:
                    _currentLoader = new YooAssetResourceLoader();
                    break;
                case ResourceLoadMode.Resources:
                    _currentLoader = new ResourcesResourceLoader();
                    break;
            }

            Log.Info($"[ResourceManager] 资源加载模式: {mode}");
        }
    }
}
