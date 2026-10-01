using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Audio
{
    internal sealed class SfxPlayer
    {
        private AudioConfig _config;
        private SfxSlot[] _slots;

        // 占用序号全局单调。它同时承担两件事：这次播放的身份，以及淘汰判定在同优先级时的第二判据。
        private long _nextSequence;

        // 轮转游标按内容资产记录，表只增不减：键持有资产引用，模块存活期间这些资产不会被卸载。
        private readonly Dictionary<SfxCue, ClipRotation> _rotations = new Dictionary<SfxCue, ClipRotation>();

        // 结束通知在两帧之间攒批：本帧产生的通知先入队，推进结束后统一派发。
        private List<EndNotice> _pendingEnds = new List<EndNotice>();
        private List<EndNotice> _dispatchingEnds = new List<EndNotice>();

        // 释放期间置位，用来抑制结束通知——调用方不该在模块关掉之后还收到回调。
        private bool _shuttingDown;

        internal void Init(AudioConfig config, Transform root)
        {
            _config = config;
            _slots = new SfxSlot[config.SlotCount];

            for (int i = 0; i < _slots.Length; i++)
                _slots[i] = SfxSlot.Create(root, i);
        }

        internal PlayResult Play(
            SfxCue cue,
            bool hasPosition,
            Vector3 position,
            Transform follow,
            Action<PlayEndReason> onEnded)
        {
            if (_slots == null)
                return PlayResult.Rejected(PlayFailure.NotReady);

            if (cue == null)
                return PlayResult.Rejected(PlayFailure.CueMissing);

            if (cue.ClipCount == 0)
                return PlayResult.Rejected(PlayFailure.NoClip);

            if (cue.Spatial && !hasPosition)
                return PlayResult.Rejected(PlayFailure.NeedsPosition);

            AudioBus bus = BusOf(cue);

            // 受理分两段：Admit 只数数、不改任何状态，被判拒绝时槽位内容逐字节不变。
            // 淘汰既有实例发生在受理通过之后的 Apply 里，所以不会出现"请求被拒、但已经有人被顶掉"。
            PlayFailure failure = Admit(cue, bus);
            if (failure != PlayFailure.None)
                return PlayResult.Rejected(failure);

            Apply(cue, bus);

            SfxSlot slot = TakeFreeSlot();
            slot.Begin(
                ++_nextSequence,
                cue,
                bus,
                cue.ClipAt(NextClipIndex(cue)),
                NextPitch(cue),
                follow != null ? follow.position : position,
                follow,
                onEnded);
            slot.Source.Play();

            return PlayResult.Accepted(new PlayHandle(slot.Index, slot.Sequence));
        }

        internal void Stop(PlayHandle handle)
        {
            if (_slots == null)
                return;

            SfxSlot slot = Resolve(handle);
            if (slot != null)
                EndPlayback(slot, PlayEndReason.Stopped);
        }

        internal void Stop(SfxCue cue)
        {
            if (_slots == null || cue == null)
                return;

            for (int i = 0; i < _slots.Length; i++)
            {
                SfxSlot slot = _slots[i];
                if (!slot.IsFree && slot.Cue == cue)
                    EndPlayback(slot, PlayEndReason.Stopped);
            }
        }

        internal void StopAll()
        {
            if (_slots == null)
                return;

            for (int i = 0; i < _slots.Length; i++)
            {
                if (!_slots[i].IsFree)
                    EndPlayback(_slots[i], PlayEndReason.Stopped);
            }
        }

        internal bool IsPlaying(PlayHandle handle)
        {
            return _slots != null && Resolve(handle) != null;
        }

        internal void Tick(float audioDelta)
        {
            if (_slots == null)
                return;

            for (int i = 0; i < _slots.Length; i++)
            {
                SfxSlot slot = _slots[i];
                if (slot.IsFree)
                    continue;

                // 跟随目标被销毁后这个引用会变成"已销毁"，比较结果为空，跟随自然停止，声音留在最后一次的位置播完。
                if (slot.Follow != null)
                    slot.Transform.position = slot.Follow.position;

                if (slot.Cue.Loop)
                    continue;

                // 结束判据用模块自己累积的素材时间，按音高绝对值换算（音高会改变播放速率）。
                // 不读引擎的播放状态：那个读数在被剔除、刚起播这些情形下没有文档保证。
                slot.Elapsed += audioDelta * Mathf.Abs(slot.Pitch);
                if (slot.Elapsed >= slot.Duration)
                    EndPlayback(slot, PlayEndReason.Finished);
            }

            DispatchEnd();
        }

        internal void Dispose()
        {
            if (_slots == null)
                return;

            _shuttingDown = true;

            for (int i = 0; i < _slots.Length; i++)
            {
                if (!_slots[i].IsFree)
                    EndPlayback(_slots[i], PlayEndReason.Stopped);
            }

            _pendingEnds.Clear();
            _dispatchingEnds.Clear();
            _rotations.Clear();

            for (int i = 0; i < _slots.Length; i++)
                UnityEngine.Object.Destroy(_slots[i].Transform.gameObject);

            _slots = null;
            _config = null;
            _shuttingDown = false;
        }

        private AudioBus BusOf(SfxCue cue)
        {
            return cue.Bus != null ? cue.Bus : _config.DefaultBus;
        }

        private int NextClipIndex(SfxCue cue)
        {
            int count = cue.ClipCount;
            if (count == 1)
                return 0;

            ClipRotation rotation;
            if (!_rotations.TryGetValue(cue, out rotation))
            {
                rotation = new ClipRotation();
                _rotations.Add(cue, rotation);
            }

            return rotation.Next(count, cue.Selection);
        }

        private static float NextPitch(SfxCue cue)
        {
            float min = cue.PitchMin;
            float max = cue.PitchMax;

            if (min == max)
                return min;

            if (min > max)
            {
                float temp = min;
                min = max;
                max = temp;
            }

            return UnityEngine.Random.Range(min, max);
        }

        private PlayFailure Admit(SfxCue cue, AudioBus bus)
        {
            PlayFailure failure = AdmitOne(
                cue.MaxInstances, cue.Overflow, cue, null, PlayFailure.CueLimitReached);
            if (failure != PlayFailure.None)
                return failure;

            failure = AdmitOne(
                bus.MaxInstances, bus.Overflow, null, bus, PlayFailure.BusLimitReached);
            if (failure != PlayFailure.None)
                return failure;

            return AdmitOne(
                _config.SlotCount, _config.SlotOverflow, null, null, PlayFailure.SlotLimitReached);
        }

        // 上限为负数表示不限，为 0 表示禁止。这一段只读计数，不改任何状态。
        private PlayFailure AdmitOne(
            int max, OverflowPolicy policy, SfxCue cue, AudioBus bus, PlayFailure rejection)
        {
            if (max < 0)
                return PlayFailure.None;

            if (max == 0)
                return rejection;

            if (CountInScope(cue, bus) < max)
                return PlayFailure.None;

            return policy == OverflowPolicy.RejectNew ? rejection : PlayFailure.None;
        }

        // 腾位循环到"再加一条也不超限"为止。上限被调小、在播数量远超新上限时，
        // 一次受理就收敛到该有的数量，不会长期超限。
        private void Apply(SfxCue cue, AudioBus bus)
        {
            while (true)
            {
                bool overCue = Over(cue.MaxInstances, cue, null);
                bool overBus = Over(bus.MaxInstances, null, bus);
                bool overSlot = Over(_config.SlotCount, null, null);

                if (!overCue && !overBus && !overSlot)
                    return;

                SfxSlot victim = SelectVictim(overCue, overBus, overSlot, cue, bus);
                if (victim == null)
                    return;

                EndPlayback(victim, PlayEndReason.Stolen);
            }
        }

        private bool Over(int max, SfxCue cue, AudioBus bus)
        {
            return max >= 0 && CountInScope(cue, bus) + 1 > max;
        }

        // 槽位这一级超限时，全体在播实例都是候选；定义级与总线级只在自己的范围内挑。
        private SfxSlot SelectVictim(bool overCue, bool overBus, bool overSlot, SfxCue cue, AudioBus bus)
        {
            SfxSlot worst = null;

            for (int i = 0; i < _slots.Length; i++)
            {
                SfxSlot slot = _slots[i];
                if (slot.IsFree)
                    continue;

                bool inScope = overSlot
                    || (overCue && slot.Cue == cue)
                    || (overBus && BusOf(slot.Cue) == bus);

                if (!inScope)
                    continue;

                if (worst == null || IsMoreSacrificial(slot, worst))
                    worst = slot;
            }

            return worst;
        }

        // 判据只依赖优先级与占用序号，与遍历顺序、帧率、容器排列都无关，所以同帧并列也有确定结果。
        private static bool IsMoreSacrificial(SfxSlot candidate, SfxSlot current)
        {
            if (candidate.Cue.Priority != current.Cue.Priority)
                return candidate.Cue.Priority > current.Cue.Priority;

            return candidate.Sequence < current.Sequence;
        }

        private int CountInScope(SfxCue cue, AudioBus bus)
        {
            int count = 0;

            for (int i = 0; i < _slots.Length; i++)
            {
                SfxSlot slot = _slots[i];
                if (slot.IsFree)
                    continue;

                if (cue != null && slot.Cue != cue)
                    continue;

                if (bus != null && BusOf(slot.Cue) != bus)
                    continue;

                count++;
            }

            return count;
        }

        // 槽位在下标顺序上取第一个空闲；空闲与否只看占用序号是不是 0。
        private SfxSlot TakeFreeSlot()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].IsFree)
                    return _slots[i];
            }

            return null;
        }

        // 序号对不上说明这个槽位已经被复用，旧的指认物不再指向任何播放。
        private SfxSlot Resolve(PlayHandle handle)
        {
            if (!handle.IsValid)
                return null;

            if (handle.Slot < 0 || handle.Slot >= _slots.Length)
                return null;

            SfxSlot slot = _slots[handle.Slot];
            return slot.Sequence == handle.Sequence ? slot : null;
        }

        // 结束的唯一出口：四种结束方式都走这里，各通知一次。
        // 回调先取出再清空槽位，所以清空之后这个槽位可以被立即复用。
        private void EndPlayback(SfxSlot slot, PlayEndReason reason)
        {
            Action<PlayEndReason> callback = slot.OnEnded;
            slot.End();

            if (callback != null && !_shuttingDown)
                _pendingEnds.Add(new EndNotice(reason, callback));
        }

        // 通知统一在本帧推进结束后派发，两个好处：回调里再触发播放会落到下一帧；
        // 回调里停掉别的实例所产生的通知进下一批，派发过程中不会改到正在遍历的队列。
        private void DispatchEnd()
        {
            if (_pendingEnds.Count == 0)
                return;

            List<EndNotice> batch = _pendingEnds;
            _pendingEnds = _dispatchingEnds;
            _dispatchingEnds = batch;

            for (int i = 0; i < _dispatchingEnds.Count; i++)
            {
                EndNotice notice = _dispatchingEnds[i];
                if (notice.Callback != null)
                    notice.Callback(notice.Reason);
            }

            _dispatchingEnds.Clear();
        }

        private readonly struct EndNotice
        {
            internal readonly PlayEndReason Reason;
            internal readonly Action<PlayEndReason> Callback;

            internal EndNotice(PlayEndReason reason, Action<PlayEndReason> callback)
            {
                Reason = reason;
                Callback = callback;
            }
        }

        private sealed class ClipRotation
        {
            private int[] _order;
            private int _cursor;

            // 候选数量与顺序表对不上、或者这一轮已经取完时重洗；否则沿用上一轮的排列。
            internal int Next(int count, ClipSelection selection)
            {
                if (selection == ClipSelection.RandomEach)
                    return UnityEngine.Random.Range(0, count);

                if (_order == null || _order.Length != count || _cursor >= count)
                    Shuffle(count);

                return _order[_cursor++];
            }

            private void Shuffle(int count)
            {
                if (_order == null || _order.Length != count)
                    _order = new int[count];

                for (int i = 0; i < count; i++)
                    _order[i] = i;

                for (int i = count - 1; i > 0; i--)
                {
                    int j = UnityEngine.Random.Range(0, i + 1);
                    int temp = _order[i];
                    _order[i] = _order[j];
                    _order[j] = temp;
                }

                _cursor = 0;
            }
        }
    }
}
