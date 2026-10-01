using System;
using UnityEngine;

namespace GameFramework.Audio
{
    internal sealed class SfxSlot
    {
        internal readonly Transform Transform;

        internal readonly AudioSource Source;

        internal readonly int Index;

        internal long Sequence;

        internal SfxCue Cue;

        internal Transform Follow;

        internal float Duration;

        internal float Pitch;

        internal float Elapsed;

        internal Action<PlayEndReason> OnEnded;

        private SfxSlot(Transform transform, AudioSource source, int index)
        {
            Transform = transform;
            Source = source;
            Index = index;
        }

        internal bool IsFree
        {
            get { return Sequence == 0; }
        }

        internal static SfxSlot Create(Transform root, int index)
        {
            var gameObject = new GameObject("SfxSlot_" + index);
            var transform = gameObject.transform;
            transform.SetParent(root);

            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;

            return new SfxSlot(transform, source, index);
        }

        internal void Begin(
            long sequence,
            SfxCue cue,
            AudioBus bus,
            AudioClip clip,
            float pitch,
            Vector3 position,
            Transform follow,
            Action<PlayEndReason> onEnded)
        {
            Sequence = sequence;
            Cue = cue;
            Follow = follow;
            Duration = clip.length;
            Pitch = pitch;
            Elapsed = 0f;
            OnEnded = onEnded;

            Source.clip = clip;
            Source.loop = cue.Loop;
            Source.pitch = pitch;
            Source.volume = cue.Volume;
            // 模块的优先级方向与引擎一致（数字大 = 更容易被牺牲），直接写入即可，不做换算。
            Source.priority = cue.Priority;
            Source.spatialBlend = cue.Spatial ? 1f : 0f;
            Source.outputAudioMixerGroup = bus.OutputGroup;

            // 位置只对空间化的声音有意义，非空间化不写位置。
            if (cue.Spatial)
                Transform.position = position;
        }

        internal void End()
        {
            Source.Stop();
            Source.clip = null;

            // 一次占用的全部状态都在这里清空；漏掉任何一个字段，下一次占用就会带着上一次的残留。
            Sequence = 0;
            Cue = null;
            Follow = null;
            Duration = 0f;
            Pitch = 0f;
            Elapsed = 0f;
            OnEnded = null;
        }
    }
}
