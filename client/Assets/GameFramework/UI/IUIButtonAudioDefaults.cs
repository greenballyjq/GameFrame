using GameFramework.Audio;

namespace GameFramework.UI
{
    /// <summary>
    /// 按钮音效默认值来源。由游戏侧实现并注入 UIButton，提供未单独覆盖时的悬停与点击音效。
    /// </summary>
    public interface IUIButtonAudioDefaults
    {
        /// <summary>
        /// 指针悬停时播放的音效。为 null 时不播。
        /// </summary>
        SfxCue Hover { get; }

        /// <summary>
        /// 点击时播放的音效。为 null 时不播。
        /// </summary>
        SfxCue Click { get; }
    }
}
