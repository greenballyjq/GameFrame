using GameFramework.Core.Effect;
using GameFramework.Core.Pool;
using UnityEngine;

namespace GameFramework.Core.Effect
{
    /// <summary>
    /// 粒子特效对象（池化）
    /// </summary>
    [DisallowMultipleComponent]
    public class ParticleEffectItem : MonoBehaviour, IPoolItem, IEffectItem
    {
        /// <summary>
        /// 粒子系统缓存
        /// </summary>
        private ParticleSystem[] _particleSystems;

        private void Awake()
        {
            // 缓存粒子系统，包含子对象
            _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }

        #region IEffectItem
        public bool IsLoop
        {
            get
            {
                if (_particleSystems == null || _particleSystems.Length == 0)
                    return false;

                for (int i = 0; i < _particleSystems.Length; i++)
                {
                    var ps = _particleSystems[i];
                    if (ps == null) continue;

                    if (ps.main.loop)
                        return true;
                }

                return false;
            }
        }

        public void Play()
        {
            if (_particleSystems == null || _particleSystems.Length == 0)
                _particleSystems = GetComponentsInChildren<ParticleSystem>(true);

            // 为避免复用残留，先清再播
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                var ps = _particleSystems[i];
                if (ps == null) continue;

                ps.Clear(true);
                ps.Play(true);
            }
        }

        public void StopSoft()
        {
            if (_particleSystems == null) return;

            // 停止发射，不清屏，允许尾巴自然结束
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                var ps = _particleSystems[i];
                if (ps == null) continue;

                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        public void StopImmediate()
        {
            if (_particleSystems == null) return;

            // 立即停止并清屏，确保下次复用干净
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                var ps = _particleSystems[i];
                if (ps == null) continue;

                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Clear(true);
            }
        }

        public bool IsAlive()
        {
            if (_particleSystems == null) return false;

            // 任意一个子粒子仍存活，都视为存活
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                var ps = _particleSystems[i];
                if (ps == null) continue;

                if (ps.IsAlive(true))
                    return true;
            }

            return false;
        }

        public float GetEstimatedDuration()
        {
            if (_particleSystems == null || _particleSystems.Length == 0)
                return 0f;

            if (IsLoop)
                return -1f;

            float max = 0f;

            // 简单估算：duration + startLifetime(max)
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                var ps = _particleSystems[i];
                if (ps == null) continue;

                var main = ps.main;

                float lifetimeMax = 0f;
                var lifetime = main.startLifetime;

                if (lifetime.mode == ParticleSystemCurveMode.Constant)
                    lifetimeMax = lifetime.constant;
                else if (lifetime.mode == ParticleSystemCurveMode.TwoConstants)
                    lifetimeMax = lifetime.constantMax;
                else
                    lifetimeMax = lifetime.constantMax;

                float t = main.duration + lifetimeMax;
                if (t > max) max = t;
            }

            return max;
        }
        #endregion

        #region IPoolItem
        public void OnSpawned()
        {
            // 取出时直接播放
            Play();
        }

        public void OnDespawned()
        {
            // 回收时强制清理，避免下次复用残留
            StopImmediate();
        }
        #endregion
    }
}
