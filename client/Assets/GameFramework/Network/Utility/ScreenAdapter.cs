using UnityEngine;
using UnityEngine.UI;

namespace GooseCatcher
{
    public class ScreenAdapter : MonoBehaviour
    {
        // [SerializeField] private RectTransform rootRect;

        public void SetRootRect(RectTransform rect)
        {
            CanvasScaler cs = transform.GetComponent<CanvasScaler>();
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
            var info = WeChatWASM.WX.GetWindowInfo();
            if (info == null) return;
            Debug.Log($"WindowInfo, top:{info.safeArea.top}, bottom: {info.safeArea.bottom}, left: {info.safeArea.left}" +
                      $", windowHeight: {info.windowHeight}, windowWidth: {info.windowWidth}");
            float py = (float)info.safeArea.top / (float)info.windowHeight;
            float by = ((float) info.windowHeight -(float) info.safeArea.bottom) / (float) info.windowHeight;
            // Rootrect初始时设置其Anchor，使其与父节点一样大，也就是屏幕的大小
            // 调整屏幕移到刘海屏下面,
            rect.offsetMax = new Vector2(rect.offsetMax.x, rect.offsetMax.y - (cs.referenceResolution.y * py));
            rect.offsetMin = new Vector2(rect.offsetMin.x, (rect.offsetMin.y + cs.referenceResolution.y * by));
            // 重新计算缩放，让高度占满刘海屏以下的区域
            cs.referenceResolution = new Vector2(cs.referenceResolution.x, cs.referenceResolution.y * (1 - (py + by)));
#elif UNITY_WEBGL && DOUYINMINIGAME && !UNITY_EDITOR
            // 抖音小游戏获取安全区域的API
            var systemInfo = TTSDK.TT.GetSystemInfo();
            if (systemInfo == null) return;

            // 抖音安全区域信息
            var safeArea = systemInfo.safeArea;
            float screenWidth = systemInfo.screenWidth;
            float screenHeight = systemInfo.screenHeight;

            Debug.Log($"Douyin SafeArea - left:{safeArea.left}, right:{safeArea.right}, top:{safeArea.top}, bottom:{safeArea.bottom}");

            // 计算安全区域的比例
            float topRatio = safeArea.top / screenHeight;
            float bottomRatio = (screenHeight - safeArea.bottom) / screenHeight;

            // 调整RectTransform的偏移以适应安全区域
            rect.offsetMax = new Vector2(rect.offsetMax.x, rect.offsetMax.y - (cs.referenceResolution.y * topRatio));
            rect.offsetMin = new Vector2(rect.offsetMin.x, rect.offsetMin.y + (cs.referenceResolution.y * bottomRatio));

            // 调整参考分辨率
            cs.referenceResolution = new Vector2(cs.referenceResolution.x, cs.referenceResolution.y * (1 - (topRatio + bottomRatio)));
#elif UNITY_WEBGL && KUAISHOUMINIGAME && !UNITY_EDITOR
            KSWASM.KS.GetSystemInfo(new KSWASM.GetSystemInfoOption
            {
                success = (systemInfo) =>
                {
                    if (systemInfo == null) return;

                    var safeArea = systemInfo.safeArea;
                    float screenHeight = (float)systemInfo.screenHeight;

                    float topRatio = (float)safeArea.top / screenHeight;
                    float bottomRatio = (float)(screenHeight - safeArea.bottom) / screenHeight;

                    rect.offsetMax = new Vector2(rect.offsetMax.x, rect.offsetMax.y - (cs.referenceResolution.y * topRatio));
                    rect.offsetMin = new Vector2(rect.offsetMin.x, rect.offsetMin.y + (cs.referenceResolution.y * bottomRatio));
                    cs.referenceResolution = new Vector2(cs.referenceResolution.x, cs.referenceResolution.y * (1 - (topRatio + bottomRatio)));
                }
            });
#endif
        }

    }
}