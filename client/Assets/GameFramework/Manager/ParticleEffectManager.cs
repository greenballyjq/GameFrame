using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Manager
{
    /// <summary>
    /// 粒子特效管理器 - 负责粒子特效的加载、生成和生命周期管理
    /// </summary>
    public class ParticleEffectManager : IManager
    {
        #region 私有字段
        
        /// <summary>
        /// 活跃的粒子特效列表
        /// </summary>
        private readonly List<ActiveParticleEffect> _activeEffects = new();
        
        /// <summary>
        /// 对象池引用
        /// </summary>
        private GameObjectPoolManager _gameObjectPoolManager;

        /// <summary>
        /// 粒子特效加载目录路径
        /// </summary>
        private const string ParticleLoadPath = "Arts/Prefabs/Particles";
        
        #endregion

        #region 公共方法
        
        /// <summary>
        /// 生成粒子特效
        /// </summary>
        /// <param name="effectName">粒子特效名称</param>
        /// <param name="position">生成位置，为null时使用粒子默认位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="autoDestroy">是否自动销毁</param>
        /// <param name="destroyDelay">延迟销毁时间（秒）</param>
        /// <param name="parent">父物体</param>
        /// <returns>生成的粒子特效游戏对象</returns>
        public async UniTask<GameObject> SpawnParticleEffectAsync(string effectName, Vector3? position = null, 
            Quaternion rotation = default, bool autoDestroy = true, float destroyDelay = 0f, Transform parent = null)
        {
            try
            {
                // 从对象池获取或创建新对象
                var particle = await GetFromPoolAsync(effectName);
                
                if (particle == null)
                {
                    Debug.LogError($"[ParticleEffectManager] 无法创建粒子特效: {effectName}");
                    return null;
                }

                // 设置位置和旋转
                if (position.HasValue)
                {
                    particle.transform.position = position.Value;
                }
                particle.transform.rotation = rotation == default ? Quaternion.identity : rotation;
                
                if (parent != null)
                {
                    particle.transform.SetParent(parent);
                }

                // 激活粒子系统
                var particleSystems = particle.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particleSystems)
                {
                    ps.Play();
                }

                // 添加到活跃列表
                if (autoDestroy || destroyDelay > 0)
                {
                    var delay = destroyDelay > 0 ? destroyDelay : GetParticleDuration(particle);
                    var activeEffect = new ActiveParticleEffect
                    {
                        EffectName = effectName,
                        GameObject = particle,
                        SpawnTime = Time.time,
                        DestroyTime = Time.time + delay,
                        AutoDestroy = autoDestroy
                    };
                    _activeEffects.Add(activeEffect);
                }

                Debug.Log($"[ParticleEffectManager] 生成粒子特效: {effectName} at {position}");
                
                // 触发生成事件
                // var eventManager = GameServiceLocator.EventManager;
                // if (eventManager != null)
                // {
                //     var args = new ParticleEffectSpawnedEventArgs(
                //         effectName, particle, position, autoDestroy, destroyDelay);
                //     eventManager.Trigger(ParticleEffectEvents.ParticleEffectSpawned, args);
                // }
                
                return particle;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ParticleEffectManager] 生成粒子特效异常: {effectName} - {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 同步生成粒子特效（需要预制体已缓存）
        /// </summary>
        /// <param name="effectName">粒子特效名称</param>
        /// <param name="position">生成位置，为null时使用粒子默认位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="autoDestroy">是否自动销毁</param>
        /// <param name="destroyDelay">延迟销毁时间（秒）</param>
        /// <param name="parent">父物体</param>
        /// <returns>生成的粒子特效游戏对象</returns>
        public GameObject SpawnParticleEffect(string effectName, Vector3? position = null, 
            Quaternion rotation = default, bool autoDestroy = true, float destroyDelay = -1f, Transform parent = null)
        {
            try
            {
                // 从对象池获取或创建新对象
                var particle = GetFromPool(effectName);
                
                if (particle == null)
                {
                    Debug.LogError($"[ParticleEffectManager] 无法创建粒子特效: {effectName}");
                    return null;
                }

                // 设置位置和旋转
                if (position.HasValue)
                {
                    particle.transform.position = position.Value;
                }
                particle.transform.rotation = rotation == default ? Quaternion.identity : rotation;
                
                if (parent != null)
                {
                    particle.transform.SetParent(parent);
                }

                // 激活粒子系统
                var particleSystems = particle.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particleSystems)
                {
                    ps.Play();
                }

                // 添加到活跃列表
                if (autoDestroy || destroyDelay > 0)
                {
                    var delay = destroyDelay > 0 ? destroyDelay : GetParticleDuration(particle);
                    var activeEffect = new ActiveParticleEffect
                    {
                        EffectName = effectName,
                        GameObject = particle,
                        SpawnTime = Time.time,
                        DestroyTime = Time.time + delay,
                        AutoDestroy = autoDestroy
                    };
                    _activeEffects.Add(activeEffect);
                }

                Debug.Log($"[ParticleEffectManager] 生成粒子特效: {effectName} at {(position.HasValue ? position.Value.ToString() : "默认位置")}");
                
                return particle;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ParticleEffectManager] 生成粒子特效异常: {effectName} - {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 立即销毁粒子特效
        /// </summary>
        /// <param name="particle">粒子特效游戏对象</param>
        /// <param name="stopFirst">是否先停止粒子系统</param>
        public void DestroyParticleEffect(GameObject particle, bool stopFirst = true)
        {
            if (particle == null) return;

            try
            {
                // 停止粒子系统
                if (stopFirst)
                {
                    var particleSystems = particle.GetComponentsInChildren<ParticleSystem>();
                    foreach (var ps in particleSystems)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }

                // 从活跃列表移除
                _activeEffects.RemoveAll(effect => effect.GameObject == particle);

                // 回收到对象池
                ReturnToPool(particle);

                Debug.Log($"[ParticleEffectManager] 销毁粒子特效: {particle.name}");
                
                // 触发销毁事件
                // TriggerParticleEffectDestroyed(effectName, particle, DestroyReason.Manual, duration);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ParticleEffectManager] 销毁粒子特效异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 延迟销毁粒子特效
        /// </summary>
        /// <param name="particle">粒子特效游戏对象</param>
        /// <param name="delay">延迟时间（秒）</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async UniTask DestroyParticleEffectAsync(GameObject particle, float delay, CancellationToken cancellationToken = default)
        {
            if (particle == null) return;

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken);
                
                if (!cancellationToken.IsCancellationRequested)
                {
                    DestroyParticleEffect(particle);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[ParticleEffectManager] 延迟销毁被取消: {particle.name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ParticleEffectManager] 延迟销毁粒子特效异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 预加载粒子特效
        /// </summary>
        /// <param name="effectNames">粒子特效名称列表</param>
        public async UniTask PreloadParticleEffectsAsync(params string[] effectNames)
        {
            var effectPaths = effectNames.Select(item => $"{ParticleLoadPath}/{item}").ToArray();
            await _gameObjectPoolManager.PreloadAsync(effectPaths);
        }

        /// <summary>
        /// 清理所有粒子特效
        /// </summary>
        public void ClearAllParticleEffects()
        {
            try
            {
                // 销毁所有活跃的粒子特效
                var effectsToDestroy = new List<GameObject>(_activeEffects.Count);
                foreach (var effect in _activeEffects)
                {
                    if (effect.GameObject != null)
                    {
                        effectsToDestroy.Add(effect.GameObject);
                    }
                }

                foreach (var particle in effectsToDestroy)
                {
                    DestroyParticleEffect(particle, false);
                }

                _activeEffects.Clear();
                var clearedCount = effectsToDestroy.Count;
            Debug.Log("[ParticleEffectManager] 已清理所有粒子特效");
            
            // 触发清理完成事件
            // TriggerParticleEffectCleared(clearedCount, poolCount, ClearReason.Manual);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ParticleEffectManager] 清理粒子特效异常: {ex.Message}");
            }
        }

        #endregion

        #region IManager 实现

        /// <summary>
        /// 初始化粒子特效管理器
        /// </summary>
        public void Init()
        {
            Debug.Log("[ParticleEffectManager] 粒子特效管理器初始化完成");
            _gameObjectPoolManager = GameFrameworkManager.Instance.GetManager<GameObjectPoolManager>();
        }

        /// <summary>
        /// 更新粒子特效管理器
        /// </summary>
        public void Update()
        {
            // 检查并销毁过期的粒子特效
            var currentTime = Time.time;
            var effectsToDestroy = new List<ActiveParticleEffect>();

            foreach (var effect in _activeEffects)
            {
                if (currentTime >= effect.DestroyTime && effect.AutoDestroy)
                {
                    effectsToDestroy.Add(effect);
                }  
            }

            foreach (var effect in effectsToDestroy)
            {
                _activeEffects.Remove(effect);
                DestroyParticleEffect(effect.GameObject, false);
            }     
        }
         
        /// <summary>
        /// 释放粒子特效管理器资源
        /// </summary>
        public void Release()
        {
            ClearAllParticleEffects();
            
            Debug.Log("[ParticleEffectManager] 粒子特效管理器资源释放完成");
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 从对象池获取粒子特效 - 异步
        /// </summary>
        /// <param name="effectName">特效名称</param>
        /// <returns>粒子特效游戏对象</returns>
        private async UniTask<GameObject> GetFromPoolAsync(string effectName)
        {
            var assetPath = $"{ParticleLoadPath}/{effectName}";
            return await _gameObjectPoolManager.SpawnAsync(assetPath);
        }

        /// <summary>
        /// 从对象池获取粒子特效 
        /// </summary>
        /// <param name="effectName"></param>
        /// <returns></returns>
        private GameObject GetFromPool(string effectName)
        {
            var assetPath = $"{ParticleLoadPath}/{effectName}";
            return _gameObjectPoolManager.Spawn(assetPath);
        }

        /// <summary>
        /// 将粒子特效回收到对象池
        /// </summary>
        /// <param name="particle">粒子特效游戏对象</param>
        private void ReturnToPool(GameObject particle)
        {
            if (particle == null) return;
            
            _gameObjectPoolManager.Despawn(particle);
        }

        /// <summary>
        /// 获取粒子特效持续时间
        /// </summary>
        /// <param name="particle">粒子特效游戏对象</param>
        /// <returns>持续时间（秒）</returns>
        private float GetParticleDuration(GameObject particle)
        {
            var particleSystems = particle.GetComponentsInChildren<ParticleSystem>();
            var maxDuration = 3f; // 默认3秒

            foreach (var ps in particleSystems)
            {
                if (ps.main.duration > maxDuration)
                {
                    maxDuration = ps.main.duration;
                }
                
                // 考虑开始延迟
                var totalDuration = ps.main.duration + ps.main.startDelay.constant;
                if (totalDuration > maxDuration)
                {
                    maxDuration = totalDuration;
                }
            }

            return maxDuration + 0.5f; // 额外增加0.5秒确保粒子完全播放完
        }

        #endregion

        #region 内部数据结构

        /// <summary>
        /// 活跃的粒子特效信息
        /// </summary>
        private class ActiveParticleEffect
        {
            public string EffectName { get; set; }
            public GameObject GameObject { get; set; }
            public float SpawnTime { get; set; }
            public float DestroyTime { get; set; }
            public bool AutoDestroy { get; set; }
        }

        #endregion

        #region 事件触发方法

        /// <summary>
        /// 触发粒子特效加载完成事件
        /// </summary>
        private void TriggerParticleEffectLoaded(string effectName, bool success, string errorMessage, long loadTimeMs)
        {
            // var eventManager = GameServiceLocator.EventManager;
            // if (eventManager != null)
            // {
            //     var args = new ParticleEffectLoadedEventArgs(effectName, success, errorMessage, loadTimeMs);
            //     eventManager.Trigger(ParticleEffectEvents.ParticleEffectLoaded, args);
            // }
        }

        /// <summary>
        /// 触发粒子特效销毁事件
        /// </summary>
        // private void TriggerParticleEffectDestroyed(string effectName, GameObject particleObject, 
        //     DestroyReason reason, float duration)
        // {
        //     var eventManager = GameServiceLocator.EventManager;
        //     if (eventManager != null)
        //     {
        //         var args = new ParticleEffectDestroyedEventArgs(effectName, particleObject, reason, duration);
        //         eventManager.Trigger(ParticleEffectEvents.ParticleEffectDestroyed, args);
        //     }
        // }

        /// <summary>
        /// 触发粒子特效预加载完成事件
        /// </summary>
        // private void TriggerParticleEffectPreloaded(int totalCount, int successCount, int failureCount, 
        //     long loadTimeMs, string[] effectNames)
        // {
        //     var eventManager = GameServiceLocator.EventManager;
        //     if (eventManager != null)
        //     {
        //         var args = new ParticleEffectPreloadedEventArgs(totalCount, successCount, failureCount, 
        //             loadTimeMs, effectNames);
        //         eventManager.Trigger(ParticleEffectEvents.ParticleEffectPreloaded, args);
        //     }
        // }

        /// <summary>
        /// 触发粒子特效清理完成事件
        /// </summary>
        // private void TriggerParticleEffectCleared(int clearedCount, int poolCount, ClearReason reason)
        // {
        //     var eventManager = GameServiceLocator.EventManager;
        //     if (eventManager != null)
        //     {
        //         var args = new ParticleEffectClearedEventArgs(clearedCount, poolCount, reason);
        //         eventManager.Trigger(ParticleEffectEvents.ParticleEffectCleared, args);
        //     }
        // }

        #endregion
    }
}