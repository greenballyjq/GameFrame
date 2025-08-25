using UnityEngine;
using UnityEngine.UI;
#if WEIXINMINIGAME
using WeChatWASM;
#endif

public class ScreenAdapter : MonoBehaviour
{
    // [SerializeField] private RectTransform rootRect;

    public void SetRootRect(RectTransform rect)
    {
#if UNITY_WEBGL && WEIXINMINIGAME
        CanvasScaler cs = transform.GetComponent<CanvasScaler>();
        var info = WX.GetWindowInfo();
        if (info == null) return;
        Debug.Log($"WindowInfo, top:{info.safeArea.top}, bottom: {info.safeArea.bottom}, left: {info.safeArea.left}" +
                  $", windowHeight: {info.windowHeight}, windowWidth: {info.windowWidth}");
        float py = (float)info.safeArea.top / (float)info.windowHeight;
        float by = ((float)info.windowHeight - (float)info.safeArea.bottom) / (float)info.windowHeight;
        // Rootrect初始时设置其Anchor，使其与父节点一样大，也就是屏幕的大小
        // 调整屏幕移到刘海屏下面,
        rect.offsetMax = new Vector2(rect.offsetMax.x, rect.offsetMax.y - (cs.referenceResolution.y * py));
        rect.offsetMin = new Vector2(rect.offsetMin.x, (rect.offsetMin.y + cs.referenceResolution.y * by));
        // 重新计算缩放，让高度占满刘海屏以下的区域
        cs.referenceResolution = new Vector2(cs.referenceResolution.x, cs.referenceResolution.y * (1 - (py + by)));
#endif
    }
}