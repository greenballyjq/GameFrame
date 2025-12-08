using UnityEngine;

namespace GameFramework.Utility
{
    /// <summary>
    /// 坐标转换工具
    /// </summary>
    public static class PointConverter
    {
        /// <summary>
        /// UI坐标转屏幕坐标
        /// 当 Canvas renderMode 为 RenderMode.ScreenSpaceCamera、RenderMode.WorldSpace 时 传递参数 canvas.worldCamera
        /// 当 Canvas renderMode 为 RenderMode.ScreenSpaceOverlay 时 传递参数 null
        /// </summary>
        /// <param name="uiPoint">UI坐标</param>
        /// <param name="uiCamera">UI摄像机</param>
        /// <returns></returns>
        public static Vector2 UiPointToScreenPoint(Vector3 uiPoint, Camera uiCamera = null)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, uiPoint);
            return screenPoint;
        }

        /// <summary>
        /// 屏幕坐标转为UI的anchoredPosition
        /// </summary>
        /// <param name="parentRt"></param>
        /// <param name="screenPoint"></param>
        /// <param name="uiCamera"></param>
        /// <returns></returns>
        public static Vector3 ScreenPointToUiLocalPoint(RectTransform parentRt, Vector2 screenPoint,
            Camera uiCamera = null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, screenPoint, uiCamera, out localPoint);
            return localPoint;
        }

        /// <summary>
        /// 屏幕坐标转为UI坐标
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="screenPoint"></param>
        /// <param name="uiCamera"></param>
        /// <returns></returns>
        public static Vector3 ScreenPointToUiPoint(RectTransform rt, Vector2 screenPoint, Camera uiCamera = null)
        {
            Vector3 globalPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, screenPoint, uiCamera, out globalPos);
            return globalPos;
        }

        /// <summary>
        /// 世界坐标转屏幕坐标
        /// </summary>
        /// <param name="worldPoint">世界坐标</param>
        /// <param name="camera">摄像机（MainCamera）</param>
        /// <returns></returns>
        public static Vector2 WorldPointToScreenPoint(Vector3 worldPoint, Camera camera)
        {
            Vector2 worldToScreenPoint = camera.WorldToScreenPoint(worldPoint);
            return worldToScreenPoint;
        }

        /// <summary>
        /// 屏幕坐标转世界坐标
        /// </summary>
        /// <param name="screenPoint">屏幕坐标</param>
        /// <param name="camera">摄像机（MainCamera）</param>
        /// <param name="planeZ">距离摄像机 Z 平面的距离</param>
        /// <returns></returns>
        public static Vector3 ScreenPointToWorldPoint(Vector2 screenPoint, Camera camera, float planeZ)
        {
            Vector3 position = new Vector3(screenPoint.x, screenPoint.y, planeZ);
            Vector3 worldPoint = camera.ScreenToWorldPoint(position);
            return worldPoint;
        }

        /// <summary>
        /// UI坐标转世界坐标
        /// </summary>
        /// <param name="uiPoint"></param>
        /// <param name="camera"></param>
        /// <param name="planeZ"></param>
        /// <returns></returns>
        public static Vector3 UiPointToWorldPoint(Vector3 uiPoint, Camera camera, float planeZ)
        {
            Vector2 screenPoint = UiPointToScreenPoint(uiPoint, camera);
            Vector3 worldPoint = ScreenPointToWorldPoint(screenPoint, camera, planeZ);
            return worldPoint;
        }
    }
}