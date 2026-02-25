using GameFramework.Manager;

namespace GameFramework.Core.Effect
{
    /// <summary>
    /// 特效项接口
    /// </summary>
    public interface IEffectItem
    {
        /// <summary>
        /// 是否仍有可见内容
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// 播放特效
        /// </summary>
        void Play();

        /// <summary>
        /// 停止特效
        /// </summary>
        /// <param name="stopMode">停止模式</param>
        void Stop(EffectStopMode stopMode);

        /// <summary>
        /// 暂停特效
        /// </summary>
        void Pause();

        /// <summary>
        /// 恢复特效
        /// </summary>
        void Resume();
    }
}
