using Cysharp.Threading.Tasks;

namespace GameFramework.Game
{
    /// <summary>
    /// 应用级管理器接口
    /// </summary>
    public interface IAppManager
    {
        UniTask InitAsync();
        void Dispose();
    }

    /// <summary>
    /// 可更新的应用级管理器
    /// </summary>
    public interface IAppUpdatable
    {
        void DoUpdate(float dt);
    }

    /// <summary>
    /// 可暂停的应用级管理器
    /// </summary>
    public interface IAppPausable
    {
        void Pause();
        void Resume();
    }
}
