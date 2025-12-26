using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Core.Pool;
using GameFramework.Manager;
using UnityEngine;

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
    private ResourceManager _resourceManager;

    /// <summary>
    /// 对象池根节点
    /// </summary>
    private Transform _poolRoot;

    public void Init()
    {
        // 创建对象池根节点
        GameObject rootObj = new GameObject("ObjectPoolRoot");
        _poolRoot = rootObj.transform;
        _poolRoot.position = Vector3.zero;
        Object.DontDestroyOnLoad(rootObj);

        // 获取资源管理器
        _resourceManager = GameFrameworkManager.Instance.GetManager<ResourceManager>();

        // Debug.Log("[GameObjectPoolManager] 初始化完成");
    }

    public void Update()
    {
    }

    public void Release()
    {
        // 遍历所有池，销毁池中的对象
        foreach (var pool in _pools.Values)
            pool.Clear();

        _pools.Clear();

        // 清理预制体缓存
        _prefabCache.Clear();

        // 清理实例映射
        _instancePrefabs.Clear();

        // 销毁根节点
        if (_poolRoot != null)
        {
            Object.Destroy(_poolRoot.gameObject);
            _poolRoot = null;
        }

        // Debug.Log("[GameObjectPoolManager] 所有对象池已释放");
    }

    #region 公共方法

    /// <summary>
    /// 获取预制体对应对象池
    /// </summary>
    /// <param name="prefab">预制体</param>
    /// <returns>对象池实例</returns>
    public GameObjectPool GetPool(GameObject prefab)
    {
        if (prefab == null)
        {
            // Debug.LogError("[GameObjectPoolManager] prefab不能为null");
            return null;
        }

        if (!_pools.TryGetValue(prefab, out GameObjectPool pool))
        {
            pool = new GameObjectPool(prefab, _poolRoot);
            _pools.Add(prefab, pool);

            // Debug.Log($"[GameObjectPoolManager] 创建对象池：{prefab.name}");
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
        if (string.IsNullOrEmpty(prefabPath))
        {
            // Debug.LogError("[GameObjectPoolManager] 预制体路径不能为空");
            return null;
        }

        // 从缓存获取预制体，如果不存在则加载
        if (!_prefabCache.TryGetValue(prefabPath, out var prefab))
        {
            prefab = await _resourceManager.LoadAssetAsync<GameObject>(prefabPath);
            if (prefab == null)
            {
                // Debug.LogError($"[GameObjectPoolManager] 加载预制体失败：{prefabPath}");
                return null;
            }

            // 在放入缓存前检查预制体是否有IPoolItem组件
            if (prefab.GetComponent<IPoolItem>() == null)
            {
                // Debug.LogError($"[GameObjectPoolManager] 预制体 {prefab.name} 没有实现IPoolItem接口，无法放入对象池");
                return null;
            }

            _prefabCache[prefabPath] = prefab;
        }

        GameObject obj = GetPool(prefab).Spawn();

        // 记录实例对应的预制体
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
        if (prefab == null)
        {
            Debug.LogError("[GameObjectPoolManager] prefab不能为null");
            return null;
        }

        // 检查预制体是否有IPoolItem组件
        if (prefab.GetComponent<IPoolItem>() == null)
        {
            Debug.LogError($"[GameObjectPoolManager] 预制体 {prefab.name} 没有实现IPoolItem接口");
            return null;
        }

        GameObject obj = GetPool(prefab).Spawn();

        // 记录实例对应的预制体
        _instancePrefabs[obj] = prefab;

        return obj;
    }

    /// <summary>
    /// 取出对象（路径方式）-需要已缓存
    /// </summary>
    /// <param name="prefabPath"></param>
    /// <returns></returns>
    public GameObject Spawn(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath))
        {
            // Debug.LogError("[GameObjectPoolManager] 预制体路径不能为空");
            return null;
        }

        // 从缓存获取预制体，如果不存在则返回空
        if (_prefabCache.TryGetValue(prefabPath, out var prefab))
        {
            GameObject obj = GetPool(prefab).Spawn();
            return obj;
        }

        return null;
    }

    /// <summary>
    /// 回收对象
    /// </summary>
    /// <param name="obj">要回收的对象</param>
    public void Despawn(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("[GameObjectPoolManager] 回收对象不能为null");
            return;
        }

        // 检查对象是否已经被回收
        if (!obj.activeSelf)
        {
            Debug.LogError($"[GameObjectPoolManager] 对象 {obj.name} 已经被回收，请勿重复回收");
            return;
        }

        // 从映射中获取预制体
        if (!_instancePrefabs.TryGetValue(obj, out var prefab))
        {
            Debug.LogError($"[GameObjectPoolManager] 对象 {obj.name} 不在对象池中");
            return;
        }

        GetPool(prefab).Despawn(obj);
        _instancePrefabs.Remove(obj);
    }

    /// <summary>
    /// 预加载预制体
    /// </summary>
    /// <param name="prefabPaths"></param>
    public async UniTask PreloadAsync(params string[] prefabPaths)
    {
        foreach (var prefabPath in prefabPaths)
        {
            if (_prefabCache.ContainsKey(prefabPath)) continue;
            var prefab = await _resourceManager.LoadAssetAsync<GameObject>(prefabPath);
            _prefabCache[prefabPath] = prefab;
            
            if (_pools.ContainsKey(prefab)) continue;
            var gameObjectPool = new GameObjectPool(prefab, _poolRoot);
            _pools[prefab] = gameObjectPool;
        }
    }

    #endregion
}