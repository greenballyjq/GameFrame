using UnityEngine;

namespace GameFramework.Core.UI
{
    /// <summary>
    /// UI基类 - 所有UI界面的基类
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        /// <summary>
        /// 初始化界面
        /// </summary>
        public virtual void OnInit(object userData)
        {
            // Debug.Log($"[{GetType().Name}] 初始化");
        }
        
        /// <summary>
        /// 更新界面
        /// </summary>
        public virtual void OnUpdate()
        {
            // 子类可以重写此方法来实现特定的更新逻辑
        }
        
        /// <summary>
        /// 关闭界面
        /// </summary>
        public virtual void OnClose()
        {
            // Debug.Log($"[{GetType().Name}] 关闭");
        }
        
        /// <summary>
        /// 显示界面
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }
        
        /// <summary>
        /// 隐藏界面
        /// </summary>
        public virtual void Hide()
        {
            gameObject.SetActive(false);
            OnHide();
        }
        
        /// <summary>
        /// 显示界面时调用
        /// </summary>
        protected virtual void OnShow()
        {
            Debug.Log($"[{GetType().Name}] 显示");
        }
        
        /// <summary>
        /// 隐藏界面时调用
        /// </summary>
        protected virtual void OnHide()
        {
            Debug.Log($"[{GetType().Name}] 隐藏");
        }
        
        /// <summary>
        /// 关闭当前界面
        /// </summary>
        public void Close()
        {
            var uiManager = GameFrameworkManager.Instance.GetManager<UIManager>();
            uiManager.CloseUI(gameObject.name);
        }
    }
} 