using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Manager
{
    /// <summary>
    /// 计时器信息
    /// </summary>
    public class TimerInfo
    {
        /// <summary>
        /// id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 计时器时长
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// 已过时长
        /// </summary>
        public float ElapsedTime { get; set; }

        /// <summary>
        /// 剩余时长
        /// </summary>
        public float RemainTime => Duration - ElapsedTime;

        /// <summary>
        /// PlayerLoop执行时机
        /// </summary>
        public PlayerLoopTiming PlayerLoopTiming { get; set; }

        /// <summary>
        /// 是否忽略时间缩放
        /// </summary>
        public bool IgnoreTimeScale { get; set; }

        /// <summary>
        /// 计时器回调
        /// </summary>
        public Action Callback { get; set; }

        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool IsPaused { get; set; }

        /// <summary>
        /// 重复参数
        /// </summary>
        public int Repeat { get; set; }

        /// <summary>
        /// 取消令牌
        /// </summary>
        public CancellationTokenSource Cts { get; set; }
    }

    /// <summary>
    /// 基于UniTask的定时器
    /// </summary>
    public class TimerManager : IManager
    {
        /// <summary>
        /// 激活的计时器集合
        /// </summary>
        private readonly Dictionary<int, TimerInfo> _activeTimers = new Dictionary<int, TimerInfo>();

        /// <summary>
        /// 循环执行参数
        /// </summary>
        public const int LOOP = -1;

        /// <summary>
        /// 开启计时器
        /// </summary>
        /// <param name="duration">时长</param>
        /// <param name="callback">回调</param>
        /// <param name="playerLoopTiming"></param>
        /// <param name="repeat">重复参数</param>
        /// <param name="ignoreTimeScale">是否忽略时间缩放</param>
        /// <returns></returns>
        public int StartTimer(float duration, Action callback,
            PlayerLoopTiming playerLoopTiming = PlayerLoopTiming.Update, int repeat = 1, bool ignoreTimeScale = false)
        {
            int timerId = Guid.NewGuid().GetHashCode();
            TimerInfo timerInfo = new TimerInfo
            {
                Id = timerId,
                Duration = duration,
                IsPaused = false,
                PlayerLoopTiming = playerLoopTiming,
                IgnoreTimeScale = ignoreTimeScale,
                Callback = callback,
                Repeat = repeat,
                Cts = new CancellationTokenSource(),
            };

            // UnityEngine.Debug.Log(
            //     $"[TimerManager] 计时器 {timerId} 已启动, 时长：{duration:F2}秒, 重复次数：{repeat}, 是否忽略时间缩放：{ignoreTimeScale}");

            _activeTimers[timerId] = timerInfo;
            ExecuteTimerAsync(timerId, timerInfo).Forget();

            return timerId;
        }
        
        /// <summary>
        /// 开启计时器(一次性)
        /// </summary>
        /// <param name="duration">时长</param>
        /// <param name="callback">回调</param>
        /// <param name="playerLoopTiming"></param>
        /// <param name="ignoreTimeScale">是否忽略时间缩放</param>
        /// <returns></returns>
        public int StartTimerOneShot(float duration, Action callback,
            PlayerLoopTiming playerLoopTiming = PlayerLoopTiming.Update, bool ignoreTimeScale = false)
        {
            int timerId = Guid.NewGuid().GetHashCode();
            TimerInfo timerInfo = new TimerInfo
            {
                Id = timerId,
                Duration = duration,
                IsPaused = false,
                PlayerLoopTiming = playerLoopTiming,
                IgnoreTimeScale = ignoreTimeScale,
                Callback = callback,
                Repeat = 1,
                Cts = new CancellationTokenSource(),
            };
            
            // UnityEngine.Debug.Log(
            //     $"[TimerManager] 计时器 {timerId} 已启动, 时长：{duration:F2}秒, 重复次数：{1}, 是否忽略时间缩放：{ignoreTimeScale}");

            _activeTimers[timerId] = timerInfo;
            ExecuteTimerAsync(timerId, timerInfo).Forget();

            return timerId;
        }

        /// <summary>
        /// 重置计时器
        /// </summary>
        /// <param name="timerId">计时器id</param>
        /// <returns></returns>
        public bool ResetTimer(int timerId)
        {
            if (_activeTimers.TryGetValue(timerId, out var timerInfo))
            {
                timerInfo.ElapsedTime = 0f;
            }

            return false;
        }

        /// <summary>
        /// 暂停计时器
        /// </summary>
        /// <param name="timerId">计时器id</param>
        /// <returns></returns>
        public bool PauseTimer(int timerId)
        {
            if (_activeTimers.TryGetValue(timerId, out TimerInfo timerInfo))
            {
                if (!timerInfo.IsPaused)
                {
                    timerInfo.IsPaused = true;
                    // UnityEngine.Debug.Log($"[TimerManager] 计时器 {timerId} 已暂停, 剩余时间：{timerInfo.RemainTime:F2}秒");
                    return true;
                }

                // UnityEngine.Debug.Log($"[TimerManager] 计时器 {timerId} 已经暂停");
            }

            // UnityEngine.Debug.Log($"[TimerManager] 计时器 {timerId} 不存在");

            return false;
        }
        
        /// <summary>
        /// 恢复计时器
        /// </summary>
        /// <param name="timerId">计时器id</param>
        /// <returns></returns>
        public bool ResumeTimer(int timerId)
        {
            if (_activeTimers.TryGetValue(timerId, out TimerInfo timerInfo))
            {
                if (timerInfo.IsPaused)
                {
                    timerInfo.IsPaused = false;
                    // UnityEngine.Debug.Log($"[TimerManager] 计时器 {timerId} 已恢复, 剩余时间：{timerInfo.RemainTime:F2}秒");
                    return true;
                }

                // UnityEngine.Debug.Log($"[TimerManager] 计时器 {timerId} 未暂停");
            }

            // UnityEngine.Debug.Log($"[TimerManager] 计时器 {timerId} 不存在");

            return false;
        }

        /// <summary>
        /// 执行计时Tick
        /// </summary>
        /// <param name="timerId">计时器id</param>
        /// <param name="timerInfo">计时器信息</param>
        private async UniTask ExecuteTimerAsync(int timerId, TimerInfo timerInfo)
        {
            try
            {
                while (true)
                {
                    if (timerInfo.IsPaused)
                    {
                        while (timerInfo.IsPaused)
                        {
                            await UniTask.Yield(timerInfo.PlayerLoopTiming, timerInfo.Cts.Token);
                        }
                    }

                    timerInfo.ElapsedTime += timerInfo.IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                    
                    if (timerInfo.RemainTime <= 0f)
                    {
                        timerInfo.Callback?.Invoke();
                        timerInfo.ElapsedTime = 0f;
                        if (timerInfo.Repeat != LOOP)
                        {
                            timerInfo.Repeat--;
                            if (timerInfo.Repeat <= 0)
                            {
                                break;
                            }
                        }
                    }
                    await UniTask.Yield(timerInfo.PlayerLoopTiming, timerInfo.Cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // 计时器被取消
                CleanUpTimer(timerId);
            }
            finally
            {
                CleanUpTimer(timerId);
            }
        }

        /// <summary>
        /// 停止（取消）计时器
        /// </summary>
        /// <param name="timerId"></param>
        /// <returns></returns>
        public bool StopTimer(int timerId)
        {
            if (_activeTimers.TryGetValue(timerId, out var timerInfo))
            {
                timerInfo.Cts.Cancel();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 停止（取消）所有计时器
        /// </summary>
        public void StopAllTimers()
        {
            foreach (var timerInfo in _activeTimers.Values)
            {
                timerInfo.Cts.Cancel();
            }
        }

        /// <summary>
        /// 获取激活计时器集合数量
        /// </summary>
        /// <returns></returns>
        public int GetActiveTimerCount() => _activeTimers.Count;

        /// <summary>
        /// 获取一个计时器是否激活（存在）
        /// </summary>
        /// <param name="timerId">计时器id</param>
        /// <returns></returns>
        public bool IsTimerActive(int timerId) => _activeTimers.ContainsKey(timerId);

        public float GetRemainTime(int timerId)
        {
            if (_activeTimers.TryGetValue(timerId, out var timerInfo))
            {
                return timerInfo.RemainTime;
            }

            return 0f;
        }

        public TimerInfo GetTimerInfo(int timerId)
        {
            return _activeTimers.GetValueOrDefault(timerId);
        }

        /// <summary>
        /// 清理计时器
        /// </summary>
        /// <param name="timerId">计时器id</param>
        private void CleanUpTimer(int timerId)
        {
            if (_activeTimers.TryGetValue(timerId, out var timerInfo))
            {
                timerInfo.Cts.Dispose();
                _activeTimers.Remove(timerId);
            }
        }

        public void Init()
        {
            // 计时管理器无初始化逻辑
        }

        public void Update()
        {
            // 计时管理器无更新逻辑
        }

        public void Release()
        {
            // 计时管理器无释放逻辑
        }
    }
}