using GameFramework.Core;
using UnityEngine;

namespace GameFramework.UI
{
    public abstract class UIForm : MonoBehaviour
    {
        protected virtual void OnCreate()
        {
        }

        protected virtual void OnOpen()
        {
        }

        protected virtual void OnClose()
        {
        }

        protected virtual void OnHide()
        {
        }

        protected virtual void OnShow()
        {
        }

        public void Close()
        {
            GameFrameworkManager.Instance.GetManager<UIManager>().Close(this);
        }

        internal void NotifyCreate()
        {
            OnCreate();
        }

        internal void NotifyOpen()
        {
            OnOpen();
        }

        internal void NotifyClose()
        {
            OnClose();
        }

        internal void NotifyHide()
        {
            OnHide();
        }

        internal void NotifyShow()
        {
            OnShow();
        }
    }

    public abstract class UIForm<TArgs> : UIForm
    {
        protected virtual void OnOpen(TArgs args)
        {
        }

        protected sealed override void OnOpen()
        {
        }

        internal void NotifyOpen(TArgs args)
        {
            OnOpen(args);
        }
    }
}
