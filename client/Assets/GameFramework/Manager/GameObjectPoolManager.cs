using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Core.Pool;
using UnityEngine;

/// <summary>
/// GameObject对象池管理器。  
/// </summary>
public class GameObjectPoolManager : IManager
{
    /// <summary>
    /// prefab对应的对象池字典
    /// </summary>
    private readonly Dictionary<GameObject, GameObjectPool> _pools = new Dictionary<GameObject, GameObjectPool>();
    
    /// <summary>
    /// 预制体缓存字典
    /// </summary>
    private readonly Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();
    
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
        var rootObj = new GameObject("ObjectPoolRoot");
        _poolRoot = rootObj.transform;
        _poolRoot.position = Vector3.zero;
        Object.DontDestroyOnLoad(rootObj);
        
        // 获取资源管理器
        _resourceManager = GameFrameworkManager.Instance.GetManager<ResourceManager>();
        
        Debug.Log("[GameObjectPoolManager] 初始化完成");
    }

    public void Update()
    {
        // 此管理器不需要更新
    }

    public void Release()
    {
        // 遍历所有池，销毁池中的对象
        foreach (var pool in _pools.Values)
            pool.Clear();

        _pools.Clear();
        
        // 清理预制体缓存
        _prefabCache.Clear();
        
        // 销毁根节点
        if (_poolRoot != null)
        {
            Object.Destroy(_poolRoot.gameObject);
            _poolRoot = null;
        }
        
        Debug.Log("[GameObjectPoolManager] 所有对象池已释放");
    }

    #region 公共方法
    /// <summary>
    /// 获取某prefab对应的池
    /// </summary>
    /// <param name="prefab">预制体</param>
    /// <returns>对象池实例</returns>
    public GameObjectPool GetPool(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("[GameObjectPoolManager] prefab不能为null");
            return null;
        }
        
        if (!_pools.TryGetValue(prefab, out GameObjectPool pool))
        {
            pool = new GameObjectPool(prefab, _poolRoot);
            _pools.Add(prefab, pool);

            Debug.Log($"[GameObjectPoolManager] 创建对象池：{prefab.name}");
        }

        return pool;
    }

    /// <summary>
    /// 创建对象
    /// </summary>
    /// <param name="prefab">预制体</param>
    /// <returns>创建的GameObject</returns>
    public GameObject Spawn(GameObject prefab)
    {
        return GetPool(prefab).Spawn();
    }

    /// <summary>
    /// 异步创建对象（根据资源路径）
    /// </summary>
    /// <param name="prefabPath">预制体资源路径</param>
    /// <returns>创建的GameObject</returns>
    public async UniTask<GameObject> SpawnAsync(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError("[GameObjectPoolManager] 预制体路径不能为空");
            return null;
        }

        // 加载或从缓存获取预制体
        if (!_prefabCache.TryGetValue(prefabPath, out var prefab))
        {
            prefab = await _resourceManager.LoadAssetAsync<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[GameObjectPoolManager] 加载预制体失败：{prefabPath}");
                return null;
            }
            _prefabCache[prefabPath] = prefab;
        }

        var obj = GetPool(prefab).Spawn();

        // 设置IPoolItem中的路径
        var poolItem = obj.GetComponent<IPoolItem>();
        if (poolItem != null)
            poolItem.PrefabPath = prefabPath;

        return obj;
    }

    /// <summary>
    /// 回收对象
    /// </summary>
    /// <param name="obj">要回收的对象</param>
    /// <param name="prefab">预制体</param>
    public void Despawn(GameObject obj, GameObject prefab)
    {
        GetPool(prefab).Despawn(obj);
    }

    /// <summary>
    /// 回收对象（根据资源路径）
    /// </summary>
    /// <param name="prefabPath">预制体资源路径</param>
    /// <param name="obj">要回收的对象</param>
    public void Despawn(GameObject obj,string prefabPath)
    {
        if (obj == null)
        {
            Debug.LogError("[GameObjectPoolManager] 回收对象不能为null");
            return;
        }
        
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError("[GameObjectPoolManager] 预制体路径不能为空");
            return;
        }
        
        // 从缓存获取预制体
        if (!_prefabCache.TryGetValue(prefabPath, out var prefab))
        {
            Debug.LogError($"[GameObjectPoolManager] 预制体 {prefabPath} 不在缓存中，对象可能不是从这个路径创建的");
            return;
        }
        
        GetPool(prefab).Despawn(obj);
    }

    /// <summary>
    /// 回收对象（从接口获取路径）
    /// </summary>
    /// <param name="obj">要回收的对象</param>
    public void Despawn(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("[GameObjectPoolManager] 回收对象不能为null");
            return;
        }
        
        // 从接口获取路径
        var poolItem = obj.GetComponent<IPoolItem>();
        if (poolItem == null || string.IsNullOrEmpty(poolItem.PrefabPath))
        {
            Debug.LogError($"[GameObjectPoolManager] 对象 {obj.name} 没有实现IPoolItem或PrefabPath为空");
            return;
        }
        
        // 从缓存获取预制体
        if (!_prefabCache.TryGetValue(poolItem.PrefabPath, out var prefab))
        {
            Debug.LogError($"[GameObjectPoolManager] 预制体 {poolItem.PrefabPath} 不在缓存中");
            return;
        }
        
        GetPool(prefab).Despawn(obj);
    }
    #endregion
}
