using UnityEngine;

/// <summary>
/// 可池化对象接口 - 需要被存入对象池的对象必须实现此接口
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// 从对象池中取出时调用
    /// </summary>
    void OnPull();
    
    /// <summary>
    /// 回收到对象池时调用
    /// </summary>
    void OnPush();
    
    /// <summary>
    /// 对象池销毁时调用（可选实现）
    /// </summary>
    void OnPoolDestroy();
}