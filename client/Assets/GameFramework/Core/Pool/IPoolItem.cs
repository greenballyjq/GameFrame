
namespace GameFramework.Core.Pool
{
    /// <summary>
    /// 可池化对象接口
    /// </summary>
    public interface IPoolItem
    {
        /// <summary>
        /// 对象从池子取出后调用
        /// </summary>
        void OnSpawned();

        /// <summary>
        /// 对象回收到池子之前调用
        /// </summary>
        void OnDespawned();
    }
}