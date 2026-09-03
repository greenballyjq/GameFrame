using UnityEngine;

namespace GameFramework.UI
{
    /// <summary>
    /// 可被入场动画改写的基准位。闲时漂浮叠在基准位上，两者不抢同一份坐标。
    /// </summary>
    public interface IUIMotionBase
    {
        Vector2 BaseAnchoredPosition { get; set; }
    }
}
