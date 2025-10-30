using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using UnityEngine;

/// <summary>
/// 游戏对象池管理器
/// </summary>
public class GameObjectPoolManager : IManager
{
    /// <summary>
    /// 对象池字典
    /// </summary>
    private readonly Dictionary<string, Stack<GameObject>> _objectPools = new Dictionary<string, Stack<GameObject>>();

    /// <summary>
    /// 预制体缓存字典
    /// </summary>
    private readonly Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();

    /// <summary>
    /// 对象到路径的映射表
    /// </summary>
    private readonly Dictionary<GameObject, string> _objectToPathMap = new Dictionary<GameObject, string>();

    /// <summary>
    /// 正在使用中的对象集合
    /// </summary>
    private readonly HashSet<GameObject> _activeObjects = new HashSet<GameObject>();

    /// <summary>
    /// 资源管理器引用
    /// </summary>
    private ResourceManager _resourceManager;

    /// <summary>
    /// 对象池根节点
    /// </summary>
    private Transform _poolRoot;

    /// <summary>
    /// 每个对象池的最大容量（默认100）
    /// </summary>
    private int _maxPoolSize = 100;

    public void Init()
    {
        Debug.Log("[GameObjectPoolManager] 初始化游戏对象池管理器");

        _resourceManager = GameFrameworkManager.Instance.GetManager<ResourceManager>();

        CreatePoolRoot();
    }

    public void Update()
    {
        // 对象池管理器无需更新
    }

    public void Release()
    {
        DestroyAll();

        if (_poolRoot != null)
        {
            GameObject.Destroy(_poolRoot.gameObject);
            _poolRoot = null;
        }

        Debug.Log("[GameObjectPoolManager] 释放游戏对象池管理器");
    }

    #region 对象池核心方法
    /// <summary>
    /// 从对象池中取出对象
    /// </summary>
    /// <param name="prefabPath">预制体资源路径</param>
    /// <returns>返回GameObject，如果失败返回null</returns>
    public async UniTask<GameObject> PullAsync(string prefabPath)
    {
        if (!ValidatePrefabPath(prefabPath))
            return null;

        GameObject obj = null;

        // 检查对象池是否有可用对象
        if (_objectPools.TryGetValue(prefabPath, out var pool) && pool.Count > 0)
        {
            obj = pool.Pop();
        }
        // 对象池为空，检查预制体缓存
        else if (_prefabCache.TryGetValue(prefabPath, out var cachedPrefab))
        {
            obj = GameObject.Instantiate(cachedPrefab);
            _objectToPathMap[obj] = prefabPath;
        }
        // 预制体未缓存，需要加载
        else
        {
            var prefab = await _resourceManager.LoadAssetAsync<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[GameObjectPoolManager] 加载预制体失败: {prefabPath}");
                return null;
            }

            _prefabCache[prefabPath] = prefab;
            obj = GameObject.Instantiate(prefab);
            _objectToPathMap[obj] = prefabPath;
        }

        // 激活对象并移出池
        obj.SetActive(true);
        obj.transform.SetParent(null);

        // 加入活跃对象集合
        _activeObjects.Add(obj);

        // 调用池化回调
        TryInvokePoolableCallback(obj, PoolableCallback.OnPull);

        Debug.Log($"[GameObjectPoolManager] 从对象池取出对象: {prefabPath}");
        return obj;
    }

    /// <summary>
    /// 回收对象到对象池
    /// </summary>
    /// <param name="obj">要回收的GameObject</param>
    public void Push(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("[GameObjectPoolManager] 回收对象不能为空");
            return;
        }

        // 检查对象是否由对象池管理
        if (!_objectToPathMap.TryGetValue(obj, out string prefabPath))
        {
            Debug.LogError($"[GameObjectPoolManager] 无法回收对象 {obj.name}：该对象不是从对象池中创建的");
            return;
        }

        // 检查是否重复回收
        if (!_activeObjects.Contains(obj))
        {
            Debug.LogWarning($"[GameObjectPoolManager] 对象 {obj.name} 已经在池中或已被回收，忽略重复回收");
            return;
        }

        // 从活跃对象集合中移除
        _activeObjects.Remove(obj);

        // 调用池化回调
        TryInvokePoolableCallback(obj, PoolableCallback.OnPush);

        var pool = EnsurePoolExists(prefabPath);

        // 检查对象池容量
        if (pool.Count >= _maxPoolSize)
        {
            DestroyManagedObject(obj);
            Debug.Log($"[GameObjectPoolManager] 对象池已满，销毁对象: {prefabPath}");
            return;
        }

        // 重置对象状态
        ResetObjectTransform(obj);

        // 回收到对象池
        pool.Push(obj);

        Debug.Log($"[GameObjectPoolManager] 回收对象到池: {prefabPath}");
    }

    /// <summary>
    /// 预加载对象到对象池
    /// </summary>
    /// <param name="prefabPath">预制体路径</param>
    /// <param name="count">预加载数量</param>
    public async UniTask PreloadAsync(string prefabPath, int count)
    {
        if (!ValidatePrefabPath(prefabPath))
            return;

        if (count <= 0)
        {
            Debug.LogError("[GameObjectPoolManager] 预加载数量必须大于0");
            return;
        }

        var prefab = await _resourceManager.LoadAssetAsync<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[GameObjectPoolManager] 预加载失败，无法加载预制体: {prefabPath}");
            return;
        }

        _prefabCache[prefabPath] = prefab;

        var pool = EnsurePoolExists(prefabPath);

        for (int i = 0; i < count; i++)
        {
            var obj = GameObject.Instantiate(prefab, _poolRoot);
            obj.SetActive(false);
            obj.name = prefab.name;

            _objectToPathMap[obj] = prefabPath;

            pool.Push(obj);
        }

        Debug.Log($"[GameObjectPoolManager] 预加载完成: {prefabPath}, 数量: {count}");
    }

    /// <summary>
    /// 清除指定对象池（不影响活跃对象）
    /// </summary>
    /// <param name="prefabPath">预制体路径</param>
    public void ClearPool(string prefabPath)
    {
        if (!ValidatePrefabPath(prefabPath))
            return;

        if (_objectPools.TryGetValue(prefabPath, out var pool))
        {
            while (pool.Count > 0)
            {
                var obj = pool.Pop();
                DestroyManagedObject(obj);
            }

            _objectPools.Remove(prefabPath);
            _prefabCache.Remove(prefabPath);
            Debug.Log($"[GameObjectPoolManager] 清除对象池: {prefabPath}");
        }
    }

    /// <summary>
    /// 清除所有对象池（不影响活跃对象）
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var kvp in _objectPools)
        {
            var pool = kvp.Value;
            while (pool.Count > 0)
            {
                var obj = pool.Pop();
                DestroyManagedObject(obj);
            }
        }

        _objectPools.Clear();
        _prefabCache.Clear();
        Debug.Log($"[GameObjectPoolManager] 清除所有对象池，剩余活跃对象: {_activeObjects.Count}");
    }

    /// <summary>
    /// 强制销毁所有对象，包括正在使用的活跃对象
    /// </summary>
    public void DestroyAll()
    {
        ClearAllPools();

        var activeObjectsCopy = new List<GameObject>(_activeObjects);
        foreach (var obj in activeObjectsCopy)
        {
            if (obj != null)
                DestroyManagedObject(obj);
        }

        _activeObjects.Clear();
        Debug.Log("[GameObjectPoolManager] 已销毁所有对象");
    }

    #endregion

    #region 查询方法

    /// <summary>
    /// 设置每个对象池的最大容量
    /// </summary>
    /// <param name="maxSize">最大容量</param>
    public void SetMaxPoolSize(int maxSize)
    {
        if (maxSize < 0)
        {
            Debug.LogWarning("[GameObjectPoolManager] 对象池最大容量不能小于0，使用默认值100");
            _maxPoolSize = 100;
            return;
        }
        _maxPoolSize = maxSize;
        Debug.Log($"[GameObjectPoolManager] 设置对象池最大容量: {_maxPoolSize}");
    }

    /// <summary>
    /// 获取每个对象池的最大容量
    /// </summary>
    /// <returns>最大容量</returns>
    public int GetMaxPoolSize()
    {
        return _maxPoolSize;
    }

    /// <summary>
    /// 获取对象池中指定类型的对象数量
    /// </summary>
    /// <param name="prefabPath">预制体路径</param>
    /// <returns>池中对象数量</returns>
    public int GetPoolCount(string prefabPath)
    {
        if (_objectPools.TryGetValue(prefabPath, out var pool))
            return pool.Count;

        return 0;
    }

    /// <summary>
    /// 检查对象池是否存在
    /// </summary>
    /// <param name="prefabPath">预制体路径</param>
    /// <returns>是否存在该对象池</returns>
    public bool HasPool(string prefabPath)
    {
        return _objectPools.ContainsKey(prefabPath);
    }

    /// <summary>
    /// 获取正在使用中的对象数量
    /// </summary>
    /// <param name="prefabPath">预制体路径，如果为null则返回所有活跃对象数量</param>
    /// <returns>活跃对象数量</returns>
    public int GetActiveCount(string prefabPath = null)
    {
        if (string.IsNullOrEmpty(prefabPath))
            return _activeObjects.Count;

        int count = 0;
        foreach (var obj in _activeObjects)
        {
            if (_objectToPathMap.TryGetValue(obj, out string path) && path == prefabPath)
                count++;
        }
        return count;
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 创建对象池根节点
    /// </summary>
    private void CreatePoolRoot()
    {
        var poolRootObj = new GameObject("ObjectPoolRoot");
        poolRootObj.transform.position = Vector3.zero;
        GameObject.DontDestroyOnLoad(poolRootObj);

        _poolRoot = poolRootObj.transform;

        Debug.Log("[GameObjectPoolManager] 对象池根节点创建完成");
    }

    /// <summary>
    /// 验证预制体路径是否有效
    /// </summary>
    /// <param name="prefabPath">预制体路径</param>
    /// <returns>路径是否有效</returns>
    private bool ValidatePrefabPath(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError("[GameObjectPoolManager] 预制体路径不能为空");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 确保指定路径的对象池存在
    /// </summary>
    /// <param name="prefabPath">预制体路径</param>
    /// <returns>对象池实例</returns>
    private Stack<GameObject> EnsurePoolExists(string prefabPath)
    {
        if (!_objectPools.ContainsKey(prefabPath))
            _objectPools[prefabPath] = new Stack<GameObject>();

        return _objectPools[prefabPath];
    }

    /// <summary>
    /// 重置对象的Transform状态
    /// </summary>
    /// <param name="obj">要重置的对象</param>
    private void ResetObjectTransform(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(_poolRoot);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 尝试调用IPoolable回调
    /// </summary>
    /// <param name="obj">游戏对象</param>
    /// <param name="callback">回调类型</param>
    private void TryInvokePoolableCallback(GameObject obj, PoolableCallback callback)
    {
        var poolable = obj.GetComponent<IPoolable>();
        if (poolable != null)
        {
            switch (callback)
            {
                case PoolableCallback.OnPull:
                    poolable.OnPull();
                    break;
                case PoolableCallback.OnPush:
                    poolable.OnPush();
                    break;
                case PoolableCallback.OnPoolDestroy:
                    poolable.OnPoolDestroy();
                    break;
            }
        }
    }

    /// <summary>
    /// 销毁对象并清理相关记录
    /// </summary>
    /// <param name="obj">要销毁的对象</param>
    private void DestroyManagedObject(GameObject obj)
    {
        TryInvokePoolableCallback(obj, PoolableCallback.OnPoolDestroy);

        _objectToPathMap.Remove(obj);

        GameObject.Destroy(obj);
    }

    /// <summary>
    /// 池化回调类型
    /// </summary>
    private enum PoolableCallback
    {
        OnPull,
        OnPush,
        OnPoolDestroy
    }

    #endregion
}