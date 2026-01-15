using UnityEngine;

namespace GameFramework.Core.Effect
{
    /// <summary>
    /// 特效接口
    /// </summary>
    public interface IEffectItem
    {
        /// <summary>
        /// 当前特效是否仍有可见内容
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// 播放特效
        /// </summary>
        void Play();

        /// <summary>
        /// 收尾停止
        /// </summary>
        void StopGraceful();

        /// <summary>
        /// 立即停止
        /// </summary>
        void StopImmediate();

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
