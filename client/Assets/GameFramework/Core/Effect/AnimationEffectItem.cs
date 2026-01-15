using GameFramework.Core.Effect;
using GameFramework.Core.Pool;
using UnityEngine;

namespace GameFramework.Core.Effect
{
    /// <summary>
    /// 动画特效对象（支持Animator/Animation）
    /// </summary>
    [DisallowMultipleComponent]
    public class AnimationEffectItem : MonoBehaviour, IEffectItem, IPoolItem
    {
        /// <summary>
        /// Animator组件（优先使用）
        /// </summary>
        private Animator _animator;

        /// <summary>
        /// Animation组件（旧动画系统）
        /// </summary>
        private Animation _animation;

        /// <summary>
        /// 默认播放的状态名（Animator）或动画名（Animation）
        /// </summary>
        [SerializeField]
        private string _defaultStateName = "Default";

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>(true);
            _animation = GetComponentInChildren<Animation>(true);
        }

        /// <summary>
        /// 当前特效是否仍有可见内容
        /// </summary>
        public bool IsAlive
        {
            get
            {
                // Animator优先
                if (_animator != null)
                {
                    var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

                    // 循环动画：认为一直存活
                    if (stateInfo.loop)
                        return true;

                    // 非循环动画：播放未结束时认为存活
                    return stateInfo.normalizedTime < 1f;
                }

                if (_animation != null)
                {
                    // Animation：只要有任意clip在播放就认为存活
                    return _animation.isPlaying;
                }

                // 没有动画组件，视为不存活
                return false;
            }
        }

        #region IEffectItem实现
        /// <summary>
        /// 从头播放特效
        /// </summary>
        public void Play()
        {
            if (_animator != null)
            {
                _animator.speed = 1f;
                if (!string.IsNullOrEmpty(_defaultStateName))
                    _animator.Play(_defaultStateName, 0, 0f);
                else
                    _animator.Play(0, 0, 0f);
                return;
            }

            if (_animation != null)
            {
                if (!string.IsNullOrEmpty(_defaultStateName))
                    _animation.Play(_defaultStateName);
                else
                    _animation.Play();
            }
        }

        /// <summary>
        /// 收尾停止：停止新增表现，让当前已经存在的表现自然结束
        /// 对于动画来说，等价于不再触发新的播放，让当前这次播完即可，故这里通常不需要额外操作
        /// </summary>
        public void StopGraceful()
        {
            // Animator：不再重新Play，保持当前状态自然结束
            // Animation：不再重新Play，保持当前播放自然结束
            // 若未来需要“减速收尾”等逻辑，可以在这里扩展
        }

        /// <summary>
        /// 立即停止：立刻清除所有表现
        /// </summary>
        public void StopImmediate()
        {
            if (_animator != null)
            {
                _animator.Play(0, 0, 1f); // 跳到末尾
                _animator.Update(0f);
                _animator.speed = 0f;
                return;
            }

            if (_animation != null)
            {
                _animation.Stop();
            }
        }

        /// <summary>
        /// 暂停特效
        /// </summary>
        public void Pause()
        {
            if (_animator != null)
            {
                _animator.speed = 0f;
                return;
            }

            if (_animation != null)
                _animation.Sample(); // 保持当前帧
        }

        /// <summary>
        /// 恢复特效
        /// </summary>
        public void Resume()
        {
            if (_animator != null)
            {
                _animator.speed = 1f;
                return;
            }

            if (_animation != null)
                _animation.Play();
        }
        #endregion

        #region IPoolItem实现
        public void OnSpawned()
        {
            Play();
        }

        public void OnDespawned()
        {
            StopImmediate();
        }
        #endregion
    }
}
