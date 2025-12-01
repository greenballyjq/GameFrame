using UnityEngine;

namespace GameFramework.Core.Pool
{
    /// <summary>
    /// 通用可池化对象组件
    /// </summary>
    public class PoolableObject : MonoBehaviour, IPoolItem
    {
        /// <summary>
        /// 从池中取出时调用
        /// </summary>
        public virtual void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 回收到池中时调用
        /// </summary>
        public virtual void OnDespawned()
        {
            gameObject.SetActive(false);
        }
    }
}

