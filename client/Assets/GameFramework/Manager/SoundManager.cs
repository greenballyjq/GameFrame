using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Core.Audio;
using UnityEngine;

namespace GameFramework.Manager
{
    /// <summary>
    /// 音效管理器 - 负责音效的加载、播放和管理
    /// </summary>
    public class SoundManager : IManager
    {
        /// <summary>
        /// 音频资源缓存
        /// </summary>
        private readonly Dictionary<string, AudioClip> _audioClipCache = new Dictionary<string, AudioClip>();

        /// <summary>
        /// 正在播放的音效播放器列表
        /// </summary>
        private readonly List<AudioPlayer> _activePlayers = new List<AudioPlayer>();

        /// <summary>
        /// 音效播放器预制体路径
        /// </summary>
        private const string AUDIO_PLAYER_PREFAB_PATH = "Arts/Prefabs/Sounds/AudioPlayer";

        /// <summary>
        /// 音效加载路径
        /// </summary>
        private const string AUDIO_LOAD_PATH = "Arts/Audios";

        /// <summary>
        /// 对象池管理器
        /// </summary>
        private GameObjectPoolManager _poolManager;

        /// <summary>
        /// 资源管理器
        /// </summary>
        private ResourceManager _resourceManager;

        /// <summary>
        /// 全局音量（0-1）
        /// </summary>
        private float _globalVolume = 1f;

        public async void Init()
        {
            // 获取对象池管理器
            _poolManager = GameFrameworkManager.Instance.GetManager<GameObjectPoolManager>();
            
            // 获取资源管理器
            _resourceManager = GameFrameworkManager.Instance.GetManager<ResourceManager>();

            await _poolManager.PreloadAsync(AUDIO_PLAYER_PREFAB_PATH);

            Debug.Log("[SoundManager] 音效管理器初始化完成");
        }

        public void Update()
        {
            // 清理已完成的播放器
            for (int i = _activePlayers.Count - 1; i >= 0; i--)
            {
                if (_activePlayers[i] == null || !_activePlayers[i].IsPlaying())
                {
                    _activePlayers.RemoveAt(i);
                }
            }
        }

        public void Release()
        {
            // 停止所有正在播放的音效
            StopAll();

            // 清理缓存
            _audioClipCache.Clear();
            _activePlayers.Clear();

            // Debug.Log("[SoundManager] 音效管理器已释放");
        }

        #region 公共方法

        /// <summary>
        /// 播放指定音效
        /// </summary>
        /// <param name="audioData">音效数据</param>
        /// <param name="loop">播放次数（1表示播放一次，2表示播放2次，-1表示无限循环）</param>
        /// <returns>音效播放器实例，可用于链式调用添加回调</returns>
        public AudioCallback PlaySound(AudioData audioData, int loop = 1)
        {
            // 加载音频资源
            // AudioClip clip = await LoadAudioClip(audioData.AudioName);
            AudioClip clip = audioData.audioClip;
            if (clip == null)
            {
                Debug.LogError($"[SoundManager] 获取音效失败: {audioData.audioName}");
                return null;
            }

            // 从对象池获取音效播放器
            // AudioPlayer player = await GetAudioPlayerAsync();
            AudioPlayer player = GetAudioPlayer();
            if (player == null)
            {
                Debug.LogError("[SoundManager] 获取音效播放器失败");
                return null;
            }

            // 设置播放器
            float finalVolume = audioData.volume >= 0 ? audioData.volume : _globalVolume;
            
            // 注册内部回收回调
            player.OnPlayComplete += OnAudioPlayComplete;

            // 添加到活动列表
            _activePlayers.Add(player);
            
            // 播放音效（支持链式调用）
            return player.Play(clip, loop, finalVolume);
        }

        /// <summary>
        /// 播放一次性音效
        /// </summary>
        /// <param name="audioData">音效数据</param>
        /// <returns></returns>
        public AudioCallback PlaySoundOneShot(AudioData audioData)
        {
            // 加载音频资源
            AudioClip clip = audioData.audioClip;
            if (clip == null)
            {
                Debug.LogError($"[SoundManager] 加载音效失败: {audioData.audioName}");
                return null;
            }

            // 从对象池获取音效播放器
            AudioPlayer player = GetAudioPlayer();
            if (player == null)
            {
                Debug.LogError("[SoundManager] 获取音效播放器失败");
                return null;
            }

            // 设置播放器
            float finalVolume = audioData.volume >= 0 ? audioData.volume : _globalVolume;
            
            // 注册内部回收回调
            player.OnPlayComplete += OnAudioPlayComplete;

            // 添加到活动列表
            _activePlayers.Add(player);
            
            // 播放音效（支持链式调用）
            return player.Play(clip, volume: finalVolume);
        }

        // /// <summary>
        // /// 加载音效资源
        // /// </summary>
        // /// <param name="audioName">音效文件名</param>
        // /// <returns></returns>
        // private async UniTask<AudioClip> LoadAudioClip(string audioName)
        // {
        //     if (_audioClipCache.TryGetValue(audioName, out var audioClip))
        //     {
        //         return audioClip;
        //     }
        //     
        //     audioClip = await _resourceManager.LoadAssetAsync<AudioClip>($"{AUDIO_LOAD_PATH}/{audioName}");
        //     _audioClipCache[audioName] = audioClip;
        //     return audioClip;
        // }

        /// <summary>
        /// 停止指定音效播放器
        /// </summary>
        public void StopSound(AudioPlayer player)
        {
            if (player == null) return;

            player.Stop();
            _activePlayers.Remove(player);
            
            // 回收到对象池
            _poolManager.Despawn(player.gameObject);
        }

        /// <summary>
        /// 停止所有音效
        /// </summary>
        public void StopAll()
        {
            for (int i = _activePlayers.Count - 1; i >= 0; i--)
            {
                if (_activePlayers[i] != null)
                {
                    _activePlayers[i].Stop();
                    _poolManager.Despawn(_activePlayers[i].gameObject);
                }
            }
            _activePlayers.Clear();
        }

        /// <summary>
        /// 设置全局音量
        /// </summary>
        public void SetGlobalVolume(float volume)
        {
            _globalVolume = Mathf.Clamp01(volume);
            
            // 更新所有正在播放的音效音量
            foreach (var player in _activePlayers)
            {
                if (player != null)
                {
                    player.SetVolume(_globalVolume);
                }
            }
        }

        /// <summary>
        /// 获取全局音量
        /// </summary>
        public float GetGlobalVolume()
        {
            return _globalVolume;
        }

        /// <summary>
        /// 预加载音效
        /// </summary>
        public async UniTask PreloadSound(params string[] audioNames)
        {
            foreach (var audioName in audioNames)
            {
                // await _resourceManager.PreloadAsync(audioName);
            }
        }

        /// <summary>
        /// 卸载音效资源
        /// </summary>
        public void UnloadSound(string audioName)
        {
            if (_audioClipCache.ContainsKey(audioName))
            {
                _audioClipCache.Remove(audioName);
            }
        }

        /// <summary>
        /// 获取正在播放的音效数量
        /// </summary>
        public int GetActivePlayerCount()
        {
            return _activePlayers.Count;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 从对象池获取音效播放器
        /// </summary>
        private async UniTask<AudioPlayer> GetAudioPlayerAsync()
        {
            GameObject playerObj = await _poolManager.SpawnAsync(AUDIO_PLAYER_PREFAB_PATH);
            if (playerObj == null)
            {
                Debug.LogError("[SoundManager] 从对象池获取音效播放器失败");
                return null;
            }

            AudioPlayer player = playerObj.GetComponent<AudioPlayer>();
            if (player == null)
            {
                Debug.LogError("[SoundManager] 音效播放器组件不存在");
                _poolManager.Despawn(playerObj);
                return null;
            }

            // 设置父节点为场景根节点（播放时不在对象池根节点下）
            // playerObj.transform.SetParent(_soundRoot);

            return player;
        }

        private AudioPlayer GetAudioPlayer()
        {
            return _poolManager.Spawn(AUDIO_PLAYER_PREFAB_PATH).GetComponent<AudioPlayer>();
        }
        
        /// <summary>
        /// 音效播放完成回调
        /// </summary>
        private void OnAudioPlayComplete(AudioPlayer player)
        {
            if (player == null) return;

            player.OnPlayComplete -= OnAudioPlayComplete;
            
            // 从活动列表移除
            _activePlayers.Remove(player);

            // 回收到对象池
            _poolManager.Despawn(player.gameObject);
        }

        #endregion
    }
}