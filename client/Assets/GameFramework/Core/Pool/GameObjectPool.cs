using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Core.Pool
{
    /// <summary>
    /// GameObject对象池
    /// </summary>
    public class GameObjectPool
    {
        /// <summary>
        /// 对象池栈
        /// </summary>
        private readonly Stack<GameObject> _stack = new Stack<GameObject>();

        /// <summary>
        /// 预制体引用
        /// </summary>
        private readonly GameObject _prefab;

        /// <summary>
        /// 父节点
        /// </summary>
        private readonly Transform _parent;

        /// <summary>
        /// 构造GameObject对象池。
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">父节点</param>
        public GameObjectPool(GameObject prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        #region 公共方法
        /// <summary>
        /// 取出对象
        /// </summary>
        public GameObject Spawn()
        {
            GameObject obj = _stack.Count > 0 ? _stack.Pop() : Object.Instantiate(_prefab);

            IPoolItem item = obj.GetComponent<IPoolItem>();
            item.OnSpawned();
            obj.SetActive(true);

            return obj;
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="obj">要回收的对象</param>
        public void Despawn(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[GameObjectPool] 回收对象不能为null");
                return;
            }

            if (!obj.activeSelf)
            {
                Debug.LogError($"[GameObjectPool] 对象 {obj.name} 已经被回收，请勿重复回收");
                return;
            }

            IPoolItem item = obj.GetComponent<IPoolItem>();
            item.OnDespawned();
            obj.SetActive(false);

            obj.transform.SetParent(_parent);
            _stack.Push(obj);
        }

        /// <summary>
        /// 清理池中的所有对象
        /// </summary>
        public void Clear()
        {
            while (_stack.Count > 0)
            {
                var obj = _stack.Pop();
                if (obj != null)
                    Object.Destroy(obj);
            }
        }
        #endregion
    }

}

