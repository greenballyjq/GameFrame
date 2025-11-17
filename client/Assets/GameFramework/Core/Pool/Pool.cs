using System;
using System.Collections.Generic;

namespace GameFramework.Core.Pool
{
    /// <summary>
    /// 泛型对象池
    /// </summary>
    public class Pool<T> where T : IPoolItem
    {
        private readonly Stack<T> _stack = new Stack<T>();
        private readonly Func<T> _factory;

        /// <summary>
        /// 构造对象池
        /// </summary>
        /// <param name="factory">创建T的方法</param>
        public Pool(Func<T> factory)
        {
            _factory = factory;
        }

        #region 公共方法
        /// <summary>
        /// 从池中取出一个对象
        /// </summary>
        public T Spawn()
        {
            T item = _stack.Count > 0 ? _stack.Pop() : _factory();
            item.OnSpawned();
            return item;
        }

        /// <summary>
        /// 将对象回收到池子中
        /// </summary>
        public void Despawn(T item)
        {
            item.OnDespawned();
            _stack.Push(item);
        }
        #endregion
    }
}
