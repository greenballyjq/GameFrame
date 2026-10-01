namespace GameFramework.Audio
{
    /// <summary>
    /// 多候选片段的选取方式。
    /// </summary>
    public enum ClipSelection
    {
        /// <summary>每次独立随机，允许连续播同一条片段。</summary>
        RandomEach = 0,

        /// <summary>打乱后逐个播完一轮再重洗。它的用意是避免短时间内反复播同一条，但跨轮不保证不重复。</summary>
        ShuffleRound = 1
    }
}
