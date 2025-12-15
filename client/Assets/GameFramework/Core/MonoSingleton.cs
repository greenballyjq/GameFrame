using UnityEngine;

namespace GameFramework.Core
{
    /// <summary>
    /// 继承自Mono的单例模式
    /// </summary>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance;
        public static T Instance => _instance;

        protected virtual void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this as T;

            DontDestroyOnLoad(gameObject);
        }
    }
}