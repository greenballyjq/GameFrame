using GameFramework.Core;
using GameFramework.Network;

namespace GameFramework.Network.Proxy
{
    /// <summary>
    /// 网络代理基类 - 消除每个 Proxy 重复的单例 + ServerClient 初始化代码
    /// </summary>
    public abstract class BaseProxy<T> where T : BaseProxy<T>, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                }

                return _instance;
            }
        }

        protected ServerClient ServerClient { get; private set; }

        public virtual void Init()
        {
            ServerClient = GameFrameworkManager.Instance.GetManager<ServerClient>();
        }
    }
}
