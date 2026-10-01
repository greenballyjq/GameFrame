namespace GameFramework.Audio
{
    /// <summary>
    /// 到达上限时的处置。
    /// </summary>
    public enum OverflowPolicy
    {
        /// <summary>拒绝新请求，在播的实例一条都不动。</summary>
        RejectNew = 0,

        /// <summary>
        /// 先放行新请求，再淘汰在播实例腾位，腾到不再超限为止。
        /// 淘汰判据：优先级数字大的先走，同优先级时先播的先走。
        /// </summary>
        StopOldest = 1
    }
}
