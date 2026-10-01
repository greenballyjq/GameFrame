namespace GameFramework.Audio
{
    /// <summary>
    /// 受理结果。失败是返回值，不是异常。
    /// </summary>
    public readonly struct PlayResult
    {
        /// <summary>失败原因。None 表示受理成功。</summary>
        public readonly PlayFailure Failure;

        private readonly PlayHandle _handle;

        /// <summary>是否受理成功。只有成功时句柄才指向一次真实的播放。</summary>
        public bool Succeeded
        {
            get { return Failure == PlayFailure.None; }
        }

        private PlayResult(PlayHandle handle, PlayFailure failure)
        {
            _handle = handle;
            Failure = failure;
        }

        /// <summary>取本次播放的指认物。被拒绝时返回假，并把句柄置为无效值。</summary>
        public bool TryGetHandle(out PlayHandle handle)
        {
            handle = _handle;
            return Succeeded;
        }

        internal static PlayResult Accepted(PlayHandle handle)
        {
            return new PlayResult(handle, PlayFailure.None);
        }

        internal static PlayResult Rejected(PlayFailure failure)
        {
            return new PlayResult(PlayHandle.Invalid, failure);
        }

        public override string ToString()
        {
            return Succeeded ? "Accepted(" + _handle + ")" : "Rejected(" + Failure + ")";
        }
    }
}
