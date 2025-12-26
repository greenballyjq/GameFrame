using System;
using GameFramework.Core.Pool;
using UnityEngine;

namespace GameFramework.Core.Audio
{
    /// <summary>
    /// 音效播放器 - 负责单个音效的播放
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour, IPoolItem
    {
        private AudioSource _audioSource;
        private int _remainingLoops;
        private bool _isPlaying;
        // private Action<AudioPlayer> _onCompleteCallback;
        
        private AudioCallback _audioCallback;

        /// <summary>
        /// 音效播放完毕回调事件
        /// </summary>
        public event Action<AudioPlayer> OnPlayComplete;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip">音频剪辑</param>
        /// <param name="loop">播放次数（1表示播放一次，2表示播放2次，-1表示无限循环）</param>
        /// <param name="volume">音量（0-1）</param>
        /// <returns>返回自身，支持链式调用</returns>
        public AudioCallback Play(AudioClip clip, int loop = 1, float volume = 1f)
        {
            if (clip == null)
            {
                Debug.LogError("[AudioPlayer] AudioClip 不能为空");
                return _audioCallback;
            }

            _audioSource.clip = clip;
            _audioSource.volume = Mathf.Clamp01(volume);
            _audioSource.loop = loop == -1; // -1表示无限循环
            _remainingLoops = loop;
            _isPlaying = true;

            _audioSource.Play();

            // 如果不是无限循环，则计算播放完成时间
            if (loop != -1)
            {
                Invoke(nameof(CheckPlayComplete), clip.length);
            }

            return _audioCallback;
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.Stop();
            }

            _isPlaying = false;
            CancelInvoke(nameof(CheckPlayComplete));
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        public void SetVolume(float volume)
        {
            if (_audioSource != null)
            {
                _audioSource.volume = Mathf.Clamp01(volume);
            }
        }

        /// <summary>
        /// 获取音量
        /// </summary>
        public float GetVolume()
        {
            return _audioSource != null ? _audioSource.volume : 0f;
        }

        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying()
        {
            return _isPlaying && _audioSource != null && _audioSource.isPlaying;
        }

        /// <summary>
        /// 检查播放是否完成
        /// </summary>
        private void CheckPlayComplete()
        {
            if (_remainingLoops > 1)
            {
                _remainingLoops--;
                if (_audioSource.clip != null)
                {
                    _audioSource.Play();
                    Invoke(nameof(CheckPlayComplete), _audioSource.clip.length);
                }
            }
            else if (_remainingLoops == 1)
            {
                _isPlaying = false;
                
                // 触发链式回调
                _audioCallback?.InvokeCallback();
                // 触发事件
                OnPlayComplete?.Invoke(this);
            }
            // 如果 _remainingLoops == -1，表示无限循环，不触发完成回调
        }

        #region IPoolItem 实现

        public void OnSpawned()
        {
            gameObject.SetActive(true);
            _isPlaying = false;
            _remainingLoops = 1;
            _audioCallback = new AudioCallback();
        }

        public void OnDespawned()
        {
            Stop();
            OnPlayComplete = null;
            _audioSource.clip = null;
            gameObject.SetActive(false);
            _audioCallback.ClearCallback();
        }

        #endregion
    }
}
