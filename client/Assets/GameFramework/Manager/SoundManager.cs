using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.Core.Audio;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace GameFramework.Manager
{
    /// <summary>
    /// 音效管理器 - 负责音效的加载、播放和管理
    /// 依赖于资源管理器 ResourceManager
    /// </summary>
    public class SoundManager : IManager
    {
        /// <summary>
        /// 正在播放的音效播放器列表
        /// </summary>
        private readonly List<AudioPlayer> _activePlayers = new List<AudioPlayer>();
        
        /// <summary>
        /// 正在播放的音效播放器列表，按音频通道分组
        /// </summary>
        private readonly Dictionary<AudioChannel, List<AudioPlayer>> _activePlayersByChannel = new Dictionary<AudioChannel, List<AudioPlayer>>();
        
        /// <summary>
        /// 音频通道的音量倍数
        /// </summary>
        private readonly Dictionary<AudioChannel, float> _channelVolumeMultiplier = new Dictionary<AudioChannel, float>();

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
        
        private GameObject _audioPlayerPrefab;

        public void Init()
        {
            // 获取对象池管理器
            _poolManager = GameFrameworkManager.Instance.GetManager<GameObjectPoolManager>();
            
            // 获取资源管理器
            _resourceManager = GameFrameworkManager.Instance.GetManager<ResourceManager>();

            PrepareAudioPlayerPrefab();

            Debug.Log("[SoundManager] 音效管理器初始化完成");
        }

        /// <summary>
        /// 准备音效播放器预制体
        /// </summary>
        private void PrepareAudioPlayerPrefab()
        {
            _audioPlayerPrefab = new GameObject("AudioPlayer");
            _audioPlayerPrefab.AddComponent<AudioSource>();
            _audioPlayerPrefab.AddComponent<AudioPlayer>();
            _audioPlayerPrefab.SetActive(false);
            _audioPlayerPrefab.transform.SetParent(_poolManager.PoolRoot);
        }

        public void DoUpdate(float dt)
        {
            // 清理已完成的播放器
            for (int i = _activePlayers.Count - 1; i >= 0; i--)
            {
                var audioPlayer = _activePlayers[i];
                if (audioPlayer == null || !audioPlayer.IsPlaying())
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
            _activePlayers.Clear();

            Debug.Log("[SoundManager] 音效管理器已释放");
        }

        #region 公共方法

        /// <summary>
        /// 播放指定2D音效（通过音频文件）
        /// </summary>
        /// <param name="audioClip">音频引用</param>
        /// <param name="channel">音频通道，默认为Sound</param>
        /// <param name="loop">播放次数（1表示播放一次，2表示播放2次，-1表示无限循环）</param>
        /// <param name="volume">音量（0-1），默认为1</param>
        /// <returns></returns>
        public AudioCallback PlaySound2D(AudioClip audioClip, AudioChannel channel = AudioChannel.Sound, int loop = 1, float volume = 1f)
        {
            if (audioClip == null)
            {
                Debug.LogError($"[SoundManager] 音效文件为空，无法播放");
                return new AudioCallback();
            }
            
            var audioData = new AudioData
            {
                audioClip = audioClip,
                audioChannel = channel,
                volume = volume,
            };

            return PlaySound2DInternal(audioData, loop);
        }

        /// <summary>
        /// 播放指定2D音效（通过路径）
        /// </summary>
        /// <param name="resourcePath">资源路径</param>
        /// <param name="channel">音频通道，默认为Sound</param>
        /// <param name="loop">播放次数（1表示播放一次，2表示播放2次，-1表示无限循环）</param>
        /// <param name="volume">音量（0-1），默认为1</param>
        /// <returns>音效播放器实例，可用于链式调用添加回调</returns>
        public async UniTask<AudioCallback> PlaySound2DAsync(string resourcePath, AudioChannel channel = AudioChannel.Sound, int loop = 1, float volume = 1f)
        {
            var audioClip = await GetAudioClipAsync(resourcePath);
            if (audioClip == null)
            {
                return new AudioCallback();
            }
            
            var audioData = new AudioData(audioClip, volume);
            audioData.audioChannel = channel;
            return PlaySound2DInternal(audioData, loop);
        }

        /// <summary>
        /// 播放3D世界音效（不绑定其它物体）
        /// </summary>
        /// <param name="resourcePath">音效资源路径</param>
        /// <param name="position">世界位置</param>
        /// <param name="channel">音频通道，默认为Sound</param>
        /// <param name="loop">循环播放次数</param>
        /// <param name="volume">音量（0-1），默认为1</param>
        /// <returns></returns>
        public async UniTask<AudioCallback> PlaySound3DWorldAsync(string resourcePath, Vector3 position = default
            , AudioChannel channel = AudioChannel.Sound, int loop = 1, float volume = 1f)
        {
            var audioClip = await GetAudioClipAsync(resourcePath);
            if (audioClip == null)
            {
                return null;
            }
            
            var audioData = new AudioData(audioClip, volume);
            audioData.audioChannel = channel;

            return PlaySound3DWorldInternal(audioData, loop, position);
        }

        /// <summary>
        /// todo 播放3D跟随物体音效（绑定其它物体）
        /// </summary>
        /// <param name="resourcePath">音效资源路径</param>
        /// <param name="position">世界位置</param>
        /// <param name="channel">音频通道，默认为Sound</param>
        /// <param name="loop">循环播放次数</param>
        /// <param name="volume">音量（0-1），默认为1</param>
        /// <returns></returns>
        public AudioCallback PlaySound3DLocal(string resourcePath, Vector3 position = default
            , AudioChannel channel = AudioChannel.Sound, int loop = 1, float volume = 1f)
        {
            return null;
        }

        /// <summary>
        /// 强调音频通道
        /// 确保关键音频不被无关的音效淹没
        /// </summary>
        /// <param name="audioChannel">需要强调的音频通道</param>
        /// <param name="volumeMultiplier">其它音频通道的音量倍数（0-1）</param>
        public void EmphasizeAudioChannelVolume(AudioChannel audioChannel, float volumeMultiplier = 0.5f)
        {
            volumeMultiplier = Mathf.Clamp01(volumeMultiplier);
            foreach (var item in _activePlayersByChannel)
            {
                if (item.Key == audioChannel) continue;
                _channelVolumeMultiplier[item.Key] = volumeMultiplier;
                foreach (var player in item.Value)
                {
                    player.SetVolumeMultiplier(volumeMultiplier);
                }
            }
        }

        /// <summary>
        /// 强调音频通道后，恢复音频通道音量
        /// </summary>
        /// <param name="excludeAudioChannel">强调的音频通道</param>
        public void RestoreAudioChannelVolume(AudioChannel excludeAudioChannel)
        {
            foreach (var item in _activePlayersByChannel)
            {
                if (item.Key == excludeAudioChannel) continue;
                _channelVolumeMultiplier[item.Key] = 1f;
                foreach (var player in item.Value)
                {
                    player.RestoreVolume();
                }
            }
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
        /// 获取正在播放的音效数量
        /// </summary>
        public int GetActivePlayerCount()
        {
            return _activePlayers.Count;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 获取音效Clip，根据资源路径加载
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        private async UniTask<AudioClip> GetAudioClipAsync(string resourcePath)
        {
            var audioClip = await _resourceManager.LoadAssetAsync<AudioClip>(resourcePath);
            if (audioClip == null)
            {
                Debug.LogError($"[SoundManager] 加载音效失败，路径: {resourcePath}");
                return null;
            }

            return audioClip;
        }
        
        /// <summary>
        /// 播放2D音效内部实现
        /// </summary>
        /// <param name="audioData">音效数据</param>
        /// <param name="loop">播放次数（1表示播放一次，2表示播放2次，-1表示无限循环）</param>
        /// <returns></returns>
        private AudioCallback PlaySound2DInternal(AudioData audioData, int loop)
        {
            var player = InitAudioPlayer(audioData);

            var clip = audioData.audioClip;
            if (clip == null)
            {
                Debug.LogError($"[SoundManager] 获取音效失败: {audioData.audioName}");
                return null;
            }
            
            // 设置音量
            var finalVolume = GetFinalVolume(audioData.volume, audioData.audioChannel);
            
            // 播放音效（支持链式调用）
            return player.Play(clip, audioData.audioChannel, loop, finalVolume);
        }

        /// <summary>
        /// 播放3D世界音效内部实现
        /// </summary>
        /// <param name="audioData">音效数据</param>
        /// <param name="loop">循环播放次数</param>
        /// <param name="position">世界位置</param>
        /// <returns></returns>
        private AudioCallback PlaySound3DWorldInternal(AudioData audioData, int loop, Vector3 position)
        {
            var player = InitAudioPlayer(audioData);
            
            var clip = audioData.audioClip;
            if (clip == null)
            {
                Debug.LogError($"[SoundManager] 获取音效失败: {audioData.audioName}");
                return null;
            }
            
            // 设置音量
            var finalVolume = GetFinalVolume(audioData.volume, audioData.audioChannel);

            player.transform.position = position;
            
            return player.Play(clip, audioData.audioChannel, loop, finalVolume);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private AudioCallback PlaySound3DLocalInternal(AudioData audioData, int loop)
        {
            return null;
        }

        /// <summary>
        /// 根据音频通道获取最终设置的音量
        /// </summary>
        /// <param name="volume">AudioData的Volume</param>
        /// <param name="audioChannel">音频通道</param>
        /// <returns></returns>
        private float GetFinalVolume(float volume, AudioChannel audioChannel)
        {
            var volumeMultiplier = 1f;
            if (_channelVolumeMultiplier.TryGetValue(audioChannel, out var multiplier))
            {
                volumeMultiplier = multiplier;
            }
            // 设置播放器
            var finalVolume = volume * volumeMultiplier;
            return finalVolume;
        }

        /// <summary>
        /// 根据AudioData生成并初始化AudioPlayer
        /// </summary>
        /// <param name="audioData"></param>
        /// <returns></returns>
        private AudioPlayer InitAudioPlayer(AudioData audioData)
        {
            // 从对象池获取音效播放器
            var player = SpawnAudioPlayer();
            if (player == null)
            {
                Debug.LogError("[SoundManager] 获取音效播放器失败");
                return null;
            }
            
            // 注册内部回收回调
            player.OnPlayComplete += OnAudioPlayComplete;

            // 添加到活动列表 
            _activePlayers.Add(player);
            
            // 添加到音频通道列表
            if (!_activePlayersByChannel.ContainsKey(audioData.audioChannel))
            {
                _activePlayersByChannel[audioData.audioChannel] = new List<AudioPlayer>();
            }
            _activePlayersByChannel[audioData.audioChannel].Add(player);

            return player;
        }

        /// <summary>
        /// 从对象池获取音效播放器
        /// </summary>
        /// <returns></returns>
        private AudioPlayer SpawnAudioPlayer()
        {
            return _poolManager.Spawn(_audioPlayerPrefab).GetComponent<AudioPlayer>();
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

            // 从音频通道列表移除
            if (_activePlayersByChannel.TryGetValue(player.Channel, out var players))
            {
                players.Remove(player);
            }

            // 回收到对象池
            _poolManager.Despawn(player.gameObject);
        }

        #endregion
    }
    
    /// <summary>
    /// 音频通道
    /// </summary>
    public enum AudioChannel
    {
        Voice, // 人声
        Bgm, // 背景音乐
        Sound // 音效
    }
}