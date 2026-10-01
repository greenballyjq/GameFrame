using UnityEngine;

namespace GameFramework.Core
{
    /// <summary>
    /// 场景物体单例
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;

        public static T Instance => _instance;

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = (T)this;
            DontDestroyOnLoad(gameObject);
            OnSingletonAwake();
        }

        protected virtual void OnSingletonAwake() { }

        protected virtual void OnSingletonDestroy() { }

        protected virtual void OnDestroy()
        {
            if (_instance != this)
                return;

            OnSingletonDestroy();
            _instance = null;
        }
    }
}
