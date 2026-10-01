namespace GameFramework.Audio
{
    /// <summary>
    /// 这次播放是怎么结束的。四种结束方式都会通知一次并带上原因，
    /// 唯一的例外是模块释放：那条路径不发出任何通知。
    /// </summary>
    public enum PlayEndReason
    {
        /// <summary>素材播完。循环音效不会收到这一条。</summary>
        Finished = 0,

        /// <summary>被显式停止：按指认物停、按定义停、全部停止都算。</summary>
        Stopped = 1,

        /// <summary>被后到的播放淘汰腾位。</summary>
        Stolen = 2
    }
}
