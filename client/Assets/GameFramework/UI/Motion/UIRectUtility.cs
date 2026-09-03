using UnityEngine;

namespace GameFramework.UI
{
    /// <summary>
    /// RectTransform 与屏幕空间的换算。入场起点用屏幕描述，不写死布局像素。
    /// </summary>
    public static class UIRectUtility
    {
        public static Camera GetCanvasCamera(RectTransform rect)
        {
            Canvas canvas = rect.GetComponentInParent<Canvas>();
            if (canvas == null)
                return null;
            return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }

        public static Vector2 WorldPointToAnchored(RectTransform rect, Vector3 worldPoint)
        {
            RectTransform parent = rect.parent as RectTransform;
            if (parent == null)
                return rect.anchoredPosition;

            Vector2 local = parent.InverseTransformPoint(worldPoint);
            Vector2 parentSize = parent.rect.size;
            Vector2 anchorBoxMin = parent.rect.min + Vector2.Scale(parentSize, rect.anchorMin);
            return local - anchorBoxMin;
        }

        public static bool TryScreenPointToAnchored(RectTransform rect, Vector2 screenPoint, out Vector2 anchored)
        {
            anchored = rect.anchoredPosition;
            RectTransform parent = rect.parent as RectTransform;
            if (parent == null)
                return false;

            Camera camera = GetCanvasCamera(rect);
            if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, screenPoint, camera, out Vector3 world))
                return false;

            anchored = WorldPointToAnchored(rect, world);
            return true;
        }

        /// <summary>
        /// 枢轴对齐屏幕水平中线时的 X，Y 保持休息位。
        /// </summary>
        public static Vector2 ScreenCenterX(RectTransform rect, Vector2 rest, Vector2 extraOffset)
        {
            if (!TryScreenPointToAnchored(rect, new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), out Vector2 screenCenter))
                return rest + extraOffset;

            return new Vector2(screenCenter.x + extraOffset.x, rest.y + extraOffset.y);
        }

        /// <summary>
        /// 整块 UI 移到屏幕顶外。X 保持休息位。
        /// </summary>
        public static Vector2 ScreenOutsideTop(RectTransform rect, Vector2 rest, Vector2 extraOffset)
        {
            if (!TryScreenPointToAnchored(rect, new Vector2(Screen.width * 0.5f, Screen.height), out Vector2 screenTop))
                return rest + extraOffset;

            float clearance = rect.rect.height * (1f - rect.pivot.y);
            return new Vector2(rest.x + extraOffset.x, screenTop.y + clearance + extraOffset.y);
        }
    }
}
