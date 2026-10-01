using System;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Utility;
using UnityEngine;

namespace GameFramework.Audio
{
    /// <summary>
    /// 音频模块的对外入口：音效的播放与停止、音乐的播放与暂停、总线音量设置。
    /// 模块只执行约束、不产生约束——上限、优先级、输出分组都写在资产上，调用方不需要知道。
    /// </summary>
    public class AudioManager : IManager, IManagerUpdatable
    {
        private readonly AudioConfig _config;
        private readonly SfxPlayer _sfx = new SfxPlayer();
        private readonly MusicPlayer _music = new MusicPlayer();

        private GameObject _root;

        public AudioManager(AudioConfig config)
        {
            _config = config;
        }

        /// <summary>初始化。配置缺项时记一条错误并保持"未就绪"，之后所有播放返回 NotReady。</summary>
        public UniTask InitAsync()
        {
            string reason;
            if (!Validate(out reason))
            {
                Log.Error("[Audio] 初始化失败：" + reason);
                return UniTask.CompletedTask;
            }

            _root = new GameObject("AudioRuntime");
            UnityEngine.Object.DontDestroyOnLoad(_root);

            _sfx.Init(_config, _root.transform);
            _music.Init(_config, _root.transform);

            Log.Info("[Audio] 初始化完成：槽位 " + _config.SlotCount + " 个。");

            return UniTask.CompletedTask;
        }

        /// <summary>播放一条音效，不给位置。定义要求空间化时会被拒绝，失败原因 NeedsPosition。</summary>
        /// <param name="onEnded">
        /// 这次播放结束时回调一次，带回结束原因。四种结束方式都会回调；模块释放时不回调。
        /// </param>
        public PlayResult Play(SfxCue cue, Action<PlayEndReason> onEnded = null)
        {
            return _sfx.Play(cue, false, default(Vector3), null, onEnded);
        }

        /// <summary>播放一条音效，固定在给定位置。位置只对空间化的声音有意义。结束回调的约定同不给位置的重载。</summary>
        public PlayResult Play(SfxCue cue, Vector3 position, Action<PlayEndReason> onEnded = null)
        {
            return _sfx.Play(cue, true, position, null, onEnded);
        }

        /// <summary>
        /// 播放一条音效并每帧跟随目标。目标被销毁后跟随停止，声音留在最后一次的位置播完。
        /// 跟随目标传空等同于走不给位置的入口。结束回调的约定同不给位置的重载。
        /// </summary>
        public PlayResult Play(SfxCue cue, Transform follow, Action<PlayEndReason> onEnded = null)
        {
            if (follow == null)
                return _sfx.Play(cue, false, default(Vector3), null, onEnded);

            return _sfx.Play(cue, true, follow.position, follow, onEnded);
        }

        /// <summary>停掉这次播放。指认物无效、或它对应的槽位已经被复用，就什么都不做。</summary>
        public void Stop(PlayHandle handle)
        {
            _sfx.Stop(handle);
        }

        /// <summary>停掉这条定义当前所有的播放，每一条各回调一次。</summary>
        public void Stop(SfxCue cue)
        {
            _sfx.Stop(cue);
        }

        /// <summary>停掉全部音效，每一条各回调一次。不影响音乐。</summary>
        public void StopSfx()
        {
            _sfx.StopAll();
        }

        /// <summary>
        /// 这次播放是否仍在进行。只比对指认物是否还指向那个槽位，不读引擎的播放状态；
        /// 素材刚播完、还没到本帧回收的那一刻，它仍返回真。
        /// </summary>
        public bool IsSfxPlaying(PlayHandle handle)
        {
            return _sfx.IsPlaying(handle);
        }

        /// <summary>
        /// 换一首音乐，与当前音乐交叉淡变。淡变时长为 0 表示立即切换。
        /// 传入空定义、或传入没配片段的定义，等同于停止当前音乐。
        /// 暂停期间调用不会解除暂停，新曲同样保持暂停。
        /// </summary>
        public void PlayMusic(MusicCue cue, float fadeSeconds)
        {
            _music.Play(cue, fadeSeconds);
        }

        /// <summary>停止当前音乐。淡变时长为 0 表示立即停止；停止动作不改暂停状态。</summary>
        public void StopMusic(float fadeSeconds)
        {
            _music.Stop(fadeSeconds);
        }

        /// <summary>暂停音乐。只影响音乐，音效不受影响。</summary>
        public void PauseMusic()
        {
            _music.SetPaused(true);
        }

        /// <summary>恢复被暂停的音乐。暂停期间播放的新曲也会一起恢复。</summary>
        public void ResumeMusic()
        {
            _music.SetPaused(false);
        }

        /// <summary>
        /// 设置一条总线的音量，取值 0 到 1（线性，写进混音器前换成分贝）。
        /// 返回假表示这条总线的音量参数没有在混音器上暴露，音量没有生效。
        /// </summary>
        public bool SetBusVolume(AudioBus bus, float volume01)
        {
            if (_config == null || _config.Mixer == null || bus == null || string.IsNullOrEmpty(bus.VolumeParam))
                return false;

            const float SilenceThreshold = 0.0001f;
            const float SilentDecibel = -80f;

            float linear = Mathf.Clamp01(volume01);
            float decibel = linear <= SilenceThreshold
                ? SilentDecibel
                : 20f * Mathf.Log10(linear);

            return _config.Mixer.SetFloat(bus.VolumeParam, decibel);
        }

        /// <summary>
        /// 每帧驱动。参数只表示"该跑一帧"，不参与推进——音频时间由模块自己取，
        /// 因此游戏时间缩放（暂停、慢动作、快进）不改变音频的播放。
        /// </summary>
        public void DoUpdate(float heartbeat)
        {
            float audioDelta = SampleAudioDelta();
            _sfx.Tick(audioDelta);
            _music.Tick(audioDelta);
        }

        // 音频时间取不受游戏时间缩放影响的帧增量；要换成采样级精度（音频设备时钟差分）时只改这一处。
        private static float SampleAudioDelta()
        {
            return Time.unscaledDeltaTime;
        }

        /// <summary>释放模块，销毁它创建的全部对象。这条路径上不再发出结束通知。</summary>
        public void Dispose()
        {
            _sfx.Dispose();
            _music.Dispose();

            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
                _root = null;
            }
        }

        private bool Validate(out string reason)
        {
            if (_config == null)
            {
                reason = "未指定配置资产。";
                return false;
            }

            if (_config.Mixer == null)
            {
                reason = "配置未指定混音器。音量相乘由混音器完成，缺了它任何总线的音量都调不了。";
                return false;
            }

            if (_config.SlotCount <= 0)
            {
                reason = "槽位数量必须大于 0，否则任何音效都播不出来。";
                return false;
            }

            if (_config.DefaultBus == null)
            {
                reason = "配置未指定默认总线。没填总线的音效定义与音乐定义会因此无法确定输出分组。";
                return false;
            }

            reason = null;
            return true;
        }
    }
}
