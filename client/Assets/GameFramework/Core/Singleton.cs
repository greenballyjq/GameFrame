namespace GameFramework.Core
{
    /// <summary>
    /// 单例基类
    /// </summary>
    public class Singleton<T> where T : Singleton<T>, new()
    {
        private static T _instance;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new T();

                return _instance;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        protected Singleton()
        {
            _instance = (T)this;
        }

        public static bool HasInstance => _instance != null;

        public static void ClearInstance()
        {
            _instance = null;
        }
    }
}

