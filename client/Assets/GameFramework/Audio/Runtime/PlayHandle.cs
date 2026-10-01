namespace GameFramework.Audio
{
    /// <summary>
    /// 一次播放的指认物：槽位下标 + 该槽位被占用的次序号。
    /// 槽位被复用后，旧的指认物不会再指向任何播放，也不需要显式失效。
    /// </summary>
    public readonly struct PlayHandle
    {
        /// <summary>没有指向任何播放。</summary>
        public static readonly PlayHandle Invalid = default;

        public readonly int Slot;

        /// <summary>该槽位被占用的次序号，从 1 起。0 表示槽位空闲。</summary>
        public readonly long Sequence;

        /// <summary>
        /// 格式上是否合法。它回答不了"这次播放还在不在"——那个问题要用 IsSfxPlaying 问。
        /// </summary>
        public bool IsValid
        {
            get { return Sequence > 0; }
        }

        internal PlayHandle(int slot, long sequence)
        {
            Slot = slot;
            Sequence = sequence;
        }

        public override string ToString()
        {
            return IsValid
                ? "PlayHandle(Slot=" + Slot + ", Sequence=" + Sequence + ")"
                : "PlayHandle(Invalid)";
        }
    }
}
