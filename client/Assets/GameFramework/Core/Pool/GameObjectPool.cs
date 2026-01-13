using System.Collections.Generic;
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
        /// 对象池根节点
        /// </summary>
        private readonly Transform _poolRoot;

        public GameObjectPool(GameObject prefab, Transform poolRoot)
        {
            _prefab = prefab;
            _poolRoot = poolRoot;
        }

        #region 公共方法
        /// <summary>
        /// 取出对象
        /// </summary>
        public GameObject Spawn()
        {
            GameObject obj = _stack.Count > 0 ? _stack.Pop() : Object.Instantiate(_prefab);
            obj.transform.SetParent(null);

            obj.SetActive(true);
            obj.GetComponent<IPoolItem>()?.OnSpawned();

            return obj;
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="obj">要回收的对象</param>
        public void Despawn(GameObject obj)
        {
            obj.transform.SetParent(_poolRoot);

            obj.GetComponent<IPoolItem>()?.OnDespawned();
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            obj.SetActive(false);

            _stack.Push(obj);
        }

        /// <summary>
        /// 预热对象池
        /// </summary>
        /// <param name="count">预热数量</param>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject obj = Object.Instantiate(_prefab);
                obj.transform.SetParent(_poolRoot);

                obj.GetComponent<IPoolItem>()?.OnDespawned();
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
                obj.SetActive(false);

                _stack.Push(obj);
            }
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

