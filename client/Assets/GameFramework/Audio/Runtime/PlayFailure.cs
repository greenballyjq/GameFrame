namespace GameFramework.Audio
{
    /// <summary>
    /// 受理被拒绝的原因。一次受理只会给出其中一个：检查按"未就绪 → 定义为空 → 无可用片段 →
    /// 缺位置 → 定义上限 → 总线上限 → 槽位上限"的顺序进行，多个条件同时成立时给出靠前的那个。
    /// </summary>
    public enum PlayFailure
    {
        /// <summary>没有失败，即受理成功。</summary>
        None = 0,

        /// <summary>模块还没初始化完成，或者已经释放。</summary>
        NotReady = 1,

        /// <summary>传入的音效定义为空。</summary>
        CueMissing = 2,

        /// <summary>定义里一个非空片段都没有。</summary>
        NoClip = 3,

        /// <summary>定义要求空间化，但走的是不给位置的入口。跟随目标传空也算不给位置。</summary>
        NeedsPosition = 4,

        /// <summary>总线上限已满，且这条总线的处置是拒绝新请求。</summary>
        BusLimitReached = 5,

        /// <summary>这条定义的上限已满，且它的处置是拒绝新请求。</summary>
        CueLimitReached = 6,

        /// <summary>模块的槽位全部占用，且配置里的槽位处置是拒绝新请求。</summary>
        SlotLimitReached = 7
    }
}
