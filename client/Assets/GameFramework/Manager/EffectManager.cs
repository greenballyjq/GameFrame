using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Core.Effect;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GameFramework.Manager
{
    /// <summary>
    /// 特效停止模式
    /// </summary>
    public enum EffectStopMode
    {
        /// <summary>
        /// 软停止：停止发射/停止播放，等待尾巴结束后回收
        /// </summary>
        Soft,

        /// <summary>
        /// 立即停止：立刻清除并回收
        /// </summary>
        Immediate
    }

    /// <summary>
    /// 特效管理器（统一要求特效预制体挂ParticleEffectItem）
    /// </summary>
    public class EffectManager : IManager
    {
        /// <summary>
        /// 对象池管理器引用
        /// </summary>
        private GameObjectPoolManager _poolManager => GameFrameworkManager.Instance.GetManager<GameObjectPoolManager>();

        /// <summary>
        /// 特效对象回收任务取消令牌
        /// </summary>
        private readonly Dictionary<GameObject, CancellationTokenSource> _recycleTokens = new Dictionary<GameObject, CancellationTokenSource>();

        public void Init()
        {
            Debug.Log("[EffectManager] 初始化完成");
        }

        public void Update() { }

        public void Release()
        {
            foreach (var cts in _recycleTokens.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }

            _recycleTokens.Clear();

            Debug.Log("[EffectManager] 已释放");
        }

        #region 公共方法
        /// <summary>
        /// 播放一次性特效（预制体方式），播放完自动回收
        /// </summary>
        public GameObject PlayOneShot(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject obj = _poolManager.Spawn(prefab);
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            IEffectItem effect = GetEffectItem(obj);

            StartAutoRecycle_OneShot(obj, effect);

            return obj;
        }

        /// <summary>
        /// 播放一次性特效（路径方式），播放完自动回收
        /// </summary>
        public async UniTask<GameObject> PlayOneShotAsync(string prefabPath, Vector3 position, Quaternion rotation)
        {
            GameObject obj = await _poolManager.SpawnAsync(prefabPath);
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            IEffectItem effect = GetEffectItem(obj);

            StartAutoRecycle_OneShot(obj, effect);

            return obj;
        }

        /// <summary>
        /// 播放循环特效（预制体方式），不会自动回收，必须手动Stop
        /// </summary>
        public GameObject PlayLoop(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject obj = _poolManager.Spawn(prefab);
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            // 循环特效不自动回收
            GetEffectItem(obj); // 强制校验资源规范

            return obj;
        }

        /// <summary>
        /// 播放持续特效（预制体方式），到时间后软停止并自动回收
        /// </summary>
        public GameObject PlayForDuration(GameObject prefab, Vector3 position, Quaternion rotation, float duration)
        {
            GameObject obj = _poolManager.Spawn(prefab);
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            IEffectItem effect = GetEffectItem(obj);

            StartAutoRecycle_Duration(obj, effect, duration);

            return obj;
        }

        /// <summary>
        /// 停止特效并回收
        /// </summary>
        public void Stop(GameObject obj, EffectStopMode stopMode = EffectStopMode.Soft)
        {
            if (obj == null)
            {
                Debug.LogError("[EffectManager] 停止失败：obj为null");
                return;
            }

            // 取消已有的自动回收任务，防止重复回收
            CancelAutoRecycle(obj);

            IEffectItem effect = GetEffectItem(obj);

            if (stopMode == EffectStopMode.Immediate)
            {
                effect.StopImmediate();
                FinalDespawn(obj);
                return;
            }

            // 软停止：停发射，等待尾巴结束
            effect.StopSoft();
            StartWaitAliveAndDespawn(obj, effect);
        }

        /// <summary>
        /// 直接回收特效（不等待尾巴）
        /// </summary>
        public void Recycle(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[EffectManager] 回收失败：obj为null");
                return;
            }

            CancelAutoRecycle(obj);
            FinalDespawn(obj);
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 获取特效接口（强制要求存在）
        /// </summary>
        private IEffectItem GetEffectItem(GameObject obj)
        {
            IEffectItem effect = obj.GetComponent<IEffectItem>();
            if (effect == null)
            {
                Debug.LogError($"[EffectManager] 特效对象缺少IEffectItem组件：{obj.name}");
            }

            return effect;
        }

        /// <summary>
        /// 一次性特效自动回收
        /// </summary>
        private void StartAutoRecycle_OneShot(GameObject obj, IEffectItem effect)
        {
            // 资源规范：必须存在
            if (effect == null)
                return;

            var cts = new CancellationTokenSource();
            _recycleTokens[obj] = cts;

            AutoRecycle_OneShotTask(obj, effect, cts.Token).Forget();
        }

        private async UniTaskVoid AutoRecycle_OneShotTask(GameObject obj, IEffectItem effect, CancellationToken token)
        {
            // 估算等待（带buffer）
            float t = effect.GetEstimatedDuration();
            if (t > 0f)
            {
                int ms = Mathf.CeilToInt((t + 0.1f) * 1000f);
                await UniTask.Delay(ms, cancellationToken: token);
            }

            if (token.IsCancellationRequested)
                return;

            // 兜底：等待真正结束
            await WaitAliveEnd(effect, token);

            if (token.IsCancellationRequested)
                return;

            FinalDespawn(obj);
        }

        /// <summary>
        /// 持续时间特效：到点软停止 + 等尾巴 + 回收
        /// </summary>
        private void StartAutoRecycle_Duration(GameObject obj, IEffectItem effect, float duration)
        {
            // 资源规范：必须存在
            if (effect == null)
                return;

            var cts = new CancellationTokenSource();
            _recycleTokens[obj] = cts;

            AutoRecycle_DurationTask(obj, effect, duration, cts.Token).Forget();
        }

        private async UniTaskVoid AutoRecycle_DurationTask(GameObject obj, IEffectItem effect, float duration, CancellationToken token)
        {
            int ms = Mathf.CeilToInt(Mathf.Max(0f, duration) * 1000f);
            await UniTask.Delay(ms, cancellationToken: token);

            if (token.IsCancellationRequested)
                return;

            effect.StopSoft();

            await WaitAliveEnd(effect, token);

            if (token.IsCancellationRequested)
                return;

            FinalDespawn(obj);
        }

        /// <summary>
        /// 软停止后的等待回收
        /// </summary>
        private void StartWaitAliveAndDespawn(GameObject obj, IEffectItem effect)
        {
            var cts = new CancellationTokenSource();
            _recycleTokens[obj] = cts;

            WaitAliveAndDespawnTask(obj, effect, cts.Token).Forget();
        }

        private async UniTaskVoid WaitAliveAndDespawnTask(GameObject obj, IEffectItem effect, CancellationToken token)
        {
            await WaitAliveEnd(effect, token);

            if (token.IsCancellationRequested)
                return;

            FinalDespawn(obj);
        }

        /// <summary>
        /// 等待特效完全结束
        /// </summary>
        private static async UniTask WaitAliveEnd(IEffectItem effect, CancellationToken token)
        {
            while (!token.IsCancellationRequested && effect.IsAlive())
                await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        /// <summary>
        /// 取消自动回收任务
        /// </summary>
        private void CancelAutoRecycle(GameObject obj)
        {
            if (_recycleTokens.TryGetValue(obj, out var cts))
            {
                _recycleTokens.Remove(obj);
                cts.Cancel();
                cts.Dispose();
            }
        }

        /// <summary>
        /// 最终回收（先清理令牌，再回收到对象池）
        /// </summary>
        private void FinalDespawn(GameObject obj)
        {
            CancelAutoRecycle(obj);
            _poolManager.Despawn(obj);
        }
        #endregion
    }
}
