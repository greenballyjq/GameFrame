using GameFramework.Core.Effect;
using GameFramework.Core.Pool;
using UnityEngine;

namespace GameFramework.Core.Effect
{
    /// <summary>
    /// 粒子特效对象
    /// </summary>
    public class ParticleEffectItem : MonoBehaviour, IEffectItem, IPoolItem
    {
        /// <summary>
        /// 粒子系统数组
        /// </summary>
        private ParticleSystem[] _particleSystems;

        private void Awake()
        {
            _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }

        /// <summary>
        /// 当前特效是否仍有可见内容
        /// </summary>
        public bool IsAlive
        {
            get
            {
                if (_particleSystems == null || _particleSystems.Length == 0)
                    return false;

                for (int i = 0; i < _particleSystems.Length; i++)
                {
                    var ps = _particleSystems[i];
                    if (ps == null)
                        continue;

                    if (ps.IsAlive(true))
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 播放特效
        /// </summary>
        public void Play()
        {
            if (_particleSystems == null || _particleSystems.Length == 0)
                _particleSystems = GetComponentsInChildren<ParticleSystem>(true);

            if (_particleSystems == null)
                return;

            for (int i = 0; i < _particleSystems.Length; i++)
            {
                var ps = _particleSystems[i];
                if (ps == null)
                    continue;

                ps.Clear(true);
                ps.Play(true);
            }
        }

        /// <summary>
        /// 收尾停止
        /// </summary>
        public void StopGraceful()
        {
            if (_particleSystems == null)
                return;

            for (int i = 0; i < _particleSystems.Length; i++)
            {
                var ps = _particleSystems[i];
                if (ps == null)
                    continue;

                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        /// <summary>
        /// 立即停止
        /// </summary>
        public void StopImmediate()
        {
            if (_particleSystems == null)
                return;

            for (int i = 0; i < _particleSystems.Length; i++)
            {
                var ps = _particleSystems[i];
                if (ps == null)
                    continue;

                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Clear(true);
            }
        }

        /// <summary>
        /// 暂停特效
        /// </summary>
        public void Pause()
        {
            if (_particleSystems == null)
                return;

            for (int i = 0; i < _particleSystems.Length; i++)
            {
                var ps = _particleSystems[i];
                if (ps == null)
                    continue;

                ps.Pause(true);
            }
        }

        /// <summary>
        /// 恢复特效
        /// </summary>
        public void Resume()
        {
            if (_particleSystems == null)
                return;

            for (int i = 0; i < _particleSystems.Length; i++)
            {
                var ps = _particleSystems[i];
                if (ps == null)
                    continue;

                ps.Play(true);
            }
        }

        public void OnSpawned()
        {
            Play();
        }

        public void OnDespawned()
        {
            StopImmediate();
        }
    }
}
