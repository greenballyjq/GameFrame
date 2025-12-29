namespace GameFramework.Game
{
    /// <summary>
    /// 应用级管理器接口
    /// </summary>
    public interface IAppManager
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Init();

        /// <summary>
        /// 释放
        /// </summary>
        void Dispose();
    }

    /// <summary>
    /// 可更新的应用级管理器
    /// </summary>
    public interface IAppUpdatable
    {
        /// <summary>
        /// 每帧更新
        /// </summary>
        /// <param name="dt">时间增量</param>
        void DoUpdate(float dt);
    }

    /// <summary>
    /// 可暂停的应用级管理器
    /// </summary>
    public interface IAppPausable
    {
        /// <summary>
        /// 暂停
        /// </summary>
        void Pause();

        /// <summary>
        /// 恢复
        /// </summary>
        void Resume();
    }
}
