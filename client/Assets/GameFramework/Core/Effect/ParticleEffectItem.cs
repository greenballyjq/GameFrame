using GameFramework.Manager;
using UnityEngine;

namespace GameFramework.Core.Effect
{
    /// <summary>
    /// 粒子特效对象
    /// </summary>
    public class ParticleEffectItem : MonoBehaviour, IEffectItem
    {
        /// <summary>
        /// 粒子系统数组
        /// </summary>
        private ParticleSystem[] _particleSystems;

        private void Awake()
        {
            _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }

        public bool IsAlive
        {
            get
            {
                for (int i = 0; i < _particleSystems.Length; i++)
                    if (_particleSystems[i].IsAlive(true))
                        return true;

                return false;
            }
        }

        public void Play()
        {
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                var ps = _particleSystems[i];
                ps.Clear(true);
                ps.Play(true);
            }
        }

        public void Stop(EffectStopMode stopMode)
        {
            switch (stopMode)
            {
                case EffectStopMode.Graceful:
                    StopGraceful();
                    break;
                case EffectStopMode.Immediate:
                    StopImmediate();
                    break;
            }
        }

        public void Pause()
        {
            for (int i = 0; i < _particleSystems.Length; i++)
                _particleSystems[i].Pause(true);
        }

        public void Resume()
        {
            for (int i = 0; i < _particleSystems.Length; i++)
                _particleSystems[i].Play(true);
        }

        #region 私有方法
        /// <summary>
        /// 收尾停止
        /// </summary>
        private void StopGraceful()
        {
            for (int i = 0; i < _particleSystems.Length; i++)
                _particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        /// <summary>
        /// 立即停止
        /// </summary>
        private void StopImmediate()
        {
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                var ps = _particleSystems[i];
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Clear(true);
            }
        }
        #endregion
    }
}
