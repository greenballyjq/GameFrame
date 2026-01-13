using UnityEngine;

namespace GameFramework.Core.Effect
{
    /// <summary>
    /// 特效对象接口（用于统一播放、停止与结束判定）
    /// </summary>
    public interface IEffectItem
    {
        /// <summary>
        /// 是否循环播放
        /// </summary>
        bool IsLoop { get; }

        /// <summary>
        /// 开始播放
        /// </summary>
        void Play();

        /// <summary>
        /// 软停止：停止发射/停止播放，但允许尾巴自然结束
        /// </summary>
        void StopSoft();

        /// <summary>
        /// 立即停止：立刻清除/终止显示
        /// </summary>
        void StopImmediate();

        /// <summary>
        /// 是否仍在播放/是否仍有残留
        /// </summary>
        bool IsAlive();

        /// <summary>
        /// 获取一次性特效的估算时长（循环返回-1）
        /// </summary>
        float GetEstimatedDuration();
    }
}
