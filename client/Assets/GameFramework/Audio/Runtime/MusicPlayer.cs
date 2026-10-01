using UnityEngine;

namespace GameFramework.Audio
{
    internal sealed class MusicPlayer
    {
        // 三个声部分别承担：当前曲、正在淡出的曲、正在淡入的新曲。
        // 三声部让"淡变还没走完就再换一首"也能平滑过渡，不必硬切。
        private const int VoiceCount = 3;

        private readonly Voice[] _voices = new Voice[VoiceCount];

        private AudioConfig _config;
        private GameObject _container;

        // 暂停状态只在这里存一份；每个音源上的暂停由它派生。
        private bool _paused;

        internal void Init(AudioConfig config, Transform root)
        {
            _config = config;
            _container = new GameObject("Music");
            _container.transform.SetParent(root);

            for (int i = 0; i < _voices.Length; i++)
            {
                var source = _container.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = true;

                _voices[i] = new Voice(source);
            }
        }

        internal void Play(MusicCue cue, float fadeSeconds)
        {
            if (_container == null)
                return;

            if (fadeSeconds < 0f)
                fadeSeconds = 0f;

            // 传入空定义、或传入没配片段的定义，等于停止当前音乐，而不是无声播一段空音乐。
            if (cue == null || cue.Clip == null)
            {
                Stop(fadeSeconds);
                return;
            }

            Voice target = TakeVoice();
            target.Cue = cue;
            target.Source.clip = cue.Clip;
            target.Source.outputAudioMixerGroup = BusOf(cue).OutputGroup;
            target.Source.volume = 0f;
            target.Source.Play();

            // 暂停期间换曲不解除暂停，新曲同样从暂停状态开始。
            if (_paused)
                target.Source.Pause();

            target.FadeIn(cue.Volume, fadeSeconds);

            // 其余在用声部各自从当前音量淡出，所以淡变被中途打断也不会跳变。
            for (int i = 0; i < _voices.Length; i++)
            {
                Voice voice = _voices[i];
                if (voice == target || voice.Cue == null)
                    continue;

                voice.FadeOut(fadeSeconds);
            }
        }

        internal void Stop(float fadeSeconds)
        {
            if (_container == null)
                return;

            if (fadeSeconds < 0f)
                fadeSeconds = 0f;

            for (int i = 0; i < _voices.Length; i++)
            {
                Voice voice = _voices[i];
                if (voice == null || voice.Cue == null)
                    continue;

                // 时长为 0 表示立即停止；停止动作本身不改暂停状态。
                if (fadeSeconds <= 0f)
                    voice.End();
                else
                    voice.FadeOut(fadeSeconds);
            }
        }

        internal void SetPaused(bool value)
        {
            if (_container == null || _paused == value)
                return;

            _paused = value;

            // 这一遍只施加到当前在用的声部；暂停之后新起的播放由 Play 另行施加。
            for (int i = 0; i < _voices.Length; i++)
            {
                Voice voice = _voices[i];
                if (voice == null || voice.Cue == null)
                    continue;

                if (value)
                    voice.Source.Pause();
                else
                    voice.Source.UnPause();
            }
        }

        internal void Tick(float audioDelta)
        {
            // 暂停期间淡变不推进，恢复后从原处继续。
            if (_container == null || _paused)
                return;

            for (int i = 0; i < _voices.Length; i++)
            {
                Voice voice = _voices[i];
                if (voice == null || voice.Cue == null || voice.Duration <= 0f)
                    continue;

                voice.Elapsed += audioDelta;
                float progress = Mathf.Clamp01(voice.Elapsed / voice.Duration);
                voice.Source.volume = Mathf.Lerp(voice.FromVolume, voice.ToVolume, progress);

                if (voice.Elapsed < voice.Duration)
                    continue;

                // 时长归零表示这一段淡变已经走完，避免后续帧重复处理。
                voice.Duration = 0f;
                if (voice.Rising)
                {
                    // 淡入结束只把音量钉在目标值，声部继续持有这首曲子。
                    voice.Source.volume = voice.ToVolume;
                }
                else
                {
                    // 淡出结束才释放声部。
                    voice.End();
                }
            }
        }

        internal void Dispose()
        {
            if (_container == null)
                return;

            for (int i = 0; i < _voices.Length; i++)
            {
                if (_voices[i] != null)
                    _voices[i].End();
            }

            UnityEngine.Object.Destroy(_container);

            _container = null;
            _config = null;
            _paused = false;
        }

        private AudioBus BusOf(MusicCue cue)
        {
            return cue.Bus != null ? cue.Bus : _config.DefaultBus;
        }

        // 优先取空闲声部；三个都用满时取当前音量最小的那个立即结束，这是声部用满时的兜底。
        private Voice TakeVoice()
        {
            Voice quietest = null;

            for (int i = 0; i < _voices.Length; i++)
            {
                Voice voice = _voices[i];

                if (voice.Cue == null)
                    return voice;

                if (quietest == null || voice.Source.volume < quietest.Source.volume)
                    quietest = voice;
            }

            quietest.End();
            return quietest;
        }

        private sealed class Voice
        {
            internal readonly AudioSource Source;

            internal MusicCue Cue;
            internal float FromVolume;
            internal float ToVolume;
            internal float Elapsed;
            internal float Duration;
            internal bool Rising;

            internal Voice(AudioSource source)
            {
                Source = source;
            }

            internal void FadeIn(float to, float seconds)
            {
                FromVolume = 0f;
                ToVolume = to;
                Elapsed = 0f;
                Duration = seconds;
                Rising = true;

                if (seconds <= 0f)
                    Source.volume = to;
            }

            // 起点取当前音量，所以上一次淡变没走完时被打断也不会跳变。
            internal void FadeOut(float seconds)
            {
                FromVolume = Source.volume;
                ToVolume = 0f;
                Elapsed = 0f;
                Duration = seconds;
                Rising = false;

                if (seconds <= 0f)
                    End();
            }

            internal void End()
            {
                Source.Stop();
                Source.clip = null;
                Source.volume = 0f;

                Cue = null;
                FromVolume = 0f;
                ToVolume = 0f;
                Elapsed = 0f;
                Duration = 0f;
                Rising = false;
            }
        }
    }
}
