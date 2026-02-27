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
    /// 特效运行时
    /// </summary>
    internal class EffectRuntime
    {
        /// <summary>
        /// 特效对象
        /// </summary>
        public GameObject GameObject;

        /// <summary>
        /// 特效项组件
        /// </summary>
        public IEffectItem EffectItem;

        /// <summary>
        /// 播放类型
        /// </summary>
        public EffectPlayType PlayType;

        /// <summary>
        /// 停止模式
        /// </summary>
        public EffectStopMode StopMode;

        /// <summary>
        /// 持续时间（仅对持续时间特效有效）
        /// </summary>
        public float Duration;

        /// <summary>
        /// 累计有效播放时间（仅对持续时间特效有效）
        /// </summary>
        public float Elapsed;

        /// <summary>
        /// 是否请求停止
        /// </summary>
        public bool IsRequestedStop;

        /// <summary>
        /// 是否处于停止状态
        /// </summary>
        public bool IsStopping;

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
        /// 对象池管理器
        /// </summary>
        private GameObjectPoolManager _poolManager => GameFrameworkManager.Instance.GetManager<GameObjectPoolManager>();

        /// <summary>
        /// 活跃特效字典
        /// </summary>
        private readonly Dictionary<GameObject, EffectRuntime> _activeEffects = new Dictionary<GameObject, EffectRuntime>();

        /// <summary>
        /// 待回收特效集合
        /// </summary>
        private readonly HashSet<GameObject> _pendingDespawnEffects = new HashSet<GameObject>();

        public void Init()
        {
            Debug.Log("[EffectManager] 初始化完成");
        }

        public void DoUpdate(float dt)
        {
            _pendingDespawnEffects.Clear();

            foreach (var kvp in _activeEffects)
            {
                var obj = kvp.Key;
                var effectRuntime = kvp.Value;
                var effectItem = effectRuntime.EffectItem;

                bool shouldDespawn = false;

                // 等待特效播放结束
                if (!effectRuntime.IsStopping)
                {
                    switch (effectRuntime.PlayType)
                    {
                        case EffectPlayType.OneShot:
                            if (effectRuntime.IsRequestedStop || !effectItem.IsAlive)
                            {
                                effectRuntime.IsStopping = true;
                            }
                            break;

                        case EffectPlayType.Loop:
                            if (effectRuntime.IsRequestedStop)
                            {
                                effectRuntime.IsStopping = true;
                            }
                            break;

                        case EffectPlayType.Duration:
                            if (effectRuntime.IsRequestedStop)
                            {
                                effectRuntime.IsStopping = true;
                            }
                            else
                            {
                                if (!effectRuntime.IsPaused)
                                {
                                    effectRuntime.Elapsed += dt;
                                }

                                if (effectRuntime.Elapsed >= effectRuntime.Duration)
                                {
                                    effectItem.Stop(effectRuntime.StopMode);
                                    effectRuntime.IsStopping = true;
                                }
                            }
                            break;
                    }
                }

                // 等待特效收尾结束
                if (effectRuntime.IsStopping && !effectItem.IsAlive)
                    shouldDespawn = true;

                if (shouldDespawn)
                    _pendingDespawnEffects.Add(obj);
            }

            foreach (var obj in _pendingDespawnEffects)
                InternalDespawn(obj);

            _pendingDespawnEffects.Clear();
        }

        public void Release()
        {
            Debug.Log("[EffectManager] 已释放");
        }

        #region 公共方法
        /// <summary>
        /// 播放一次性特效（预制体方式）
        /// 默认播放完自动收尾停止
        /// </summary>
        public GameObject PlayOneShot(GameObject prefab, Vector3 position = default, Quaternion rotation = default, bool dontDestroyOnLoad = true)
        {
            GameObject obj = _poolManager.Spawn(prefab);
            obj.transform.SetPositionAndRotation(position, rotation);

            if (dontDestroyOnLoad)
                Object.DontDestroyOnLoad(obj);

            IEffectItem effect = obj.GetComponent<IEffectItem>();
            var instance = RegisterEffect(obj, effect, EffectPlayType.OneShot, 0f, EffectStopMode.Graceful);

            effect.Play();

            return obj;
        }

        /// <summary>
        /// 播放一次性特效（路径方式）
        /// 默认播放完自动收尾停止
        /// </summary>
        public async UniTask<GameObject> PlayOneShotAsync(string prefabPath, Vector3 position, Quaternion rotation, bool dontDestroyOnLoad = true)
        {
            GameObject obj = await _poolManager.SpawnAsync(prefabPath);
            obj.transform.SetPositionAndRotation(position, rotation);
            
            if (dontDestroyOnLoad)
                Object.DontDestroyOnLoad(obj);

            IEffectItem effect = obj.GetComponent<IEffectItem>();
            var instance = RegisterEffect(obj, effect, EffectPlayType.OneShot, 0f, EffectStopMode.Graceful);

            effect.Play();

            return obj;
        }

        /// <summary>
        /// 播放循环特效（预制体方式）
        /// 需要手动停止
        /// </summary>
        public GameObject PlayLoop(GameObject prefab, Vector3 position = default, Quaternion rotation = default, bool dontDestroyOnLoad = true)
        {
            GameObject obj = _poolManager.Spawn(prefab);
            obj.transform.SetPositionAndRotation(position, rotation);

            if (dontDestroyOnLoad)
                Object.DontDestroyOnLoad(obj);

            IEffectItem effect = obj.GetComponent<IEffectItem>();
            var instance = RegisterEffect(obj, effect, EffectPlayType.Loop, 0f, EffectStopMode.Graceful);

            effect.Play();

            return obj;
        }

        /// <summary>
        /// 播放循环特效（路径方式）
        /// 需要手动停止
        /// </summary>
        public async UniTask<GameObject> PlayLoopAsync(string prefabPath, Vector3 position, Quaternion rotation, bool dontDestroyOnLoad = true)
        {
            GameObject obj = await _poolManager.SpawnAsync(prefabPath);
            obj.transform.SetPositionAndRotation(position, rotation);
            
            if (dontDestroyOnLoad)
                Object.DontDestroyOnLoad(obj);

            IEffectItem effect = obj.GetComponent<IEffectItem>();
            var instance = RegisterEffect(obj, effect, EffectPlayType.Loop, 0f, EffectStopMode.Graceful);

            effect.Play();

            return obj;
        }

        /// <summary>
        /// 播放持续时间特效（预制体方式）
        /// 到时间后默认自动收尾停止
        /// </summary>
        public GameObject PlayForDuration(GameObject prefab, Vector3 position, Quaternion rotation, float duration, EffectStopMode endMode = EffectStopMode.Graceful, bool dontDestroyOnLoad = true)
        {
            GameObject obj = _poolManager.Spawn(prefab);
            obj.transform.SetPositionAndRotation(position, rotation);

            if (dontDestroyOnLoad)
                Object.DontDestroyOnLoad(obj);

            IEffectItem effect = obj.GetComponent<IEffectItem>();
            var instance = RegisterEffect(obj, effect, EffectPlayType.Duration, Mathf.Max(0f, duration), endMode);

            effect.Play();

            return obj;
        }

        /// <summary>
        /// 播放持续时间特效
        /// 到时间后默认自动收尾停止
        /// </summary>
        public async UniTask<GameObject> PlayForDurationAsync(string prefabPath, Vector3 position, Quaternion rotation, float duration, EffectStopMode endMode = EffectStopMode.Graceful, bool dontDestroyOnLoad = true)
        {
            GameObject obj = await _poolManager.SpawnAsync(prefabPath);
            obj.transform.SetPositionAndRotation(position, rotation);
            
            if (dontDestroyOnLoad)
                Object.DontDestroyOnLoad(obj);

            IEffectItem effect = obj.GetComponent<IEffectItem>();
            var instance = RegisterEffect(obj, effect, EffectPlayType.Duration, Mathf.Max(0f, duration), endMode);

            effect.Play();

            return obj;
        }

        /// <summary>
        /// 停止特效
        /// </summary>
        /// <param name="obj">特效对象</param>
        /// <param name="stopMode">停止模式</param>
        public void Stop(GameObject obj, EffectStopMode stopMode = EffectStopMode.Graceful)
        {
            if (!_activeEffects.TryGetValue(obj, out var instance))
                return;

            instance.IsRequestedStop = true;

            instance.EffectItem.Stop(stopMode);
        }

        /// <summary>
        /// 暂停特效
        /// </summary>
        public void Pause(GameObject obj)
        {
            if (!_activeEffects.TryGetValue(obj, out var instance))
                return;

            instance.IsPaused = true;

            instance.EffectItem.Pause();
        }

        /// <summary>
        /// 恢复特效
        /// </summary>
        public void Resume(GameObject obj)
        {
            if (!_activeEffects.TryGetValue(obj, out var instance))
                return;

            instance.IsPaused = false;

            instance.EffectItem.Resume();
        }

        /// <summary>
        /// 清理场上所有特效
        /// </summary>
        public void ClearAllEffects()
        {
            _pendingDespawnEffects.Clear();

            foreach (var kvp in _activeEffects)
            {
                kvp.Value.EffectItem.Stop(EffectStopMode.Immediate);
                _pendingDespawnEffects.Add(kvp.Key);
            }

            foreach (var obj in _pendingDespawnEffects)
                InternalDespawn(obj);

            _pendingDespawnEffects.Clear();
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 注册特效
        /// </summary>
        private EffectRuntime RegisterEffect(GameObject obj, IEffectItem effect, EffectPlayType playType, float duration, EffectStopMode endMode)
        {
            var instance = new EffectRuntime
            {
                GameObject = obj,
                EffectItem = effect,
                PlayType = playType,
                Duration = duration,
                StopMode = endMode,
                Elapsed = 0f,
                IsRequestedStop = false,
                IsStopping = false,
                IsPaused = false
            };

            _activeEffects[obj] = instance;

            return instance;
        }

        /// <summary>
        /// 回收特效
        /// </summary>
        private void InternalDespawn(GameObject obj)
        {
            if (obj == null)
                return;

            if (!_activeEffects.Remove(obj))
                return;

            _poolManager.Despawn(obj);
        }
        #endregion
    }
}
    