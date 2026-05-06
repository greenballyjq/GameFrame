using System;
using UnityEngine;
#if WEIXINMINIGAME
using WeChatWASM;
#endif

namespace GameFramework.Utility
{
    /// <summary>
    /// 平台 RectTransform 屏幕点转换器。
    /// </summary>
    public static class PlatformRectTransformScreenPointTransformer
    {
#if WEIXINMINIGAME
        /// <summary>
        /// RectTransform 转换为微信平台创建用户按钮屏幕点
        /// </summary>
        /// <param name="rectTransform">需要转换的ui rectTransform</param>
        /// <param name="uiCamera">ui的worldCamera</param>
        /// <returns></returns>
        public static WeChatPlatform.WechatPoint RectTransformToWechatCreateUserInfoButtonPoint(RectTransform rectTransform, Camera uiCamera)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[i]);
            }
            
            // 计算宽高
            float width = corners[2].x - corners[0].x;
            float height = corners[2].y - corners[0].y;
            
            // 左下角坐标 (x, y)
            Vector2 position = corners[0];
            
            Rect screenRect = new Rect(position.x, position.y, width, height);
            
            float wxXFloat = screenRect.x;
            float wxYFloat = Screen.height - screenRect.y - screenRect.height;
            
            // 转换为整数像素（微信小游戏通常需要 int）
            int wxX = Mathf.RoundToInt(wxXFloat);
            int wxY = Mathf.RoundToInt(wxYFloat);
            int wxWidth = Mathf.RoundToInt(screenRect.width);
            int wxHeight = Mathf.RoundToInt(screenRect.height);
            
            Debug.Log($"[微信按钮参数] x: {wxX}, y: {wxY}, width: {wxWidth}, height: {wxHeight}");

            return new WeChatPlatform.WechatPoint
            {
                x = wxX,
                y = wxY,
                width = wxWidth,
                height = wxHeight
            };
        }

        /// <summary>
        /// RectTransform 转换为微信平台创建游戏圈按钮屏幕点
        /// </summary>
        /// <param name="rectTransform">需要转换的ui rectTransform</param>
        /// <param name="uiCamera">ui的worldCamera</param>
        /// <returns></returns>
        public static WeChatPlatform.WechatPoint RectTransformToWechatCreateGameClubButtonPoint(RectTransform rectTransform,
            Camera uiCamera)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[i]);
            }
            
            float leftX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float topY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

            // Debug.Log($"[测试微信按钮参数] corners[0]: {corners[0]}, corners[1]: {corners[1]}, corners[2]: {corners[2]}, corners[3]: {corners[3]}");
            // Debug.Log($"[测试微信按钮参数] leftX: {leftX}, topY: {topY}");

            double wRatio = WX.GetSystemInfoSync().screenWidth / Screen.width;
            double hRatio = WX.GetSystemInfoSync().screenHeight / Screen.height;

            float wRatioF = (float)(WX.GetSystemInfoSync().screenWidth / Screen.width);
            float hRatioF = (float)(WX.GetSystemInfoSync().screenHeight / Screen.height);

            Debug.Log($"[测试微信按钮参数] wRadioF: {wRatioF}, hRadioF: {hRatioF}; wRatio: {wRatio}, hRatio: {hRatio}");

            int width = (int)(rectTransform.sizeDelta.x * wRatio);
            int height = (int)(rectTransform.sizeDelta.x * hRatio);

            width += (width / 4);
            height += (height / 4);
            
            int left = (int)(leftX * wRatio) + width / 4;
            int top =  (int)(WX.GetSystemInfoSync().screenHeight - topY * hRatio) + height / 8;
    
            Debug.Log($"[测试微信按钮参数] WX screenWidth: {WX.GetSystemInfoSync().screenWidth}, Screen.width: {Screen.width}, " +
                      $"WX screenHeight: {WX.GetSystemInfoSync().screenHeight}, Screen.height: {Screen.height}");
            
            Debug.Log($"[测试微信按钮参数] x: {left}, y: {top}, width: {width}, height: {height}");

            return new WeChatPlatform.WechatPoint
            {
                x = left,
                y = top,
                width = width,
                height = height
            };
        }
#endif
    }
}