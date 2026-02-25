namespace GameFramework.Core
{
    /// <summary>
    /// 管理器接口
    /// </summary>
    public interface IManager
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Init();
        
        /// <summary>
        /// 更新
        /// </summary>
        void DoUpdate(float dt);
        
        /// <summary>
        /// 释放
        /// </summary>
        void Release();
    }
} 