using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Core.Pool;
using GameFramework.Utility;
using UnityEngine;

namespace GameFramework.Manager
{
    /// <summary>
    /// GameObject对象池管理器
    /// </summary>
    public class GameObjectPoolManager : IManager
    {
        /// <summary>
        /// 预制体对应对象池字典
        /// </summary>
        private readonly Dictionary<GameObject, GameObjectPool> _pools = new Dictionary<GameObject, GameObjectPool>();

        /// <summary>
        /// 预制体缓存字典
        /// </summary>
        private readonly Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();

        /// <summary>
        /// 实例对应的源预制体
        /// </summary>
        private readonly Dictionary<GameObject, GameObject> _instancePrefabs = new Dictionary<GameObject, GameObject>();

        /// <summary>
        /// 资源管理器引用
        /// </summary>
        private ResourceManager _resourceManager => GameFrameworkManager.Instance.GetManager<ResourceManager>();

        /// <summary>
        /// 对象池根节点
        /// </summary>
        private Transform _poolRoot;
        public Transform PoolRoot => _poolRoot;

        public UniTask InitAsync()
        {
            // 创建对象池根节点
            GameObject rootObj = new GameObject("ObjectPoolRoot");
            _poolRoot = rootObj.transform;
            _poolRoot.position = Vector3.zero;
            Object.DontDestroyOnLoad(rootObj);

            Log.Info("[GameObjectPoolManager] 初始化完成");
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            // 遍历所有池，销毁池中的对象
            ClearAllPools();

            // 销毁根节点
            Object.Destroy(_poolRoot.gameObject);
            _poolRoot = null;

            Debug.Log("[GameObjectPoolManager] 所有对象池已释放");
        }

        #region 公共方法
        /// <summary>
        /// 获取预制体对应对象池
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <returns>对象池实例</returns>
        public GameObjectPool GetPool(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out GameObjectPool pool))
            {
                pool = new GameObjectPool(prefab, _poolRoot);
                _pools.Add(prefab, pool);

                Debug.Log($"[GameObjectPoolManager] 创建对象池：{prefab.name}");
            }

            return pool;
        }

        /// <summary>
        /// 取出对象（路径方式）
        /// </summary>
        /// <param name="prefabPath">预制体资源路径</param>
        /// <returns>取出的对象</returns>
        public async UniTask<GameObject> SpawnAsync(string prefabPath)
        {
            // 从缓存获取预制体，如果不存在则加载
            if (!_prefabCache.TryGetValue(prefabPath, out var prefab))
            {
                prefab = await _resourceManager.LoadAssetAsync<GameObject>(prefabPath);
                _prefabCache[prefabPath] = prefab;
            }

            GameObject obj = GetPool(prefab).Spawn();

            _instancePrefabs[obj] = prefab;

            return obj;
        }

        /// <summary>
        /// 取出对象（预制体方式）
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <returns>取出的对象</returns>
        public GameObject Spawn(GameObject prefab)
        {
            GameObject obj = GetPool(prefab).Spawn();

            _instancePrefabs[obj] = prefab;

            return obj;
        }

        /// <summary>
        /// 预热对象池（路径方式）
        /// </summary>
        /// <param name="prefabPath">预制体资源路径</param>
        /// <param name="count">预热数量</param>
        public async UniTask PrewarmAsync(string prefabPath, int count)
        {
            if (!_prefabCache.TryGetValue(prefabPath, out var prefab))
            {
                prefab = await _resourceManager.LoadAssetAsync<GameObject>(prefabPath);
                _prefabCache[prefabPath] = prefab;
            }

            GetPool(prefab).Prewarm(count);
        }

        /// <summary>
        /// 预热对象池（预制体方式）
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="count">预热数量</param>
        public void Prewarm(GameObject prefab, int count)
        {
            GetPool(prefab).Prewarm(count);
        }

        /// <summary>
        /// 预加载预制体资源（仅缓存，不创建对象池）
        /// </summary>
        /// <param name="prefabPath">预制体资源路径</param>
        public async UniTask PreloadAsync(string prefabPath)
        {
            if (!_prefabCache.TryGetValue(prefabPath, out var prefab))
            {
                prefab = await _resourceManager.LoadAssetAsync<GameObject>(prefabPath);
                _prefabCache[prefabPath] = prefab;
            }
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="obj">要回收的对象</param>
        public void Despawn(GameObject obj)
        {
            // 检查对象是否已经被回收
            if (!obj.activeSelf)
            {
                Debug.LogError($"[GameObjectPoolManager] 对象 {obj.name} 已经被回收，请勿重复回收");
                return;
            }

            var prefab = _instancePrefabs[obj];

            GetPool(prefab).Despawn(obj);

            _instancePrefabs.Remove(obj);
        }

        /// <summary>
        /// 清理单个对象池（路径方式）
        /// </summary>
        /// <param name="prefabPath">预制体资源路径</param>
        public void ClearPool(string prefabPath)
        {
            var prefab = _prefabCache[prefabPath];
            ClearPool(prefab);
        }

        /// <summary>
        /// 清理单个对象池（预制体方式）
        /// </summary>
        /// <param name="prefab">预制体</param>
        public void ClearPool(GameObject prefab)
        {
            var pool = _pools[prefab];

            pool.Clear();
            _pools.Remove(prefab);

            var instances = new List<GameObject>();
            foreach (var instance in _instancePrefabs)
            {
                if (instance.Value == prefab)
                    instances.Add(instance.Key);
            }

            foreach (var instance in instances)
                _instancePrefabs.Remove(instance);
        }

        /// <summary>
        /// 清理所有对象池
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in _pools.Values)
                pool.Clear();

            _pools.Clear();
            _instancePrefabs.Clear();
            _prefabCache.Clear();
        }
        #endregion
    }
}