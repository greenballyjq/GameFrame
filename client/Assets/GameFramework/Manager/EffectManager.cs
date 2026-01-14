using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Core.Effect;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Manager
{
    /// <summary>
    /// 特效停止模式
    /// </summary>
    public enum EffectStopMode
    {
        /// <summary>
        /// 收尾停止
        /// </summary>
        Graceful,

        /// <summary>
        /// 立即停止
        /// </summary>
        Immediate
    }

    /// <summary>
    /// 特效播放类型
    /// </summary>
    internal enum EffectPlayType
    {
        /// <summary>
        /// 一次性特效
        /// </summary>
        OneShot,

        /// <summary>
        /// 循环特效
        /// </summary>
        Loop,

        /// <summary>
        /// 持续时间特效
        /// </summary>
        Duration
    }

    /// <summary>
    /// 特效实例状态
    /// </summary>
    internal class EffectInstance
    {
        /// <summary>
        /// 特效对象
        /// </summary>
        public GameObject GameObject;

        /// <summary>
        /// 特效接口
        /// </summary>
        public IEffectItem EffectItem;

        /// <summary>
        /// 播放类型
        /// </summary>
        public EffectPlayType PlayType;

        /// <summary>
        /// 特效停止模式
        /// </summary>
        public EffectStopMode DurationEndMode;

        /// <summary>
        /// 持续时间
        /// </summary>
        public float Duration;

        /// <summary>
        /// 累计有效播放时间
        /// </summary>
        public float Elapsed;

        /// <summary>
        /// 是否已经请求收尾停止
        /// </summary>
        public bool RequestedGracefulStop;

        /// <summary>
        /// 是否已经被外部强制结束
        /// </summary>
        public bool ForceFinished;

        /// <summary>
        /// 是否处于暂停状态
        /// </summary>
        public bool IsPaused;
    }

    /// <summary>
    /// 特效管理器
    /// </summary>
    public class EffectManager : IManager
    {
        /// <summary>
        /// 对象池管理器引用
        /// </summary>
        private GameObjectPoolManager _poolManager => GameFrameworkManager.Instance.GetManager<GameObjectPoolManager>();

        /// <summary>
        /// 正在播放的特效实例
        /// </summary>
        private readonly Dictionary<GameObject, EffectInstance> _instances = new Dictionary<GameObject, EffectInstance>();

        public void Init()
        {
            Debug.Log("[EffectManager] 初始化完成");
        }

        public void Update() { }

        public void Release()
        {
            foreach (var kvp in _instances)
            {
                GameObject obj = kvp.Key;
                if (obj != null)
                    _poolManager.Despawn(obj);
            }

            _instances.Clear();

            Debug.Log("[EffectManager] 已释放");
        }

        #region 公共方法
        /// <summary>
        /// 播放一次性特效，播放完自动回收（预制体方式）
        /// </summary>
        public GameObject PlayOneShot(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject obj = _poolManager.Spawn(prefab);
            obj.transform.SetPositionAndRotation(position, rotation);

            IEffectItem effect = obj.GetComponent<IEffectItem>();
            var instance = RegisterInstance(obj, effect, EffectPlayType.OneShot, 0f, EffectStopMode.Graceful);

            effect.Play();
            RunLifetime(instance).Forget();

            return obj;
        }

        /// <summary>
        /// 播放一次性特效，播放完自动回收（路径方式）
        /// </summary>
        public async UniTask<GameObject> PlayOneShotAsync(string prefabPath, Vector3 position, Quaternion rotation)
        {
            GameObject obj = await _poolManager.SpawnAsync(prefabPath);
            obj.transform.SetPositionAndRotation(position, rotation);

            IEffectItem effect = obj.GetComponent<IEffectItem>();
            var instance = RegisterInstance(obj, effect, EffectPlayType.OneShot, 0f, EffectStopMode.Graceful);

            effect.Play();
            RunLifetime(instance).Forget();

            return obj;
        }

        /// <summary>
        /// 播放循环特效，需要手动停止（预制体方式）
        /// </summary>
        public GameObject PlayLoop(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject obj = _poolManager.Spawn(prefab);
            obj.transform.SetPositionAndRotation(position, rotation);

            IEffectItem effect = obj.GetComponent<IEffectItem>();
            var instance = RegisterInstance(obj, effect, EffectPlayType.Loop, 0f, EffectStopMode.Graceful);

            effect.Play();
            RunLifetime(instance).Forget();

            return obj;
        }

        /// <summary>
        /// 播放循环特效，需要手动停止（路径方式）
        /// </summary>
        public async UniTask<GameObject> PlayLoopAsync(string prefabPath, Vector3 position, Quaternion rotation)
        {
            GameObject obj = await _poolManager.SpawnAsync(prefabPath);
            obj.transform.SetPositionAndRotation(position, rotation);

            IEffectItem effect = obj.GetComponent<IEffectItem>();
            var instance = RegisterInstance(obj, effect, EffectPlayType.Loop, 0f, EffectStopMode.Graceful);

            effect.Play();
            RunLifetime(instance).Forget();

            return obj;
        }

        /// <summary>
        /// 播放持续时间特效，到时间后停止并自动回收（预制体方式）
        /// </summary>
        public GameObject PlayForDuration(GameObject prefab, Vector3 position, Quaternion rotation, float duration, EffectStopMode endMode = EffectStopMode.Graceful)
        {
            GameObject obj = _poolManager.Spawn(prefab);
            obj.transform.SetPositionAndRotation(position, rotation);

            IEffectItem effect = obj.GetComponent<IEffectItem>();
            var instance = RegisterInstance(obj, effect, EffectPlayType.Duration, Mathf.Max(0f, duration), endMode);

            effect.Play();
            RunLifetime(instance).Forget();

            return obj;
        }

        /// <summary>
        /// 播放持续时间特效，到时间后停止并自动回收（路径方式）
        /// </summary>
        public async UniTask<GameObject> PlayForDurationAsync(string prefabPath, Vector3 position, Quaternion rotation, float duration, EffectStopMode endMode = EffectStopMode.Graceful)
        {
            GameObject obj = await _poolManager.SpawnAsync(prefabPath);
            obj.transform.SetPositionAndRotation(position, rotation);

            IEffectItem effect = obj.GetComponent<IEffectItem>();
            var instance = RegisterInstance(obj, effect, EffectPlayType.Duration, Mathf.Max(0f, duration), endMode);

            effect.Play();
            RunLifetime(instance).Forget();

            return obj;
        }

        /// <summary>
        /// 停止特效
        /// </summary>
        public void Stop(GameObject obj, EffectStopMode stopMode = EffectStopMode.Graceful)
        {
            if (!_instances.TryGetValue(obj, out var instance))
                return;

            var effect = instance.EffectItem;

            if (stopMode == EffectStopMode.Immediate)
            {
                instance.ForceFinished = true;
                effect.StopImmediate();
                InternalDespawn(obj);
                return;
            }

            // 收尾停止
            instance.RequestedGracefulStop = true;
            effect.StopGraceful();
        }

        /// <summary>
        /// 暂停特效
        /// </summary>
        public void Pause(GameObject obj)
        {
            if (!_instances.TryGetValue(obj, out var instance))
                return;

            if (instance.IsPaused)
                return;

            instance.IsPaused = true;
            instance.EffectItem.Pause();
        }

        /// <summary>
        /// 恢复特效
        /// </summary>
        public void Resume(GameObject obj)
        {
            if (!_instances.TryGetValue(obj, out var instance))
                return;

            if (!instance.IsPaused)
                return;

            instance.IsPaused = false;
            instance.EffectItem.Resume();
        }

        /// <summary>
        /// 直接回收特效（不考虑收尾逻辑）
        /// </summary>
        public void Recycle(GameObject obj)
        {
            if (!_instances.TryGetValue(obj, out var instance))
            {
                _poolManager.Despawn(obj);
                return;
            }

            instance.ForceFinished = true;
            instance.EffectItem.StopImmediate();
            InternalDespawn(obj);
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 注册特效实例
        /// </summary>
        private EffectInstance RegisterInstance(GameObject obj, IEffectItem effect, EffectPlayType playType, float duration, EffectStopMode endMode)
        {
            var instance = new EffectInstance
            {
                GameObject = obj,
                EffectItem = effect,
                PlayType = playType,
                Duration = duration,
                DurationEndMode = endMode,
                Elapsed = 0f,
                RequestedGracefulStop = false,
                ForceFinished = false,
                IsPaused = false
            };

            _instances[obj] = instance;
            return instance;
        }

        /// <summary>
        /// 生命周期协程
        /// </summary>
        private async UniTaskVoid RunLifetime(EffectInstance instance)
        {
            var obj = instance.GameObject;
            var effect = instance.EffectItem;

            switch (instance.PlayType)
            {
                case EffectPlayType.OneShot:
                    while (!instance.ForceFinished && effect.IsAlive)
                        await UniTask.Yield(PlayerLoopTiming.Update);

                    if (!instance.ForceFinished)
                        InternalDespawn(obj);
                    break;

                case EffectPlayType.Duration:
                    // 根据有效播放时间判断是否到达持续时间
                    while (!instance.ForceFinished &&
                           !instance.RequestedGracefulStop &&
                           instance.Elapsed < instance.Duration)
                    {
                        if (!instance.IsPaused)
                            instance.Elapsed += Time.deltaTime;

                        await UniTask.Yield(PlayerLoopTiming.Update);
                    }

                    if (instance.ForceFinished)
                        break;

                    // 根据endMode决定如何结束
                    if (!instance.RequestedGracefulStop)
                    {
                        // 立即停止
                        if (instance.DurationEndMode == EffectStopMode.Immediate)
                        {
                            instance.ForceFinished = true;
                            effect.StopImmediate();
                            InternalDespawn(obj);
                            break;
                        }

                        instance.RequestedGracefulStop = true;
                        effect.StopGraceful();
                    }

                    // 收尾停止
                    while (!instance.ForceFinished && effect.IsAlive)
                        await UniTask.Yield(PlayerLoopTiming.Update);

                    if (!instance.ForceFinished)
                        InternalDespawn(obj);
                    break;

                case EffectPlayType.Loop:
                    // 等待外部请求收尾停止或强制结束
                    while (!instance.ForceFinished && !instance.RequestedGracefulStop)
                        await UniTask.Yield(PlayerLoopTiming.Update);

                    if (instance.ForceFinished)
                        break;

                    while (!instance.ForceFinished && effect.IsAlive)
                        await UniTask.Yield(PlayerLoopTiming.Update);

                    if (!instance.ForceFinished)
                        InternalDespawn(obj);
                    break;
            }
        }

        /// <summary>
        /// 内部回收对象
        /// </summary>
        private void InternalDespawn(GameObject obj)
        {
            _instances.Remove(obj);
            _poolManager.Despawn(obj);
        }
        #endregion
    }
}
