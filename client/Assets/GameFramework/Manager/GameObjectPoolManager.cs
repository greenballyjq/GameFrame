using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using UnityEngine;

/// <summary>
/// 游戏对象池管理器 - 管理GameObject的对象池
/// </summary>
public class GameObjectPoolManager : IManager
{
    // 对象池字典 - 按预制体路径分组
    private readonly Dictionary<string, Stack<GameObject>> _objectPools = new Dictionary<string, Stack<GameObject>>();

    // 预制体缓存字典
    private readonly Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();

    // 【新增】对象到路径的映射表
    private readonly Dictionary<GameObject, string> _objectToPathMap = new Dictionary<GameObject, string>();

    // 资源管理器引用
    private ResourceManager _resourceManager;

    // 对象池根节点
    private Transform _poolRoot;

    public void Init()
    {
        Debug.Log("[GameObjectPoolManager] 初始化游戏对象池管理器");

        _resourceManager = GameFrameworkManager.Instance.GetManager<ResourceManager>();

        // 创建对象池根节点
        CreatePoolRoot();
    }

    public void Update()
    {
        // 对象池管理器无需更新
    }

    public void Release()
    {
        // 销毁所有对象池
        ClearAllPools();

        // 清除预制体缓存
        _prefabCache.Clear();

        // 【新增】清除映射表
        _objectToPathMap.Clear();

        // 销毁根节点
        if (_poolRoot != null)
        {
            GameObject.Destroy(_poolRoot.gameObject);
            _poolRoot = null;
        }

        Debug.Log("[GameObjectPoolManager] 释放游戏对象池管理器");
    }

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
    /// 从对象池中取出对象
    /// </summary>
    public async UniTask<T> PullAsync<T>(string prefabPath) where T : Component, IPoolable
    {
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError("[GameObjectPoolManager] 预制体路径不能为空");
            return null;
        }

        // 1. 检查对象池是否有可用对象
        if (_objectPools.TryGetValue(prefabPath, out var pool) && pool.Count > 0)
        {
            var pooledObj = pool.Pop();
            pooledObj.SetActive(true);
            pooledObj.transform.SetParent(null); // 从池中移出

            var poolableComponent = pooledObj.GetComponent<T>();
            if (poolableComponent != null)
            {
                poolableComponent.OnPull();
            }

            Debug.Log($"[GameObjectPoolManager] 从对象池取出对象: {prefabPath}");
            return poolableComponent;
        }

        // 2. 对象池为空，检查预制体缓存
        GameObject prefab = null;
        if (_prefabCache.TryGetValue(prefabPath, out prefab))
        {
            // 预制体已缓存，直接实例化
            var newObj = GameObject.Instantiate(prefab);
            newObj.SetActive(true);

            // 【新增】记录对象到路径的映射
            _objectToPathMap[newObj] = prefabPath;

            var newPoolableComponent = newObj.GetComponent<T>();
            if (newPoolableComponent != null)
            {
                newPoolableComponent.OnPull();
            }

            Debug.Log($"[GameObjectPoolManager] 从缓存实例化对象: {prefabPath}");
            return newPoolableComponent;
        }

        // 3. 预制体未缓存，需要加载
        prefab = await _resourceManager.LoadAssetAsync<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[GameObjectPoolManager] 加载预制体失败: {prefabPath}");
            return null;
        }

        // 4. 缓存预制体
        _prefabCache[prefabPath] = prefab;

        // 5. 实例化新对象
        var instantiatedObj = GameObject.Instantiate(prefab);
        instantiatedObj.SetActive(true);

        // 【新增】记录对象到路径的映射
        _objectToPathMap[instantiatedObj] = prefabPath;

        var instantiatedPoolableComponent = instantiatedObj.GetComponent<T>();
        if (instantiatedPoolableComponent != null)
        {
            instantiatedPoolableComponent.OnPull();
        }

        Debug.Log($"[GameObjectPoolManager] 加载并实例化对象: {prefabPath}");
        return instantiatedPoolableComponent;
    }

    /// <summary>
    /// 回收对象到对象池
    /// </summary>
    public void Push(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("[GameObjectPoolManager] 回收对象不能为空");
            return;
        }

        // 【修改】从映射表获取预制体路径
        if (!_objectToPathMap.TryGetValue(obj, out string prefabPath))
        {
            Debug.LogError($"[GameObjectPoolManager] 对象 {obj.name} 没有路径映射记录");
            return;
        }

        // 调用对象的回收方法
        var poolableComponent = obj.GetComponent<IPoolable>();
        if (poolableComponent != null)
        {
            poolableComponent.OnPush();
        }

        // 重置对象状态
        obj.SetActive(false);
        obj.transform.SetParent(_poolRoot);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        // 确保对象池存在
        if (!_objectPools.ContainsKey(prefabPath))
        {
            _objectPools[prefabPath] = new Stack<GameObject>();
        }

        // 回收到对象池
        _objectPools[prefabPath].Push(obj);

        Debug.Log($"[GameObjectPoolManager] 回收对象到池: {prefabPath}");
    }

    /// <summary>
    /// 预加载对象到对象池
    /// </summary>
    public async UniTask PreloadAsync(string prefabPath, int count)
    {
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError("[GameObjectPoolManager] 预制体路径不能为空");
            return;
        }

        if (count <= 0)
        {
            Debug.LogError("[GameObjectPoolManager] 预加载数量必须大于0");
            return;
        }

        // 加载预制体
        var prefab = await _resourceManager.LoadAssetAsync<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[GameObjectPoolManager] 预加载失败，无法加载预制体: {prefabPath}");
            return;
        }

        // 缓存预制体
        _prefabCache[prefabPath] = prefab;

        // 确保对象池存在
        if (!_objectPools.ContainsKey(prefabPath))
        {
            _objectPools[prefabPath] = new Stack<GameObject>();
        }

        // 预创建指定数量的对象
        for (int i = 0; i < count; i++)
        {
            var obj = GameObject.Instantiate(prefab, _poolRoot);
            obj.SetActive(false);
            obj.name = prefab.name; // 确保名称一致

            // 【新增】记录对象到路径的映射
            _objectToPathMap[obj] = prefabPath;

            _objectPools[prefabPath].Push(obj);
        }

        Debug.Log($"[GameObjectPoolManager] 预加载完成: {prefabPath}, 数量: {count}");
    }

    /// <summary>
    /// 清除指定对象池
    /// </summary>
    public void ClearPool(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError("[GameObjectPoolManager] 预制体路径不能为空");
            return;
        }

        if (_objectPools.TryGetValue(prefabPath, out var pool))
        {
            // 调用所有对象的销毁方法
            while (pool.Count > 0)
            {
                var obj = pool.Pop();
                var poolableComponent = obj.GetComponent<IPoolable>();
                if (poolableComponent != null)
                {
                    poolableComponent.OnPoolDestroy();
                }

                // 【新增】清理映射记录
                _objectToPathMap.Remove(obj);

                GameObject.Destroy(obj);
            }

            _objectPools.Remove(prefabPath);
            Debug.Log($"[GameObjectPoolManager] 清除对象池: {prefabPath}");
        }
    }

    /// <summary>
    /// 清除所有对象池
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var kvp in _objectPools)
        {
            var pool = kvp.Value;
            while (pool.Count > 0)
            {
                var obj = pool.Pop();
                var poolableComponent = obj.GetComponent<IPoolable>();
                if (poolableComponent != null)
                {
                    poolableComponent.OnPoolDestroy();
                }

                // 【新增】清理映射记录
                _objectToPathMap.Remove(obj);

                GameObject.Destroy(obj);
            }
        }

        _objectPools.Clear();
        Debug.Log("[GameObjectPoolManager] 清除所有对象池");
    }

    /// <summary>
    /// 获取对象池中指定类型的对象数量
    /// </summary>
    public int GetPoolCount(string prefabPath)
    {
        if (_objectPools.TryGetValue(prefabPath, out var pool))
        {
            return pool.Count;
        }
        return 0;
    }

    /// <summary>
    /// 检查对象池是否存在
    /// </summary>
    public bool HasPool(string prefabPath)
    {
        return _objectPools.ContainsKey(prefabPath);
    }
}