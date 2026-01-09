using System;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using EventArgs = System.EventArgs;

namespace GameFramework.Manager
{
    /// <summary>
    /// 场景加载事件参数
    /// </summary>
    public class SceneLoadEventArgs : EventArgs
    {
        /// <summary>
        /// 场景名称
        /// </summary>
        public string SceneName { get; set; }

        /// <summary>
        /// 加载进度（0-1）
        /// </summary>
        public float Progress { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// 游戏场景管理器 - 基于 UnityEngine.SceneManager 实现，负责场景的加载和管理
    /// </summary>
    public class GameSceneManager : IManager
    {
        /// <summary>
        /// 场景加载前事件
        /// </summary>
        public event EventHandler<SceneLoadEventArgs> OnSceneLoadStart;

        /// <summary>
        /// 场景加载进度事件
        /// </summary>
        public event EventHandler<SceneLoadEventArgs> OnSceneLoadProgress;

        /// <summary>
        /// 场景加载完成事件
        /// </summary>
        public event EventHandler<SceneLoadEventArgs> OnSceneLoadComplete;

        /// <summary>
        /// 场景切换事件
        /// </summary>
        public event Action<string, string> OnSceneChanged;

        /// <summary>
        /// 场景卸载前事件
        /// </summary>
        public event EventHandler<SceneLoadEventArgs> OnSceneUnloadStart;

        /// <summary>
        /// 场景卸载完成事件
        /// </summary>
        public event EventHandler<SceneLoadEventArgs> OnSceneUnloadComplete;

        /// <summary>
        /// 当前加载的场景信息
        /// </summary>
        private string _currentSceneName;
        // private string _currentScenePath;

        /// <summary>
        /// 场景加载操作的引用（用于进度监控和取消操作）
        /// </summary>
        private AsyncOperation _currentLoadOperation;

        /// <summary>
        /// 正在加载的标志
        /// </summary>
        private bool _isLoading = false;

        public void Init()
        {
            // 获取当前场景信息
            Scene currentScene = SceneManager.GetActiveScene();
            _currentSceneName = currentScene.name;
            // _currentScenePath = GetScenePath(currentScene.name);

            // Debug.Log($"[GameSceneManager] 场景管理器初始化完成，当前场景: {_currentSceneName}");
        }

        public void Update()
        {
            // 检查场景加载进度
            if (_isLoading && _currentLoadOperation != null && !_currentLoadOperation.isDone)
            {
                float progress = _currentLoadOperation.progress;
                OnSceneLoadProgress?.Invoke(this, new SceneLoadEventArgs
                {
                    SceneName = _currentSceneName,
                    // ScenePath = _currentScenePath,
                    Progress = progress,
                    IsSuccess = false
                });
            }
        }

        public void Release()
        {
            // 清理事件
            OnSceneLoadStart = null;
            OnSceneLoadProgress = null;
            OnSceneLoadComplete = null;
            OnSceneUnloadStart = null;
            OnSceneUnloadComplete = null;

            // 取消当前加载操作
            if (_currentLoadOperation != null)
            {
                _currentLoadOperation = null;
                _isLoading = false;
            }

            // Debug.Log("[GameSceneManager] 场景管理器已释放");
        }

        #region 公共方法

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="loadMode">场景加载模式</param>
        /// <param name="activateOnLoad">加载完成后是否自动激活</param>
        /// <returns>加载操作任务</returns>
        public async UniTask<bool> LoadSceneAsync(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[GameSceneManager] 场景名称不能为空");
                return false;
            }

            if (_isLoading)
            {
                Debug.LogWarning("[GameSceneManager] 场景正在加载中，请等待完成");
                return false;
            }

            // 检查是否是当前场景（仅在Single模式下）
            if (loadMode == LoadSceneMode.Single && _currentSceneName == sceneName)
            {
                Debug.Log($"[GameSceneManager] 场景 {sceneName} 已经是当前场景");
                return true;
            }

            try
            {
                _isLoading = true;
                _currentSceneName = sceneName;
                // _currentScenePath = GetScenePath(sceneName);
                
                // 触发加载前事件
                OnSceneLoadStart?.Invoke(this, new SceneLoadEventArgs
                {
                    SceneName = sceneName,
                    // ScenePath = _currentScenePath,
                    Progress = 0f,
                    IsSuccess = true
                });

                OnSceneChanged?.Invoke(_currentSceneName, sceneName);
                // 使用 UnityEngine.SceneManager 加载场景
                _currentLoadOperation = SceneManager.LoadSceneAsync(sceneName, loadMode);
                
                if (_currentLoadOperation == null)
                {
                    throw new Exception("场景加载操作为空");
                }
                
                _currentLoadOperation.allowSceneActivation = activateOnLoad;

                // 等待加载完成
                await _currentLoadOperation;

                // 检查加载结果
                Scene activeScene = SceneManager.GetActiveScene();
                _currentSceneName = activeScene.name;
                // _currentScenePath = GetScenePath(activeScene.name);

                // 触发加载完成事件
                OnSceneLoadComplete?.Invoke(this, new SceneLoadEventArgs
                {
                    SceneName = sceneName,
                    // ScenePath = _currentScenePath,
                    Progress = 1f,
                    IsSuccess = true,
                    ErrorMessage = "",
                });

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameSceneManager] 加载场景 {sceneName} 时发生异常: {ex.Message}");

                // 触发加载完成事件（失败）
                OnSceneLoadComplete?.Invoke(this, new SceneLoadEventArgs
                {
                    SceneName = sceneName,
                    // ScenePath = _currentScenePath,
                    Progress = 0f,
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                });

                return false;
            }
            finally
            {
                _isLoading = false;
                _currentLoadOperation = null;
            }
        }

        /// <summary>
        /// 同步加载场景（不建议使用，会阻塞主线程）
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="loadMode">场景加载模式</param>
        public void LoadScene(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            LoadSceneAsync(sceneName, loadMode).Forget();
        }

        /// <summary>
        /// 加载场景到索引（多场景模式）
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="sceneIndex">场景索引</param>
        public async UniTask<bool> LoadSceneToIndexAsync(string sceneName, int sceneIndex)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[GameSceneManager] 场景名称不能为空");
                return false;
            }

            try
            {
                _isLoading = true;
                _currentSceneName = sceneName;
                // _currentScenePath = GetScenePath(sceneName);

                // 触发加载前事件
                OnSceneLoadStart?.Invoke(this, new SceneLoadEventArgs
                {
                    SceneName = sceneName,
                    // ScenePath = _currentScenePath,
                    Progress = 0f,
                    IsSuccess = true
                });

                // 加载场景到指定索引
                OnSceneChanged?.Invoke(_currentSceneName, sceneName);
                _currentLoadOperation = SceneManager.LoadSceneAsync(sceneName, new LoadSceneParameters
                {
                    loadSceneMode = LoadSceneMode.Additive,
                    localPhysicsMode = LocalPhysicsMode.None,
                    // activateOnLoad = true
                });

                await _currentLoadOperation;

                // 设置新加载的场景为活动场景
                Scene loadedScene = SceneManager.GetSceneByName(sceneName);
                if (loadedScene.IsValid())
                {
                    SceneManager.SetActiveScene(loadedScene);
                    _currentSceneName = loadedScene.name;
                    // _currentScenePath = GetScenePath(loadedScene.name);
                }

                // 触发加载完成事件
                OnSceneLoadComplete?.Invoke(this, new SceneLoadEventArgs
                {
                    SceneName = sceneName,
                    // ScenePath = _currentScenePath,
                    Progress = 1f,
                    IsSuccess = true
                });

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameSceneManager] 加载场景到索引 {sceneIndex} 失败: {ex.Message}");

                OnSceneLoadComplete?.Invoke(this, new SceneLoadEventArgs
                {
                    SceneName = sceneName,
                    // ScenePath = _currentScenePath,
                    Progress = 0f,
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                });

                return false;
            }
            finally
            {
                _isLoading = false;
                _currentLoadOperation = null;
            }
        }

        /// <summary>
        /// 卸载当前场景
        /// </summary>
        public async UniTask<bool> UnloadCurrentSceneAsync()
        {
            if (string.IsNullOrEmpty(_currentSceneName))
            {
                Debug.LogWarning("[GameSceneManager] 没有可卸载的场景");
                return true;
            }

            return await UnloadSceneAsync(_currentSceneName);
        }

        /// <summary>
        /// 卸载指定场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public async UniTask<bool> UnloadSceneAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[GameSceneManager] 场景名称不能为空");
                return false;
            }

            try
            {
                // 触发卸载前事件
                OnSceneUnloadStart?.Invoke(this, new SceneLoadEventArgs
                {
                    SceneName = sceneName,
                    Progress = 0f,
                    IsSuccess = true
                });

                // 检查场景是否存在
                Scene scene = SceneManager.GetSceneByName(sceneName);
                if (scene.IsValid() && scene.isLoaded)
                {
                    // 卸载场景
                    AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(scene);
                    await unloadOperation;

                    // Debug.Log($"[GameSceneManager] 场景 {sceneName} 卸载成功");
                }

                // 触发卸载完成事件
                OnSceneUnloadComplete?.Invoke(this, new SceneLoadEventArgs
                {
                    SceneName = sceneName,
                    Progress = 1f,
                    IsSuccess = true
                });

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameSceneManager] 卸载场景 {sceneName} 时发生异常: {ex.Message}");

                // 触发卸载完成事件（失败）
                OnSceneUnloadComplete?.Invoke(this, new SceneLoadEventArgs
                {
                    SceneName = sceneName,
                    Progress = 0f,
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                });

                return false;
            }
        }

        /// <summary>
        /// 获取当前场景名称
        /// </summary>
        public string GetCurrentSceneName()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            return activeScene.name;
        }

        // /// <summary>
        // /// 获取当前场景路径
        // /// </summary>
        // public string GetCurrentScenePath()
        // {
        //     return _currentScenePath;
        // }

        /// <summary>
        /// 获取场景信息
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns>场景信息</returns>
        public Scene GetScene(string sceneName)
        {
            return SceneManager.GetSceneByName(sceneName);
        }

        /// <summary>
        /// 检查场景是否已加载
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public bool IsSceneLoaded(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene.IsValid() && scene.isLoaded;
        }

        /// <summary>
        /// 设置活动场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns>是否成功</returns>
        public bool SetActiveScene(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid())
            {
                bool success = SceneManager.SetActiveScene(scene);
                if (success)
                {
                    _currentSceneName = scene.name;
                    // _currentScenePath = GetScenePath(scene.name);
                }
                return success;
            }
            return false;
        }

        /// <summary>
        /// 检查是否正在加载场景
        /// </summary>
        public bool IsLoadingScene()
        {
            return _isLoading;
        }

        /// <summary>
        /// 取消当前场景加载（注意：Unity SceneManager 不支持真正的取消，只能标记为取消状态）
        /// </summary>
        public void CancelCurrentLoad()
        {
            if (_currentLoadOperation != null)
            {
                _currentLoadOperation = null;
                _isLoading = false;
                Debug.Log("[GameSceneManager] 已标记场景加载为取消状态");
            }
        }

        /// <summary>
        /// 获取所有已加载的场景
        /// </summary>
        /// <returns>已加载的场景数组</returns>
        public Scene[] GetLoadedScenes()
        {
            return SceneManager.GetAllScenes();
        }

        /// <summary>
        /// 获取已加载场景的数量
        /// </summary>
        public int GetLoadedSceneCount()
        {
            return SceneManager.sceneCount;
        }

        #endregion
    }
}